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

    public bool EstaVacia() => cabeza == null;

    public Nodo<T>? ObtenerCabeza() => cabeza;
}

