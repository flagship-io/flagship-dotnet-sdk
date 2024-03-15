using Flagship.Config;
using Flagship.Enums;
using Flagship.Model;
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
        private readonly bool _hasConsented;
        private IDictionary<string, object> _context;
        private readonly string _visitorId;
        private readonly IConfigManager _configManager;
        private  InstanceType _instanceType;
        private readonly SdkInitialData _sdkInitialData;


        private VisitorBuilder(IConfigManager configManager, string visitorId, bool hasConsented, SdkInitialData sdkInitialData = null)
        {
            _visitorId = visitorId;
            _isAuthenticated = false;
            _context = new Dictionary<string, object>();
            _configManager = configManager;
            _hasConsented = hasConsented;
            _sdkInitialData = sdkInitialData;
            _instanceType = InstanceType.NEW_INSTANCE;
        }

        internal static VisitorBuilder Builder(IConfigManager configManager, string visitorId, bool hasConsented, SdkInitialData sdkInitialData = null)
        {
            return new VisitorBuilder(configManager, visitorId, hasConsented, sdkInitialData);
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
        /// If NEW_INSTANCE, the newly created visitor instance won't be saved and will simply be returned. Otherwise, the newly created visitor instance will be returned and saved into the Flagship.
        /// </summary>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        public VisitorBuilder WithInstanceType(InstanceType instanceType)
        {
            _instanceType = instanceType;
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
            var visitorDelegate = new VisitorDelegate(_visitorId, _isAuthenticated, _context, _hasConsented, _configManager, _sdkInitialData);
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
