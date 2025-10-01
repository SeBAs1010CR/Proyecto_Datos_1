namespace CrazyRisk.Models
{
    public class Jugador
    {
        public string Alias { get; set; }
        public string Color { get; set; }
        public int TropasDisponibles { get; set; }
        public Lista<Territorio> Territorios { get; set; }
        public Lista<Tarjeta> Tarjetas { get; set; }
        //Sebas v
        public bool DebeIntercambiarTarjetas { get; set; } = false; // Nuevo: Estado de bloqueo
        //sebas ^

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

        public void AgregarTerritorio(Territorio t) => Territorios.Agregar(t);

        public Territorio? BuscarTerritorio(string nombre)
        {
            Nodo<Territorio>? actual = Territorios.ObtenerCabeza();
            while (actual != null)
            {
                if (actual.Valor.Nombre == nombre)
                    return actual.Valor;
                actual = actual.Siguiente;
            }
            return null;
        }
    }
}


//     public class GameManager
//     {
//         public Mapa Mapa { get; private set; }
//         public Jugador Jugador1 { get; private set; }
//         public Jugador Jugador2 { get; private set; }
//         public Jugador Actual { get; private set; }

  
//         private int turno;

//         public GameManager(string alias1, string alias2)
//         {
//             Mapa = new Mapa();
//             Jugador1 = new Jugador(alias1, "Azul");
//             Jugador2 = new Jugador(alias2, "Rojo");
//             Actual = Jugador1;
//             turno = 1;
//             contadorIntercambios = 0;
//         }

//         public void CambiarTurno()
//         {
//             Actual = (Actual == Jugador1) ? Jugador2 : Jugador1;
//             turno++;
//         }

//         public int CalcularRefuerzos(Jugador j)
//         {
//             int territorios = ContarTerritorios(j);
//             int refuerzos = Math.Max(territorios / 3, 3);

//             // Bonus de continentes
//             refuerzos += CalcularBonusContinente(j);

//             return refuerzos;
//         }

//         private int ContarTerritorios(Jugador j)
//         {
//             int count = 0;
//             var actual = Mapa.Territorios.ObtenerCabeza();
//             while (actual != null)
//             {
//                 if (actual.Valor.Dueño == j)
//                     count++;
//                 actual = actual.Siguiente;
//             }
//             return count;
//         }

//         private int CalcularBonusContinente(Jugador j)
//         {
//             // Aquí defines los continentes y verificas si j controla todos los territorios del continente
//             // Por ahora devuelvo 0 como placeholder
//             return 0;
//         }

//         public bool Atacar(Territorio origen, Territorio destino, int tropasAtacantes)
//         {
//             if (origen.Dueño != Actual) return false;
//             if (origen.Tropas <= 1) return false;
//             if (!EsAdyacente(origen, destino)) return false;

//             // Simulación de dados
//             var random = new Random();
//             var dadosAtacante = Enumerable.Range(0, Math.Min(3, tropasAtacantes))
//                                           .Select(_ => random.Next(1, 7))
//                                           .OrderByDescending(x => x).ToList();

//             var dadosDefensor = Enumerable.Range(0, Math.Min(2, destino.Tropas))
//                                           .Select(_ => random.Next(1, 7))
//                                           .OrderByDescending(x => x).ToList();

//             int comparaciones = Math.Min(dadosAtacante.Count, dadosDefensor.Count);
//             for (int i = 0; i < comparaciones; i++)
//             {
//                 if (dadosAtacante[i] > dadosDefensor[i])
//                     destino.Tropas--;
//                 else
//                     origen.Tropas--;
//             }

//             // Conquista
//             if (destino.Tropas <= 0)
//             {
//                 destino.Dueño = origen.Dueño;
//                 destino.Tropas = tropasAtacantes; // mover tropas mínimas
//                 origen.Tropas -= tropasAtacantes;
//             }

//             return true;
//         }

//         public bool EsAdyacente(Territorio a, Territorio b)
//         {
//             var actual = a.Adyacentes.ObtenerCabeza();
//             while (actual != null)
//             {
//                 if (actual.Valor == b)
//                     return true;
//                 actual = actual.Siguiente;
//             }
//             return false;
//         }

//         public bool HaGanado(Jugador j)
//         {
//             var actual = Mapa.Territorios.ObtenerCabeza();
//             while (actual != null)
//             {
//                 if (actual.Valor.Dueño != j)
//                     return false;
//                 actual = actual.Siguiente;
//             }
//             return true;
//         }
//     }
// }


