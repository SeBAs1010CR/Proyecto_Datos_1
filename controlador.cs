using CrazyRisk.Models;

namespace CrazyRisk.ViewModels
{
    public class GameController
    {
        public Mapa Mapa { get; private set; }
        public Jugador Jugador1 { get; private set; }
        public Jugador Jugador2 { get; private set; }
        // Contador para seguir la secuencia de refuerzo: 1er intercambio = 2, 2do = 3, etc.
        private int ContadorGlobalIntercambios = 1; 

        public Jugador Actual { get; private set; } = null!;

  
        private int turno;
        //Sebas
        // Método que la UI llamará para obtener el valor del refuerzo
        public int ObtenerValorRefuerzoPorIntercambio()
        {
            // 1. Obtener el valor de la serie basado en el contador actual.
            // Asumimos que LogicaJuego.NthFibonacci está implementado para dar el valor correcto.
            int valorRefuerzo = LogicaJuego.NthFibonacci(ContadorGlobalIntercambios);

            return valorRefuerzo;
        }

        // Método que se llama SOLO DESPUÉS de un intercambio válido
        private void IncrementarContadorIntercambio()
        {
            ContadorGlobalIntercambios++;
        }
        public GameController()
        {
            Mapa = new Mapa();
            Jugador1 = new Jugador("Player 1", "Azul");
            Jugador2 = new Jugador("Player 2", "Rojo");

            InicializarMapa();
        }
        //Sebas
        public void CambiarTurno()
        {
            Actual = (Actual == Jugador1) ? Jugador2 : Jugador1;
            turno++;
        }

        public int CalcularRefuerzos(Jugador j)
        {
            int territorios = ContarTerritorios(j);
            int refuerzos = Math.Max(territorios / 3, 3);

            // Bonus de continentes
            refuerzos += CalcularBonusContinente(j);

            return refuerzos;
        }

        private int ContarTerritorios(Jugador j)
        {
            int count = 0;
            var actual = Mapa.Territorios.ObtenerCabeza();
            while (actual != null)
            {
                if (actual.Valor.Dueño == j)
                    count++;
                actual = actual.Siguiente;
            }
            return count;
        }

        private int CalcularBonusContinente(Jugador j)
        {
            // Aquí defines los continentes y verificas si j controla todos los territorios del continente
            // Por ahora devuelvo 0 como placeholder
            return 0;
        }



        public bool EsAdyacente(Territorio a, Territorio b)
        {
            var actual = a.Adyacentes.ObtenerCabeza();
            while (actual != null)
            {
                if (actual.Valor == b)
                    return true;
                actual = actual.Siguiente;
            }
            return false;
        }

        public bool HaGanado(Jugador j)
        {
            var actual = Mapa.Territorios.ObtenerCabeza();
            while (actual != null)
            {
                if (actual.Valor.Dueño != j)
                    return false;
                actual = actual.Siguiente;
            }
            return true;
        }
    //uuh

        private void InicializarMapa()
        {
            // Ejemplo básico (luego debes expandir a los 42 territorios)
            var usa = new Territorio("Estados Unidos") { Dueño = Jugador1, Tropas = 5 };
            var canada = new Territorio("Canada") { Dueño = Jugador2, Tropas = 3 };
            var mexico = new Territorio("Mexico") { Dueño = Jugador1, Tropas = 2 };
            Mapa.AgregarTerritorio(usa);
            Mapa.AgregarTerritorio(canada);
        }
        // Mazo global de donde se sacan las tarjetas.
        private Lista<Tarjeta> MazoDeTarjetas { get; set; } = new Lista<Tarjeta>();
        



