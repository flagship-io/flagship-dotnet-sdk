using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Services.ExceptionHandler
{
    public interface IExceptionHandler
    {
        void Handle(Exception exception);
    }
}
