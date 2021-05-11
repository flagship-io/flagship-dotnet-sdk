using System.Collections.Generic;

namespace Flagship.Model
{
    public class Modifications
    {
        /// <summary>
        /// The modification type
        /// </summary>
        public ModificationType Type { get; set; }

        /// <summary>
        /// The modification value (key/value pair object)
        /// </summary>
        public IDictionary<string, object> Value { get; set; }
    }
}