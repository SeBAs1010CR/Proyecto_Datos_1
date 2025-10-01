using System.Windows;
using System.Windows.Controls;

namespace CrazyRisk.Pages
{
    public partial class SetupPage : Page
    {
        private Frame mainFrame;
        private bool esServidor;

        public SetupPage(Frame frame, bool servidor)
        {
            InitializeComponent();
            mainFrame = frame;
            esServidor = servidor;

            lblModo.Text = esServidor ? "Crear Partida" : "Unirse a Partida";
            txtIP.Visibility = esServidor ? Visibility.Collapsed : Visibility.Visible;
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            string alias = txtAlias.Text;
            string ip = txtIP.Text;
            int tropas = int.TryParse(txtTropas.Text, out int t) ? t : 40;

            mainFrame.Navigate(new LobbyPage(mainFrame, esServidor, alias, ip, tropas));
        }

        private void btnRegresar_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new MainMenu(mainFrame));
        }
    }
}
