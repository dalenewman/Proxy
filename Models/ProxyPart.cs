using System;
using Orchard.ContentManagement;

namespace Proxy.Models {
    public class ProxyPart : ContentPart<ProxyPartRecord> {
        public string ServiceUrl {
            get { return Record.ServiceUrl; }
            set { Record.ServiceUrl = value; }
        }

        public bool IsValid() {
            return !String.IsNullOrEmpty(ServiceUrl);
        }
    }
}