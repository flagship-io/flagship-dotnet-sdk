using Flagship.Model.Bucketing;
using Flagship.Model.Logs;
using Flagship.Services.Logger;
using System;
using System.Text;

namespace Flagship.Services.Bucketing
{
    public class VariationAllocation
    {
        private readonly ILogger logger;
        public VariationAllocation(ILogger logger)
        {
            this.logger = logger;
        }

        public Variation GetVariation(VariationGroup vg, string visitorId)
        {
            using (var murmur = Murmur.MurmurHash.Create32())
            {
                var hashBytes = murmur.ComputeHash(Encoding.UTF8.GetBytes(vg.Id + visitorId));
                var hash = BitConverter.ToUInt32(hashBytes, 0);

                var hash100 = hash % 100;

                var alloc = 0;
                foreach (var v in vg.Variations)
                {
                    alloc += v.Allocation;
                    if (hash100 <= alloc)
                    {
                        return v;
                    }
                }
                logger.Log(LogLevel.INFO, LogCode.VISITOR_NOT_TRACKED);
                return null;
            }
        }
    }
}
