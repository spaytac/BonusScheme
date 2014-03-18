using BonusScheme.Repositories;
using BonusScheme.RuleEngine;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusScheme.Commands
{
    public interface IRuleCommand
    {
        ResultValue Process();
    }
}
