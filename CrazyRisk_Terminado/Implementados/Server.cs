#nullable enable
using CrazyRisk.Comms;
using CrazyRisk.Models;
using CrazyRisk.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Server
{
    private const int MainPort = 1234;
    private const int player1Port = 1665;
    private const int player2Port = 1666;

    private static GameController game = new GameController();

    // ---- Added: helpers to allow starting the server from the UI ----
    private static bool _mainServerStarted = false;
    private static readonly object _startLock = new object();

    /// <summary>
    /// Public accessor to the main server port (read-only).
    /// </summary>
    public static int MainServerPort => MainPort;

    /// <summary>
    /// Returns whether the main server has been started.
    /// </summary>
    public static bool IsMainServerRunning => _mainServerStarted;

    /// <summary>
    /// Ensures the main server and game servers are started (safe to call multiple times).
    /// This method launches the internal async servers on background Tasks.
    /// </summary>
    public static void EnsureMainServerStarted()
    {
        lock (_startLock)
        {
            if (_mainServerStarted) return;
            _mainServerStarted = true;

            try
            {
                // Start main server and two game servers in background tasks
                _ = Task.Run(() => StartMainServer());
                _ = Task.Run(() => StartGameServer(player1Port, "Jugador 1"));
                _ = Task.Run(() => StartGameServer(player2Port, "Jugador 2"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER] Error iniciando servidores: {ex.Message}");
                _mainServerStarted = false;
            }
        }
    }

    // ---- end added helpers ----

    private static readonly ConcurrentDictionary<string, StreamWriter> gameClients =
        new ConcurrentDictionary<string, StreamWriter>();

    // Helpers para extraer valores desde MensajeJuego.Datos (puede contener JsonElement)
    private static string? GetStringFromDict(Dictionary<string, object> dict, string key)
    {
        if (dict == null || !dict.ContainsKey(key)) return null;
        var val = dict[key];
        if (val == null) return null;
        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String) return je.GetString();
            return je.ToString();
        }
        return val.ToString();
    }

    private static int GetIntFromDict(Dictionary<string, object> dict, string key, int defaultValue = 0)
    {
        if (dict == null || !dict.ContainsKey(key)) return defaultValue;
        var val = dict[key];
        if (val == null) return defaultValue;
        if (val is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out int v)) return v;
            if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out v)) return v;
            return defaultValue;
        }
        if (int.TryParse(val.ToString(), out int res)) return res;
        return defaultValue;
    }

    public static async Task Main(string[] args)
    {
        Console.WriteLine("[SERVER] Iniciando servidores...");

        // Inicia servidor principal (lobby) y servidores de juego
        var mainTask = StartMainServer();
        var p1Task = StartGameServer(player1Port, "Jugador 1");
        var p2Task = StartGameServer(player2Port, "Jugador 2");

        await Task.WhenAll(mainTask, p1Task, p2Task);
    }

    private static async Task StartMainServer()
    {
        var listener = new TcpListener(IPAddress.Any, MainPort);
        listener.Start();
        Console.WriteLine($"[SERVER] Main server escuchando en puerto {MainPort}...");

        int assignedCount = 0;
        while (true)
        {
            try
            {
                var tcp = await listener.AcceptTcpClientAsync();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var stream = tcp.GetStream();
                        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                        // Alternar puertos asignados entre player1Port y player2Port
                        int assigned = (assignedCount % 2 == 0) ? player1Port : player2Port;
                        assignedCount++;
                        await writer.WriteLineAsync(assigned.ToString());
                        Console.WriteLine($"[SERVER] Cliente conectado - puerto asignado: {assigned}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SERVER MAIN] Error aceptando cliente: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER MAIN] Error en listener: {ex.Message}");
            }
        }
    }

    private static async Task StartGameServer(int port, string playerName)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"[GAME SERVER] {playerName} escuchando en puerto {port}...");

        while (true)
        {
            try
            {
                var tcp = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleGameClient(tcp, playerName));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GAME SERVER] Error en puerto {port}: {ex.Message}");
            }
        }
    }

    private static async Task HandleGameClient(TcpClient tcpClient, string playerName)
    {
        try
        {
            using var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Registrar writer para broadcast
            gameClients[playerName] = writer;

            // Enviar estado inicial al cliente
            try
            {
                var init = new MensajeJuego("StateUpdate", "Server");
                init.Datos["State"] = game.GetSimplifiedMapState();
                await BroadcastUpdate(init);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER] Error enviando estado inicial: {ex.Message}");
            }

            Console.WriteLine($"[GAME CLIENT] {playerName} conectado.");

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                Console.WriteLine($"[GAME {playerName}] Mensaje recibido: {line}");
                try
                {
                    var msg = JsonHelper.DeserializarComando(line);
                    if (msg == null) continue;

                    var tipo = (msg.TipoComando ?? "").ToUpperInvariant();
                    bool actionSuccess = false;
                    string actionMsg = "";

                    if (tipo == "ATACAR" || tipo == "ATTACK")
                    {
                        string? origen = GetStringFromDict(msg.Datos, "Origen") ?? GetStringFromDict(msg.Datos, "TerritorioOrigen");
                        string? destino = GetStringFromDict(msg.Datos, "Destino") ?? GetStringFromDict(msg.Datos, "TerritorioDestino");
                        int dados = GetIntFromDict(msg.Datos, "Dados", 1);
                        int tropasMover = GetIntFromDict(msg.Datos, "TropasMover", dados);

                        var tOrigen = game.Mapa.BuscarTerritorio(origen ?? "");
                        var tDestino = game.Mapa.BuscarTerritorio(destino ?? "");

                        if (tOrigen != null && tDestino != null)
                        {
                            var resultado = game.ProcesarAtaque(tOrigen, tDestino, dados, tropasMover);
                            actionSuccess = true;
                            actionMsg = $"Ataque procesado: conquista={resultado.conquista}, bajasAtk={resultado.bajasAtk}, bajasDef={resultado.bajasDef}";
                        }
                        else
                        {
                            actionSuccess = false;
                            actionMsg = "Territorio origen o destino inválido.";
                        }
                    }
                    else if (tipo == "MOVER" || tipo == "MOVE")
                    {
                        string? origen = GetStringFromDict(msg.Datos, "Origen");
                        string? destino = GetStringFromDict(msg.Datos, "Destino");
                        int cantidad = GetIntFromDict(msg.Datos, "Cantidad", 0);

                        var tOrigen = game.Mapa.BuscarTerritorio(origen ?? "");
                        var tDestino = game.Mapa.BuscarTerritorio(destino ?? "");

                        if (tOrigen != null && tDestino != null)
                        {
                            bool ok = game.MoverTropas(tOrigen, tDestino, cantidad);
                            actionSuccess = ok;
                            actionMsg = ok ? "Movimiento realizado" : "Movimiento inválido (rutas o tropas insuficientes)";
                        }
                        else
                        {
                            actionSuccess = false;
                            actionMsg = "Territorio origen o destino inválido.";
                        }
                    }
                    else if (tipo == "REFORZAR" || tipo == "COLOCAR" || tipo == "PLACE_REINFORCEMENT")
                    {
                        string? territorio = GetStringFromDict(msg.Datos, "Territorio");
                        if (!string.IsNullOrEmpty(territorio))
                        {
                            bool ok = game.ColocarRefuerzo(territorio);
                            actionSuccess = ok;
                            actionMsg = ok ? $"Refuerzo colocado en {territorio}" : "No se pudo colocar refuerzo.";
                        }
                        else
                        {
                            actionSuccess = false;
                            actionMsg = "Territorio inválido.";
                        }
                    }
                    else if (tipo == "ENDTURN" || tipo == "PASAR_TURNO")
                    {
                        game.CambiarTurno();
                        actionSuccess = true;
                        actionMsg = "Turno finalizado.";
                    }
                    else
                    {
                        actionSuccess = false;
                        actionMsg = $"Comando no reconocido: {msg.TipoComando}";
                    }

                    // Enviar resultado al jugador que hizo la acción
                    var resultMsg = new MensajeJuego("ActionResult", "Server");
                    resultMsg.Datos["Success"] = actionSuccess;
                    resultMsg.Datos["Message"] = actionMsg;
                    await SendToClient(playerName, resultMsg);

                    // Enviar estado actualizado a todos
                    var update = new MensajeJuego("StateUpdate", "Server");
                    update.Datos["State"] = game.GetSimplifiedMapState();
                    await BroadcastUpdate(update);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER] Error procesando mensaje: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GAME CLIENT] Error con {playerName}: {ex.Message}");
        }
    }

    private static async Task BroadcastUpdate(MensajeJuego update)
    {
        try
        {
            string json = JsonHelper.SerializarComando(update);
            foreach (var writer in gameClients.Values)
            {
                try
                {
                    await writer.WriteLineAsync(json);
                    await writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BROADCAST] Error enviando a cliente: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BROADCAST] Error general: {ex.Message}");
        }
    }

    private static async Task SendToClient(string playerName, MensajeJuego update)
    {
        try
        {
            if (gameClients.TryGetValue(playerName, out var writer))
            {
                string json = JsonHelper.SerializarComando(update);
                await writer.WriteLineAsync(json);
                await writer.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVER SEND] Error enviando a {playerName}: {ex.Message}");
        }
    }
}
