using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship
{
    public class Flagship
    {
        private static Flagship instance;

        protected static Flagship GetInstance() 
        {
            if (instance == null)
            {
                instance = new Flagship();
            }
            return instance;
        }

        private Flagship()
        {

        }
    }
}
