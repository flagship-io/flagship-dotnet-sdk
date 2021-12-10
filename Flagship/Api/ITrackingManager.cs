using Flagship.Config;
using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Api
{
    public interface ITrackingManager
    {
        public FlagshipConfig Config { get; set; }

        public Task SendActive(VisitorDelegateAbstract visitor, FlagDTO flag);

        public Task SendHit(object hit);

        public Task SendConsentHit(VisitorDelegateAbstract visitor);
    }
}
