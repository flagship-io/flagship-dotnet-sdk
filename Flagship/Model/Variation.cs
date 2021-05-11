namespace Flagship.Model
{
    public class Variation
    {
        /// <summary>
        /// The variation ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Is Reference
        /// </summary>
        public bool Reference { get; set; }

        /// <summary>
        /// Modifications of the variation
        /// </summary>
        public Modifications Modifications { get; set; }
    }
}