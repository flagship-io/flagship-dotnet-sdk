using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal class NoConsentStrategy : DefaultStrategy
    {
        public NoConsentStrategy(VisitorDelegateAbstract visitor) : base(visitor)
        {
        }
    }
}
