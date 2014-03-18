using BonusScheme.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusScheme.Commands
{
    public class NotImplementedCommand : IRuleCommand
    {
        public ResultValue Process()
        {
            throw new NotImplementedException();
        }
    }
}
