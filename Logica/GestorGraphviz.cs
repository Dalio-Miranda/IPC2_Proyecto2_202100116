using SistemaDrones.Modelos;
using SistemaDrones.TDAs;
using System.Diagnostics;
using System.IO;

namespace SistemaDrones.Logica
{
    public class GestorGraphviz
    {
        private string rutaDot = @"C:\Program Files\Graphviz\bin\dot.exe";

        // ─────────────────────────────────────────────────────────────
        // Genera imagen del sistema de drones (tabla de alturas)
        // ─────────────────────────────────────────────────────────────
        public string GenerarSistemaDrones(SistemaDronesModelo sistema, string carpetaSalida)
        {
            string contenidoDot = GenerarDotSistema(sistema);
            return EjecutarDot(contenidoDot, carpetaSalida, $"sistema_{sistema.Nombre}");
        }

        // ─────────────────────────────────────────────────────────────
        // Genera imagen de las instrucciones de un mensaje
        // ─────────────────────────────────────────────────────────────
        public string GenerarMensaje(ResultadoSimulacion resultado, string carpetaSalida)
        {
            string contenidoDot = GenerarDotMensaje(resultado);
            return EjecutarDot(contenidoDot, carpetaSalida, $"mensaje_{resultado.NombreMensaje}");
        }

        // ─────────────────────────────────────────────────────────────
        // Genera el contenido .dot del sistema de drones
        // ─────────────────────────────────────────────────────────────
        private string GenerarDotSistema(SistemaDronesModelo sistema)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("digraph SistemaDrones {");
            sb.AppendLine("  rankdir=TB;");
            sb.AppendLine("  node [shape=record, style=filled, fillcolor=\"#1f2937\", fontcolor=\"white\", fontname=\"Arial\", color=\"#374151\"];");
            sb.AppendLine("  graph [bgcolor=\"#111827\", fontcolor=\"white\", fontname=\"Arial\"];");
            sb.AppendLine("  edge [color=\"#6b7280\"];");
            sb.AppendLine($"  label=\"Sistema: {sistema.Nombre}\\nAltura máxima: {sistema.AlturaMaxima} mts | Drones: {sistema.CantidadDrones}\";");
            sb.AppendLine("  labelloc=t;");
            sb.AppendLine("  fontsize=16;");
            sb.AppendLine("  fontcolor=\"#60a5fa\";");

            // Nodo título de columnas
            sb.Append("  header [label=\"{Altura");
            Nodo<ContenidoDron> nodoDron = sistema.Contenido.ObtenerCabeza();
            while (nodoDron != null)
            {
                sb.Append($"|{nodoDron.Dato.NombreDron}");
                nodoDron = nodoDron.Siguiente;
            }
            sb.AppendLine("}\", fillcolor=\"#1e3a5f\", fontcolor=\"#60a5fa\"];");

            // Nodos por cada altura
            for (int h = sistema.AlturaMaxima; h >= 1; h--)
            {
                sb.Append($"  altura{h} [label=\"{{{h}");
                Nodo<ContenidoDron> nodo = sistema.Contenido.ObtenerCabeza();
                while (nodo != null)
                {
                    string letra = sistema.ObtenerLetra(nodo.Dato.NombreDron, h);
                    sb.Append($"|{letra ?? "-"}");
                    nodo = nodo.Siguiente;
                }
                sb.AppendLine("}\"];");
            }

            // Conectar nodos verticalmente
            sb.Append("  header");
            for (int h = sistema.AlturaMaxima; h >= 1; h--)
                sb.Append($" -> altura{h}");
            sb.AppendLine(";");

