using Microsoft.AspNetCore.Mvc;
using ProyectoServiciosWeb.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ProyectoServiciosWeb.Controllers
{

    public class ECommerceController : Controller
    {

        private readonly IConfiguration _config;


        public ECommerceController(IConfiguration config)
        {
            _config = config;

        }

        public IActionResult Index()
        {
           
            return View();
        }

        IEnumerable<Producto> inventario()
        {
            //Creamos un ListOF del tipo Cliente
            List<Producto> temporal = new List<Producto>();
            SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"].ToString());


            //Definimos un SqlCommand y su CommandType 
            SqlCommand cmd = new SqlCommand("usp_productos_ecommerce", cn);
            cmd.CommandType = CommandType.StoredProcedure;


            //Abrimos conexión
            cn.Open();
            //Ejecutamos el SqlCommand
            SqlDataReader dr = cmd.ExecuteReader();
            //Recuperamos los valores del SqlDataReader
            while (dr.Read())
            {
                Producto reg = new Producto();

                reg.idproducto = dr.GetInt32("IDPRODUCTO");
                reg.descripcion = dr.GetString("NOMBREPRODUCTO");
                reg.umedida = dr.GetString("UMEDIDA");
                reg.precio = dr.GetDecimal("PRECIOUNIDAD");
                reg.stock = dr.GetInt32("UNIDADES");


                temporal.Add(reg);
            }
            //Cerramos el SqlDataReader y la conexión a la BD
            dr.Close();
            cn.Close();



            return temporal;
        }


        public IActionResult Portal()
        {

            // Verifica si "canasta" existe en la sesión
            if (HttpContext.Session.GetString("canasta") == null)
            {
                // Si no existe, crea una nueva lista vacía de Registro y la almacena en la sesión
                HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(new List<Registro>()));
            }

            //ENVIO A LA VISTA LA LISTA DE LOS PRODUCTOS
            return View(inventario());
        }

        Producto buscar(int id)
        {
            return inventario().FirstOrDefault(x => x.idproducto == id);
        }

        public IActionResult Agregar(int id = 0)
        {
            //buscar el producto por id
            Producto reg = buscar(id);
            //si no lo encontro direccionar al Portal, sino enviar el reg a la Vista
            if (reg == null)
                return RedirectToAction("Portal");
            else
                return View(reg);
        }


        [HttpPost]
        public IActionResult Agregar(int id, int cantidad)
        {
            string canasta = HttpContext.Session.GetString("canasta");

            //deserializando el contenido del Session canasta a tipo lista de registro
            List<Registro> auxiliar =
              JsonConvert.DeserializeObject<List<Registro>>(canasta);
            //busqueda del producto en auxiliar
            Registro item = auxiliar.FirstOrDefault(x => x.idproducto == id);
            if (item != null)
                item.cantidad += cantidad;
            else
            {
                Producto reg = buscar(id);
                item = new Registro
                {
                    idproducto = id,
                    descripcion = reg.descripcion,
                    umedida = reg.umedida,
                    precio = reg.precio,
                    cantidad = cantidad,
                };
                auxiliar.Add(item);
            }

            HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(auxiliar));


            ViewBag.mensaje = $"Se ha agregado el producto {item.descripcion} la cantidad de {item.cantidad} unidad";
            return View(buscar(id));
        }


        public IActionResult Canasta()
        {

            string canasta = HttpContext.Session.GetString("canasta");

            if (string.IsNullOrEmpty(canasta))
            {
                // Si "canasta" no existe en la sesión, redirecciona a "Portal"
                return RedirectToAction("Portal");
            }
            else
            {
                // Si "canasta" existe, deserializa y envía los datos a la vista
                List<Registro> canastaView = JsonConvert.DeserializeObject<List<Registro>>(canasta);
                return View(canastaView);
            }


        }

        public IActionResult Actualizar(int idproducto, int cantidad)
        {
            // Obtiene la sesión "canasta" como string y la deserializa
            string sessionData = HttpContext.Session.GetString("canasta");
            List<Registro> auxiliar = JsonConvert.DeserializeObject<List<Registro>>(sessionData);

            // Encuentra el producto en la lista
            Registro item = auxiliar.FirstOrDefault(x => x.idproducto == idproducto);
            if (item != null)
            {
                // Actualiza la cantidad
                item.cantidad = cantidad;
                // Serializa la lista actualizada y la guarda en la sesión
                HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(auxiliar));
            }

            return RedirectToAction("Canasta");
        }

        public IActionResult Delete(int id)
        {
            // Obtiene la sesión "canasta" como string y la deserializa
            string sessionData = HttpContext.Session.GetString("canasta");
            List<Registro> auxiliar = JsonConvert.DeserializeObject<List<Registro>>(sessionData);

            // Elimina el registro por idproducto
            auxiliar.RemoveAll(x => x.idproducto == id);

            // Serializa la lista actualizada y la guarda en la sesión
            HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(auxiliar));

            return RedirectToAction("Canasta");
        }

    }
}
