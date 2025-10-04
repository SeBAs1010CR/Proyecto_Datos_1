using CrazyRisk.Models;
using System;

#nullable enable

// ===========================================
// Clase Nodo<T>
// ===========================================
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

// ===========================================
// Clase Lista<T> - Lista enlazada genérica
// ===========================================
public class Lista<T>
{
    private Nodo<T>? cabeza;

    // ------------------------------
    // Agregar un elemento al final
    // ------------------------------
    public void Agregar(T valor)
    {
        Nodo<T> nuevo = new Nodo<T>(valor);
        if (cabeza == null)
        {
            cabeza = nuevo;
        }
        else
        {
            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null)
                actual = actual.Siguiente;
            actual.Siguiente = nuevo;
        }
    }

    // ------------------------------
    // Sacar del frente (como una cola)
    // ------------------------------
    public T SacarDelFrente()
    {
        if (cabeza == null)
            throw new InvalidOperationException("La lista está vacía.");

        T valorFrente = cabeza.Valor;
        cabeza = cabeza.Siguiente;
        return valorFrente;
    }

    // ------------------------------
    // Obtener el tamaño de la lista
    // ------------------------------
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

    // ------------------------------
    // Remover un valor específico
    // ------------------------------
    public bool Remover(T valor)
    {
        if (cabeza == null) return false;

        // Caso 1: el valor está en la cabeza
        if (cabeza.Valor!.Equals(valor))
        {
            cabeza = cabeza.Siguiente;
            return true;
        }

        // Caso 2: está más adelante
        Nodo<T>? actual = cabeza;
        while (actual.Siguiente != null)
        {
            if (actual.Siguiente.Valor!.Equals(valor))
            {
                actual.Siguiente = actual.Siguiente.Siguiente;
                return true;
            }
            actual = actual.Siguiente;
        }
        return false;
    }

    // ------------------------------
    // Verificar si la lista está vacía
    // ------------------------------
    public bool EstaVacia() => cabeza == null;

    // ------------------------------
    // Obtener referencia a la cabeza
    // ------------------------------
    public Nodo<T>? ObtenerCabeza() => cabeza;

    // ------------------------------
    // Convertir lista a arreglo
    // ------------------------------
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

    // ------------------------------
    // Seleccionar un elemento aleatorio
    // ------------------------------
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

    // ------------------------------
    // Barajar los elementos de la lista
    // ------------------------------
    public void Aleatorio()
    {
        if (cabeza == null || cabeza.Siguiente == null)
            return;

        T[] elementos = ConvertirAArray();

        Random random = new Random();
        for (int i = elementos.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (elementos[i], elementos[j]) = (elementos[j], elementos[i]);
        }

        cabeza = null;
        foreach (var e in elementos)
            Agregar(e);
    }

    // ------------------------------
    // Verificar si contiene un valor
    // ------------------------------
    public bool Contiene(T valor)
    {
        var actual = cabeza;
        while (actual != null)
        {
            if (actual.Valor!.Equals(valor))
                return true;
            actual = actual.Siguiente;
        }
        return false;
    }
}

// ===========================================
// Clase Cola<T> - Implementa una cola simple
// ===========================================
public class Cola<T>
{
    private Lista<T> elementos = new Lista<T>();

    public void Encolar(T valor) => elementos.Agregar(valor);

    public T Desencolar()
    {
        if (elementos.EstaVacia())
            throw new InvalidOperationException("La cola está vacía.");
        return elementos.SacarDelFrente();
    }

    public bool EstaVacia() => elementos.EstaVacia();

    public int ObtenerTamaño() => elementos.ObtenerTamaño();
}
