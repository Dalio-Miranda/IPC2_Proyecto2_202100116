namespace SistemaDrones.TDAs
{
    // Nodo genérico para la lista enlazada
    public class Nodo<T>
    {
        public T Dato { get; set; }
        public Nodo<T> Siguiente { get; set; }

        public Nodo(T dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }
}
