using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model.Bucketing
{
    public class Variation
    {
        public string Id { get; set; }
        public bool Reference { get; set; }
        public int Allocation { get; set; }
        public Modifications Modifications { get; set; }
    }
}
