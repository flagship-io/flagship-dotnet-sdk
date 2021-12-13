using Flagship.Config;
using Flagship.FsFlag;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public class Visitor : IVisitor
    {
        public string VisitorId { get => _visitorDelegate.VisitorId; set => _visitorDelegate.VisitorId = value; }

        public string AnonymousId => _visitorDelegate.AnonymousId;

        public ICollection<FlagDTO> Flags => _visitorDelegate.Flags;

        public bool HasConsented => _visitorDelegate.HasConsented;

        public FlagshipConfig Config => _visitorDelegate.Config;

        public IDictionary<string, object> Context => _visitorDelegate.Context;

        private readonly VisitorDelegateAbstract _visitorDelegate;

        internal Visitor(VisitorDelegateAbstract visitorDelegate)
        {
            _visitorDelegate = visitorDelegate;
        }

        public void ClearContext()
        {
            _visitorDelegate.ClearContext();
        }

        public Task FetchFlags()
        {
            return _visitorDelegate.FetchFlags();
        }

        public IFlag<T> GetFlag<T>(string key, T defaultValue)
        {
            return _visitorDelegate.GetFlag(key, defaultValue); 
        }

        public void SetConsent(bool hasConsented)
        {
             _visitorDelegate.SetConsent(hasConsented);   
        }

        public void UpdateContex(IDictionary<string, object> context)
        {
            _visitorDelegate.UpdateContex(context);
        }

        IFlag<T> IVisitor.GetFlag<T>(string key, T defaultValue)
        {
            throw new NotImplementedException();
        }
    }
}
