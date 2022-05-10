using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Flagship.FsFlag
{
    public class Flag<T> : IFlag<T>
    {
        private readonly string _key;
        private readonly VisitorDelegateAbstract _visitorDelegateAbstract;
        private readonly object _defaultValue;
 
        internal Flag(string key, VisitorDelegateAbstract visitorDelegate, object DefaultValue)
        {
            _key = key;
            _visitorDelegateAbstract = visitorDelegate;
            _defaultValue = DefaultValue;
        }

        public bool Exists
        {
            get
            {
                var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
                return flagDTO != null && Utils.Utils.HasSameType(flagDTO.Value, _defaultValue);
            }
        }

        public IFlagMetadata Metadata
        {
            get
            {
                var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
                var metadata = new FlagMetadata(flagDTO?.CampaignId ?? "", flagDTO?.VariationGroupId ?? "", flagDTO?.VariationId ?? "", flagDTO?.IsReference ?? false, flagDTO?.CampaignType ?? "", flagDTO?.Slug);
                if (flagDTO == null)
                {
                    return metadata;
                }

                return _visitorDelegateAbstract.GetFlagMetadata(metadata, _key, Utils.Utils.HasSameType(flagDTO.Value, _defaultValue));
            }
        }

        public Task UserExposed()
        {
            var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
            return _visitorDelegateAbstract.UserExposed(_key, _defaultValue, flagDTO);
        }

        public T GetValue(bool userExposed = true)
        {
            var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
            return _visitorDelegateAbstract.GetFlagValue(_key, (T)_defaultValue, flagDTO, userExposed);
        }
    }
}
