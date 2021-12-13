using Flagship.Config;
using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal class TrackingManager : ITrackingManager
    {
        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }

        public TrackingManager(HttpClient httpClient, FlagshipConfig config)
        {
            HttpClient = httpClient;
            Config = config;
        }

        public Task SendActive(VisitorDelegateAbstract visitor, FlagDTO flag)
        {
            throw new NotImplementedException();
        }

        public Task SendConsentHit(VisitorDelegateAbstract visitor)
        {
            throw new NotImplementedException();
        }

        public Task SendHit(object hit)
        {
            throw new NotImplementedException();
        }
    }
}