            sb.AppendLine("}");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────
        // Genera el contenido .dot de las instrucciones de un mensaje
        // ─────────────────────────────────────────────────────────────
        private string GenerarDotMensaje(ResultadoSimulacion resultado)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("digraph Mensaje {");
            sb.AppendLine("  rankdir=LR;");
            sb.AppendLine("  node [shape=box, style=filled, fillcolor=\"#1f2937\", fontcolor=\"white\", fontname=\"Arial\", color=\"#374151\"];");
            sb.AppendLine("  graph [bgcolor=\"#111827\", fontcolor=\"white\", fontname=\"Arial\"];");
            sb.AppendLine("  edge [color=\"#6b7280\", fontcolor=\"#9ca3af\", fontname=\"Arial\"];");
            sb.AppendLine($"  label=\"Mensaje: {resultado.NombreMensaje} | Recibido: {resultado.MensajeRecibido} | Tiempo óptimo: {resultado.TiempoOptimo} seg\";");
            sb.AppendLine("  labelloc=t;");
            sb.AppendLine("  fontsize=14;");
            sb.AppendLine("  fontcolor=\"#60a5fa\";");

            // Nodo de inicio
            sb.AppendLine("  inicio [label=\"INICIO\", fillcolor=\"#065f46\", fontcolor=\"#34d399\", shape=oval];");

            // Nodos por cada instante de tiempo
            for (int t = 0; t < resultado.Instrucciones.Tamanio; t++)
            {
                InstanteTiempo instante = resultado.Instrucciones.Obtener(t);

                // Buscar si hay algún dron emitiendo luz en este instante
                string dronEmitiendo = "";
                Nodo<AccionDron> nodoAccion = instante.Acciones.ObtenerCabeza();
                while (nodoAccion != null)
                {
                    if (nodoAccion.Dato.Accion == "Emitir luz")
                    {
                        dronEmitiendo = nodoAccion.Dato.NombreDron;
                        break;
                    }
                    nodoAccion = nodoAccion.Siguiente;
                }

                // Color especial si hay emisión de luz
                string fillColor = dronEmitiendo != "" ? "\"#78350f\"" : "\"#1f2937\"";
                string fontColor = dronEmitiendo != "" ? "\"#fbbf24\"" : "\"white\"";

                // Construir etiqueta del nodo
                System.Text.StringBuilder label = new System.Text.StringBuilder();
                label.Append($"t={instante.Tiempo}\\n");
                Nodo<AccionDron> nodoAcc = instante.Acciones.ObtenerCabeza();
                while (nodoAcc != null)
                {
                    label.Append($"{nodoAcc.Dato.NombreDron}: {nodoAcc.Dato.Accion}\\n");
                    nodoAcc = nodoAcc.Siguiente;
                }

                sb.AppendLine($"  t{instante.Tiempo} [label=\"{label.ToString().TrimEnd()}\", fillcolor={fillColor}, fontcolor={fontColor}];");
            }

            // Nodo de fin
            sb.AppendLine("  fin [label=\"FIN\\n\" + \"Mensaje: " + resultado.MensajeRecibido + "\", fillcolor=\"#1e3a5f\", fontcolor=\"#60a5fa\", shape=oval];");

            // Conectar nodos
            sb.Append("  inicio");
            for (int t = 0; t < resultado.Instrucciones.Tamanio; t++)
            {
                InstanteTiempo instante = resultado.Instrucciones.Obtener(t);
                sb.Append($" -> t{instante.Tiempo}");
            }
            sb.AppendLine(" -> fin;");

            sb.AppendLine("}");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────────────────────
        // Ejecuta dot.exe y genera la imagen PNG
        // ─────────────────────────────────────────────────────────────
        private string EjecutarDot(string contenidoDot, string carpetaSalida, string nombreArchivo)
        {
            if (!Directory.Exists(carpetaSalida))
                Directory.CreateDirectory(carpetaSalida);

            string rutaDotFile = Path.Combine(carpetaSalida, $"{nombreArchivo}.dot");
            string rutaPng = Path.Combine(carpetaSalida, $"{nombreArchivo}.png");

            // Guardar archivo .dot
            File.WriteAllText(rutaDotFile, contenidoDot);

            // Ejecutar Graphviz
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = rutaDot,
                Arguments = $"-Tpng \"{rutaDotFile}\" -o \"{rutaPng}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process proceso = Process.Start(psi))
            {
                proceso.WaitForExit();
            }

            return rutaPng;
        }
    }
}