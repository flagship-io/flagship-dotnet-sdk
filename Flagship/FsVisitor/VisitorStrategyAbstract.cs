using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public abstract class VisitorStrategyAbstract : IVisitorCore
    {
        protected VisitorDelegateAbstract Visitor { get; set; }

        protected FlagshipConfig Config => Visitor.Config;

        protected ITrackingManager TrackingManager => Visitor.ConfigManager.TrackingManager;

        protected IDecisionManager DecisionManager => Visitor.ConfigManager.DecisionManager;

        public VisitorStrategyAbstract(VisitorDelegateAbstract visitor)
        {
            Visitor = visitor;
        }

        public async void SetConsent(bool hasConsented)
        {
            const string method = "setConsent";
            try
            {
                await TrackingManager.SendConsentHit(Visitor);
            }
            catch (Exception ex)
            {
                Utils.Utils.LogError(Config, ex.Message, method);
            }
        }

        abstract public void UpdateContex(IDictionary<string, object> context);

        abstract public void ClearContext();

        abstract public Task FetchFlags();
    }
}
