using SistemaDrones.Modelos;
using SistemaDrones.TDAs;

namespace SistemaDrones.Logica
{
    public class AccionDron
    {
        public string NombreDron { get; set; }
        public string Accion { get; set; }
        public AccionDron(string nombreDron, string accion)
        {
            NombreDron = nombreDron;
            Accion = accion;
        }
    }

    public class InstanteTiempo
    {
        public int Tiempo { get; set; }
        public ListaEnlazada<AccionDron> Acciones { get; set; }
        public InstanteTiempo(int tiempo)
        {
            Tiempo = tiempo;
            Acciones = new ListaEnlazada<AccionDron>();
        }
    }

    public class ResultadoSimulacion
    {
        public string NombreMensaje { get; set; }
        public string NombreSistemaDrones { get; set; }
        public int TiempoOptimo { get; set; }
        public string MensajeRecibido { get; set; }
        public ListaEnlazada<InstanteTiempo> Instrucciones { get; set; }
        public ResultadoSimulacion()
        {
            Instrucciones = new ListaEnlazada<InstanteTiempo>();
            MensajeRecibido = "";
        }
    }

    public class Simulador
    {
        public ResultadoSimulacion Simular(Mensaje mensaje, SistemaDronesModelo sistema)
        {
            ResultadoSimulacion resultado = new ResultadoSimulacion();
            resultado.NombreMensaje = mensaje.Nombre;
            resultado.NombreSistemaDrones = sistema.Nombre;

            // Posiciones usando TDA Mapa propio (sin Dictionary de C#)
            Mapa<string, int> posiciones = new Mapa<string, int>();
            Nodo<ContenidoDron> nodoDron = sistema.Contenido.ObtenerCabeza();
            while (nodoDron != null)
            {
                posiciones.Poner(nodoDron.Dato.NombreDron, 0);
                nodoDron = nodoDron.Siguiente;
            }

            int tiempoActual = 0;
            string mensajeRecibido = "";

            for (int i = 0; i < mensaje.Instrucciones.Tamanio; i++)
            {
                Instruccion instruccion = mensaje.Instrucciones.Obtener(i);
                string dronEmisor = instruccion.NombreDron;
                int alturaObjetivo = instruccion.Altura;
                int posEmisor = posiciones.Obtener(dronEmisor);
                int segundosViaje = System.Math.Abs(alturaObjetivo - posEmisor);
                Mapa<string, int> proximasPosiciones = ObtenerProximasPosiciones(mensaje, sistema, i, posiciones);
                int totalSegundos = segundosViaje + 1;

                for (int seg = 0; seg < totalSegundos; seg++)
                {
                    tiempoActual++;
                    InstanteTiempo instante = new InstanteTiempo(tiempoActual);

                    Nodo<ContenidoDron> nodo = sistema.Contenido.ObtenerCabeza();
                    while (nodo != null)
                    {
                        string nombreDron = nodo.Dato.NombreDron;
                        string accion;

                        if (nombreDron == dronEmisor)
                        {
                            if (seg < segundosViaje)
                                accion = posiciones.Obtener(dronEmisor) < alturaObjetivo ? "Subir" : "Bajar";
                            else
                                accion = "Emitir luz";
                        }
                        else
                        {
                            int posActual = posiciones.Obtener(nombreDron);
                            int proxPos = proximasPosiciones.ContieneClave(nombreDron)
                                ? proximasPosiciones.Obtener(nombreDron) : posActual;

                            if (posActual < proxPos) accion = "Subir";
                            else if (posActual > proxPos) accion = "Bajar";
                            else accion = "Esperar";
                        }

                        instante.Acciones.Agregar(new AccionDron(nombreDron, accion));
                        nodo = nodo.Siguiente;
                    }

                    resultado.Instrucciones.Agregar(instante);

                    // Actualizar posiciones
                    nodo = sistema.Contenido.ObtenerCabeza();
                    while (nodo != null)
                    {
                        string nombreDron = nodo.Dato.NombreDron;
                        if (nombreDron == dronEmisor && seg < segundosViaje)
                        {
                            int pos = posiciones.Obtener(dronEmisor);
                            posiciones.Poner(dronEmisor, pos < alturaObjetivo ? pos + 1 : pos - 1);
                        }
                        else if (nombreDron != dronEmisor && seg < totalSegundos - 1)
                        {
                            int posActual = posiciones.Obtener(nombreDron);
                            int proxPos = proximasPosiciones.ContieneClave(nombreDron)
                                ? proximasPosiciones.Obtener(nombreDron) : posActual;
                            if (posActual < proxPos) posiciones.Poner(nombreDron, posActual + 1);
                            else if (posActual > proxPos) posiciones.Poner(nombreDron, posActual - 1);
                        }
                        nodo = nodo.Siguiente;
                    }
                }

                posiciones.Poner(dronEmisor, alturaObjetivo);
                string letra = sistema.ObtenerLetra(dronEmisor, alturaObjetivo);
                mensajeRecibido += letra ?? "?";
            }

            resultado.TiempoOptimo = tiempoActual;
            resultado.MensajeRecibido = mensajeRecibido;
            return resultado;
        }

        private Mapa<string, int> ObtenerProximasPosiciones(
            Mensaje mensaje, SistemaDronesModelo sistema,
            int indiceActual, Mapa<string, int> posicionesActuales)
        {
            Mapa<string, int> proximas = new Mapa<string, int>();
            Nodo<Par<string, int>> nodo = posicionesActuales.ObtenerPares().ObtenerCabeza();
            while (nodo != null)
            {
                proximas.Poner(nodo.Dato.Clave, nodo.Dato.Valor);
                nodo = nodo.Siguiente;
            }

            string dronEmisorActual = mensaje.Instrucciones.Obtener(indiceActual).NombreDron;
            for (int j = indiceActual + 1; j < mensaje.Instrucciones.Tamanio; j++)
            {
                Instruccion sig = mensaje.Instrucciones.Obtener(j);
                if (sig.NombreDron != dronEmisorActual &&
                    proximas.Obtener(sig.NombreDron) == posicionesActuales.Obtener(sig.NombreDron))
                {
                    proximas.Poner(sig.NombreDron, sig.Altura);
                }
            }
            return proximas;
        }
    }
}
