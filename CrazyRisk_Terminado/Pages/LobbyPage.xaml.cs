using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CrazyRisk.Models;
using CrazyRisk.Comms;
using CrazyRisk.Pages;
using CrazyRisk.ViewModels;

namespace CrazyRisk.Pages
{
    public partial class LobbyPage : Page
    {
        private Frame mainFrame;
        private bool esServidor;
        private string alias;
        private string ip;
        private int tropas;
        private GameClient? gameClient;

        public LobbyPage(Frame frame, bool servidor, string aliasJugador, string ipServidor, int tropasIniciales)
        {
            InitializeComponent();
            mainFrame = frame;
            esServidor = servidor;
            alias = aliasJugador;
            ip = ipServidor;
            tropas = tropasIniciales;

            // Mostrar jugador actual
            lstJugadores.Items.Add($"{alias} (Tropas: {tropas})");

            if (esServidor)
                btnIniciar.Visibility = Visibility.Visible;

            // --- NO volver a iniciar el servidor ---
            // El servidor ya fue iniciado en SetupPage (Server.EnsureMainServerStarted())

            // --- Conectar como cliente ---
            gameClient = new GameClient(ip, alias);
            gameClient.UpdateReceived += (update) =>
            {
                Console.WriteLine($"[CLIENT-EVENT] Update recibido: {update.TipoComando}");
            };

            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"[CLIENT] Intentando conectar a {ip}:1234");
                    await gameClient.StartClientMode();
                    Console.WriteLine("[CLIENT] Conectado correctamente al servidor principal");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UI] Error iniciando cliente: {ex.Message}");
                    MessageBox.Show(
                        $"No se pudo conectar al servidor: {ex.Message}",
                        "Error de conexión",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            });
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            if (gameClient != null)
                mainFrame.Navigate(new MapPage(mainFrame, gameClient));
            else
                mainFrame.Navigate(new MapPage());
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new SetupPage(mainFrame, esServidor));
        }
    }
}
