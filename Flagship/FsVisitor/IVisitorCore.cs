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
        void UpdateContex(IDictionary<string, string> context);
        void UpdateContex(IDictionary<string, double> context);
        void UpdateContex(IDictionary<string, bool> context);
        void UpdateContex(string key, string value);
        void UpdateContex(string key, double value);
        void UpdateContex(string key, bool value);
        void ClearContext();

        Task FetchFlags();

        Task SendHit(HitAbstract hit);
    }
}
