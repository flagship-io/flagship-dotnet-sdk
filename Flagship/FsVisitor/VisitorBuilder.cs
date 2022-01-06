using Flagship.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public class VisitorBuilder
    {
        private bool _isAuthenticated;
        private bool _hasConsented;
        private IDictionary<string, object> _context;
        private string _visitorId;
        private IConfigManager _configManager;


        private VisitorBuilder(IConfigManager configManager, string visitorId)
        {
            _visitorId = visitorId;
            _isAuthenticated = false;
            _hasConsented = true;
            _context = new Dictionary<string, object>();
            _configManager = configManager;
        }

        internal static VisitorBuilder Builder(IConfigManager configManager, string visitorId)
        {
            return new VisitorBuilder(configManager, visitorId);
        }

        public VisitorBuilder IsAuthenticated(bool isAuthenticated)
        {
            _isAuthenticated=isAuthenticated;   
            return this;
        }

        public VisitorBuilder HasConsented(bool hasConsented)
        {
            _hasConsented = hasConsented;
            return this;
        }

        public VisitorBuilder Context(IDictionary<string, object> context)
        {
            if (context!=null)
            {
                _context = context;
            }
            return this;
        }

        public Visitor Build()
        {
            var visitorDelegate = new VisitorDelegate(_visitorId,_isAuthenticated,_context, _hasConsented,_configManager);
            return new Visitor(visitorDelegate);
        }

    }
}
