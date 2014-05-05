using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Proxy.Models;

namespace Proxy.Controllers {

    public class ProxyController : Controller {

        protected Localizer T { get; set; }
        private readonly IOrchardServices _services;
        private static Regex _proxyTrimmer;
        private ProxyPart _proxy;

        public ProxyController(IOrchardServices services) {
            _services = services;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index(int proxyId) {
            _proxy = _services.ContentManager.Get(proxyId).As<ProxyPart>();

            if (_proxy == null) {
                return new HttpNotFoundResult();
            }

            if (!User.Identity.IsAuthenticated) {
                System.Web.Security.FormsAuthentication.RedirectToLoginPage(Request.RawUrl);
            }

            if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, _proxy, T("Permission to use this proxy is denied"))) {
                return new HttpUnauthorizedResult();
            }

            if (HttpContext.Request.Url == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (_proxyTrimmer == null) {
                _proxyTrimmer = new Regex(string.Format(@"{0}/Proxy/\d*", Url.Content("~").TrimEnd('/')), RegexOptions.Compiled);
            }

            var url = _proxyTrimmer.Replace(HttpContext.Request.Url.PathAndQuery, string.Empty);
            return Content(RelayContent(_proxy.ServiceUrl + url));
        }

        private string RelayContent(string url) {

            string content;
            var orchardRequest = Request;
            var serviceRequest = WebRequest.Create(url);

            serviceRequest.Method = orchardRequest.HttpMethod;
            serviceRequest.ContentType = orchardRequest.ContentType;

            if (serviceRequest.Method != "GET") {

                orchardRequest.InputStream.Position = 0;

                var inStream = orchardRequest.InputStream;
                Stream webStream = null;
                try {
                    //copy incoming request body to outgoing request
                    if (inStream != null && inStream.Length > 0) {
                        serviceRequest.ContentLength = inStream.Length;
                        webStream = serviceRequest.GetRequestStream();
                        inStream.CopyTo(webStream);
                    }
                } finally {
                    if (null != webStream) {
                        webStream.Flush();
                        webStream.Close();
                    }
                }
            }

            using (var response = (HttpWebResponse)serviceRequest.GetResponse()) {
                using (var stream = response.GetResponseStream()) {
                    content = new StreamReader(stream).ReadToEnd();
                }
                Response.ContentType = response.ContentType;
            }

            return content;
        }

    }

}