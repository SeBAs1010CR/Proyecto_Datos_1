using System.Windows;
using System.Windows.Controls;

namespace CrazyRisk.Pages
{
    public partial class LobbyPage : Page
    {
        private Frame mainFrame;
        private bool esServidor;
        private string alias;
        private string ip;
        private int tropas;

        public LobbyPage(Frame frame, bool servidor, string aliasJugador, string ipServidor, int tropasIniciales)
        {
            InitializeComponent();
            mainFrame = frame;
            esServidor = servidor;
            alias = aliasJugador;
            ip = ipServidor;
            tropas = tropasIniciales;

            lstJugadores.Items.Add($"{alias} (Tropas: {tropas})");

            if (esServidor)
                btnIniciar.Visibility = Visibility.Visible;
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("¡Inicia el juego con el mapa!");
            // Aquí iría la navegación a la pantalla del mapa
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new SetupPage(mainFrame, esServidor));
        }
    }
}
