using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusScheme.RuleEngine
{
    [Serializable]
    public class ResultValue
    {
        public bool Succeeded { get; set; }
        public string Value { get; set; }
        public string Message { get; set; }
    }
}
