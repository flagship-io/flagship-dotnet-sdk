using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Delegate
{
    public delegate void StatusChangeDelegate(FlagshipStatus status);
    public delegate void OnVisitorExposedDelegate(IExposedVisitor exposedVisitor, IExposedFlag exposedFlag);
}
