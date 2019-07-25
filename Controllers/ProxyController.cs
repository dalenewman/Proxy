using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Proxy.Models;

namespace Proxy.Controllers {

   [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
   public class ProxyController : Controller {

      private static readonly HashSet<string> _restrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "Accept",
            "Accept-Encoding",
            "Accept-Language",
            "Connection",
            "Content-Length",
            "Content-Type",
            "Date",
            "Expect",
            "Host",
            "If-Modified-Since",
            "Range",
            "Referer",
            "Transfer-Encoding",
            "User-Agent",
            "Proxy-Connection"
        };

      private static Regex _proxyTrimmer;
      private static char[] _splitter = new[] { ',' };

      private ProxyPart _proxy;
      private readonly IOrchardServices _services;
      private Localizer T { get; set; }
      private ILogger Logger { get; set; }

      public ProxyController(IOrchardServices services) {
         _services = services;
         T = NullLocalizer.Instance;
         Logger = NullLogger.Instance;
      }

      [AcceptVerbs("GET", "HEAD", "POST", "PUT", "DELETE")]
      [ValidateInput(false)]
      public void Index(int proxyId) {

         Response.BufferOutput = true; //to be able to set content type

         _proxy = _services.ContentManager.Get(proxyId).As<ProxyPart>();

         if (_proxy == null) {
            Response.StatusCode = 404;
            Response.End();
            return;
         }

         if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, _proxy, T("Permission to use this proxy is denied"))) {
            Response.StatusCode = 401;
            Response.End();
            return;
         }

         if (HttpContext.Request.Url == null) {
            Response.StatusCode = 400;
            Response.End();
            return;
         }

         if (_proxyTrimmer == null) {
            _proxyTrimmer = new Regex(string.Format(@"{0}/Proxy/\d*", Url.Content("~").TrimEnd('/')), RegexOptions.Compiled);
         }

         var url = _proxyTrimmer.Replace(HttpContext.Request.Url.PathAndQuery, string.Empty);

         RelayContent(CombinePath(_proxy.ServiceUrl, url), Request, Response, Logger, _proxy.ForwardHeaders);
      }

      private static string CombinePath(string proxyUrl, string requestedUrl) {
         if (proxyUrl.EndsWith("/") && requestedUrl.StartsWith("/")) {
            return proxyUrl + requestedUrl.TrimStart(new[] { '/' });
         }
         return proxyUrl + requestedUrl;
      }

      private static void RelayContent(
          string url,
          HttpRequestBase request,
          HttpResponseBase response,
          ILogger logger,
          bool forwardHeaders
      ) {

         var uri = new Uri(url);
         var serviceRequest = WebRequest.Create(uri);

         serviceRequest.Method = request.HttpMethod;
         serviceRequest.ContentType = request.ContentType;

         if (forwardHeaders) {
            foreach (var key in request.Headers.AllKeys.Where(key => !_restrictedHeaders.Contains(key))) {
               serviceRequest.Headers.Add(key, request.Headers[key]);
            }
         }

         //pull in input
         if (serviceRequest.Method != "GET") {

            request.InputStream.Position = 0;

            var inStream = request.InputStream;
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

         //get and push output
         try {
            using (var resourceResponse = (HttpWebResponse)serviceRequest.GetResponse()) {

               var responseETag = resourceResponse.Headers["ETag"];
               if (responseETag != null) {
                  response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                  response.Cache.SetETag(responseETag);

                  var requestedETag = serviceRequest.Headers["If-None-Match"];
                  if (requestedETag == responseETag) {
                     response.StatusCode = 304;  //Not Modified
                     response.End();
                     return;
                  }
               }

               using (var resourceStream = resourceResponse.GetResponseStream()) {
                  resourceStream.CopyTo(response.OutputStream);
               }
               var cacheControl = resourceResponse.Headers["Cache-Control"];

               if (cacheControl != null) {
                  var split = cacheControl.Split(_splitter, StringSplitOptions.RemoveEmptyEntries);
                  foreach(var entry in split) {
                     var value = entry.Trim().ToLower();
                     switch (value) {
                        case "no-cache":
                           response.Cache.SetCacheability(HttpCacheability.NoCache);
                           break;
                        case "public":
                           response.Cache.SetCacheability(HttpCacheability.Public);
                           break;
                        default:
                           response.Cache.AppendCacheExtension(value);
                           break;
                     }
                     
                  }
               }

               response.ContentType = uri.IsFile ? MimeTypeMap.GetMimeType(Path.GetExtension(uri.LocalPath)) : resourceResponse.ContentType;
            }
         } catch (WebException ex) {
            if (ex.Status == WebExceptionStatus.ProtocolError) {
               var resourceResponse = ex.Response as HttpWebResponse;
               if (resourceResponse != null) {
                  response.StatusCode = (int)resourceResponse.StatusCode;
                  response.ContentType = resourceResponse.ContentType;
               } else {
                  response.StatusCode = 500;
                  response.ContentType = ex.Response.ContentType;
                  logger.Error(ex, "Proxy module protocol error: {0}", ex.Message);
               }
            } else {
               response.StatusCode = 500;
               response.ContentType = ex.Response.ContentType;
               logger.Error(ex, "Proxy module error: {0}", ex.Message);
            }
         } finally {
            response.Flush();
            response.End();
         }
      }
   }

}