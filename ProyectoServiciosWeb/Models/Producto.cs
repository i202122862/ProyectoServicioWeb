using System.ComponentModel.DataAnnotations;

namespace ProyectoServiciosWeb.Models
{
    public class Producto
    {
        [Display(Name = "Codigo Producto")] public int idproducto { get; set; }
        [Display(Name = "Descripcion")] public string descripcion { get; set; }
        [Display(Name = "Unidad Medida")] public string umedida { get; set; }
        [Display(Name = "Precio Unitario")] public decimal precio { get; set; }
        [Display(Name = "Stock Diponible")] public int stock { get; set; }

    }
}
