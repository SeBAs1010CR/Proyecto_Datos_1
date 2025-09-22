public class Jugador
{
    public string Alias { get; set; }
    public string Color { get; set; }
    public int TropasDisponibles { get; set; }
    public Lista<Territorio> Territorios { get; set; }
    public Lista<Tarjeta> Tarjetas { get; set; }

    public Jugador(string alias, string color)
    {
        Alias = alias;
        Color = color;
        TropasDisponibles = 40;
        Territorios = new Lista<Territorio>();
        Tarjetas = new Lista<Tarjeta>();
    }
}

public class Territorio
{
    public string Nombre { get; set; }
    public Jugador? Dueño { get; set; }
    public int Tropas { get; set; }
    public Lista<Territorio> Adyacentes { get; set; }

    public Territorio(string nombre)
    {
        Nombre = nombre;
        Tropas = 1;
        Adyacentes = new Lista<Territorio>();
    }
}

public class Tarjeta
{
    public string Tipo { get; set; } // "Infantería", "Caballería", "Artillería"
    public bool Usada { get; set; }

    public Tarjeta(string tipo)
    {
        Tipo = tipo;
        Usada = false;
    }
}

public class Mapa
{
    public Lista<Territorio> Territorios { get; set; }

    public Mapa()
    {
        Territorios = new Lista<Territorio>();
    }

    // Ejemplo: agregar territorios al mapa
    public void AgregarTerritorio(Territorio t)
    {
        Territorios.Agregar(t);
    }
}
