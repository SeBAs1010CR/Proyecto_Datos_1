using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CrazyRisk.Models;
using CrazyRisk.Comms;
using CrazyRisk.Pages; // para navegar a otras Pages (MapPage, SetupPage)
using CrazyRisk.ViewModels; // si necesitas acceso a Server (según estructura actual)

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

            // Mostrar jugador actual en la lista
            lstJugadores.Items.Add($"{alias} (Tropas: {tropas})");

            if (esServidor)
                btnIniciar.Visibility = Visibility.Visible;

            // --- Iniciar Servidor si este jugador lo creó ---
            if (esServidor)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine("[UI] Iniciando servidor local...");
                        await Server.Main(new string[] { });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UI] Error iniciando servidor local: {ex.Message}");
                    }
                });
            }

            // --- Conectar como cliente ---
            gameClient = new GameClient(ip, alias);
            gameClient.UpdateReceived += (update) =>
            {
                // Aquí podrías actualizar lista o mostrar eventos
                Console.WriteLine($"[CLIENT-EVENT] Update recibido: {update.TipoComando}");
            };

            _ = Task.Run(async () =>
            {
                try
                {
                    await gameClient.StartClientMode();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UI] Error iniciando cliente: {ex.Message}");
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
