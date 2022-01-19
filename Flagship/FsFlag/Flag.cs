using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    public class Flag<T> : IFlag<T>
    {
        private readonly string _key;
        private readonly VisitorDelegateAbstract _visitorDelegateAbstract;
        private readonly FlagDTO _flagDTO;
        private readonly object _defaultValue;
        private readonly IFlagMetadata _metadata;
        public bool HasSameType
        {
            get
            {
                return Utils.Utils.HasSameType(this._flagDTO.Value, _defaultValue);

            }
        }
        internal Flag(string key, VisitorDelegateAbstract visitorDelegate, FlagDTO flag, object DefaultValue)
        {
            _key = key;
            _visitorDelegateAbstract = visitorDelegate;
            _flagDTO = flag;
            _defaultValue = DefaultValue;
            _metadata = new FlagMetadata(flag?.CampaignId ?? "", flag?.VariationGroupId ?? "", flag?.VariationId ?? "", flag?.IsReference ?? false, "");
        }



        public bool Exist => _flagDTO != null && HasSameType;

        public IFlagMetadata Metadata
        {
            get
            {
                if (_flagDTO == null)
                {
                    return _metadata;
                }

               
                return _visitorDelegateAbstract.GetFlagMetadata(_metadata, _key, HasSameType);
            }
        }

        public Task UserExposed()
        {
            return _visitorDelegateAbstract.UserExposed(_key, _defaultValue, _flagDTO);
        }

        public T Value(bool userExposed = true)
        {
            return _visitorDelegateAbstract.GetFlagValue(_key, (T)_defaultValue, _flagDTO, userExposed);
        }
    }
}
