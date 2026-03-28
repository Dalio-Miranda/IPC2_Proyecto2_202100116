using Microsoft.AspNetCore.Mvc;
using SistemaDrones.Logica;
using SistemaDrones.Modelos;
using SistemaDrones.TDAs;

namespace SistemaDrones.Controllers
{
    public class HomeController : Controller
    {
        private static ListaEnlazada<Dron> listaDrones = new ListaEnlazada<Dron>();
        private static ListaEnlazada<SistemaDronesModelo> listaSistemas = new ListaEnlazada<SistemaDronesModelo>();
        private static ListaEnlazada<Mensaje> listaMensajes = new ListaEnlazada<Mensaje>();
        private static GestorXML gestorXML = new GestorXML();
        private static Simulador simulador = new Simulador();

        // PÁGINA PRINCIPAL
        public IActionResult Index()
        {
            return View();
        }

        // CARGAR XML DE ENTRADA
        [HttpPost]
        public IActionResult CargarXML(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Por favor selecciona un archivo XML válido.";
                return RedirectToAction("Index");
            }

            string rutaTemporal = Path.Combine(Path.GetTempPath(), archivo.FileName);
            using (var stream = new FileStream(rutaTemporal, FileMode.Create))
                archivo.CopyTo(stream);

            gestorXML.CargarXML(rutaTemporal, listaDrones, listaSistemas, listaMensajes);
            TempData["Exito"] = "Archivo XML cargado correctamente.";
            return RedirectToAction("Index");
        }

        // GENERAR XML DE SALIDA
        public IActionResult GenerarSalida()
        {
            ListaEnlazada<ResultadoSimulacion> resultados = new ListaEnlazada<ResultadoSimulacion>();
            for (int i = 0; i < listaMensajes.Tamanio; i++)
            {
                Mensaje msg = listaMensajes.Obtener(i);
                SistemaDronesModelo sistema = listaSistemas.Buscar(s => s.Nombre == msg.NombreSistemaDrones);
                if (sistema != null)
                    resultados.Agregar(simulador.Simular(msg, sistema));
            }

            string rutaSalida = Path.Combine(Path.GetTempPath(), "salida.xml");
            gestorXML.GenerarSalida(rutaSalida, resultados);

            byte[] bytes = System.IO.File.ReadAllBytes(rutaSalida);
            return File(bytes, "application/xml", "salida.xml");
        }

        // GESTIÓN DE DRONES
        public IActionResult Drones()
        {
            listaDrones.OrdenarAlfabeticamente((a, b) =>
                string.Compare(a.Nombre, b.Nombre, System.StringComparison.OrdinalIgnoreCase));
            ViewBag.Drones = listaDrones;
            return View();
        }

        [HttpPost]
        public IActionResult AgregarDron(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "El nombre del dron no puede estar vacío.";
                return RedirectToAction("Drones");
            }
            if (listaDrones.Existe(d => d.Nombre == nombre))
            {
                TempData["Error"] = $"Ya existe un dron con el nombre '{nombre}'.";
                return RedirectToAction("Drones");
            }
            listaDrones.Agregar(new Dron(nombre));
            TempData["Exito"] = $"Dron '{nombre}' agregado correctamente.";
            return RedirectToAction("Drones");
        }

        // GESTIÓN DE SISTEMAS DE DRONES
        public IActionResult Sistemas()
        {
            ViewBag.Sistemas = listaSistemas;
            return View();
        }

        // GESTIÓN DE MENSAJES
        public IActionResult Mensajes()
        {
            listaMensajes.OrdenarAlfabeticamente((a, b) =>
                string.Compare(a.Nombre, b.Nombre, System.StringComparison.OrdinalIgnoreCase));
            ViewBag.Mensajes = listaMensajes;
            return View();
        }

        public IActionResult VerMensaje(string nombre)
        {
            Mensaje mensaje = listaMensajes.Buscar(m => m.Nombre == nombre);
            if (mensaje == null)
            {
                TempData["Error"] = "Mensaje no encontrado.";
                return RedirectToAction("Mensajes");
            }

            SistemaDronesModelo sistema = listaSistemas.Buscar(s => s.Nombre == mensaje.NombreSistemaDrones);
            if (sistema == null)
            {
                TempData["Error"] = "Sistema de drones no encontrado.";
                return RedirectToAction("Mensajes");
            }

            ResultadoSimulacion resultado = simulador.Simular(mensaje, sistema);
            ViewBag.Resultado = resultado;
            ViewBag.Sistema = sistema;
            return View();
        }

        // GRAPHVIZ - Sistema de drones
        public IActionResult GraficarSistema(string nombre)
        {
            SistemaDronesModelo sistema = listaSistemas.Buscar(s => s.Nombre == nombre);
            if (sistema == null) return RedirectToAction("Sistemas");

            string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "graficos");
            GestorGraphviz graphviz = new GestorGraphviz();
            graphviz.GenerarSistemaDrones(sistema, carpeta);

            ViewBag.ImagenUrl = $"/graficos/sistema_{nombre}.png?t={System.DateTime.Now.Ticks}";
            ViewBag.Nombre = nombre;
            return View();
        }

        // GRAPHVIZ - Mensaje
        public IActionResult GraficarMensaje(string nombre)
        {
            Mensaje mensaje = listaMensajes.Buscar(m => m.Nombre == nombre);
            if (mensaje == null) return RedirectToAction("Mensajes");

            SistemaDronesModelo sistema = listaSistemas.Buscar(s => s.Nombre == mensaje.NombreSistemaDrones);
            if (sistema == null) return RedirectToAction("Mensajes");

            ResultadoSimulacion resultado = simulador.Simular(mensaje, sistema);
            string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "graficos");
            GestorGraphviz graphviz = new GestorGraphviz();
            graphviz.GenerarMensaje(resultado, carpeta);

            ViewBag.ImagenUrl = "/graficos/mensaje_" + nombre + ".png?t=" + System.DateTime.Now.Ticks;
            ViewBag.Nombre = nombre;
            return View();
        }

        // AYUDA
        public IActionResult Ayuda()
        {
            return View();
        }
    }
}