using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Proxy {

    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Priority = 5,
                    Route = new Route(
                        "Proxy/{proxyId}/{*path}",
                        new RouteValueDictionary { {"area", "Proxy"}, {"controller", "Proxy"}, {"action", "Index"} },
                        new RouteValueDictionary(),
                        new RouteValueDictionary { {"area", "Proxy"} },
                        new MvcRouteHandler()
                    )
                }
            };
        }

    }
}