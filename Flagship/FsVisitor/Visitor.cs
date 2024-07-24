using Flagship.Config;
using Flagship.Delegate;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;
using System.Collections.Generic;
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

        public IFlagsStatus FlagsStatus => _visitorDelegate.FlagsStatus;

        private readonly VisitorDelegateAbstract _visitorDelegate;

        public event OnFlagStatusChangedDelegate OnFlagsStatusChanged
        {
            add => _visitorDelegate.OnFlagsStatusChanged += value;
            remove => _visitorDelegate.OnFlagsStatusChanged -= value;
        }

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

        public void SetConsent(bool hasConsented)
        {
             _visitorDelegate.SetConsent(hasConsented);   
        }

        public IFlag GetFlag(string key)
        {
            return _visitorDelegate.GetFlag(key);
        }

        public IFlagCollection GetFlags()
        {
            return _visitorDelegate.GetFlags();
        }

        public Task SendHit(HitAbstract hit)
        {
            return _visitorDelegate.SendHit(hit);
        }

        public void UpdateContext(IDictionary<string, object> context)
        {
            _visitorDelegate.UpdateContext(context);
        }

        public void UpdateContext(string key, string value)
        {
            _visitorDelegate.UpdateContext(key, value);  
        }

        public void UpdateContext(string key, double value)
        {
            _visitorDelegate.UpdateContext(key, value);
        }

        public void UpdateContext(string key, bool value)
        {
            _visitorDelegate.UpdateContext(key, value);
        }

        public void Authenticate(string visitorId)
        {
            _visitorDelegate.Authenticate(visitorId);
        }

        public void Unauthenticate()
        {
            _visitorDelegate.Unauthenticate();  
        }
    }
}
