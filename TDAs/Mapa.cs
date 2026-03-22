namespace SistemaDrones.TDAs
{
    // Par clave-valor
    public class Par<K, V>
    {
        public K Clave { get; set; }
        public V Valor { get; set; }

        public Par(K clave, V valor)
        {
            Clave = clave;
            Valor = valor;
        }
    }

    // TDA Mapa propio (reemplaza Dictionary) - usa lista enlazada internamente
    public class Mapa<K, V>
    {
        private ListaEnlazada<Par<K, V>> lista;

        public Mapa()
        {
            lista = new ListaEnlazada<Par<K, V>>();
        }

        public int Tamanio => lista.Tamanio;

        // Agregar o actualizar valor
        public void Poner(K clave, V valor)
        {
            Par<K, V> existente = lista.Buscar(p => p.Clave.Equals(clave));
            if (existente != null)
                existente.Valor = valor;
            else
                lista.Agregar(new Par<K, V>(clave, valor));
        }

        // Obtener valor por clave
        public V Obtener(K clave)
        {
            Par<K, V> par = lista.Buscar(p => p.Clave.Equals(clave));
            if (par == null)
                throw new System.Exception($"Clave '{clave}' no encontrada en el mapa");
            return par.Valor;
        }

        // Verificar si existe una clave
        public bool ContieneClave(K clave)
        {
            return lista.Existe(p => p.Clave.Equals(clave));
        }

        // Obtener todos los pares (para recorrer)
        public ListaEnlazada<Par<K, V>> ObtenerPares() => lista;
    }
}
