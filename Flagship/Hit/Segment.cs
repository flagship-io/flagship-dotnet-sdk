using System.Collections.Generic;

namespace Flagship.Hit
{
    internal class Segment : HitAbstract
    {
        public const string ERROR_MESSAGE = "data property is required";
        public const string S_API_ITEM = "s";
        public IDictionary<string, object> Context { get; set; }

        public Segment(IDictionary<string, object> context)
            : base(HitType.SEGMENT)
        {
            Context = context;
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return base.IsReady(checkParent) && Context != null;
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = base.ToApiKeys();
            if (Context == null || Context.Count == 0)
            {
                apiKeys[S_API_ITEM] = new Dictionary<string, string>();
                return apiKeys;
            }

            var apiContext = new Dictionary<string, string>(Context.Count);

            foreach (var item in Context)
            {
                apiContext[item.Key] = item.Value?.ToString() ?? "null";
            }

            apiKeys[S_API_ITEM] = apiContext;
            return apiKeys;
        }

        internal override string GetErrorMessage()
        {
            return ERROR_MESSAGE;
        }
    }
}
