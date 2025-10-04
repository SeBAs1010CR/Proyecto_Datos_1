using System.Configuration;
using System.Data;
using System.Windows;
using CrazyRisk.Comms;
using System.Threading.Tasks;


namespace CrazyRisk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Iniciar servidor en background
            _ = Task.Run(() => Server.StartServersAsync());

        
        }
    }
}
