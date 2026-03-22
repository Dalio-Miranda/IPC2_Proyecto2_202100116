using SistemaDrones.TDAs;

namespace SistemaDrones.Modelos
{
    // ─────────────────────────────────────────
    // Representa un dron individual del sistema
    // ─────────────────────────────────────────
    public class Dron
    {
        public string Nombre { get; set; }
        public int PosicionActual { get; set; } // altura actual en metros (empieza en 0)

        public Dron(string nombre)
        {
            Nombre = nombre;
            PosicionActual = 0;
        }
    }

    // ─────────────────────────────────────────────────────────
    // Representa una altura dentro de un sistema de drones
    // Cada altura tiene una letra asociada para ese dron
    // ─────────────────────────────────────────────────────────
    public class EntradaAltura
    {
        public int Valor { get; set; }      // número de metros
        public string Letra { get; set; }   // letra que representa

        public EntradaAltura(int valor, string letra)
        {
            Valor = valor;
            Letra = letra;
        }
    }

    // ────────────────────────────────────────────────────────────────────────────
    // Contenido de un dron dentro de un sistema: qué letra tiene a cada altura
    // ────────────────────────────────────────────────────────────────────────────
    public class ContenidoDron
    {
        public string NombreDron { get; set; }
        public ListaEnlazada<EntradaAltura> Alturas { get; set; }

        public ContenidoDron(string nombreDron)
        {
            NombreDron = nombreDron;
            Alturas = new ListaEnlazada<EntradaAltura>();
        }

        // Devuelve la letra en una altura dada, o null si no existe
        public string ObtenerLetra(int altura)
        {
            EntradaAltura entrada = Alturas.Buscar(a => a.Valor == altura);
            return entrada?.Letra;
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Sistema de drones: tabla completa de drones x alturas x letras
    // ────────────────────────────────────────────────────────────────
    public class SistemaDronesModelo
    {
        public string Nombre { get; set; }
        public int AlturaMaxima { get; set; }
        public int CantidadDrones { get; set; }
        public ListaEnlazada<ContenidoDron> Contenido { get; set; }

        public SistemaDronesModelo(string nombre, int alturaMaxima, int cantidadDrones)
        {
            Nombre = nombre;
            AlturaMaxima = alturaMaxima;
            CantidadDrones = cantidadDrones;
            Contenido = new ListaEnlazada<ContenidoDron>();
        }

        // Busca la letra para un dron a una altura dada
        public string ObtenerLetra(string nombreDron, int altura)
        {
            ContenidoDron cd = Contenido.Buscar(c => c.NombreDron == nombreDron);
            return cd?.ObtenerLetra(altura);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Una instrucción: qué dron debe emitir luz y a qué altura
    // ─────────────────────────────────────────────────────────────
    public class Instruccion
    {
        public string NombreDron { get; set; }
        public int Altura { get; set; }

        public Instruccion(string nombreDron, int altura)
        {
            NombreDron = nombreDron;
            Altura = altura;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Mensaje: nombre, sistema de drones que usa, lista de instrucciones
    // ─────────────────────────────────────────────────────────────────────
    public class Mensaje
    {
        public string Nombre { get; set; }
        public string NombreSistemaDrones { get; set; }
        public ListaEnlazada<Instruccion> Instrucciones { get; set; }

        public Mensaje(string nombre, string nombreSistemaDrones)
        {
            Nombre = nombre;
            NombreSistemaDrones = nombreSistemaDrones;
            Instrucciones = new ListaEnlazada<Instruccion>();
        }
    }
}
