using CrazyRisk.Models;
using System.Collections.Generic;
using System;
﻿public static class LogicaJuego
{
    private static int contadorFibonacci = 2; // empieza en 2

    public static int CalcularRefuerzos(Jugador jugador, int totalTerritorios, int bonusContinente, bool intercambioTarjetas)
    {
        int tropas = (totalTerritorios / 3) + bonusContinente;

        if (intercambioTarjetas)
        {
            tropas += contadorFibonacci;
            contadorFibonacci = SiguienteFibonacci(contadorFibonacci);
        }

        return tropas;
    }

    private static int SiguienteFibonacci(int actual)
    {
        // Sencillo: calculamos el siguiente
        int a = 1, b = 2;
        while (b <= actual)
        {
            int temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }
    public static int NthFibonacci(int n)
    {
        // La serie de Risk (2, 3, 5, 8...) corresponde a F(3), F(4), F(5), F(6)...
        // donde F(0)=0, F(1)=1.
        
        // Si el contador es 1 (primer intercambio), queremos el valor 2.
        // Si el contador es 2, queremos 3.
        // ... por lo tanto, necesitamos calcular el (n + 2) - ésimo término.
        int index = n + 2; 

        // Casos base
        if (index <= 0) return 0;
        if (index == 1) return 1;

        int a = 0; // F(0)
        int b = 1; // F(1)
        int c = 1; // F(2)

        for (int i = 2; i < index; i++)
        {
            c = a + b;
            a = b;
            b = c;
        }
        return c;
    }
}



public static class Combate
{
    private static Random rnd = new Random();

    public static (int perdidasAtacante, int perdidasDefensor) Resolver(int tropasAtacante, int tropasDefensor, int dadosAtacante, int dadosDefensor)
    {
        // Lanzar dados
        List<int> atk = new List<int>();
        List<int> def = new List<int>();

        for (int i = 0; i < dadosAtacante; i++) atk.Add(rnd.Next(1, 7));
        for (int i = 0; i < dadosDefensor; i++) def.Add(rnd.Next(1, 7));

        atk.Sort((a, b) => b.CompareTo(a)); // ordenar desc
        def.Sort((a, b) => b.CompareTo(a));

        int perdidasAtk = 0, perdidasDef = 0;
        int comparaciones = Math.Min(atk.Count, def.Count);

        for (int i = 0; i < comparaciones; i++)
        {
            if (atk[i] > def[i]) perdidasDef++;
            else perdidasAtk++;
        }

        return (perdidasAtk, perdidasDef);
    }
}
