using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;

namespace Flagship.FsFlag
{
    public class FlagCollection : IFlagCollection
    {
        private VisitorDelegate _visitor;
        private HashSet<string> _keys = new HashSet<string>();
        private Dictionary<string, IFlag> _flags = new Dictionary<string, IFlag>();

        internal FlagCollection(VisitorDelegate visitor = null, Dictionary<string, IFlag> flags = null)
        {
            _visitor = visitor;
            _flags = flags ?? new Dictionary<string, IFlag>();

            if (_flags.Count == 0 && visitor != null)
            {
                _keys = new HashSet<string>(visitor.Flags.Select(flag => flag.Key));
                foreach (var key in _keys)
                {
                    _flags.Add(key, new Flag(key, visitor));
                }
            }
            else
            {
                _keys = new HashSet<string>(_flags.Keys);
            }
        }

        public int Size => _keys.Count;

        public IFlag Get(string key)
        {
            if (!_flags.TryGetValue(key, out var flag))
            {
                // Assuming logWarningSprintf is a method to log warnings, replace with actual logging method.
                // LogWarningSprintf(_visitor?.Config, GET_FLAG, GET_FLAG_NOT_FOUND, _visitor?.VisitorId, key);
                Log.LogWarning(_visitor?.Config, string.Format("Flag with key {0} not found", key), Constants.GET_FLAG);
                return new Flag(key, null);
            }
            return flag;
        }

        public bool Has(string key)
        {
            return _keys.Contains(key);
        }

        public HashSet<string> Keys()
        {
            return _keys;
        }

        public IEnumerator<KeyValuePair<string, IFlag>> GetEnumerator()
        {
            foreach (var key in _keys)
            {
                yield return new KeyValuePair<string, IFlag>(key, _flags[key]);
            }
        }

        public IFlagCollection Filter(Func<IFlag, string, IFlagCollection, bool> predicate)
        {
            var flags = new Dictionary<string, IFlag>();
            foreach (var (key, flag) in _flags)
            {
                if (predicate(flag, key, this))
                {
                    flags.Add(key, flag);
                }
            }
            return new FlagCollection(_visitor, flags);
        }

        public async Task ExposeAllAsync()
        {
            await Task.WhenAll(_flags.Values.Select(flag => flag.VisitorExposed()));
        }

        public Dictionary<string, IFlagMetadata> GetMetadata()
        {
            var metadata = new Dictionary<string, IFlagMetadata>();
            foreach (var (key, flag) in _flags)
            {
                metadata.Add(key, flag.Metadata);
            }
            return metadata;
        }

        public string ToJson()
        {
            var serializedData = new List<SerializedFlagMetadata>();
            foreach (var (key, flag) in _flags)
            {
                var metadata = flag.Metadata;
                serializedData.Add(new SerializedFlagMetadata
                {
                    Key = key,
                    CampaignId = metadata.CampaignId,
                    CampaignName = metadata.CampaignName,
                    VariationGroupId = metadata.VariationGroupId,
                    VariationGroupName = metadata.VariationGroupName,
                    VariationId = metadata.VariationId,
                    VariationName = metadata.VariationName,
                    IsReference = metadata.IsReference,
                    CampaignType = metadata.CampaignType,
                    Slug = metadata.Slug,
                    Hex = Utils.Helper.ValueToHex(flag.GetValue<object>(null, false))
                });
            }
            return JsonConvert.SerializeObject(serializedData);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); 
        }
    }
}