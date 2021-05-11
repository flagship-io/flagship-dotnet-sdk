using Flagship.Model;
using Flagship.Model.Hits;
using Flagship.Model.Logs;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Services.HitSender
{
    public class Sender : ISender
    {
        private const string HitEndpointUrl = "https://ariane.abtasty.com";
        private const string DecisionApiUrl = "https://decision.flagship.io";

        private readonly string environmentId;
        private readonly HttpClient httpClient;
        private readonly IExceptionHandler exceptionHandler;
        private readonly ILogger logger;

        public Sender(FlagshipContext flagshipContext)
        {
            environmentId = flagshipContext.EnvironmentId;
            exceptionHandler = flagshipContext.ExceptionHandler;
            logger = flagshipContext.Logger;
            httpClient = new HttpClient();
        }

        public async Task Send(string visitorId, HitType type, BaseHit hit)
        {
            if (hit == null)
            {
                throw new ArgumentException("visitor should not be null");
            }

            switch (type)
            {
                case HitType.EVENT:
                    hit = hit as Event;
                    break;
                case HitType.PAGEVIEW:
                    hit = hit as Pageview;
                    break;
                case HitType.SCREENVIEW:
                    hit = hit as Screenview;
                    break;
                case HitType.TRANSACTION:
                    hit = hit as Transaction;
                    break;
                case HitType.ITEM:
                    hit = hit as Item;
                    break;
            }

            if (hit == null)
            {
                exceptionHandler.Handle(new ArgumentException("Hit is malformed"));
                return;
            }

            try
            {
                hit.SetBaseInfos(type, environmentId, visitorId);

                var validationResult = hit.Validate();
                if (!validationResult.Success)
                {
                    exceptionHandler.Handle(new ArgumentException(string.Join(", ", validationResult.Errors)));
                    return;
                }

                var json = JsonConvert.SerializeObject(hit,
                    Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    HttpResponseMessage response = await httpClient.PostAsync(HitEndpointUrl, stringContent);
                    response.EnsureSuccessStatusCode();

                    logger.Log(LogLevel.INFO, LogCode.HIT_SENT, new { hit });
                }
            }
            catch (InvalidCastException e)
            {
                exceptionHandler.Handle(e);
            }
            catch (HttpRequestException e)
            {
                exceptionHandler.Handle(e);
            }
        }

        public async Task Send<T>(string visitorId, T hit) where T : BaseHit
        {
            switch (hit)
            {
                case Event e:
                    await Send(visitorId, HitType.EVENT, e);
                    break;
                case Transaction e:
                    await Send(visitorId, HitType.TRANSACTION, e);
                    break;
                case Item e:
                    await Send(visitorId, HitType.ITEM, e);
                    break;
                case Screenview e:
                    await Send(visitorId, HitType.SCREENVIEW, e);
                    break;
                default:
                    exceptionHandler.Handle(new ArgumentException("Type not handled"));
                    return;
            }
        }

        public async Task Activate(ActivateRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync($"{DecisionApiUrl}/v2/activate", stringContent);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    exceptionHandler.Handle(e);
                }
            }
        }

        public async Task SendEvent(EventRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync($"{DecisionApiUrl}/v2/{request.EnvironmentId}/events", stringContent);
                    var test = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    exceptionHandler.Handle(e);
                }
            }
        }
    }
}
