using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    internal class BatchConverter : CustomCreationConverter<ICollection<HitAbstract>>
    {
        ICollection<HitAbstract> Hits { get; set; }

        public override ICollection<HitAbstract> Create(Type objectType)
        {
            return Hits;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = JArray.ReadFrom(reader);

            Hits = new List<HitAbstract>();

            foreach (var jobj in array)
            {
                HitAbstract hit = null;
                var type = jobj["Type"].ToObject<HitType>();
                switch (type)
                {
                    case HitType.PAGEVIEW:
                        hit = jobj.ToObject<Page>(serializer);
                        break;
                    case HitType.SCREENVIEW:
                        hit = jobj.ToObject<Screen>(serializer);
                        break;
                    case HitType.TRANSACTION:
                        hit = jobj.ToObject<Transaction>(serializer);
                        break;
                    case HitType.ITEM:
                        hit = jobj.ToObject<Item>(serializer);
                        break;
                    case HitType.EVENT:
                        hit = jobj.ToObject<Event>(serializer);
                        break;
                    case HitType.BATCH:
                        hit = jobj.ToObject<Batch>(serializer);
                        break;
                }
                Hits.Add(hit);
            }

            
            return Hits;
        }
      
    }
}
