using System;
using System.Web.Mvc;

namespace Proxy.Controllers {
    public class TokenPassThroughProviderFactory : ValueProviderFactory {
        private readonly string _token;
        private readonly string _contentType;
        private readonly JsonValueProviderFactory _existing;

        public TokenPassThroughProviderFactory(
            string token, 
            string contentType, 
            JsonValueProviderFactory existing) {
            _token = token;
            _contentType = contentType;
            _existing = existing;
        }

        public override IValueProvider GetValueProvider(ControllerContext controllerContext) {
            if (!controllerContext.HttpContext.Request.ContentType.StartsWith(_contentType, StringComparison.OrdinalIgnoreCase)) {
                return null;
            }
            if (controllerContext.RouteData.DataTokens.ContainsKey(_token)) {
                return null;
            }
            return _existing.GetValueProvider(controllerContext);
        }
    }
}