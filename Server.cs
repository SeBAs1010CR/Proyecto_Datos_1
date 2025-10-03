using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CrazyRisk.Comms;

class Server
{
    private static int playerCount = 0;
    private static readonly object lockObject = new object();

    private const int mainPort = 1234;
    private const int player1Port = 1665;
    private const int player2Port = 1666;
    private static GameController game = new GameController(); // Instancia ÚNICA del juego
    // Diccionario para almacenar el StreamWriter de cada jugador por su alias.
    // Esto permite enviar mensajes a ambos clientes.
    private static readonly ConcurrentDictionary<string, StreamWriter> gameClients = new ConcurrentDictionary<string, StreamWriter>();

    public static async Task Main(string[] args)
    {
        // Tarea para el servidor principal de asignación
        var mainServerTask = Task.Run(() => StartMainServer());

        // Tareas para los servidores de juego de cada jugador
        var player1ServerTask = Task.Run(() => StartGameServer(player1Port, "Jugador 1"));
        var player2ServerTask = Task.Run(() => StartGameServer(player2Port, "Jugador 2"));

        // Espera a que todas las tareas se completen (lo que en este caso no ocurrirá)
        await Task.WhenAll(mainServerTask, player1ServerTask, player2ServerTask);
    }

    // Servidor principal que asigna los puertos
    private static void StartMainServer()
    {
        TcpListener server = null;
        try
        {
            server = new TcpListener(IPAddress.Any, mainPort);
            server.Start();
            Console.WriteLine($"Servidor principal escuchando en el puerto {mainPort}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Nueva conexión en el puerto principal.");
                Task.Run(() => AssignPlayerPort(client));
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Error del servidor principal: {e.Message}");
        }
        finally
        {
            server?.Stop();
        }
    }

