using System.Xml;
using SistemaDrones.Modelos;
using SistemaDrones.TDAs;

namespace SistemaDrones.Logica
{
    public class GestorXML
    {
        // ─────────────────────────────────────────────────────────────
        // LEER XML DE ENTRADA (incremental - agrega a las listas)
        // ─────────────────────────────────────────────────────────────
        public void CargarXML(
            string rutaArchivo,
            ListaEnlazada<Dron> listaDrones,
            ListaEnlazada<SistemaDronesModelo> listaSistemas,
            ListaEnlazada<Mensaje> listaMensajes)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(rutaArchivo);

            // 1. Cargar drones
            XmlNodeList nodosDrones = doc.SelectNodes("//listaDrones/dron");
            foreach (XmlNode nodo in nodosDrones)
            {
                string nombre = nodo.InnerText.Trim();
                // Solo agregar si no existe
                if (!listaDrones.Existe(d => d.Nombre == nombre))
                    listaDrones.Agregar(new Dron(nombre));
            }

            // 2. Cargar sistemas de drones
            XmlNodeList nodosSistemas = doc.SelectNodes("//listaSistemasDrones/sistemaDrones");
            foreach (XmlNode nodoSistema in nodosSistemas)
            {
                string nombreSistema = nodoSistema.Attributes["nombre"].Value;
                int alturaMaxima = int.Parse(nodoSistema.SelectSingleNode("alturaMaxima").InnerText.Trim());
                int cantidadDrones = int.Parse(nodoSistema.SelectSingleNode("cantidadDrones").InnerText.Trim());

                SistemaDronesModelo sistema = new SistemaDronesModelo(nombreSistema, alturaMaxima, cantidadDrones);

                // Cargar contenido (drones x alturas)
                XmlNodeList nodosContenido = nodoSistema.SelectNodes("contenido");
                foreach (XmlNode contenido in nodosContenido)
                {
                    string nombreDron = contenido.SelectSingleNode("dron").InnerText.Trim();
                    ContenidoDron contenidoDron = new ContenidoDron(nombreDron);

                    XmlNodeList nodosAlturas = contenido.SelectNodes("alturas/altura");
                    foreach (XmlNode nodoAltura in nodosAlturas)
                    {
                        int valorAltura = int.Parse(nodoAltura.Attributes["valor"].Value);
                        string letra = nodoAltura.InnerText.Trim();
                        contenidoDron.Alturas.Agregar(new EntradaAltura(valorAltura, letra));
                    }

                    sistema.Contenido.Agregar(contenidoDron);
                }

                // Solo agregar si no existe
                if (!listaSistemas.Existe(s => s.Nombre == nombreSistema))
                    listaSistemas.Agregar(sistema);
            }

            // 3. Cargar mensajes
            XmlNodeList nodosMensajes = doc.SelectNodes("//listaMensajes/Mensaje");
            foreach (XmlNode nodoMensaje in nodosMensajes)
            {
                string nombreMensaje = nodoMensaje.Attributes["nombre"].Value;
                string nombreSistema = nodoMensaje.SelectSingleNode("sistemaDrones").InnerText.Trim();

                Mensaje mensaje = new Mensaje(nombreMensaje, nombreSistema);

                XmlNodeList nodosInstrucciones = nodoMensaje.SelectNodes("instrucciones/instruccion");
                foreach (XmlNode nodoInstruccion in nodosInstrucciones)
                {
                    string nombreDron = nodoInstruccion.Attributes["dron"].Value;
                    int altura = int.Parse(nodoInstruccion.InnerText.Trim());
                    mensaje.Instrucciones.Agregar(new Instruccion(nombreDron, altura));
                }

                if (!listaMensajes.Existe(m => m.Nombre == nombreMensaje))
                    listaMensajes.Agregar(mensaje);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GENERAR XML DE SALIDA
        // ─────────────────────────────────────────────────────────────
        public void GenerarSalida(string rutaSalida, ListaEnlazada<ResultadoSimulacion> resultados)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(decl);

            XmlElement raiz = doc.CreateElement("respuesta");
            doc.AppendChild(raiz);

            XmlElement listaMensajes = doc.CreateElement("listaMensajes");
            raiz.AppendChild(listaMensajes);

            for (int i = 0; i < resultados.Tamanio; i++)
            {
                ResultadoSimulacion res = resultados.Obtener(i);

                XmlElement nodoMensaje = doc.CreateElement("mensaje");
                nodoMensaje.SetAttribute("nombre", res.NombreMensaje);
                listaMensajes.AppendChild(nodoMensaje);

                // Sistema de drones
                XmlElement sistema = doc.CreateElement("sistemaDrones");
                sistema.InnerText = res.NombreSistemaDrones;
                nodoMensaje.AppendChild(sistema);

                // Tiempo óptimo
                XmlElement tiempo = doc.CreateElement("tiempoOptimo");
                tiempo.InnerText = res.TiempoOptimo.ToString();
                nodoMensaje.AppendChild(tiempo);

                // Mensaje recibido
                XmlElement mensajeRecibido = doc.CreateElement("mensajeRecibido");
                mensajeRecibido.InnerText = res.MensajeRecibido;
                nodoMensaje.AppendChild(mensajeRecibido);

                // Instrucciones detalladas
                XmlElement instrucciones = doc.CreateElement("instrucciones");
                nodoMensaje.AppendChild(instrucciones);

                for (int t = 0; t < res.Instrucciones.Tamanio; t++)
                {
                    InstanteTiempo instante = res.Instrucciones.Obtener(t);

                    XmlElement nodoTiempo = doc.CreateElement("tiempo");
                    nodoTiempo.SetAttribute("valor", instante.Tiempo.ToString());
                    instrucciones.AppendChild(nodoTiempo);

                    XmlElement acciones = doc.CreateElement("acciones");
                    nodoTiempo.AppendChild(acciones);

                    for (int a = 0; a < instante.Acciones.Tamanio; a++)
                    {
                        AccionDron accion = instante.Acciones.Obtener(a);
                        XmlElement nodoAccion = doc.CreateElement("dron");
                        nodoAccion.SetAttribute("nombre", accion.NombreDron);
                        nodoAccion.InnerText = accion.Accion;
                        acciones.AppendChild(nodoAccion);
                    }
                }
            }

            doc.Save(rutaSalida);
        }
    }
}
