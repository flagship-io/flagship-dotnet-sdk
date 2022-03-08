using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flagship.Main;

namespace ConsoleApp2
{
    internal class Program
    {
        static async void test()
        {
            Flagship.Main.Flagship.Start("c1ndrd07m0300ro0jf20", "QzdTI1M9iqaIhnJ66a34C5xdzrrvzq6q8XSVOsS6");
            var visitor = Flagship.Main.Flagship.NewVisitor().Build();
            await visitor.FetchFlags();
            Console.WriteLine(visitor.GetFlag("array","default").Value());
        }
        static void Main(string[] args)
        {
            test();
            Console.ReadKey();
        }
    }
}
