using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;

namespace RiskDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Manejador de clic en un territorio
        // Manejador de clic en un territorio
       private void Territorio_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Path territorio)
            {
                string nombre = territorio.Name;
                string dueno = "Jugador 1";   // aquí más adelante pondrías la lógica real
                int ejercitos = 5;            // valor fijo de ejemplo

                // Mostrar en el panel lateral
                LblNombre.Text = nombre;
                LblDueno.Text = dueno;
                LblEjercitos.Text = ejercitos.ToString();

                // Resaltar el territorio clicado
                ResetBorders();
                territorio.Stroke = Brushes.DarkBlue;
                territorio.StrokeThickness = 4;
            }
        }


        // Resetea el borde de todos los territorios
        private void ResetBorders()
        {
            Estados_Unidos.Stroke = Brushes.Black;
            Estados_Unidos.StrokeThickness = 2;
            Canada.Stroke = Brushes.Black;
            Canada.StrokeThickness = 2;
            Alaska.Stroke = Brushes.Black;
            Alaska.StrokeThickness = 2;
            Mexico.Stroke = Brushes.Black;
            Mexico.StrokeThickness = 2;
            Dinamarca.Stroke = Brushes.Black;
            Dinamarca.StrokeThickness = 2;
            Guatemala.Stroke = Brushes.Black;
            Guatemala.StrokeThickness = 2;
            Belice.Stroke = Brushes.Black;
            Belice.StrokeThickness = 2;
            El_Salvador.Stroke = Brushes.Black;
            El_Salvador.StrokeThickness = 2;
            Honduras.Stroke = Brushes.Black;
            Honduras.StrokeThickness = 2;
            Nicaragua.Stroke = Brushes.Black;
            Nicaragua.StrokeThickness = 2;
            Costa_Rica.Stroke = Brushes.Black;
            Costa_Rica.StrokeThickness = 2;
            Panama.Stroke = Brushes.Black;
            Panama.StrokeThickness = 2;
            Colombia.Stroke = Brushes.Black;
            Colombia.StrokeThickness = 2;
            Venezuela.Stroke = Brushes.Black;
            Venezuela.StrokeThickness = 2;
            Ecuador.Stroke = Brushes.Black;
            Ecuador.StrokeThickness = 2;
            Peru.Stroke = Brushes.Black;
            Peru.StrokeThickness = 2;
            Brasil.Stroke = Brushes.Black;
            Brasil.StrokeThickness = 2;
            Guyana_Francesa.Stroke = Brushes.Black;
            Guyana_Francesa.StrokeThickness = 2;
            Guyana.Stroke = Brushes.Black;
            Guyana.StrokeThickness = 2;
            Suriname.Stroke = Brushes.Black;
            Suriname.StrokeThickness = 2;
            Bolivia.Stroke = Brushes.Black;
            Bolivia.StrokeThickness = 2;
            Paraguay.Stroke = Brushes.Black;
            Paraguay.StrokeThickness = 2;
            Chile.Stroke = Brushes.Black;
            Chile.StrokeThickness = 2;
            Argentina.Stroke = Brushes.Black;
            Argentina.StrokeThickness = 2;
            Uruguay.Stroke = Brushes.Black;
            Uruguay.StrokeThickness = 2;
            Cuba.Stroke = Brushes.Black;
            Cuba.StrokeThickness = 2;
            Jamaica.Stroke = Brushes.Black;
            Jamaica.StrokeThickness = 2;
            Haiti.Stroke = Brushes.Black;
            Haiti.StrokeThickness = 2;
            Republica_Dominicana.Stroke = Brushes.Black;
            Republica_Dominicana.StrokeThickness = 2;
            Puerto_Rico.Stroke = Brushes.Black;
            Puerto_Rico.StrokeThickness = 2;
            Islandia.Stroke = Brushes.Black;
            Islandia.StrokeThickness = 2;
            Rusia.Stroke = Brushes.Black;
            Rusia.StrokeThickness = 2;
            China.Stroke = Brushes.Black;
            China.StrokeThickness = 2;
            Indonesia.Stroke = Brushes.Black;
            Indonesia.StrokeThickness = 2;
            Mongolia.Stroke = Brushes.Black;
            Mongolia.StrokeThickness = 2;
            India.Stroke = Brushes.Black;
            India.StrokeThickness = 2;
            Australia.Stroke = Brushes.Black;
            Australia.StrokeThickness = 2;
            Arabia_Saudita.Stroke = Brushes.Black;
            Arabia_Saudita.StrokeThickness = 2;
            Europa_1.Stroke = Brushes.Black;
            Europa_1.StrokeThickness = 2;
            Europa_2.Stroke = Brushes.Black;
            Europa_2.StrokeThickness = 2;
            Turquia.Stroke = Brushes.Black;
            Turquia.StrokeThickness = 2;
            Asia_1.Stroke = Brushes.Black;
            Asia_1.StrokeThickness = 2;
            Africa_1.Stroke = Brushes.Black;
            Africa_1.StrokeThickness = 2;
            Africa_2.Stroke = Brushes.Black;
            Africa_2.StrokeThickness = 2;

        }
    }
}
