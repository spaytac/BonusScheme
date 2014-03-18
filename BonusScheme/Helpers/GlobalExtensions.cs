using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BonusScheme.Helpers
{
    public static class GlobalExtensions
    {
        internal static string AddUriPath(this string url, string path)
        {
            return String.Format("{0}{1}{2}", url, url.EndsWith("/") ? "" : "/", path.StartsWith("/") ? path.Remove(0, 1) : path);
        }
    }
}
