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

        /// <summary>
        /// Specify if the Visitor is authenticated or anonymous.
        /// </summary>
        /// <param name="isAuthenticated">True for an authenticated visitor, false for an anonymous visitor.</param>
        /// <returns></returns>
        public VisitorBuilder IsAuthenticated(bool isAuthenticated)
        {
            _isAuthenticated=isAuthenticated;   
            return this;
        }

        /// <summary>
        /// Specify if the Visitor has consented for personal data usage. When false some features will be deactivated, cache will be deactivated and cleared.
        /// </summary>
        /// <param name="hasConsented">Set to true when the visitor has consented, false otherwise.</param>
        /// <returns></returns>
        public VisitorBuilder HasConsented(bool hasConsented)
        {
            _hasConsented = hasConsented;
            return this;
        }

        /// <summary>
        /// Specify Visitor initial context key / values used for targeting.
        /// </summary>
        /// <param name="context">visitor initial context.</param>
        /// <returns></returns>
        public VisitorBuilder WithContext(IDictionary<string, object> context)
        {
            if (context!=null)
            {
                _context = context;
            }
            return this;
        }

        /// <summary>
        /// Complete the Visitor Creation process 
        /// </summary>
        /// <returns>Return an instance of \Flagship\Visitor\Visitor</returns>
        public Visitor Build()
        {
            var visitorDelegate = new VisitorDelegate(_visitorId,_isAuthenticated,_context, _hasConsented,_configManager);
            var visitor = new Visitor(visitorDelegate);
            Main.Fs.Visitor = null;
            if (_instanceType == InstanceType.SINGLE_INSTANCE)
            {
                Main.Fs.Visitor = visitor;
            }
            return visitor;
        }

    }
}
