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
    public delegate void StatusChangeDelegate(FSSdkStatus status);
    /// <summary>
    /// Define an event to get callback each time a Flag has been exposed to a visitor (When a flag has been seen by your visitor) and succeeded.
    /// </summary>
    /// <param name="exposedVisitor">An interface to get information about the visitor to whom the flag has been exposed</param>
    /// <param name="exposedFlag">An interface to get information about the flag that has been exposed.</param>
    public delegate void OnVisitorExposedDelegate(IExposedVisitor exposedVisitor, IExposedFlag exposedFlag);
}
