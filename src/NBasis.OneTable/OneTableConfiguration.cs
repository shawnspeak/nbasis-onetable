using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBasis.OneTable
{
    public class OneTableConfiguration
    {
        public class KeyAttributeConfiguration
        {
            public string PK { get; set; }
            
            public string SK { get; set; }

            public string GPK { get; set; }

            public string GSK { get; set; }
        }

        public KeyAttributeConfiguration KeyAttributes { get; set; }

        public int GSIndexCount { get; set; }

        public IDictionary<string, string> Tables { get; set; }

        public static OneTableConfiguration Default()
        {
            return new OneTableConfiguration
            {
                Tables = new Dictionary<string, string>(),
                GSIndexCount = Constants.DefaultGSIndexCount,
                KeyAttributes = new KeyAttributeConfiguration
                {
                    PK = Constants.KeyAttributeNames.PK,
                    SK = Constants.KeyAttributeNames.SK,
                    GPK = Constants.KeyAttributeNames.GPK,
                    GSK = Constants.KeyAttributeNames.GSK,
                }
            };
        }

        internal void Validate()
        {

        }
    }
}
