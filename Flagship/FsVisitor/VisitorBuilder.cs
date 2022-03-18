using Flagship.Config;
using Flagship.Enums;
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
        private readonly string _visitorId;
        private readonly IConfigManager _configManager;
        private readonly InstanceType _instanceType;


        private VisitorBuilder(IConfigManager configManager, string visitorId, InstanceType instanceType)
        {
            _visitorId = visitorId;
            _isAuthenticated = false;
            _hasConsented = true;
            _context = new Dictionary<string, object>();
            _configManager = configManager;
            _instanceType = instanceType;
        }

        internal static VisitorBuilder Builder(IConfigManager configManager, string visitorId, InstanceType instanceType)
        {
            return new VisitorBuilder(configManager, visitorId, instanceType);
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

        public VisitorBuilder WithContext(IDictionary<string, object> context)
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
            var visitor = new Visitor(visitorDelegate);
            if (_instanceType == InstanceType.SINGLE_INSTANCE)
            {
                Main.Fs.Visitor = visitor;
            }
            return visitor;
        }

    }
}
