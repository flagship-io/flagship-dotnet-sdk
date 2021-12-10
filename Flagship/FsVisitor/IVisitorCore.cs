using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public interface IVisitorCore
    { 
        public void UpdateContex(IDictionary<string, object> context);
        public void ClearContext();

        public Task FetchFlags();

        public void SetConsent(bool hasConsented);
    }
}
