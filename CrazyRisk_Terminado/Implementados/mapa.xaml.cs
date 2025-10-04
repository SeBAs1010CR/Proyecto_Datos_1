using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace RiskDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ResetBorders()
        {
            if (this.FindName("GameMap") is Canvas canvas)
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
                ResetBorders();
                path.Stroke = Brushes.Red;
                path.StrokeThickness = 4;
                try
                {
                    string territoryName = path.Name;
                    Console.WriteLine($"[Map] Territorio clickeado: {territoryName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en Territorio_Click (mapa.xaml.cs): {ex.Message}");
                }
            }
        }
    }
}