        //sebas
        private void EliminarTrioDeLaMano(Jugador jugador, Lista<Tarjeta> trio)
        {
            // Usamos la propiedad ObtenerCabeza() de la lista trio para iterar sobre los nodos.
            var nodoTrio = trio.ObtenerCabeza();
            while (nodoTrio != null)
            {
                // Llamamos al método Remover implementado en Lista<T>
                jugador.Tarjetas.Remover(nodoTrio.Valor);
                
                // El requisito original de RISK es devolver las tarjetas al mazo,
                // pero dado el requisito de 'tarjetas por territorio' y la mecánica de Fibonacci,
                // simplemente las removemos. Si quieres devolverlas, agrégalas al MazoDeTarjetas aquí.
                
                nodoTrio = nodoTrio.Siguiente;
            }
        }
        public bool EsTrioValido(Lista<Tarjeta> trio)
        {
            if (trio.ObtenerTamaño() != 3)
            {
                return false; // No son 3 tarjetas. (Asume que ObtenerTamaño() está implementado)
            }

            // Convertir la Lista<Tarjeta> a un arreglo o lista temporal de valores 
            // para facilitar el conteo sin manipular nodos (ya que no se usa List<T> nativa).
            string[] tipos = new string[3];
            var actual = trio.ObtenerCabeza();
            int i = 0;
            while (actual != null)
            {
                tipos[i++] = actual.Valor.Tipo;
                actual = actual.Siguiente;
            }

            // Conteo de Tipos
            int infanteria = 0;
            int caballeria = 0;
            int artilleria = 0;

            foreach (string tipo in tipos)
            {
                if (tipo == "Infantería") infanteria++;
                else if (tipo == "Caballería") caballeria++;
                else if (tipo == "Artillería") artilleria++;
            }

            // Validación de Trío:
            // 1. Tres Iguales: 3 de Infantería, 3 de Caballería o 3 de Artillería.
            bool tresIguales = (infanteria == 3 || caballeria == 3 || artilleria == 3);

            // 2. Uno de Cada Tipo: 1 Infantería, 1 Caballería, 1 Artillería.
            bool unoDeCada = (infanteria == 1 && caballeria == 1 && artilleria == 1);

            return tresIguales || unoDeCada;
        }
        private void InicializarTarjetas()
        {
            // 1. Crear las 42 tarjetas (ejemplo: 14 Infantería, 14 Caballería, 14 Artillería)
            for (int i = 0; i < 14; i++) MazoDeTarjetas.Agregar(new Tarjeta("Infantería"));
            for (int i = 0; i < 14; i++) MazoDeTarjetas.Agregar(new Tarjeta("Caballería"));
            for (int i = 0; i < 14; i++) MazoDeTarjetas.Agregar(new Tarjeta("Artillería"));

            // 2. Barajar la lista (necesitas un método de barajado en tu Lista o mover los Nodos de forma pseudo-aleatoria)
            MazoDeTarjetas.Aleatorio();  
        }
        // Fragmento de la Lógica de Ataque en GameController (o una nueva función)
        public void AsignarTarjetaAlJugador(Jugador jugador)
        {
            if (!MazoDeTarjetas.EstaVacia())
            {
                // 1. Sacar la tarjeta del mazo y asignarla
                Tarjeta tarjetaRobada = MazoDeTarjetas.SacarDelFrente();
                jugador.Tarjetas.Agregar(tarjetaRobada);

                // 2. Verificar la condición de límite (si el tamaño de la mano es 6 o más)
                // [Recordatorio: Debes asegurar que ObtenerTamaño() está en tu clase Lista<T>]
                if (jugador.Tarjetas.ObtenerTamaño() > 5)
                {
                    // Activar el estado de bloqueo para el jugador actual.
                    jugador.DebeIntercambiarTarjetas = true;

                    // Lógica de Comms: Notificar a ambos clientes que este jugador debe intercambiar.
                }
            }
        }
        public int IntercambiarTarjetas(Jugador jugador, Lista<Tarjeta> trio)
        {
            if (EsTrioValido(trio))
            {
                // 1. Obtener el valor del refuerzo para ESTE intercambio
                int refuerzo = ObtenerValorRefuerzoPorIntercambio(); 

                // 2. Aplicar el cambio de estado global: ¡Muy importante!
                IncrementarContadorIntercambio(); 

                // 3. Eliminar el trío de la mano del jugador
                EliminarTrioDeLaMano(jugador, trio);

                // 4. Limpiar el estado de bloqueo si estaba activo
                if (jugador.DebeIntercambiarTarjetas)
                {
                    // Asumimos que un solo intercambio de trío es suficiente para cumplir el requisito.
                    jugador.DebeIntercambiarTarjetas = false; 
                }
                // 5. Asignar las tropas de refuerzo al jugador.
                jugador.TropasDisponibles += refuerzo;

                // 6. Devolver la cantidad de tropas
                return refuerzo;
            }
            return 0;
        }
        private static readonly Random random = new Random();

