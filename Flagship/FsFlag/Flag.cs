using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Flagship.Enums;

namespace Flagship.FsFlag
{
    public class Flag : IFlag
    {
        private readonly string _key;
        private readonly VisitorDelegateAbstract _visitorDelegateAbstract;
        private object _defaultValue;
        private bool _hasGetValueBeenCalled;
 
        internal Flag(string key, VisitorDelegateAbstract visitorDelegate)
        {
            _key = key;
            _visitorDelegateAbstract = visitorDelegate;
            _hasGetValueBeenCalled = false;
        }

        public bool Exists
        {
            get
            {
                if (_visitorDelegateAbstract == null || _visitorDelegateAbstract.Flags == null)
                {
                    return false;
                }

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
                if (_visitorDelegateAbstract == null || _visitorDelegateAbstract.Flags == null)
                {
                    return  FSFlagMetadata.EmptyMetadata();
                }

                var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);

                return _visitorDelegateAbstract.GetFlagMetadata(_key, flagDTO);
            }
        }

        public FSFlagStatus Status
        {
            get
            {
                var fetchFlagsStatus = _visitorDelegateAbstract?.FetchFlagsStatus;
                if (fetchFlagsStatus?.Status == FSFetchStatus.PANIC)
                {
                    return FSFlagStatus.PANIC;
                }

                if (!Exists)
                {
                    return FSFlagStatus.NOT_FOUND;
                }

                if (fetchFlagsStatus?.Status == FSFetchStatus.FETCH_REQUIRED || fetchFlagsStatus?.Status == FSFetchStatus.FETCHING)
                {
                    return FSFlagStatus.FETCH_REQUIRED;
                }

                return FSFlagStatus.FETCHED;
            }
        }

        public Task VisitorExposed()
        {
            if (_visitorDelegateAbstract == null || _visitorDelegateAbstract.Flags == null)
            {
                return Utils.Helper.VoidTask();
            }

            var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);

            return _visitorDelegateAbstract.VisitorExposed(_key, _defaultValue, flagDTO, _hasGetValueBeenCalled);
        }

        public T GetValue<T>(T defaultValue, bool visitorExposed = true)
        {
            _defaultValue = defaultValue;
            _hasGetValueBeenCalled = true;

            if (_visitorDelegateAbstract == null || _visitorDelegateAbstract.Flags == null)
            {
                return defaultValue;
            }


            var flagDTO = _visitorDelegateAbstract.Flags?.FirstOrDefault(x => x.Key == _key);
            return _visitorDelegateAbstract.GetFlagValue(_key, defaultValue, flagDTO, visitorExposed);
        }
    }
}
