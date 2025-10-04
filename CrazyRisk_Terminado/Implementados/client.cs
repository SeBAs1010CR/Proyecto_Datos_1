using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CrazyRisk.Comms
{
    public class GameClient
    {
        private readonly string serverIp;
        public string PlayerAlias { get; private set; }

        private TcpClient? tcpMain;
        private TcpClient? tcpGame;
        private StreamReader? reader;
        private StreamWriter? writer;

        public event Action<MensajeJuego>? UpdateReceived;

        public GameClient(string ip, string alias)
        {
            serverIp = ip;
            PlayerAlias = alias;
        }

        // Inicia la conexión: primero consulta puerto en MainPort (1234), luego se conecta al puerto asignado.
        public async Task StartClientMode()
        {
            try
            {
                // Conectar al main server para obtener puerto asignado
                tcpMain = new TcpClient();
                await tcpMain.ConnectAsync(serverIp, 1234);
                using (var stream = tcpMain.GetStream())
                using (var r = new StreamReader(stream, Encoding.UTF8))
                {
                    string? line = await r.ReadLineAsync();
                    if (!int.TryParse(line, out int assignedPort))
                    {
                        // fallback en caso de parseo
                        assignedPort = 1665;
                    }

                    // Ahora conectar al puerto de juego asignado
                    tcpGame = new TcpClient();
                    await tcpGame.ConnectAsync(serverIp, assignedPort);
                    var ns = tcpGame.GetStream();
                    reader = new StreamReader(ns, Encoding.UTF8);
                    writer = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                    // Enviar mensaje de join (opcional)
                    var join = new MensajeJuego("Join", PlayerAlias);
                    join.Datos["Alias"] = PlayerAlias;
                    await SendCommand(join);

                    // Iniciar loop de lectura
                    _ = Task.Run(ReadLoopAsync);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Error StartClientMode: {ex.Message}");
                throw;
            }
        }

        private async Task ReadLoopAsync()
        {
            try
            {
                while (reader != null)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;
                    var msg = JsonHelper.DeserializarComando(line);
                    if (msg != null)
                    {
                        // Propagar evento
                        try { UpdateReceived?.Invoke(msg); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Read loop error: {ex.Message}");
            }
            finally
            {
                // Cleanup si se desconecta
                try { tcpGame?.Close(); } catch { }
            }
        }

        // Único SendCommand: envía MensajeJuego al servidor
        public async Task SendCommand(MensajeJuego msg)
        {
            if (writer == null)
            {
                Console.WriteLine("[CLIENT] Writer no establecido. No se puede enviar comando.");
                return;
            }

            try
            {
                string json = JsonHelper.SerializarComando(msg);
                await writer.WriteLineAsync(json);
                await writer.FlushAsync();
                Console.WriteLine($"[CLIENT] Enviado: {msg.TipoComando}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Error al enviar comando: {ex.Message}");
            }
        }

        // Opcional: método para cerrar cliente de forma ordenada
        public void Close()
        {
            try { tcpGame?.Close(); } catch { }
            try { tcpMain?.Close(); } catch { }
        }
    }
}
