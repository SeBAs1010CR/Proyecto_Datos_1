public class Nodo<T>
{
    public T Valor { get; set; }
    public Nodo<T>? Siguiente { get; set; }

    public Nodo(T valor)
    {
        Valor = valor;
        Siguiente = null;
    }
}

public class Lista<T>
{
    private Nodo<T>? cabeza;

    public void Agregar(T valor)
    {
        Nodo<T> nuevo = new Nodo<T>(valor);
        if (cabeza == null)
            cabeza = nuevo;
        else
        {
            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null)
                actual = actual.Siguiente;
            actual.Siguiente = nuevo;
        }
    }


    //Sebas
    public T SacarDelFrente()
    {
        if (cabeza == null)
        {
            // Opcional: Lanza una excepción si la lista está vacía.
            throw new InvalidOperationException("La lista está vacía.");
        }

        T valorFrente = cabeza.Valor;
        cabeza = cabeza.Siguiente; // Mueve la cabeza al siguiente nodo (elimina el original)
        return valorFrente;
    }
    public int ObtenerTamaño()
    {
        int count = 0;
        Nodo<T>? actual = cabeza;
        while (actual != null)
        {
            count++;
            actual = actual.Siguiente;
        }
        return count;
    }
    public bool Remover(T valor)
    {
        if (cabeza == null) return false;

        // Caso 1: El elemento a remover es la cabeza.
        if (cabeza.Valor!.Equals(valor)) // Usar Equals para comparar objetos T
        {
            cabeza = cabeza.Siguiente;
            return true;
        }

        // Caso 2: El elemento está en el medio o al final.
        Nodo<T>? actual = cabeza;
        while (actual.Siguiente != null)
        {
            if (actual.Siguiente.Valor!.Equals(valor))
            {
                // Encontrado: el actual.Siguiente es el nodo a eliminar.
                actual.Siguiente = actual.Siguiente.Siguiente; // Salta el nodo a remover
                return true;
            }
            actual = actual.Siguiente;
        }
        return false; // Valor no encontrado.
    }

    //Método para aleatorizar la lista.
    public void Aleatorio()
    {
        if (cabeza == null || cabeza.Siguiente == null)
        {
            return; // No hay nada que barajar o solo hay un elemento.
        }

        int tamaño = ObtenerTamaño();

        // 1. EXTRAER: Mover todos los valores a un arreglo.
        // NOTA: Usamos el arreglo de C# como herramienta de proceso,
        // no como estructura de datos principal.
        T[] elementos = new T[tamaño];
        Nodo<T>? actual = cabeza;
        for (int i = 0; i < tamaño; i++)
        {
            elementos[i] = actual!.Valor;
            actual = actual.Siguiente;
        }

        // 2. BARAJAR (Algoritmo Fisher-Yates)
        // Este algoritmo es eficiente para la aleatorización.
        Random random = new Random();
        for (int i = tamaño - 1; i > 0; i--)
        {
            // Elige un índice aleatorio 'j' antes del actual 'i'
            int j = random.Next(0, i + 1);

            // Intercambiar elementos[i] y elementos[j]
            T temp = elementos[i];
            elementos[i] = elementos[j];
            elementos[j] = temp;
        }

        // 3. RECONSTRUIR: Reemplazar los valores de la lista enlazada con el orden aleatorio.
        cabeza = null; // Borramos la lista existente

        // Recorremos el arreglo barajado y reconstruimos la lista
        for (int i = 0; i < tamaño; i++)
        {
            Agregar(elementos[i]);
        }
    }
    public bool EstaVacia() => cabeza == null;

    public Nodo<T>? ObtenerCabeza() => cabeza;
    
    
// ✅ Nuevo: Convertir a arreglo
    public T[] ConvertirAArray()
    {
        int tamaño = ObtenerTamaño();
        T[] elementos = new T[tamaño];
        Nodo<T>? actual = cabeza;
        for (int i = 0; i < tamaño; i++)
        {
            elementos[i] = actual!.Valor;
            actual = actual.Siguiente;
        }
        return elementos;
    }

    // ✅ Nuevo: Seleccionar elemento aleatorio
    public T SeleccionarAleatorio()
    {
        if (EstaVacia())
            throw new InvalidOperationException("La lista está vacía.");

        int tamaño = ObtenerTamaño();
        Random random = new Random();
        int indice = random.Next(0, tamaño);

        Nodo<T>? actual = cabeza;
        for (int i = 0; i < indice; i++)
            actual = actual!.Siguiente;

        return actual!.Valor;
    }

    // ✅ Nuevo: Barajar lista (ya tienes Aleatorio, lo dejamos igual)
    public void Aleatorio()
    {
        if (cabeza == null || cabeza.Siguiente == null)
            return;

        T[] elementos = ConvertirAArray();

        Random random = new Random();
        for (int i = elementos.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            T temp = elementos[i];
            elementos[i] = elementos[j];
            elementos[j] = temp;
        }

        cabeza = null;
        foreach (var e in elementos)
            Agregar(e);
    }
}




