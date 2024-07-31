using System;

namespace Flagship.Model
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}
