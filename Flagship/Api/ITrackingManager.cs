﻿using Flagship.Config;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal interface ITrackingManager : ITrackingManagerCommon
    {
        void StartBatchingLoop();

        void StopBatchingLoop();

        Task SendTroubleshootingHit(Troubleshooting hit);
        void AddTroubleshootingHit(Troubleshooting hit);
        Task SendUsageHit(UsageHit hit); 

        TroubleshootingData TroubleshootingData { set; get; }
    }
}
