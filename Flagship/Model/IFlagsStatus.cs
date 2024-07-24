using Flagship.Enums;

namespace Flagship.Model
{
    public interface IFlagsStatus
    {
        FSFetchReasons Reason { get; set; }
        FSFlagStatus Status { get; set; }
    }
}