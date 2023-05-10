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
        public string AnonymoudId { get; set; }
        public IDictionary<string, object> Context { get ; set ; }

        public ExposedVisitor(string id, string anonymoudId, IDictionary<string, object> context)
        {
            Id = id;
            AnonymoudId = anonymoudId;
            Context = context;
        }
    }
}
