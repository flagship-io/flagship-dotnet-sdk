﻿using Flagship.Config;
using Flagship.Delegate;
using Flagship.Enum;
using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Decision
{
    public abstract class DecisionManager : IDecisionManager
    {
        public event StatusChangeDelegate StatusChange;
        protected bool _isPanic = false;

        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }
        public bool IsPanic { 
            get => _isPanic; 
            protected set {
                _isPanic = value;
                StatusChange?.Invoke(_isPanic? FlagshipStatus.READY_PANIC_ON: FlagshipStatus.READY);
            }
        }

        public DecisionManager(HttpClient httpClient, FlagshipConfig config)
        {
            HttpClient = httpClient;
            Config = config;    
        }

        abstract public Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor);

        public Task<ICollection<FlagDTO>> GetFlags(ICollection<Campaign> campaigns)
        {
           return Task.Run(() =>
            {
                ICollection<FlagDTO> flags = new Collection<FlagDTO>();
                try
                {
                    foreach (var campaign in campaigns)
                    {
                        foreach (var item in campaign.Variation.Modifications.Value)
                        {
                            var flag = new FlagDTO()
                            {
                                Key = item.Key,
                                CampaignId = campaign.Id,
                                VariationGroupId = campaign.VariationGroupId,
                                VariationId = campaign.Variation.Id,
                                IsReference = campaign.Variation.Reference,
                                Value = item.Value
                            };
                            flags.Add(flag);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.Utils.LogError(Config, ex.Message, "GetFlags");
                }
               
                return flags;
            });
        }
    }
}