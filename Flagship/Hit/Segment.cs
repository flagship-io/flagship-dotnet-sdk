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
        public const string S_API_ITEM = "s";
        public IDictionary<string, object> Context { get; set; }

        public Segment(IDictionary<string, object> context) :base(HitType.SEGMENT)
        {
            Context = context;
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return base.IsReady(checkParent) && Context!=null;
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = base.ToApiKeys();
            apiKeys[S_API_ITEM] = Context;
            return apiKeys;
        }

        internal override string GetErrorMessage()
        {
            return ERROR_MESSAGE;
        }
    }
}
