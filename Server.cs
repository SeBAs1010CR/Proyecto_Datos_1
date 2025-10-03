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
        NetworkStream stream = null;
        try
        {
            stream = client.GetStream();
            using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
            {
                string? receivedJson;
                // Lee línea por línea, esperando un mensaje completo por línea
                while ((receivedJson = await reader.ReadLineAsync()) != null)
                {
                    Console.WriteLine($"{playerName} dice (JSON): {receivedJson}");
                    
                    // 1. Deserializar el JSON recibido
                    var comando = JsonHelper.DeserializarComando(receivedJson);

                    if (comando != null)
                    {
                        // 2. Procesar el Comando (Esto es la parte clave)
                        // Aquí llamarías a tu GameController para ejecutar la lógica:
                        // GameController.ProcesarComando(comando);

                        // --- EJEMPLO de RESPUESTA ---
                        // El servidor genera una respuesta (Ej: Estado actualizado)
                        var responseComando = new MensajeJuego("RESPUESTA", "SERVER");
                        responseComando.Datos.Add("status", "OK");
                        responseComando.Datos.Add("message", $"Comando {comando.TipoComando} recibido.");
                        
                        string responseJson = JsonHelper.SerializarComando(responseComando);
                        
                        // 3. Serializar y Enviar la Respuesta (con un delimitador \n)
                        byte[] responseData = Encoding.UTF8.GetBytes(responseJson + "\n");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
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
}