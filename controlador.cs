using CrazyRisk.Models;

namespace CrazyRisk.ViewModels
{
    public class GameController
    {
        public Mapa Mapa { get; private set; }
        public Jugador Jugador1 { get; private set; }
        public Jugador Jugador2 { get; private set; }

        public GameController()
        {
            Mapa = new Mapa();
            Jugador1 = new Jugador("Player 1", "Azul");
            Jugador2 = new Jugador("Player 2", "Rojo");

            InicializarMapa();
        }

        private void InicializarMapa()
        {
            // Ejemplo básico (luego debes expandir a los 42 territorios)
            var usa = new Territorio("Estados Unidos") { Dueño = Jugador1, Tropas = 5 };
            var canada = new Territorio("Canada") { Dueño = Jugador2, Tropas = 3 };
            var mexico = new Territorio("Mexico") { Dueño = Jugador1, Tropas = 2 };
            Mapa.AgregarTerritorio(usa);
            Mapa.AgregarTerritorio(canada);
        }

        public Territorio? GetTerritorio(string nombre) =>
            Mapa.BuscarTerritorio(nombre);
    }
}
