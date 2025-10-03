using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CrazyRisk.Comms; // Asumiendo que aquí está MensajeJuego y JsonHelper

namespace CrazyRisk.Comms
{
    // Clase principal del cliente de juego
    public class GameClient
    {
        // Delegado y Evento: La interfaz se suscribe a este evento para recibir actualizaciones.
        public delegate void ServerUpdateHandler(MensajeJuego update);
        public event ServerUpdateHandler? UpdateReceived; 
        
        // El escritor es la clave para enviar comandos al servidor.
        private StreamWriter? Writer { get; set; }
        private readonly string _serverIP;
        public readonly string PlayerAlias;

        private const int MainPort = 1234;

        public GameClient(string serverIP, string alias)
        {
            _serverIP = serverIP;
            PlayerAlias = alias;
        }

        // 1. Punto de entrada principal (llamado por MainWindow.xaml.cs)
        public async Task StartClientMode()
        {
            Console.WriteLine($"Conectando a {_serverIP} como {PlayerAlias}...");
            
            // Paso 1: Obtener el puerto de juego asignado (1665 o 1666)
            int gamePort = await GetGamePortAsync(_serverIP); 

            if (gamePort != -1)
            {
                Console.WriteLine($"Conexión exitosa. Iniciando bucle de juego en puerto {gamePort}.");
                
                // Paso 2: Iniciar el bucle de comunicación principal en el puerto asignado
                await RunGameLoop(_serverIP, gamePort, PlayerAlias);
            }
            else
            {
                Console.WriteLine("Error: No se pudo conectar o la sala está llena.");
            }
        }

        // 2. Conexión inicial al puerto 1234 para obtener el puerto de juego
        private async Task<int> GetGamePortAsync(string serverIP)
        {
            try
            {
                using var client = new TcpClient();
                // Conectar al puerto principal (Lobby)
                await client.ConnectAsync(IPAddress.Parse(serverIP), MainPort); 

                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                // El servidor envía el puerto de juego como una cadena simple ("1665" o "1666")
                string? portString = await reader.ReadLineAsync();
                
                if (int.TryParse(portString, out int gamePort))
                {
                    return gamePort;
                }
                return -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener puerto de juego: {ex.Message}");
                return -1;
            }
        }

        // 3. Bucle de juego: Conexión al puerto asignado y bucle de escucha
        private async Task RunGameLoop(string serverIP, int port, string alias)
        {
            TcpClient client = new TcpClient();
            try
            {
                // Conectar al puerto de juego asignado (1665 o 1666)
                await client.ConnectAsync(IPAddress.Parse(serverIP), port); 

                NetworkStream stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                // Usar StreamWriter con AutoFlush para enviar comandos inmediatamente
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true }; 
                
                // **ALMACENAR EL ESCRITOR:** Es el canal de salida para SendCommand
                Writer = writer; 

                // Iniciar la escucha continua del servidor en un hilo separado
                var listenerTask = ListenForServerUpdates(reader);
                
                // NOTA: No enviamos un comando de CONEXION explícito aquí. 
                // El servidor ya sabe qué alias eres por el puerto (1665 -> Jugador 1).

                await listenerTask; // Esperar a que la escucha termine
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en RunGameLoop: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        // 4. Método para que la UI envíe un comando al servidor
        public async Task SendCommand(MensajeJuego comando)
        {
            if (Writer != null)
            {
                string jsonToSend = JsonHelper.SerializarComando(comando);
                // WriteLineAsync envía la cadena y añade el delimitador \n
                await Writer.WriteLineAsync(jsonToSend);
            }
            else
            {
                Console.WriteLine("Error: Conexión de escritura no establecida.");
            }
        }

        // 5. Escucha constante del servidor
        private async Task ListenForServerUpdates(StreamReader reader)
        {
            try
            {
                string? receivedJson;
                while ((receivedJson = await reader.ReadLineAsync()) != null)
                {
                    var update = JsonHelper.DeserializarComando(receivedJson);
                    if (update != null)
                    {
                        // Notificar a la interfaz (MainWindow.xaml.cs) que hay una actualización
                        UpdateReceived?.Invoke(update); 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Conexión con el servidor perdida: {ex.Message}");
            }
        }
    }
}