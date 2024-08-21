using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private readonly VisitorDelegateAbstract _visitor;
        private readonly HashSet<string> _keys = new HashSet<string>();
        private readonly Dictionary<string, IFlag> _flags = new Dictionary<string, IFlag>();

        internal FlagCollection(
            VisitorDelegateAbstract visitor = null,
            Dictionary<string, IFlag> flags = null
        )
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

        public FSFlagStatus Status => _visitor?.FlagsStatus.Status ?? FSFlagStatus.NOT_FOUND;

        public IFlag Get(string key)
        {
            if (!_flags.TryGetValue(key, out var flag))
            {
                Log.LogWarning(
                    _visitor?.Config,
                    string.Format(Constants.GET_FLAG_NOT_FOUND, _visitor?.VisitorId, key),
                    Constants.GET_FLAG
                );
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
            foreach (var kvp in _flags)
            {
                var key = kvp.Key;
                var flag = kvp.Value;
                if (predicate(flag, key, this))
                {
                    flags.Add(key, flag);
                }
            }
            return new FlagCollection(_visitor, flags);
        }

        public async Task ExposeAllAsync()
        {
            await Task.WhenAll(_flags.Values.Select(flag => flag.VisitorExposed()))
                .ConfigureAwait(false);
        }

        public Dictionary<string, IFlagMetadata> GetMetadata()
        {
            var metadata = new Dictionary<string, IFlagMetadata>();
            foreach (var kvp in _flags)
            {
                metadata.Add(kvp.Key, kvp.Value.Metadata);
            }
            return metadata;
        }

        public string ToJson()
        {
            var serializedData = new List<SerializedFlagMetadata>();

            foreach (var kvp in _flags)
            {
                var key = kvp.Key;
                var flag = kvp.Value;
                var metadata = flag.Metadata;
                var value = flag.GetValue<object>(null, false);
                serializedData.Add(
                    new SerializedFlagMetadata
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
                        Hex = Utils.Helper.ValueToHex(
                            new Dictionary<string, object> { { "v", value } }
                        ),
                    }
                );
            }
            return JsonConvert.SerializeObject(serializedData);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
