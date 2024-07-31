using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal class ExposedVisitor : IExposedVisitor
    {
        public string Id { get ;  set; }
        public string AnonymousId { get; set; }
        public IDictionary<string, object> Context { get ; set ; }

        internal ExposedVisitor(string id, string anonymousId, IDictionary<string, object> context)
        {
            Id = id;
            AnonymousId = anonymousId;
            Context = context;
        }
    }
}
