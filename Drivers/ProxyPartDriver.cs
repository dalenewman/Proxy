using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Proxy.Models;

namespace Proxy.Drivers {
    public class ProxyPartDriver : ContentPartDriver<ProxyPart> {

        protected override string Prefix {
            get { return "Proxy"; }
        }

        //GET
        protected override DriverResult Editor(ProxyPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Proxy_Edit", () => shapeHelper
                .EditorTemplate(TemplateName: "Parts/Proxy", Model: part, Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(ProxyPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override DriverResult Display(ProxyPart part, string displayType, dynamic shapeHelper) {

            if (displayType.StartsWith("Summary")) {
                return Combined(
                    ContentShape("Parts_Proxy_SummaryAdmin", () => shapeHelper.Parts_Proxy_SummaryAdmin(Part: part))
                );
            }

            return null;
        }


    }
}
