namespace Flagship.Model.Bucketing
{
    public class Variation
    {
        public string Id { get; set; }
        public string Name { get; set; }    
        public bool Reference { get; set; }
        public int Allocation { get; set; }
        public Modifications Modifications { get; set; }
    }
}
