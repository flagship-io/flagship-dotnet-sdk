using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag 
{
    public class Flag<T> : IFlag<T>
    {
        private string _key;
        private VisitorDelegateAbstract _visitorDelegateAbstract;
        private FlagDTO _flagDTO;
        private T _defaultValue;
        private IFlagMetadata _metadata;
        internal Flag(string key, VisitorDelegateAbstract visitorDelegate, FlagDTO flag, T DefaultValue )
        {
            _key = key;
            _visitorDelegateAbstract = visitorDelegate;
            _flagDTO = flag;  
            _defaultValue = DefaultValue;
            _metadata = new FlagMetadata(flag?.CampaignId ?? "", flag?.VariationGroupId ?? "", flag?.VariationId ?? "", flag?.IsReference ?? false, "");
        }
        public bool Exist => _flagDTO!=null;

        public IFlagMetadata Metadata => throw new NotImplementedException();

        public Task UserExposed()
        {
            return _visitorDelegateAbstract.UserExposed(_key, _defaultValue, _flagDTO);
        }

        public T Value(bool userExposed)
        {
            throw new NotImplementedException();
        }
    }
}
