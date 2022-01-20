using Flagship.Config;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Model;
using Flagship.Model.Bucketing;
using Murmur;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flagship.Decision
{
    internal class BucketingManager : DecisionManager
    {
        new public event StatusChangeDelegate StatusChange;
        Murmur32 _murmur32;
        bool _isPolling;
        string _lastModified;
        BucketingDTO _bucketingContent;
        bool _isFirstPooling;
        new public BucketingConfig Config { get; set; }
        Timer _timer;

        public BucketingManager(BucketingConfig config, HttpClient httpClient, Murmur32 murmur32) : base(config, httpClient)
        {
            _murmur32 = murmur32;
            _isPolling = false;
            _lastModified = null;
            _isFirstPooling = true;
            Config = config;
        }

        public async Task StartPolling()
        {

            var pollingInterval = Config.PollingInterval.Value;
            Utils.Log.LogInfo(Config, "Bucketing polling starts", "StartPolling");
            await Polling();
            if (pollingInterval.TotalMilliseconds == 0)
            {
                return;
            }

            if (_timer != null)
            {
                _timer.Dispose();
            }

            _timer = new Timer(async (e) =>
            {
                await Polling();
            }, null, pollingInterval, pollingInterval);


        }

        public async Task Polling()
        {
            try
            {
                if (_isPolling)
                {
                    return;
                }
                _isPolling = true;

                if (_isFirstPooling)
                {
                    StatusChange?.Invoke(FlagshipStatus.POLLING);
                }

                var url = string.Format(Constants.BUCKETING_API_URL, Config.EnvId);


                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));


                if (_lastModified != null)
                {
                    requestMessage.Headers.Add(HttpRequestHeader.IfModifiedSince.ToString(), _lastModified);
                }

                var response = await HttpClient.SendAsync(requestMessage);


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    _bucketingContent = JsonConvert.DeserializeObject<BucketingDTO>(responseBody);
                }

                if (response.Headers.TryGetValues(HttpResponseHeader.LastModified.ToString(), out IEnumerable<string> lastModified))
                {
                    _lastModified = lastModified.First();
                };

                if (_isFirstPooling)
                {
                    _isFirstPooling = false;
                    StatusChange?.Invoke(FlagshipStatus.READY);
                }

                _isPolling = false;
            }
            catch (Exception ex)
            {
                _isPolling = false;

                if (_isFirstPooling)
                {
                    StatusChange?.Invoke(FlagshipStatus.NOT_INITIALIZED);
                }
                Utils.Log.LogError(Config, ex.Message, "StartPolling");
            }
        }

        public void StopPolling()
        {
            _timer.Dispose();
            _isPolling = false;
            Utils.Log.LogInfo(Config, "Bucketing polling stopped", "StopPolling");
        }

        public async void SendContext(VisitorDelegateAbstract visitor)
        {
            try
            {
                var url = string.Format(Constants.BUCKETING_API_CONTEXT_URL, Config.EnvId);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var postData = new Dictionary<string, object>
                {
                    ["visitorId"] = visitor.VisitorId,
                    ["type"] = "CONTEXT",
                    ["data"] = visitor.Context
                };

                var postDatajson = JsonConvert.SerializeObject(postData);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, "application/json");

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {

                Utils.Log.LogError(Config, ex.Message, "SendContext");
            }
        }
        public override Task<ICollection<Model.Campaign>> GetCampaigns(VisitorDelegateAbstract visitor)
        {
            SendContext(visitor);

            return Task.Factory.StartNew<ICollection<Model.Campaign>>(() =>
            {
                ICollection<Model.Campaign> campaigns = new Collection<Model.Campaign>();

                if (_bucketingContent == null)
                {
                    return campaigns;
                }

                if (_bucketingContent.Panic.HasValue && _bucketingContent.Panic.Value)
                {
                    IsPanic = true;
                    return campaigns;
                }

                IsPanic = false;

                foreach (var item in _bucketingContent.Campaigns)
                {

                }

            });

        }

        protected bool MatchOperator(string operatorName, object contextValue, object targetingValue)
        {
            bool check = false;
            switch (targetingValue)
            {
                case string value:
                    if (contextValue is string contextString)
                    {
                        return MatchOperator(operatorName, contextString, value);
                    }
                    break;
                case double value:
                    if (contextValue is double contextNumber)
                    {
                        return MatchOperator(operatorName, contextNumber, value );
                    }
                    break;
            }
            return check;
        }

        protected bool MatchOperator(string operatorName, string contextValue, string targetingValue)
        {
            bool check;
            switch (operatorName)
            {
                case "GREATER_THAN":
                    check = contextValue.CompareTo(targetingValue)>0;
                    break;
                case "LOWER_THAN":
                    check = contextValue.CompareTo(targetingValue)<0;
                    break;
                case "GREATER_THAN_OR_EQUALS":
                    check = contextValue.CompareTo(targetingValue)>=0;
                    break;
                case "LOWER_THAN_OR_EQUALS":
                    check = contextValue.CompareTo(targetingValue)<=0;
                    break;
                default:
                    check = false;
                    break;
            }
            return check;
        }

        protected bool MatchOperator(string operatorName, double contextValue, double targetingValue)
        {
            bool check;
            switch (operatorName)
            {
                case "GREATER_THAN":
                    check = contextValue.CompareTo(targetingValue) > 0;
                    break;
                case "LOWER_THAN":
                    check = contextValue.CompareTo(targetingValue) < 0;
                    break;
                case "GREATER_THAN_OR_EQUALS":
                    check = contextValue.CompareTo(targetingValue) >= 0;
                    break;
                case "LOWER_THAN_OR_EQUALS":
                    check = contextValue.CompareTo(targetingValue) <= 0;
                    break;
                default:
                    check = false;
                    break;
            }
            return check;
        }

        protected bool TestOperator(string operatorName, object contextValue, object targetingValue)
        {
            bool check = false;
            try
            {

                switch (operatorName)
                {
                    case "EQUALS":
                        check = contextValue.Equals(targetingValue);
                        break;
                    case "NOT_EQUALS":
                        check = !contextValue.Equals(targetingValue);
                        break;
                    case "CONTAINS":
                        check = contextValue.ToString().Contains(targetingValue.ToString());
                        break;
                    case "NOT_CONTAINS":
                        check = !contextValue.ToString().Contains(targetingValue.ToString());
                        break;
                    case "GREATER_THAN":
                        check = contextValue > targetingValue;
                        break;
                    case "LOWER_THAN":
                        check = contextValue < targetingValue;
                        break;
                    case "GREATER_THAN_OR_EQUALS":
                        check = contextValue >= targetingValue;
                        break;
                    case "LOWER_THAN_OR_EQUALS":
                        check = contextValue <= targetingValue;
                        break;
                    case "STARTS_WITH":
                        check = contextValue.ToString().StartsWith(targetingValue.ToString());
                        break;
                    case "ENDS_WITH":
                        check = contextValue.ToString().EndsWith(targetingValue.ToString());
                        break;
                    default:
                        check = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "TestOperator");
            }
            

            return check;
        }
    }
}
