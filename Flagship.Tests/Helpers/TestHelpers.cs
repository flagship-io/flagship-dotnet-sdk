
using System.Reflection;

namespace Flagship.Tests.Helpers
{
    public static class TestHelpers{

        public static  MethodInfo? GetPrivateMethod(object obj, string methodName){
            return obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

}