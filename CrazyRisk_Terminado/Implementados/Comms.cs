using CrazyRisk.Models;
using System;

#nullable enable

namespace CrazyRisk.Comms
{
    /// <summary>
    /// Clase encargada de manejar la comunicación entre las distintas partes del juego.
    /// </summary>
    public class Comunicador
    {
        // Ejemplo: eventos o colas para enviar mensajes entre capas del juego
        public event Action<string>? MensajeRecibido;

        private readonly Cola<string> mensajesPendientes = new Cola<string>();

        /// <summary>
        /// Envía un mensaje al sistema.
        /// </summary>
        public void EnviarMensaje(string mensaje)
        {
            mensajesPendientes.Encolar(mensaje);
            MensajeRecibido?.Invoke(mensaje);
        }

        /// <summary>
        /// Obtiene el siguiente mensaje pendiente (si existe).
        /// </summary>
        public string? ObtenerMensaje()
        {
            if (mensajesPendientes.EstaVacia())
                return null;
            return mensajesPendientes.Desencolar();
        }

        /// <summary>
        /// Indica si hay mensajes en cola.
        /// </summary>
        public bool HayMensajesPendientes => !mensajesPendientes.EstaVacia();
    }
}
