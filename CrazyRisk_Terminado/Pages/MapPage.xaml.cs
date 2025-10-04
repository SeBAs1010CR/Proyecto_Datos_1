using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CrazyRisk.Comms;

namespace CrazyRisk.Pages
{
    public partial class MapPage : Page
    {
        private enum MapMode { None, Attack, Move, Reinforce }
        private MapMode currentMode = MapMode.None;
        private string? originSelection = null;

        private GameClient? gameClient;
        private Frame? mainFrame;

        // Constructor vacío (XAML)
        public MapPage()
        {
            InitializeComponent();
            this.Unloaded += MapPage_Unloaded;
        }

        // Constructor que recibe frame y client
        public MapPage(Frame frame, GameClient client)
        {
            InitializeComponent();
            mainFrame = frame;
            gameClient = client;

            // Suscribirse a actualizaciones del servidor
            gameClient.UpdateReceived += OnServerUpdate;

            // Suscribir al evento Unloaded para limpiar evento cuando se descargue la Page
            this.Unloaded += MapPage_Unloaded;
        }

        // Handler para Unloaded (limpiar suscripciones)
        private void MapPage_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (gameClient != null)
            {
                gameClient.UpdateReceived -= OnServerUpdate;
                // No cerrar aquí la conexión; eso lo maneja GameClient o el flujo de desconexión.
            }
            this.Unloaded -= MapPage_Unloaded;
        }

        // --- EVENTO: Recibir actualización del servidor ---
        private void OnServerUpdate(MensajeJuego update)
        {
            if (update == null) return;

            if (string.Equals(update.TipoComando, "ActionResult", StringComparison.OrdinalIgnoreCase))
            {
                // Mostrar resultado de la acción al usuario
                if (update.Datos != null && update.Datos.TryGetValue("Success", out object ok))
                {
                    bool success = false;
                    try
                    {
                        // Manejar JsonElement o bool/string
                        if (ok is System.Text.Json.JsonElement je)
                        {
                            if (je.ValueKind == System.Text.Json.JsonValueKind.True) success = true;
                            else if (je.ValueKind == System.Text.Json.JsonValueKind.False) success = false;
                            else if (je.ValueKind == System.Text.Json.JsonValueKind.String) bool.TryParse(je.GetString(), out success);
                        }
                        else
                        {
                            success = Convert.ToBoolean(ok);
                        }
                    }
                    catch { success = false; }

                    string message = "";
                    try { if (update.Datos.TryGetValue("Message", out object m)) message = m?.ToString() ?? ""; } catch { }

                    Dispatcher.Invoke(() =>
                    {
                        TxtStatus.Text = success ? $"✅ Acción exitosa: {message}" : $"❌ Acción rechazada: {message}";
                    });
                }
            }
            else if (string.Equals(update.TipoComando, "StateUpdate", StringComparison.OrdinalIgnoreCase))
            {
                if (update.Datos != null && update.Datos.TryGetValue("State", out object stateObj))
                {
                    // Actualizar el mapa en el hilo del Dispatcher
                    Dispatcher.Invoke(() => ApplyStateToMap(stateObj));
                }
            }
        }

