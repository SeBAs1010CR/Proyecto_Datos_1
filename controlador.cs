
using CrazyRisk.Models;


namespace CrazyRisk.ViewModels
{
    public class GameController
    {
        public Mapa Mapa { get; private set; }
        public Jugador Jugador1 { get; private set; }
        public Jugador Jugador2 { get; private set; }
        
        public Jugador Neutro { get; private set; } //jugador neutro que hice para los territorios, atte dilan
        public EtapaTurno EtapaActual { get; private set; } = EtapaTurno.Refuerzo;
        private int ContadorGlobalIntercambios = 1;
        // Contador para seguir la secuencia de refuerzo: 1er intercambio = 2, 2do = 3, etc.

        public Jugador Actual { get; private set; } = null!;


        private int turno;
        //Sebas
        // Método que la UI llamará para obtener el valor del refuerzo
        //En la UI permitir que los jugadores coloquen una tropa por turno en un territorio propio hasta que TropasDisponibles == 0.
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
            Neutro = new Jugador("Neutro", "Gris");

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

        // Calcula el bonus de refuerzos que obtiene un jugador por controlar continentes completos.

        private int CalcularBonusContinente(Jugador j)
        {
            // Definimos un diccionario donde cada continente tiene:
            // - Un valor de bonus (tropas extra)
            // - Un arreglo con los nombres de los territorios que lo componen
            var continentes = new Dictionary<string, (int bonus, string[] territorios)>
            {
                { "América", (5, new[] {
                    "Estados_Unidos", "Canada", "Alaska", "Mexico", "Guatemala", "Belice", "El_Salvador", "Honduras",
                    "Nicaragua", "Costa_Rica", "Panama", "Colombia", "Venezuela", "Ecuador", "Peru", "Brasil",
                    "Guyana_Francesa", "Guyana", "Suriname", "Bolivia", "Paraguay", "Chile", "Argentina", "Uruguay",
                    "Cuba", "Jamaica", "Haiti", "Republica_Dominicana", "Puerto_Rico"
                }) },
                { "Europa", (8, new[] {
                    "Dinamarca", "Islandia", "Italia", "Alemania", "Turquia"
                }) },
                { "Asia", (7, new[] {
                    "Rusia", "China", "Indonesia", "Mongolia", "India", "Arabia_Saudita", "Nepal"
                }) },
                { "Oceanía", (2, new[] {
                    "Australia"
                }) },
                { "África", (3, new[] {
                    "Nigeria", "Sudafrica"
                }) }
            };

            int bonus = 0; // Acumulador para el total de tropas extra

            // Recorremos cada continente para verificar si el jugador controla todos sus territorios
            foreach (var continente in continentes)
            {
                bool controlaTodo = true; // Asumimos que sí controla todo hasta que se demuestre lo contrario

                // Recorremos los territorios del continente actual
                foreach (var nombreTerritorio in continente.Value.territorios)
                {
                    var territorio = GetTerritorio(nombreTerritorio); // Buscamos el territorio en el mapa
                    // Si el territorio no existe o no pertenece al jugador, no controla el continente
                    if (territorio == null || territorio.Dueño != j)
                    {
                        controlaTodo = false;
                        break; // Salimos del bucle porque ya sabemos que no controla todo
                    }
                }

                // Si controla todos los territorios del continente, sumamos el bonus correspondiente
                if (controlaTodo)
                    bonus += continente.Value.bonus;
            }

            return bonus; // Devolvemos el total de tropas extra por continentes controlados
        }


        //funcion que verifica si es adyacente
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
            //en caso de cambiar condicion de gane a tomar un continente
            //private bool HaGanado(Jugador j)
            //{
            //return ControlaContinente(j, "Asia"); // Nueva condición
            //}       
        }

