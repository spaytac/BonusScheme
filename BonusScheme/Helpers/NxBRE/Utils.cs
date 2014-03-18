using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusScheme.Helpers.NxBRE
{
    public abstract class Utils
    {
        public static double GetParticipantsCount(object listObj)
        {
            var returnValue = 0.0;

            var listObjType = listObj.GetType();

            var list = listObj as List<string>;
            if (list != null)
            {
                returnValue = (double)list.Count;
            }

            return returnValue;
        }
    }
}
