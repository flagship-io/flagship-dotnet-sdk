using System.Collections.Generic;
using System.Threading.Tasks;
using Flagship.Model;
using Flagship.Model.Hits;
using Flagship.Services;
using Flagship.Services.Logger;

namespace Flagship
{
    public class FlagshipVisitor : IFlagshipVisitor
    {
        private readonly IFlagshipVisitorService fsVisitorService;

        private readonly Visitor visitor;
        private readonly ILogger logger;

        public FlagshipVisitor(Visitor visitor, IFlagshipVisitorService fsVisitorService, ILogger logger)
        {
            this.fsVisitorService = fsVisitorService;
            this.visitor = visitor;
            this.logger = logger;
        }

        /// <summary>
        /// Updates the visitor context with a new full context
        /// </summary>
        /// <param name="context"></param>
        public void UpdateContext(IDictionary<string, object> context)
        {
            visitor.Context = context;
        }

        /// <summary>
        /// Updates a single context key. Adds the key if it does not exist
        /// </summary>
        /// <param name="key">The context key</param>
        /// <param name="value">The context value</param>
        public void UpdateContext(string key, object value)
        {
            if (visitor.Context.ContainsKey(key))
            {
                visitor.Context[key] = value;
            }
            else
            {
                visitor.Context.Add(key, value);
            }
        }

        /// <summary>
        /// Synchronize modifications from the Flagship decision engine
        /// </summary>
        /// <returns></returns>
        public async Task SynchronizeModifications()
        {
            await fsVisitorService.SynchronizeModifications(visitor);
        }

        /// <summary>
        /// Get a single modification key
        /// </summary>
        /// <typeparam name="T">The type of the key (bool, string, float)</typeparam>
        /// <param name="key">the key name</param>
        /// <param name="defaultValue">the default value</param>
        /// <param name="activate">activate the key associated campaign</param>
        /// <returns></returns>
        public T GetModification<T>(string key, T defaultValue = default, bool activate = true)
        {
            return fsVisitorService.GetModification<T>(visitor, key, defaultValue, activate);
        }

        /// <summary>
        /// Get a single modification info by key
        /// </summary>
        /// <param name="key">the key name</param>
        /// <returns></returns>
        public ModificationInfo GetModificationInfo(string key)
        {
            return fsVisitorService.GetModificationInfo(visitor, key);
        }

        /// <summary>
        /// Get all modifications flags
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, FlagInfo> GetAllModifications()
        {
            return visitor.FlagInfos;
        }

        /// <summary>
        /// Activate the campaign associated with the modification key
        /// </summary>
        /// <param name="key">The modification key</param>
        /// <returns></returns>
        public async Task ActivateModification(string key)
        {
            await fsVisitorService.ActivateModification(visitor, key);
        }

        /// <summary>
        /// Send a datacollect hit
        /// </summary>
        /// <param name="type">The hit type</param>
        /// <param name="hit">The hit content</param>
        /// <returns></returns>
        public async Task SendHit(HitType type, BaseHit hit)
        {
            if (visitor.IsPanic)
            {
                logger.Log(LogLevel.INFO, Model.Logs.LogCode.PANIC_NO_TRACKING);
                return;
            }
            await fsVisitorService.SendHit(visitor, type, hit);
        }

        /// <summary>
        /// Send a datacollect hit by its type
        /// </summary>
        /// <typeparam name="T">The hit type</typeparam>
        /// <param name="hit"></param>
        /// <returns></returns>
        public async Task SendHit<T>(T hit) where T : BaseHit
        {
            if (visitor.IsPanic)
            {
                logger.Log(LogLevel.INFO, Model.Logs.LogCode.PANIC_NO_TRACKING);
                return;
            }
            await fsVisitorService.SendHit<T>(visitor.Id, hit);
        }
    }
}