        // --- Aplicar estado del mapa ---
        private void ApplyStateToMap(object stateObj)
        {
            try
            {
                if (stateObj is System.Text.Json.JsonElement je)
                {
                    if (je.TryGetProperty("Territorios", out var terr))
                    {
                        foreach (var item in terr.EnumerateArray())
                        {
                            string nombre = item.GetProperty("Nombre").GetString() ?? "";
                            string dueno = item.GetProperty("Dueno").GetString() ?? "";
                            int tropas = item.GetProperty("Tropas").GetInt32();

                            var element = this.FindName(nombre) as System.Windows.Shapes.Shape;
                            if (element != null)
                            {
                                var color = Brushes.LightGray;
                                switch (dueno?.ToLower())
                                {
                                    case "azul": color = Brushes.LightBlue; break;
                                    case "rojo": color = Brushes.LightCoral; break;
                                    case "verde": color = Brushes.LightGreen; break;
                                    default: color = Brushes.LightGray; break;
                                }
                                element.Fill = color;
                                element.ToolTip = $"{dueno} - Tropas: {tropas}";
                            }
                        }
                    }
                }
                else if (stateObj is System.Collections.IDictionary dict)
                {
                    if (dict.Contains("Territorios"))
                    {
                        var list = dict["Territorios"] as System.Collections.IEnumerable;
                        if (list != null)
                        {
                            foreach (var obj in list)
                            {
                                if (obj is System.Collections.IDictionary d)
                                {
                                    string nombre = d["Nombre"]?.ToString() ?? "";
                                    string dueno = d["Dueno"]?.ToString() ?? "";
                                    int tropas = Convert.ToInt32(d["Tropas"] ?? 0);

                                    var element = this.FindName(nombre) as System.Windows.Shapes.Shape;
                                    if (element != null)
                                    {
                                        var color = Brushes.LightGray;
                                        switch (dueno?.ToLower())
                                        {
                                            case "azul": color = Brushes.LightBlue; break;
                                            case "rojo": color = Brushes.LightCoral; break;
                                            case "verde": color = Brushes.LightGreen; break;
                                            default: color = Brushes.LightGray; break;
                                        }
                                        element.Fill = color;
                                        element.ToolTip = $"{dueno} - Tropas: {tropas}";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MAP PAGE] Error aplicando estado: {ex.Message}");
            }
        }

        // --- BOTONES DE MODO ---
        private void BtnModeAttack_Click(object sender, RoutedEventArgs e)
        {
            currentMode = MapMode.Attack;
            originSelection = null;
            TxtStatus.Text = "Modo: Atacar → selecciona origen y luego destino.";
        }

        private void BtnModeMove_Click(object sender, RoutedEventArgs e)
        {
            currentMode = MapMode.Move;
            originSelection = null;
            TxtStatus.Text = "Modo: Mover → selecciona origen y luego destino.";
        }

        private void BtnModeReinforce_Click(object sender, RoutedEventArgs e)
        {
            currentMode = MapMode.Reinforce;
            originSelection = null;
            TxtStatus.Text = "Modo: Refuerzo → selecciona territorio.";
        }

        private void BtnCancelSelection_Click(object sender, RoutedEventArgs e)
        {
            currentMode = MapMode.None;
            originSelection = null;
            TxtStatus.Text = "Acción cancelada.";
            ResetBorders();
        }

        // --- CLIC EN TERRITORIO ---
        private async void Territorio_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Shapes.Path territorio) return;
            string nombre = territorio.Name;

            // Sin cliente (modo offline)
            if (gameClient == null)
            {
                LblNombre.Text = nombre;
                LblDueno.Text = territorio.ToolTip?.ToString() ?? "N/A";
                return;
            }

            // Con cliente: depende del modo actual
            if (currentMode == MapMode.None)
            {
                LblNombre.Text = nombre;
                LblDueno.Text = territorio.ToolTip?.ToString() ?? "N/A";
                return;
            }

            if (currentMode == MapMode.Reinforce)
            {
                var msg = new MensajeJuego("REFORZAR", gameClient.PlayerAlias);
                msg.Datos["Territorio"] = nombre;
                await gameClient.SendCommand(msg);
                TxtStatus.Text = $"Refuerzo enviado para {nombre}...";
                return;
            }

            if (currentMode == MapMode.Attack || currentMode == MapMode.Move)
            {
                if (originSelection == null)
                {
                    originSelection = nombre;
                    TxtStatus.Text = $"Origen seleccionado: {originSelection}. Ahora elige destino.";
                    try
                    {
                        ResetBorders();
                        territorio.Stroke = Brushes.Yellow;
                        territorio.StrokeThickness = 4;
                    }
                    catch { }
                }
                else
                {
                    string origen = originSelection;
                    string destino = nombre;
                    originSelection = null;
                    ResetBorders();

                    if (currentMode == MapMode.Attack)
                    {
                        var msg = new MensajeJuego("ATACAR", gameClient.PlayerAlias);
                        msg.Datos["Origen"] = origen;
                        msg.Datos["Destino"] = destino;
                        msg.Datos["Dados"] = 1;
                        msg.Datos["TropasMover"] = 1;
                        await gameClient.SendCommand(msg);
                        TxtStatus.Text = $"Ataque pedido: {origen} → {destino}. Esperando resultado...";
                    }
                    else if (currentMode == MapMode.Move)
                    {
                        var msg = new MensajeJuego("MOVER", gameClient.PlayerAlias);
                        msg.Datos["Origen"] = origen;
                        msg.Datos["Destino"] = destino;
                        msg.Datos["Cantidad"] = 1;
                        await gameClient.SendCommand(msg);
                        TxtStatus.Text = $"Movimiento pedido: {origen} → {destino}. Esperando resultado...";
                    }
                }
            }
        }

        // --- RESET VISUAL DE TERRITORIOS ---
        private void ResetBorders()
        {
            // Intentar restablecer bordes buscando shapes dentro del layout visual.
            try
            {
                var allShapes = LogicalTreeHelper.GetChildren(this);
                foreach (var child in allShapes)
                {
                    if (child is System.Windows.Shapes.Shape s)
                    {
                        s.Stroke = Brushes.Black;
                        s.StrokeThickness = 2;
                    }
                }
            }
            catch
            {
                // Si falla, no rompemos la UI; es solo un "mejor intento".
            }
        }
    }
}
