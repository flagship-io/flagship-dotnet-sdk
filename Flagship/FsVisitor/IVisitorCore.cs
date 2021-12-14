using Flagship.Hit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public interface IVisitorCore
    {
        void UpdateContex(IDictionary<string, object> context);
        void ClearContext();

        Task FetchFlags();

        Task SendHit(HitAbstract hit);
    }
}
