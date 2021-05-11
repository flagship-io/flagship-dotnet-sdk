using Flagship.Model.Hits;
using Flagship.Services;
using Flagship.Services.Decision;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.HitSender;
using Flagship.Services.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flagship
{
    public class FlagshipClient : IFlagshipClient
    {
        private readonly ISender sender;
        private readonly IExceptionHandler exceptionHandler;
        private readonly ILogger logger;
        private readonly IFlagshipVisitorService fsVisitorService;

        public FlagshipClient(FlagshipContext flagshipContext)
        {
            sender = flagshipContext.Sender;
            exceptionHandler = flagshipContext.ExceptionHandler;
            logger = flagshipContext.Logger;
            fsVisitorService = new FlagshipVisitorService(flagshipContext);
        }

        /// <summary>
        /// Generates a new visitor
        /// </summary>
        /// <param name="visitorId">The visitor ID</param>
        /// <param name="context">The visitor Context</param>
        /// <param name="decisionGroup">the visitor Decision Group</param>
        /// <returns></returns>
        public IFlagshipVisitor NewVisitor(string visitorId, IDictionary<string, object> context, string decisionGroup = null)
        {
            return new FlagshipVisitor(new Visitor(visitorId, context, decisionGroup), fsVisitorService, logger);
        }

        /// <summary>
        /// Send a datacollect hit
        /// </summary>
        /// <param name="type">The hit type</param>
        /// <param name="hit">The hit content</param>
        /// <returns></returns>
        public async Task SendHit(string visitorId, HitType type, BaseHit hit)
        {
            try
            {
                await sender.Send(visitorId, type, hit);
            }
            catch (Exception e)
            {
                exceptionHandler.Handle(e);
            }
        }

        /// <summary>
        /// Send a datacollect hit by its type
        /// </summary>
        /// <typeparam name="T">The hit type</param>
        /// <param name="hit">The hit content</param>
        /// <returns></returns>
        public async Task SendHit<T>(string visitorId, T hit) where T : BaseHit
        {
            await sender.Send<T>(visitorId, hit);
        }
    }
}
