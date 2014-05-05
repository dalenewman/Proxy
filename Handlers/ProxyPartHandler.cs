using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Proxy.Models;

namespace Proxy.Handlers {
    public class ProxyPartHandler : ContentHandler {

        public ProxyPartHandler(IRepository<ProxyPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var proxy = context.ContentItem.As<ProxyPart>();

            if (proxy == null)
                return;

            base.GetItemMetadata(context);
            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Proxy"},
                {"Controller", "Proxy"},
                {"Action", "Index"},
                {"proxyId", context.ContentItem.Id}
            };
        }
    }
}
