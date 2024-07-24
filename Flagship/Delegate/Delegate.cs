using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Delegate
{
    public delegate void StatusChangedDelegate(FSSdkStatus status);
    /// <summary>
    /// Define an event to get callback each time a Flag has been exposed to a visitor (When a flag has been seen by your visitor) and succeeded.
    /// </summary>
    /// <param name="exposedVisitor">An interface to get information about the visitor to whom the flag has been exposed</param>
    /// <param name="exposedFlag">An interface to get information about the flag that has been exposed.</param>
    public delegate void OnVisitorExposedDelegate(IExposedVisitor exposedVisitor, IExposedFlag exposedFlag);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flagsStatus"></param>
    public delegate void OnFlagStatusChangedDelegate(FSFlagStatus flagsStatus);

    public delegate void OnFlagStatusFetchRequiredDelegate(FSFetchReasons reason);

    public delegate void OnFlagStatusFetchedDelegate();
}
