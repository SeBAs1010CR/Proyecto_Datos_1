using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using CrazyRisk.ViewModels;
using CrazyRisk.Models;
//hola
namespace CrazyRisk
{
    public partial class MainWindow : Window
    {
        private GameController game;

        public MainWindow()
        {
            InitializeComponent();
            game = new GameController();
        }

        private void Territorio_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Path territorioPath)
            {
                string nombre = territorioPath.Name.Replace("_", " ");
                Territorio? territorio = game.GetTerritorio(nombre);

                if (territorio != null)
                {
                    // Mostrar datos reales
                    LblNombre.Text = territorio.Nombre;
                    LblDueno.Text = territorio.Dueño?.Alias ?? "Neutral";
                    LblEjercitos.Text = territorio.Tropas.ToString();

                    // Resaltar visualmente
                    ResetBorders();
                    territorioPath.Stroke = Brushes.DarkBlue;
                    territorioPath.StrokeThickness = 4;
                }
            }
        }

        private void ResetBorders()
        {
            foreach (var child in MapCanvas.Children)
            {
                if (child is Path path)
                {
                    path.Stroke = Brushes.Black;
                    path.StrokeThickness = 2;
                }
            }
        }
    }
}
