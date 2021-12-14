using Flagship.Config;
using Flagship.FsFlag;
using Flagship.Model;
using Newtonsoft.Json.Linq;
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

        public void SetConsent(bool hasConsented)
        {
             _visitorDelegate.SetConsent(hasConsented);   
        }

        public void UpdateContex(IDictionary<string, object> context)
        {
            _visitorDelegate.UpdateContex(context);
        }

        public IFlag<string> GetFlag(string key, string defaultValue)
        {
            return _visitorDelegate.GetFlag(key, defaultValue);
        }

        public IFlag<double> GetFlag(string key, double defaultValue)
        {
            return _visitorDelegate.GetFlag(key, defaultValue);
        }

        public IFlag<bool> GetFlag(string key, bool defaultValue)
        {
            return _visitorDelegate.GetFlag(key, defaultValue);
        }

        public IFlag<JObject> GetFlag(string key, JObject defaultValue)
        {
            return _visitorDelegate.GetFlag(key, defaultValue);
        }

        public IFlag<JArray> GetFlag(string key, JArray defaultValue)
        {
            return _visitorDelegate.GetFlag(key, defaultValue);
        }
    }
}
