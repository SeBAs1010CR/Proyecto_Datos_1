using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrazyRisk.Comms
{
    // 1. Clase para la Estructura del Mensaje (El "paquete" de datos)
    public class MensajeJuego
    {
        public string TipoComando { get; set; } // Ej: "ATACAR", "REFORZAR"
        public string JugadorAlias { get; set; }
        public Dictionary<string, object> Datos { get; set; } // Datos específicos (Ej: "TerritorioOrigen": "USA")

        public MensajeJuego(string tipoComando, string alias)
        {
            TipoComando = tipoComando;
            JugadorAlias = alias;
            Datos = new Dictionary<string, object>();
        }
    }

    // 2. Clase Estática para Serialización/Deserialización (Los "ayudantes")
    public static class JsonHelper
    {
        // Convierte el objeto MensajeJuego a una cadena JSON
        public static string SerializarComando(MensajeJuego comando)
        {
            // Usar JsonSerializer para convertir a string.
            // Los options son para hacer el JSON más legible en debug (indentado).
            return JsonSerializer.Serialize(comando, new JsonSerializerOptions { WriteIndented = true });
        }

        // Convierte la cadena JSON de vuelta a un objeto MensajeJuego
        public static MensajeJuego? DeserializarComando(string jsonString)
        {
            try
            {
                // El '?' al final de MensajeJuego indica que puede ser null si falla la deserialización.
                return JsonSerializer.Deserialize<MensajeJuego>(jsonString);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error de deserialización: {ex.Message}");
                return null;
            }
        }
    }
}