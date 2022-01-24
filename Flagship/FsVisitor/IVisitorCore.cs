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
        void UpdateContext(IDictionary<string, object> context);
        void UpdateContext(string key, string value);
        void UpdateContext(string key, double value);
        void UpdateContext(string key, bool value);
        void ClearContext();

        Task FetchFlags();

        Task SendHit(HitAbstract hit);

        void Authenticate(string visitorId);
        void Unauthenticate();
    }
}
