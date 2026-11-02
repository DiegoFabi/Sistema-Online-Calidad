using Microsoft.AspNetCore.Mvc.Razor;

namespace SistemaOnline.Views.Shared
{
    // Clase base de las vistas parciales de formulario compartidas entre Nuevo y Editar
    // (_ProductoForm, _ContratoForm, _ReservacionForm, _PedidoTotales, _PedidoDetalles).
    // Centraliza lo que antes se copiaba en cada parcial: el helper Cls() y el modo Nuevo/Editar.
    public abstract class FormularioPage<TModel> : RazorPage<TModel>
    {
        private const string ClaseCampo = "w-full h-11 px-md bg-surface-container-low border border-transparent rounded-lg focus:ring-2 focus:ring-primary/20 transition-all font-body-md text-body-md text-on-surface";
        private const string ClaseCampoConError = "w-full h-11 px-md bg-surface-container-low border border-error rounded-lg focus:ring-2 focus:ring-error/20 transition-all font-body-md text-body-md text-on-surface";

        // true cuando la vista contenedora es Nuevo (fija ViewData["EsNuevo"] = true).
        protected bool EsNuevo => ViewData["EsNuevo"] is true;

        // Clases CSS del campo: borde de error si el ModelState tiene errores para ese campo.
        protected string Cls(string campo) =>
            ViewData.ModelState.TryGetValue(campo, out var estado) && estado.Errors.Count > 0
                ? ClaseCampoConError
                : ClaseCampo;
    }
}