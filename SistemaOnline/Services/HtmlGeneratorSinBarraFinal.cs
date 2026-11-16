using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace SistemaOnline.Services
{
    // El campo oculto del token antifalsificación que ASP.NET Core añade a cada formulario se
    // escribe como <input ... />. En HTML la barra final de los elementos vacíos no tiene efecto
    // y el validador del W3C la señala, así que aquí se genera el mismo campo sin ella. El token,
    // su nombre y la cookie asociada siguen saliendo de IAntiforgery, por lo que la validación
    // ([ValidateAntiForgeryToken]) funciona igual.
    public class HtmlGeneratorSinBarraFinal : DefaultHtmlGenerator
    {
        private readonly IAntiforgery _antiforgery;
        private readonly HtmlEncoder _htmlEncoder;

        public HtmlGeneratorSinBarraFinal(
            IAntiforgery antiforgery,
            IOptions<MvcViewOptions> optionsAccessor,
            IModelMetadataProvider metadataProvider,
            IUrlHelperFactory urlHelperFactory,
            HtmlEncoder htmlEncoder,
            ValidationHtmlAttributeProvider validationAttributeProvider)
            : base(antiforgery, optionsAccessor, metadataProvider, urlHelperFactory, htmlEncoder, validationAttributeProvider)
        {
            _antiforgery = antiforgery;
            _htmlEncoder = htmlEncoder;
        }

        public override IHtmlContent GenerateAntiforgery(ViewContext viewContext)
        {
            ArgumentNullException.ThrowIfNull(viewContext);

            // Igual que la implementación base: un solo token por formulario.
            var formContext = viewContext.FormContext;
            if (formContext.CanRenderAtEndOfForm)
            {
                if (formContext.HasAntiforgeryToken)
                {
                    return HtmlString.Empty;
                }
                formContext.HasAntiforgeryToken = true;
            }

            var tokens = _antiforgery.GetAndStoreTokens(viewContext.HttpContext);
            return new HtmlString(
                $"<input name=\"{_htmlEncoder.Encode(tokens.FormFieldName)}\" type=\"hidden\" value=\"{_htmlEncoder.Encode(tokens.RequestToken ?? string.Empty)}\">");
        }
    }
}