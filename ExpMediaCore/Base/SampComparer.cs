using ExpMedia.Application.ActivitiyFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.Base
{
    public class SampComparer : IEqualityComparer<ActivityView>
    {
        public bool Equals(ActivityView x, ActivityView y)
        {
            if (x.Id == y.Id && x.Title.ToLower() == y.Title.ToLower())
                return true;

            return false;
        }

        public int GetHashCode(ActivityView obj)
        {
            return obj.Id.GetHashCode();
        }
    }
    public static class EM
    {
        public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
        {
            return values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
        }
    }
}
