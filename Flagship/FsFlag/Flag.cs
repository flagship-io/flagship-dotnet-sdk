﻿using Flagship.FsVisitor;
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
                return flagDTO!=null && !string.IsNullOrWhiteSpace(flagDTO.CampaignId) && 
                    !string.IsNullOrWhiteSpace(flagDTO.VariationId) && 
                    !string.IsNullOrWhiteSpace(flagDTO.VariationGroupId);
            }
        }

        public IFlagMetadata Metadata
        {
            get
            {
                var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
                var metadata = new FlagMetadata(flagDTO?.CampaignId ?? "", 
                    flagDTO?.VariationGroupId ?? "", 
                    flagDTO?.VariationId ?? "", flagDTO?.IsReference ?? false, flagDTO?.CampaignType ?? "", flagDTO?.Slug, 
                    flagDTO?.CampaignName??"", flagDTO?.VariationGroupName??"", flagDTO?.VariationName??""
                    );
                if (flagDTO == null)
                {
                    return metadata;
                }

                return _visitorDelegateAbstract.GetFlagMetadata(metadata, _key, flagDTO.Value==null || _defaultValue==null ||  Utils.Utils.HasSameType(flagDTO.Value, _defaultValue));
            }
        }

        public Task VisitorExposed()
        {
            var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
            return _visitorDelegateAbstract.VisitorExposed(_key, _defaultValue, flagDTO);
        }
        public Task UserExposed()
        {
            return VisitorExposed();
        }

        public T GetValue(bool userExposed = true)
        {
            var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
            return _visitorDelegateAbstract.GetFlagValue(_key, (T)_defaultValue, flagDTO, userExposed);
        }


    }
}
