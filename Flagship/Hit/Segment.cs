using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    internal class Segment : HitAbstract
    {
        public const string ERROR_MESSAGE = "data property is required";
        public IDictionary<string, object> Data { get; set; }

        public Segment(IDictionary<string, object> data):base(HitType.CONTEXT)
        {
            Data = data;
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return base.IsReady(checkParent) && Data!=null;
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            return new Dictionary<string, object>()
            {
                ["visitorId"] = VisitorId,
                ["data"] = Data,
                ["type"] = $"{Type}",
            };
        }

        internal override string GetErrorMessage()
        {
            return ERROR_MESSAGE;
        }
    }
}
