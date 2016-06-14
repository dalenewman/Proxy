using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;

namespace Proxy.Models {
    public class ProxyPartRecord : ContentPartRecord {

        [StringLength(255)]
        public virtual string ServiceUrl { get; set; }

        public virtual bool ForwardHeaders { get; set; }
        
    }
}