    // Servidor de juego que maneja la comunicación de un jugador
    private static void StartGameServer(int port, string playerName)
    {
        TcpListener server = null;
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Servidor de {playerName} escuchando en el puerto {port}...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine($"¡{playerName} se ha conectado!");
                HandleGameClient(client, playerName);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Error del servidor de {playerName}: {e.Message}");
        }
        finally
        {
            server?.Stop();
        }
    }

    private static void AssignPlayerPort(TcpClient client)
    {
        NetworkStream stream = null;
        try
        {
            stream = client.GetStream();
            int assignedPort;
            lock (lockObject)
            {
                playerCount++;
                if (playerCount == 1)
                {
                    assignedPort = player1Port;
                }
                else if (playerCount == 2)
                {
                    assignedPort = player2Port;
                }
                else
                {
                    assignedPort = 0; // Puerto 0 para sala llena
                }
            }

            if (assignedPort != 0)
            {
                Console.WriteLine($"Redirigiendo cliente a puerto {assignedPort}");
                byte[] response = Encoding.UTF8.GetBytes(assignedPort.ToString());
                stream.Write(response, 0, response.Length);
            }
            else
            {
                Console.WriteLine("Conexión rechazada: sala llena.");
                byte[] response = Encoding.UTF8.GetBytes("Sala llena.");
                stream.Write(response, 0, response.Length);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al asignar puerto: {e.Message}");
        }
        finally
        {
            stream?.Close();
            client.Close();
        }
    }

    // Lógica para manejar la comunicación del juego
    private static async Task HandleGameClient(TcpClient client, string playerName)
    {
        NetworkStream stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.UTF8);
        // IMPORTANTE: Usar StreamWriter para WriteLineAsync, con AutoFlush.
        var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true }; 
        
        // 1. Almacenar el escritor para el Broadcast
        // Usamos el playerName que se pasó desde StartGameServer
        gameClients.TryAdd(playerName, writer); 
        
        // **ESTABLECER EL JUGADOR INICIAL DE LA INSTANCIA DE JUEGO**
        if (game.Actual == null)
        {
            // Asumiendo que Jugador1 se inicializa en el constructor de GameController
            game.Actual = game.Jugador1; 
            Console.WriteLine($"Turno inicial asignado a: {game.Actual.Alias}");
            
            // NOTA: Si necesitas preparar refuerzos, hazlo aquí o al inicio de GameController.
            // game.PrepararRefuerzosIniciales(); 
        }

        try
        {
            string? receivedJson;
            while ((receivedJson = await reader.ReadLineAsync()) != null)
            {
                var comando = JsonHelper.DeserializarComando(receivedJson);

                if (comando != null)
                {
                    Console.WriteLine($"Comando recibido de {comando.JugadorAlias}: {comando.TipoComando}");
                    
                    // 2. VALIDACIÓN BÁSICA: ¿Es su turno?
                    if (comando.JugadorAlias != game.Actual.Alias)
                    {
                        // Opcional: Enviar mensaje de error solo a este cliente.
                        continue; 
                    }

                    bool stateChanged = false;

                    // 3. LÓGICA CENTRAL DE JUEGO (Switch Case)
                    switch (comando.TipoComando)
                    {
                        case "COLOCAR_TROPA":
                            // *TODO: Implementa el método ColocarRefuerzo en GameController.*
                            // if (game.PuedeColocarRefuerzo() && game.ColocarRefuerzo(comando.Datos, comando.JugadorAlias))
                            // {
                            //     stateChanged = true;
                            // }
                            stateChanged = true; // Asumimos éxito por ahora
                            break;

                        case "FIN_FASE": // El cliente envía esto al terminar una fase (Refuerzo, Ataque, Movimiento)
                            game.AvanzarFase(); // *TODO: Implementa AvanzarFase en GameController.*
                            stateChanged = true;
                            break;
                            
                        case "FIN_TURNO":
                            game.CambiarTurno();
                            game.EtapaActual = EtapaTurno.Refuerzo; // Reiniciar fase
                            stateChanged = true;
                            break;
                            
                        // Añadir más casos (ATAQUE, MOVER, INTERCAMBIAR_TARJETAS...)
                    }

                    // 4. BROADCAST: Enviar nuevo estado a todos
                    if (stateChanged)
                    {
                        // Crea un estado simplificado del mapa (Dueño y Tropas de cada Territorio)
                        // para enviarlo por la red (ver nota en sección C).
                        var mapState = game.GetSimplifiedMapState(); // *TODO: Implementa este método*
                        
                        var update = new MensajeJuego("ACTUALIZAR_ESTADO", "SERVER");
                        update.Datos.Add("JugadorActual", game.Actual.Alias);
                        update.Datos.Add("EtapaActual", game.EtapaActual.ToString());
                        update.Datos.Add("Mapa", mapState); 
                        
                        await BroadcastUpdate(update);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error de conexión con {playerName}: {e.Message}");
        }
        finally
        {
            stream?.Close();
            client.Close();
        }
    }
    // Envía la actualización de estado a AMBOS clientes (1665 y 1666).
    private static async Task BroadcastUpdate(MensajeJuego update)
    {
        string json = JsonHelper.SerializarComando(update);
        Console.WriteLine($"[SERVER BROADCAST] {update.TipoComando}");
        
        // Envía el mensaje a todos los jugadores conectados.
        foreach (var writer in gameClients.Values)
        {
            await writer.WriteLineAsync(json);
            await writer.FlushAsync();
        }
    }

    // Servidor de juego que maneja la comunicación de un jugador
    private static async Task StartGameServer(int port, string playerName) // Convertido a async
    {
        TcpListener server = null;
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Servidor de juego para {playerName} escuchando en puerto {port}...");

            while (true)
            {
                // Solo un cliente por este puerto
                TcpClient client = await server.AcceptTcpClientAsync(); // Usar Async
                Console.WriteLine($"{playerName} conectado al puerto {port}.");
                
                // Procesar la comunicación de este cliente en una tarea separada
                _ = Task.Run(() => HandleGameClient(client, playerName)); 
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Error del servidor {playerName}: {e.Message}");
        }
        finally
        {
            server?.Stop();
        }
    }
}
