using Flagship.Model.Config;
using Flagship.Model.Hits;
using Flagship.Services.Decision;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.HitSender;
using Flagship.Services.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flagship
{
    public interface IFlagshipClient
    {
        IFlagshipVisitor NewVisitor(string visitorId, IDictionary<string, object> context, string decisionGroup = null);

        Task SendHit(string visitorId, HitType type, BaseHit hit);
        Task SendHit<T>(string visitorId, T hit) where T : BaseHit;
    }
}
