#nullable enable
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using CrazyRisk.ViewModels;
using CrazyRisk.Models;

namespace CrazyRisk
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Navegación inicial al MainMenu (agregada automáticamente para resolver duplicado)
            MainFrame.Navigate(new Pages.MainMenu(MainFrame));
            // Arranca el servidor en background (solo una vez)
            Server.EnsureMainServerStarted();

            // El controlador de juego puede inicializarse aquí o inyectarse desde fuera.
            // game = new GameController();
        }

        private void ResetBorders()
        {
            // Intenta obtener el canvas llamado "GameMap" de la XAML y resetea los Path
            if (this.FindName("GameMap") is System.Windows.Controls.Canvas canvas)
            {
                foreach (var child in canvas.Children)
                {
                    if (child is Path path)
                    {
                        path.Stroke = Brushes.Black;
                        path.StrokeThickness = 2;
                    }
                }
            }
        }

        private void Territorio_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Path path)
            {
                // Visual feedback: resetea todos los bordes y marca el seleccionado
                ResetBorders();
                path.Stroke = Brushes.Red;
                path.StrokeThickness = 4;

                // Lógica mínima: notificar (si existe) al controlador de juego
                try
                {
                    string territoryName = path.Name;
                    // TODO: Integrar con GameController para manejar selección del territorio
                    // game?.HandleTerritorioClick(territoryName);

                    Console.WriteLine($"Territorio clickeado: {territoryName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en Territorio_Click: {ex.Message}");
                }
            }
        }
    }
}
