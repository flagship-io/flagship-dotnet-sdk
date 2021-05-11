using Flagship.Model;
using Flagship.Model.Hits;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Services.HitSender
{
    public interface ISender
    {
        Task Send(string visitorId, HitType type, BaseHit hit);
        Task Send<T>(string visitorId, T hit) where T : BaseHit;
        Task Activate(ActivateRequest request);
        Task SendEvent(EventRequest request);
    }
}
