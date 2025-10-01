using System.Windows;
using System.Windows.Controls;

namespace CrazyRisk.Pages
{
    public partial class MainMenu : Page
    {
        private Frame mainFrame;

        public MainMenu(Frame frame)
        {
            InitializeComponent();
            mainFrame = frame;
        }

        private void btnCrear_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new SetupPage(mainFrame, true));
        }

        private void btnUnirse_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new SetupPage(mainFrame, false));
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
