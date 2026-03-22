namespace SistemaDrones.TDAs
{
    // Lista enlazada simple genérica - TDA propio (NO usa List<> de C#)
    public class ListaEnlazada<T>
    {
        private Nodo<T> cabeza;
        private int tamanio;

        public ListaEnlazada()
        {
            cabeza = null;
            tamanio = 0;
        }

        public int Tamanio => tamanio;
        public bool EstaVacia => cabeza == null;

        // Agregar al final
        public void Agregar(T dato)
        {
            Nodo<T> nuevo = new Nodo<T>(dato);
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
            tamanio++;
        }

        // Obtener elemento por índice (0-based)
        public T Obtener(int indice)
        {
            if (indice < 0 || indice >= tamanio)
                throw new System.Exception("Índice fuera de rango");
            Nodo<T> actual = cabeza;
            for (int i = 0; i < indice; i++)
                actual = actual.Siguiente;
            return actual.Dato;
        }

        // Verificar si existe un elemento con una condición
        public bool Existe(System.Func<T, bool> condicion)
        {
            Nodo<T> actual = cabeza;
            while (actual != null)
            {
                if (condicion(actual.Dato)) return true;
                actual = actual.Siguiente;
            }
            return false;
        }

        // Buscar y retornar el primer elemento que cumpla la condición
        public T Buscar(System.Func<T, bool> condicion)
        {
            Nodo<T> actual = cabeza;
            while (actual != null)
            {
                if (condicion(actual.Dato)) return actual.Dato;
                actual = actual.Siguiente;
            }
            return default(T);
        }

        // Eliminar primer elemento que cumpla condición
        public bool Eliminar(System.Func<T, bool> condicion)
        {
            if (cabeza == null) return false;

            if (condicion(cabeza.Dato))
            {
                cabeza = cabeza.Siguiente;
                tamanio--;
                return true;
            }

            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null)
            {
                if (condicion(actual.Siguiente.Dato))
                {
                    actual.Siguiente = actual.Siguiente.Siguiente;
                    tamanio--;
                    return true;
                }
                actual = actual.Siguiente;
            }
            return false;
        }

        // Ordenar alfabéticamente usando una función comparadora (Bubble Sort)
        public void OrdenarAlfabeticamente(System.Func<T, T, int> comparador)
        {
            if (tamanio <= 1) return;
            bool cambio;
            do
            {
                cambio = false;
                Nodo<T> actual = cabeza;
                while (actual.Siguiente != null)
                {
                    if (comparador(actual.Dato, actual.Siguiente.Dato) > 0)
                    {
                        // Intercambiar datos
                        T temp = actual.Dato;
                        actual.Dato = actual.Siguiente.Dato;
                        actual.Siguiente.Dato = temp;
                        cambio = true;
                    }
                    actual = actual.Siguiente;
                }
            } while (cambio);
        }

        // Obtener el nodo cabeza (para recorrer externamente si se necesita)
        public Nodo<T> ObtenerCabeza() => cabeza;
    }
}
