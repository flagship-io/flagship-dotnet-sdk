using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Flagship.Api;
using Flagship.Config;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Model;

namespace Flagship.Decision
{
    internal abstract class DecisionManager : IDecisionManager
    {
        public event StatusChangedDelegate StatusChange;
        protected bool _isPanic = false;
        public TroubleshootingData TroubleshootingData { get; set; }

        public FlagshipConfig Config { get; set; }

        public string LastBucketingTimestamp { get; set; }

        public virtual bool IsPanic
        {
            get => _isPanic;
            protected set
            {
                _isPanic = value;
                StatusChange?.Invoke(
                    _isPanic ? FSSdkStatus.SDK_PANIC : FSSdkStatus.SDK_INITIALIZED
                );
            }
        }

        protected void UpdateStatus(FSSdkStatus sdkStatus)
        {
            StatusChange?.Invoke(sdkStatus);
        }

        public HttpClient HttpClient { get; set; }

        public string FlagshipInstanceId { get; set; }

        public DecisionManager(FlagshipConfig config, HttpClient httpClient)
        {
            Config = config;
            HttpClient = httpClient;
        }

        public abstract Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor);

        public ITrackingManager TrackingManager { get; set; }

        public Task<ICollection<FlagDTO>> GetFlags(ICollection<Campaign> campaigns)
        {
            return Task.Factory.StartNew(() =>
            {
                ICollection<FlagDTO> flags = new Collection<FlagDTO>();
                foreach (var campaign in campaigns)
                {
                    foreach (var item in campaign.Variation.Modifications.Value)
                    {
                        var flag = new FlagDTO()
                        {
                            Key = item.Key,
                            CampaignId = campaign.Id,
                            CampaignName = campaign.Name,
                            VariationGroupId = campaign.VariationGroupId,
                            VariationGroupName = campaign.VariationGroupName,
                            VariationId = campaign.Variation.Id,
                            VariationName = campaign.Variation.Name,
                            IsReference = campaign.Variation.Reference,
                            Value = item.Value,
                            CampaignType = campaign.Type,
                            Slug = campaign.Slug,
                        };
                        flags.Add(flag);
                    }
                }
                return flags;
            });
        }
    }
}
