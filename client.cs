using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CrazyRisk.Comms; // Asumiendo tu namespace de comunicación

public static class Client
{
    private const int MainPort = 1234;

    // 1. Punto de entrada principal (el que llama tu GUI)
    public static async Task StartClientMode(string serverIP, string alias)
    {
        Console.WriteLine($"Conectando a {serverIP} como {alias}...");
        
        // Paso 1: Obtener el puerto de juego asignado
        int gamePort = await GetGamePortAsync(serverIP); 

        if (gamePort != -1)
        {
            Console.WriteLine($"Conexión exitosa. Iniciando bucle de juego en puerto {gamePort}.");
            
            // Paso 2: Iniciar el bucle de comunicación principal
            await RunGameLoop(serverIP, gamePort, alias);
        }
        else
        {
            Console.WriteLine("Error: No se pudo conectar o la sala está llena.");
        }
    }

    // 2. Conexión inicial al puerto 1234 para obtener el puerto de juego
    private static async Task<int> GetGamePortAsync(string serverIP)
    {
        try
        {
            using var client = new TcpClient();
            // 1. Conectar al puerto principal
            await client.ConnectAsync(serverIP, MainPort);

            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // 2. Enviar solicitud (lo que el servidor espera para asignarte)
            await writer.WriteLineAsync($"SOLICITAR_PUERTO:{Environment.MachineName}"); 
            
            // 3. Recibir la respuesta del puerto (ej: "1665:Jugador 1")
            string? response = await reader.ReadLineAsync();

            if (response != null && response.Contains(':'))
            {
                string[] parts = response.Split(':');
                if (int.TryParse(parts[0], out int port))
                {
                    // Si recibimos un puerto válido, lo devolvemos
                    return port;
                }
            }
            return -1; // Fallo en la asignación o respuesta no válida
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error de conexión inicial: {ex.Message}");
            return -1;
        }
    }

    // 3. Bucle principal de juego: maneja el envío y la recepción de comandos JSON
    public static async Task RunGameLoop(string serverIP, int gamePort, string playerAlias)
    {
        // El cliente debe mantener una conexión persistente al puerto asignado.
        using var client = new TcpClient();
        await client.ConnectAsync(serverIP, gamePort);

        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        // Tarea para escuchar asíncronamente las actualizaciones del servidor (necesario para no bloquear la UI)
        var listenerTask = Task.Run(() => ListenForServerUpdates(reader));

        // --- Lógica de la interfaz de usuario para ENVÍO ---
        // Aquí es donde tu maplogic.cs o GUI llamará a SendCommand cuando el jugador haga una acción.
        
        // Ejemplo de envío de un comando de refuerzo:
        // var comandoReforzar = new MensajeJuego("REFORZAR", playerAlias);
        // comandoReforzar.Datos.Add("TerritorioDestino", "Alaska");
        // comandoReforzar.Datos.Add("Cantidad", 3);
        // await SendCommand(writer, comandoReforzar);
        
        // En una aplicación real, el bucle de juego terminaría cuando el juego termine
        // Por ahora, solo esperamos a que el listenerTask termine.
        await listenerTask; 
    }
    
    // Método para enviar cualquier comando JSON
    public static async Task SendCommand(StreamWriter writer, MensajeJuego comando)
    {
        string jsonToSend = JsonHelper.SerializarComando(comando);
        // Enviamos y añadimos el delimitador \n, que el servidor espera.
        await writer.WriteLineAsync(jsonToSend);
    }


    // Escucha constante del servidor
    private static async Task ListenForServerUpdates(StreamReader reader)
    {
        try
        {
            string? receivedJson;
            while ((receivedJson = await reader.ReadLineAsync()) != null)
            {
                var update = JsonHelper.DeserializarComando(receivedJson);
                if (update != null)
                {
                    // **LÓGICA CLAVE:** Aquí el cliente recibe el estado del juego
                    // y lo pasa a la UI (maplogic) para que se actualicen los colores y tropas.
                    Console.WriteLine($"[SERVER UPDATE] Tipo: {update.TipoComando} - Datos: {JsonHelper.SerializarComando(update)}");
                    
                    // Ejemplo: UIController.ApplyUpdate(update);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Conexión con el servidor perdida: {ex.Message}");
        }
    }
}