using Flagship.Enums;

namespace Flagship.Model
{
    public interface IFetchFlagsStatus
    {
        FSFetchReasons Reason { get; set; }
        FSFetchStatus Status { get; set; }
    }
}