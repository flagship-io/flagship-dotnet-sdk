using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public interface IExposedVisitor
    {
        string Id { get; }
        string AnonymoudId { get; }
        IDictionary<string, object> Context { get; }
    }
}
