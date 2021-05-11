using Flagship.Services.Decision;
using Flagship.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flagship.Services.HitSender;
using Flagship.Model.Hits;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.Logger;
using System;
using Flagship.Model.Logs;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Flagship.Services
{
    public class FlagshipVisitorService : IFlagshipVisitorService
    {
        private readonly FlagshipContext flagshipContext;

        private readonly IDecisionManager decisionManager;
        private readonly ISender sender;
        private readonly IExceptionHandler exceptionHandler;
        private readonly ILogger logger;

        public FlagshipVisitorService(FlagshipContext flagshipContext)
        {
            this.flagshipContext = flagshipContext;
            decisionManager = flagshipContext.DecisionManager;
            sender = flagshipContext.Sender;
            exceptionHandler = flagshipContext.ExceptionHandler;
            logger = flagshipContext.Logger;
        }

        /// <summary>
        /// Synchronize modifications from the Decision API
        /// </summary>
        /// <returns></returns>
        public async Task SynchronizeModifications(Visitor visitor)
        {
            try
            {
                var response = await decisionManager.GetResponse(new DecisionRequest(visitor.Id, visitor.Context, visitor.DecisionGroup));
                visitor.SetFlagInfos(response);
            }
            catch (HttpRequestException e)
            {
                exceptionHandler.Handle(e);
            }
        }

        /// <summary>
        /// Get a single modification key
        /// </summary>
        /// <typeparam name="T">The type of the key (bool, string, double)</typeparam>
        /// <param name="key">the key name</param>
        /// <param name="defaultValue">the default value</param>
        /// <param name="activate">activate the key associated campaign</param>
        /// <returns></returns>
        public T GetModification<T>(Visitor visitor, string key, T defaultValue = default, bool activate = true)
        {
            if (visitor == null)
            {
                throw new ArgumentException("visitor should not be null");
            }

            if (visitor.IsPanic)
            {
                return defaultValue;
            }

            if (!visitor.FlagInfos.ContainsKey(key))
            {
                return defaultValue;
            }
            var flag = visitor.FlagInfos[key];

            if (activate)
            {
                _ = ActivateModification(visitor, key);
            }

            try
            {
                if (flag.Value == null)
                {
                    return defaultValue;
                }

                if (flag.Value is JArray jarray)
                {
                    return jarray.ToObject<T>();
                }

                if (flag.Value is JObject jobject)
                {
                    return jobject.ToObject<T>();
                }
                return (T)Convert.ChangeType(flag.Value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                exceptionHandler.Handle(e);
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a single modification key
        /// </summary>
        /// <param name="key">the key name</param>
        /// <returns>A modification info</returns>
        public ModificationInfo GetModificationInfo(Visitor visitor, string key)
        {
            if (visitor == null)
            {
                throw new ArgumentException("visitor should not be null");
            }

            if (!visitor.FlagInfos.ContainsKey(key))
            {
                return null;
            }

            var flag = visitor.FlagInfos[key];
            return new ModificationInfo()
            {
                CampaignID = flag.CampaignId,
                VariationGroupID = flag.VariationGroupId,
                VariationID = flag.VariationId,
                IsReference = flag.Reference
            };
        }

        /// <summary>
        /// Activate the campaign associated with the modification key
        /// </summary>
        /// <param name="key">The modification key</param>
        /// <returns></returns>
        public async Task ActivateModification(Visitor visitor, string key)
        {
            if (!visitor.FlagInfos.ContainsKey(key))
            {
                return;
            }
            var flag = visitor.FlagInfos[key];

            try
            {
                await sender.Activate(new ActivateRequest(flagshipContext.EnvironmentId, visitor.Id, flag.VariationGroupId, flag.VariationId)).ConfigureAwait(false);
                logger.Log(LogLevel.INFO, LogCode.CAMPAIGN_ACTIVATED, new
                {
                    campaignId = flag.CampaignId,
                    variationId = flag.VariationId
                });
            }
            catch (HttpRequestException e)
            {
                exceptionHandler.Handle(e);
            }
        }

        /// <summary>
        /// Send a datacollect hit
        /// </summary>
        /// <param name="type">The hit type</param>
        /// <param name="hit">The hit content</param>
        /// <returns></returns>
        public async Task SendHit(Visitor visitor, HitType type, BaseHit hit)
        {
            await sender.Send(visitor.Id, type, hit);
        }

        public async Task SendHit<T>(string visitorId, T hit) where T : BaseHit
        {
            await sender.Send<T>(visitorId, hit);
        }
    }
}