        /// <summary>
        /// Simula el lanzamiento y comparación de dados para un único enfrentamiento.
        /// </summary>
        /// <param name="dadosAtacante">Cantidad de dados a lanzar por el atacante (1-3).</param>
        /// <param name="dadosDefensor">Cantidad de dados a lanzar por el defensor (1-2).</param>
        /// <returns>Tupla con (pérdidas del atacante, pérdidas del defensor).</returns>
        private (int perdidasAtk, int perdidasDef) ResolverCombate(int dadosAtacante, int dadosDefensor)
        {
            // 1. Lanzar Dados (usando Arrays/Arreglos)
            int[] atk = new int[dadosAtacante];
            int[] def = new int[dadosDefensor];

            for (int i = 0; i < dadosAtacante; i++)
                atk[i] = random.Next(1, 7); // Generar número aleatorio (1-6)

            for (int i = 0; i < dadosDefensor; i++)
                def[i] = random.Next(1, 7);

            // 2. Ordenar de Mayor a Menor (Descendente)
            // Se usa Array.Sort para el ordenamiento interno del array (rápido y simple)
            Array.Sort(atk, (a, b) => b.CompareTo(a)); 
            Array.Sort(def, (a, b) => b.CompareTo(a));

            int perdidasAtk = 0;
            int perdidasDef = 0;
            int comparaciones = Math.Min(dadosAtacante, dadosDefensor);

            // 3. Comparar en Pares
            for (int i = 0; i < comparaciones; i++)
            {
                // El mayor de Atk (atk[0]) vs. el mayor de Def (def[0]), etc.
                if (atk[i] > def[i])
                {
                    perdidasDef++; // Atacante gana el duelo de dados
                }
                else
                {
                    perdidasAtk++; // Defensor gana o empata (Defensor gana en caso de empate)
                }
            }

            return (perdidasAtk, perdidasDef);
        }
        /// <summary>
        /// Maneja la secuencia completa de ataque, desde la validación hasta la conquista.
        /// </summary>
        /// <param name="origen">Territorio desde donde se ataca.</param>
        /// <param name="destino">Territorio atacado.</param>
        /// <param name="dadosAtacante">Número de dados del atacante (1-3).</param>
        /// <param name="tropasAUsar">Número total de tropas a comprometer en el ataque (incluye las de los dados).</param>

        // Maes esta es la funcion de ataque 
        public (bool conquista, int bajasAtk, int bajasDef, bool victoria) ProcesarAtaque(
            Territorio origen,
            Territorio destino,
            int dadosAtacante,
            int tropasAMover // nuevo parámetro: tropas que el jugador quiere mover al conquistar
        )
        {
            //Validaciones
            // Verifica que sea el turno del jugador que ataca
            if (Actual != origen.Dueño) return (false, 0, 0, false); // No es su turno
            
            // Verifica que no se ataque a un territorio propio y que haya al menos 2 tropas
            if (origen.Dueño == destino.Dueño || origen.Tropas < 2) return (false, 0, 0, false);

            // Verifica que los territorios sean adyacentes
            if (!EsAdyacente(origen, destino)) return (false, 0, 0, false);

            // Limitar dados y tropas
            dadosAtacante = Math.Min(dadosAtacante, origen.Tropas - 1);
            int dadosDefensor = Math.Min(destino.Tropas, 2);

            // COMBATE 
            //Simula el combate y obtiene las pérdidas de ambos jugadores
            var (perdidasAtk, perdidasDef) = ResolverCombate(dadosAtacante, dadosDefensor);
            origen.Tropas -= perdidasAtk;// Aplica las pérdidas
            destino.Tropas -= perdidasDef;// Aplica las pérdidas

            bool conquistado = false;
            bool victoria = false;

            if (destino.Tropas <= 0)
            {
                conquistado = true;
                destino.Dueño = origen.Dueño; // Cambia el dueño del territorio conquistado

                // Validar tropas a mover
                int minimoAMover = dadosAtacante; // Calcula el mínimo de tropas que se deben mover (igual a los dados usados)
                tropasAMover = Math.Max(tropasAMover, minimoAMover);
                tropasAMover = Math.Min(tropasAMover, origen.Tropas - 1); // Siempre debe quedar 1

                destino.Tropas = tropasAMover; // Mueve las tropas al territorio conquistado
                origen.Tropas -= tropasAMover;

                // Asignar tarjeta
                AsignarTarjetaAlJugador(origen.Dueño); // Asigna una tarjeta al jugador por conquistar territorio

                // Verificar victoria
                if (HaGanado(origen.Dueño))
                {
                    victoria = true;
                    // Aquí podriamos notificar a los jugadores que el juego terminó
                }

                // Notificación poara la vara del OTP
                // EnviarMensajeAClientes(new {
                //     tipo = "conquista",
                //     territorio = destino.Nombre,
                //     nuevoDueno = origen.Dueño.Alias,
                //     tropas = destino.Tropas
                // });
            }
            // Devuelve los resultados del ataque, util para la UI q hizo ALI
            return (conquistado, perdidasAtk, perdidasDef, victoria);
        }

        public Territorio? GetTerritorio(string nombre) =>
            Mapa.BuscarTerritorio(nombre);
    }
}
