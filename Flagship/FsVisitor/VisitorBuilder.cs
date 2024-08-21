using System.Collections.Generic;
using Flagship.Config;
using Flagship.Model;

namespace Flagship.FsVisitor
{
    public class VisitorBuilder
    {
        private bool _isAuthenticated;
        private readonly bool _hasConsented;
        private IDictionary<string, object> _context;
        private readonly string _visitorId;
        private readonly IConfigManager _configManager;
        private bool _shouldSaveInstance;
        private readonly SdkInitialData _sdkInitialData;

        private VisitorBuilder(
            IConfigManager configManager,
            string visitorId,
            bool hasConsented,
            SdkInitialData sdkInitialData = null
        )
        {
            _visitorId = visitorId;
            _isAuthenticated = false;
            _context = new Dictionary<string, object>();
            _configManager = configManager;
            _hasConsented = hasConsented;
            _sdkInitialData = sdkInitialData;
            _shouldSaveInstance = false;
        }

        internal static VisitorBuilder Builder(
            IConfigManager configManager,
            string visitorId,
            bool hasConsented,
            SdkInitialData sdkInitialData = null
        )
        {
            return new VisitorBuilder(configManager, visitorId, hasConsented, sdkInitialData);
        }

        /// <summary>
        /// Specify if the Visitor is authenticated or anonymous.
        /// </summary>
        /// <param name="isAuthenticated">True for an authenticated visitor, false for an anonymous visitor.</param>
        /// <returns></returns>
        public VisitorBuilder SetIsAuthenticated(bool isAuthenticated)
        {
            _isAuthenticated = isAuthenticated;
            return this;
        }

        /// <summary>
        /// Specifies whether the newly created visitor instance should be saved into Flagship.
        /// </summary>
        /// <param name="value">
        /// If set to true, the newly created visitor instance will be saved into Flagship.
        /// If set to false, the newly created visitor instance will not be saved, but simply returned.
        /// </param>
        /// <returns></returns>
        public VisitorBuilder SetShouldSaveInstance(bool value)
        {
            _shouldSaveInstance = value;
            return this;
        }

        /// <summary>
        /// Specify Visitor initial context key / values used for targeting.
        /// </summary>
        /// <param name="context">visitor initial context.</param>
        /// <returns></returns>
        public VisitorBuilder SetContext(IDictionary<string, object> context)
        {
            if (context != null)
            {
                _context = context;
            }
            return this;
        }

        /// <summary>
        /// Complete the Visitor Creation process
        /// </summary>
        /// <returns></returns>
        public IVisitor Build()
        {
            var visitorDelegate = new VisitorDelegate(
                _visitorId,
                _isAuthenticated,
                _context,
                _hasConsented,
                _configManager,
                _sdkInitialData
            );
            var visitor = new Visitor(visitorDelegate);
            Main.Fs.Visitor = null;
            if (_shouldSaveInstance)
            {
                Main.Fs.Visitor = visitor;
            }
            return visitor;
        }
    }
}