        public void PrepararRefuerzosIniciales()
        //Dilan
        {
            int tropasIniciales = 40;
            int territoriosJ1 = ContarTerritorios(Jugador1);
            int territoriosJ2 = ContarTerritorios(Jugador2);

            Jugador1.TropasDisponibles = tropasIniciales - territoriosJ1;
            Jugador2.TropasDisponibles = tropasIniciales - territoriosJ2;

            // El jugador neutro no coloca tropas manualmente, se puede distribuir aleatoriamente
        }
        //Necesito que en 
        private void InicializarMapa()
        //DILAN
        {
            //  lista de 42 territorios con sus continentes
            var territorios = new Lista<Territorio>
            {
                new Territorio("Estados_Unidos", "America"),
                new Territorio("Canada", "America"),
                new Territorio("Alaska", "America"),
                new Territorio("Mexico", "America"),
                new Territorio("Dinamarca", "Europa"),
                new Territorio("Guatemala", "America"),
                new Territorio("Belice", "America"),
                new Territorio("El_Salvador", "America"),
                new Territorio("Honduras", "America"),
                new Territorio("Nicaragua", "America"),
                new Territorio("Costa_Rica", "America"),
                new Territorio("Panama", "America"),
                new Territorio("Colombia", "America"),
                new Territorio("Venezuela", "America"),
                new Territorio("Ecuador", "America"),
                new Territorio("Peru", "America"),
                new Territorio("Brasil", "America"),
                new Territorio("Guyana_Francesa", "America"),
                new Territorio("Guyana", "America"),
                new Territorio("Suriname", "America"),
                new Territorio("Bolivia", "America"),
                new Territorio("Paraguay", "America"),
                new Territorio("Chile", "America"),
                new Territorio("Argentina", "America"),
                new Territorio("Uruguay", "America"),
                new Territorio("Cuba", "America"),
                new Territorio("Jamaica", "America"),
                new Territorio("Haiti", "America"),
                new Territorio("Republica_Dominicana", "America"),
                new Territorio("Puerto_Rico", "America"),
                new Territorio("Islandia", "Europa"),
                new Territorio("Rusia", "Asia"),
                new Territorio("China", "Asia"),
                new Territorio("Indonesia", "Asia"),
                new Territorio("Mongolia", "Asia"),
                new Territorio("India", "Asia"),
                new Territorio("Australia", "Oceania"),
                new Territorio("Arabia_Saudita", "Asia"),
                new Territorio("Italia", "Europa"),
                new Territorio("Alemania", "Europa"),
                new Territorio("Turquia", "Europa"),
                new Territorio("Nepal", "Asia"),
                new Territorio("Nigeria", "Africa"),
                new Territorio("Sudafrica", "Africa"),
                new Territorio("TEC", "Especial")
            };
            // 2. Mezclar aleatoriamente
            territorios.Aleatorio();

            // 3. Repartir entre Jugador1, Jugador2 y Neutro
            int index = 0;
            foreach (var territorio in territorios)
            {
                if (index % 3 == 0)
                    territorio.Dueño = Jugador1;
                else if (index % 3 == 1)
                    territorio.Dueño = Jugador2;
                else
                    territorio.Dueño = Neutro;

                
                territorio.Tropas = 1; // Tropas iniciales
                Mapa.AgregarTerritorio(territorio);
                index++;
            }
            // Crew dicionario para acceder rapido, atte, dilan
            var mapaDict = territorios.ToDictionary(t => t.Nombre);

            //Norte amaerica
            mapaDict["Alaska"].Adyacentes.Agregar(mapaDict["Canada"]);
            mapaDict["Canada"].Adyacentes.Agregar(mapaDict["Estados_Unidos"]);
            mapaDict["Canada"].Adyacentes.Agregar(mapaDict["Alaska"]);
            mapaDict["Estados_Unidos"].Adyacentes.Agregar(mapaDict["Mexico"]);

            // América Central
            mapaDict["Mexico"].Adyacentes.Agregar(mapaDict["Estados_Unidos"]);
            mapaDict["Estados_Unidos"].Adyacentes.Agregar(mapaDict["Mexico"]);

            mapaDict["Mexico"].Adyacentes.Agregar(mapaDict["Guatemala"]);
            mapaDict["Guatemala"].Adyacentes.Agregar(mapaDict["Mexico"]);
            mapaDict["Guatemala"].Adyacentes.Agregar(mapaDict["Belice"]);
            mapaDict["Belice"].Adyacentes.Agregar(mapaDict["Guatemala"]);
            mapaDict["Guatemala"].Adyacentes.Agregar(mapaDict["El_Salvador"]);
            mapaDict["El_Salvador"].Adyacentes.Agregar(mapaDict["Guatemala"]);

            mapaDict["El_Salvador"].Adyacentes.Agregar(mapaDict["Honduras"]);
            mapaDict["Honduras"].Adyacentes.Agregar(mapaDict["El_Salvador"]);

            mapaDict["Honduras"].Adyacentes.Agregar(mapaDict["Nicaragua"]);
            mapaDict["Nicaragua"].Adyacentes.Agregar(mapaDict["Honduras"]);

            mapaDict["Nicaragua"].Adyacentes.Agregar(mapaDict["Costa_Rica"]);
            mapaDict["Costa_Rica"].Adyacentes.Agregar(mapaDict["Nicaragua"]);

            mapaDict["Costa_Rica"].Adyacentes.Agregar(mapaDict["Panama"]);
            mapaDict["Panama"].Adyacentes.Agregar(mapaDict["Costa_Rica"]);

            mapaDict["Panama"].Adyacentes.Agregar(mapaDict["Colombia"]);
            mapaDict["Colombia"].Adyacentes.Agregar(mapaDict["Panama"]);

            // Caribe
            mapaDict["Cuba"].Adyacentes.Agregar(mapaDict["Jamaica"]);
            mapaDict["Jamaica"].Adyacentes.Agregar(mapaDict["Cuba"]);

            mapaDict["Cuba"].Adyacentes.Agregar(mapaDict["Republica_Dominicana"]);
            mapaDict["Republica_Dominicana"].Adyacentes.Agregar(mapaDict["Cuba"]);

            mapaDict["Republica_Dominicana"].Adyacentes.Agregar(mapaDict["Puerto_Rico"]);
            mapaDict["Puerto_Rico"].Adyacentes.Agregar(mapaDict["Republica_Dominicana"]);
            //america del sur
            mapaDict["Colombia"].Adyacentes.Agregar(mapaDict["Venezuela"]);
            mapaDict["Venezuela"].Adyacentes.Agregar(mapaDict["Colombia"]);

            mapaDict["Colombia"].Adyacentes.Agregar(mapaDict["Ecuador"]);
            mapaDict["Ecuador"].Adyacentes.Agregar(mapaDict["Colombia"]);

            mapaDict["Ecuador"].Adyacentes.Agregar(mapaDict["Peru"]);
            mapaDict["Peru"].Adyacentes.Agregar(mapaDict["Ecuador"]);

            mapaDict["Peru"].Adyacentes.Agregar(mapaDict["Brasil"]);
            mapaDict["Brasil"].Adyacentes.Agregar(mapaDict["Peru"]);

            mapaDict["Brasil"].Adyacentes.Agregar(mapaDict["Bolivia"]);
            mapaDict["Bolivia"].Adyacentes.Agregar(mapaDict["Brasil"]);

            mapaDict["Bolivia"].Adyacentes.Agregar(mapaDict["Paraguay"]);
            mapaDict["Paraguay"].Adyacentes.Agregar(mapaDict["Bolivia"]);

            mapaDict["Paraguay"].Adyacentes.Agregar(mapaDict["Argentina"]);
            mapaDict["Argentina"].Adyacentes.Agregar(mapaDict["Paraguay"]);

            mapaDict["Argentina"].Adyacentes.Agregar(mapaDict["Chile"]);
            mapaDict["Chile"].Adyacentes.Agregar(mapaDict["Argentina"]);

            mapaDict["Argentina"].Adyacentes.Agregar(mapaDict["Uruguay"]);
            mapaDict["Uruguay"].Adyacentes.Agregar(mapaDict["Argentina"]);

            mapaDict["Venezuela"].Adyacentes.Agregar(mapaDict["Guyana"]);
            mapaDict["Guyana"].Adyacentes.Agregar(mapaDict["Venezuela"]);

            mapaDict["Guyana"].Adyacentes.Agregar(mapaDict["Guyana_Francesa"]);
            mapaDict["Guyana_Francesa"].Adyacentes.Agregar(mapaDict["Guyana"]);

            mapaDict["Guyana"].Adyacentes.Agregar(mapaDict["Suriname"]);
            mapaDict["Suriname"].Adyacentes.Agregar(mapaDict["Guyana"]);

            mapaDict["Guyana_Francesa"].Adyacentes.Agregar(mapaDict["Suriname"]);
            mapaDict["Suriname"].Adyacentes.Agregar(mapaDict["Guyana_Francesa"]);

            mapaDict["Suriname"].Adyacentes.Agregar(mapaDict["Brasil"]);
            mapaDict["Brasil"].Adyacentes.Agregar(mapaDict["Suriname"]);

            // Norte
            mapaDict["Rusia"].Adyacentes.Agregar(mapaDict["Alemania"]);
            mapaDict["Alemania"].Adyacentes.Agregar(mapaDict["Rusia"]);

            mapaDict["Rusia"].Adyacentes.Agregar(mapaDict["Nepal"]);
            mapaDict["Nepal"].Adyacentes.Agregar(mapaDict["Rusia"]);

            mapaDict["Rusia"].Adyacentes.Agregar(mapaDict["Mongolia"]);
            mapaDict["Mongolia"].Adyacentes.Agregar(mapaDict["Rusia"]);

            mapaDict["Rusia"].Adyacentes.Agregar(mapaDict["China"]);
            mapaDict["China"].Adyacentes.Agregar(mapaDict["Rusia"]);

            mapaDict["Rusia"].Adyacentes.Agregar(mapaDict["TEC"]);
            mapaDict["TEC"].Adyacentes.Agregar(mapaDict["Rusia"]);

            mapaDict["TEC"].Adyacentes.Agregar(mapaDict["Islandia"]);
            mapaDict["Islandia"].Adyacentes.Agregar(mapaDict["TEC"]);

            mapaDict["Dinamarca"].Adyacentes.Agregar(mapaDict["Canada"]);
            mapaDict["Canada"].Adyacentes.Agregar(mapaDict["Dinamarca"]);

            mapaDict["Dinamarca"].Adyacentes.Agregar(mapaDict["Islandia"]);
            mapaDict["Islandia"].Adyacentes.Agregar(mapaDict["Dinamarca"]);

            mapaDict["Islandia"].Adyacentes.Agregar(mapaDict["Italia"]);
            mapaDict["Italia"].Adyacentes.Agregar(mapaDict["Islandia"]);

            // Europa
            mapaDict["Italia"].Adyacentes.Agregar(mapaDict["Alemania"]);
            mapaDict["Alemania"].Adyacentes.Agregar(mapaDict["Italia"]);

            mapaDict["Alemania"].Adyacentes.Agregar(mapaDict["Turquia"]);
            mapaDict["Turquia"].Adyacentes.Agregar(mapaDict["Alemania"]);

            mapaDict["Turquia"].Adyacentes.Agregar(mapaDict["Rusia"]);
            mapaDict["Rusia"].Adyacentes.Agregar(mapaDict["Turquia"]);

            mapaDict["Turquia"].Adyacentes.Agregar(mapaDict["Arabia_Saudita"]);
            mapaDict["Arabia_Saudita"].Adyacentes.Agregar(mapaDict["Turquia"]);

            // África
            mapaDict["Nigeria"].Adyacentes.Agregar(mapaDict["Italia"]);
            mapaDict["Italia"].Adyacentes.Agregar(mapaDict["Nigeria"]);

            mapaDict["Nigeria"].Adyacentes.Agregar(mapaDict["Sudafrica"]);
            mapaDict["Sudafrica"].Adyacentes.Agregar(mapaDict["Nigeria"]);

            mapaDict["Sudafrica"].Adyacentes.Agregar(mapaDict["Arabia_Saudita"]);
            mapaDict["Arabia_Saudita"].Adyacentes.Agregar(mapaDict["Sudafrica"]);
            //asia
            mapaDict["Arabia_Saudita"].Adyacentes.Agregar(mapaDict["Nepal"]);
            mapaDict["Nepal"].Adyacentes.Agregar(mapaDict["Arabia_Saudita"]);

            mapaDict["Nepal"].Adyacentes.Agregar(mapaDict["Rusia"]);
            mapaDict["Rusia"].Adyacentes.Agregar(mapaDict["Nepal"]);

            mapaDict["Nepal"].Adyacentes.Agregar(mapaDict["China"]);
            mapaDict["China"].Adyacentes.Agregar(mapaDict["Nepal"]);

            mapaDict["Nepal"].Adyacentes.Agregar(mapaDict["Mongolia"]);
            mapaDict["Mongolia"].Adyacentes.Agregar(mapaDict["Nepal"]);

            mapaDict["Nepal"].Adyacentes.Agregar(mapaDict["India"]);
            mapaDict["India"].Adyacentes.Agregar(mapaDict["Nepal"]);

            mapaDict["India"].Adyacentes.Agregar(mapaDict["China"]);
            mapaDict["China"].Adyacentes.Agregar(mapaDict["India"]);

            mapaDict["China"].Adyacentes.Agregar(mapaDict["Mongolia"]);
            mapaDict["Mongolia"].Adyacentes.Agregar(mapaDict["China"]);

            mapaDict["China"].Adyacentes.Agregar(mapaDict["Indonesia"]);
            mapaDict["Indonesia"].Adyacentes.Agregar(mapaDict["China"]);

            mapaDict["Indonesia"].Adyacentes.Agregar(mapaDict["Australia"]);
            mapaDict["Australia"].Adyacentes.Agregar(mapaDict["Indonesia"]);
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
        // Colocación alternada de tropas iniciales por los jugadores
        public void ColocarTropaInicialAlternada()
        {
            while (Jugador1.TropasDisponibles > 0 || Jugador2.TropasDisponibles > 0)
            {
                if (Jugador1.TropasDisponibles > 0)
                {
                    Territorio territorio = SeleccionarTerritorioAleatorio(Jugador1);
                    territorio.Tropas++;
                    Jugador1.TropasDisponibles--;
                }

                if (Jugador2.TropasDisponibles > 0)
                {
                    Territorio territorio = SeleccionarTerritorioAleatorio(Jugador2);
                    territorio.Tropas++;
                    Jugador2.TropasDisponibles--;
                }
            }
        }

        // Selecciona un territorio aleatorio del jugador
        private Territorio SeleccionarTerritorioAleatorio(Jugador jugador)
        {
            var territorios = new Lista<Territorio>();
            var nodo = Mapa.Territorios.ObtenerCabeza();
            while (nodo != null)
            {
                if (nodo.Valor.Dueño == jugador)
                    territorios.Agregar(nodo.Valor);
                nodo = nodo.Siguiente;
            }
            return territorios.SeleccionarAleatorio();
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
            //DILAN
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
        // Mueve tropas entre territorios propios
        public bool MoverTropas(Territorio origen, Territorio destino, int cantidad)
        {
            if (origen.Dueño != Actual || destino.Dueño != Actual) return false;
            if (!ExisteRutaEntre(origen, destino, Actual)) return false;
            if (cantidad <= 0 || origen.Tropas <= cantidad) return false;

            origen.Tropas -= cantidad;
            destino.Tropas += cantidad;
            return true;
        }
        //Dilan
        // Verifica si hay ruta entre dos territorios del mismo jugador
    private bool ExisteRutaEntre(Territorio origen, Territorio destino, Jugador jugador)
    {
        var visitados = new Lista<Territorio>();
        var cola = new Cola<Territorio>();
        cola.Encolar(origen);

        while (!cola.EstaVacia())
        {
            var actual = cola.Desencolar();
            if (actual == destino) return true;

            visitados.Agregar(actual);

            var nodo = actual.Adyacentes.ObtenerCabeza();
            while (nodo != null)
            {
                var vecino = nodo.Valor;
                if (vecino.Dueño == jugador && !visitados.Contiene(vecino))
                    cola.Encolar(vecino);
                nodo = nodo.Siguiente;
            }
        }
        return false;
    }

        // Coloca tropas restantes del jugador neutro aleatoriamente
        public void DistribuirTropasNeutro()
        {
            int restantes = 40 - ContarTerritorios(Neutro);
            var territoriosNeutros = new Lista<Territorio>();

            var nodo = Mapa.Territorios.ObtenerCabeza();
            while (nodo != null)
            {
                if (nodo.Valor.Dueño == Neutro)
                    territoriosNeutros.Agregar(nodo.Valor);
                nodo = nodo.Siguiente;
            }

            while (restantes > 0)
            {
                var territorio = territoriosNeutros.SeleccionarAleatorio(); // Usa tu método nuevo
                territorio.Tropas++;
                restantes--;
            }
        }
        // Baraja el mazo de tarjetas manualmente
        public void BarajarMazo()
        {
            MazoDeTarjetas.Aleatorio();
        }

        // Asigna tarjeta con nombre del territorio (opcional)
        public void AsignarTarjetaPorTerritorio(Jugador jugador, string nombreTerritorio)
        {
            if (!MazoDeTarjetas.EstaVacia())
            {
                Tarjeta tarjeta = MazoDeTarjetas.SacarDelFrente();
                tarjeta.Tipo += $" - {nombreTerritorio}";
                jugador.Tarjetas.Agregar(tarjeta);
            }
        }

        // Activa tercer jugador como reemplazo del neutro
        public void ActivarTercerJugador(string alias, string color)
        {
            Neutro = new Jugador(alias, color);
            var nodo = Mapa.Territorios.ObtenerCabeza();
            while (nodo != null)
            {
                if (nodo.Valor.Dueño.Alias == "Neutro")
                    nodo.Valor.Dueño = Neutro;
                nodo = nodo.Siguiente;
            }
        }

        public void AvanzarEtapa()
        {
            if (EtapaActual == EtapaTurno.Refuerzo)
                EtapaActual = EtapaTurno.Ataque;
            else if (EtapaActual == EtapaTurno.Ataque)
                EtapaActual = EtapaTurno.Movimiento;
            else
            {
                EtapaActual = EtapaTurno.Refuerzo;
                CambiarTurno();
            }

        }
        // Wrapper para compatibilidad con Server.cs
        public void AvanzarFase()
        {
            AvanzarEtapa();
        }

        // Colocar refuerzo simple: coloca una tropa en el territorio indicado por nombre
        // Devuelve true si la colocación tuvo éxito.
        public bool ColocarRefuerzo(string nombreTerritorio)
        {
            var territorio = GetTerritorio(nombreTerritorio);
            if (territorio == null) return false;
            if (Actual == null) return false;
            if (Actual.TropasDisponibles <= 0) return false;

            territorio.Tropas += 1;
            Actual.TropasDisponibles -= 1;
            return true;
        }

        // Devuelve un estado simplificado del mapa para enviar por la red.
        // Estructura: Dictionary<string, object> con claves: "Territorios" -> List<Dictionary<string, object>>
        public Dictionary<string, object> GetSimplifiedMapState()
        {
            var mapList = new Lista<Dictionary<string, object>>();
            var nodo = Mapa.Territorios.ObtenerCabeza();
            while (nodo != null)
            {
                var t = nodo.Valor;
                var territorioData = new Dictionary<string, object>
                {
                    { "Nombre", t.Nombre },
                    { "Dueno", t.Dueño?.Alias ?? "N/A" },
                    { "Tropas", t.Tropas }
                };
                mapList.Agregar(territorioData);
                nodo = nodo.Siguiente;
            }

            var result = new Dictionary<string, object>();
            result["Territorios"] = mapList; // Ahora es tu Lista<T>
            result["JugadorActual"] = Actual?.Alias ?? "";
            result["EtapaActual"] = EtapaActual.ToString();
            return result;
        }
            var result = new Dictionary<string, object>();
            result["Territorios"] = mapList;
            result["JugadorActual"] = Actual?.Alias ?? "";
            result["EtapaActual"] = EtapaActual.ToString();
            return result;
        }
        public bool PuedeAtacar()
        {
            return EtapaActual == EtapaTurno.Ataque && !Actual.DebeIntercambiarTarjetas;
        }

        public bool PuedeMover()
        {
            return EtapaActual == EtapaTurno.Movimiento && !Actual.DebeIntercambiarTarjetas;
        }

        public bool PuedeColocarRefuerzo()
        {
            return EtapaActual == EtapaTurno.Refuerzo && !Actual.DebeIntercambiarTarjetas;
        }
        public bool VerificarFinDePartida()
        {
            if (HaGanado(Jugador1))
            {
                Console.WriteLine($"¡{Jugador1.Alias} ha ganado!");
                return true;
            }
            else if (HaGanado(Jugador2))
            {
                Console.WriteLine($"¡{Jugador2.Alias} ha ganado!");
                return true;
            }
            else if (HaGanado(Neutro))
            {
                Console.WriteLine($"¡{Neutro.Alias} ha ganado!");
                return true;
            }
            return false;
        }
        public void PrepararRefuerzosInicialesTresJugadores()
        {
            int tropasIniciales = 35;
            Jugador1.TropasDisponibles = tropasIniciales - ContarTerritorios(Jugador1);
            Jugador2.TropasDisponibles = tropasIniciales - ContarTerritorios(Jugador2);
            Neutro.TropasDisponibles = tropasIniciales - ContarTerritorios(Neutro);
        }    
    }
    
}
