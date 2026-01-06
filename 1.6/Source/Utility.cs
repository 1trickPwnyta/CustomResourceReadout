using System.Collections.Generic;
using System.Linq;

namespace CustomResourceReadout
{
    public static class Utility
    {
        public static bool CanAccept(this IEnumerable<ResourceReadoutItem> items, ResourceReadoutItem item)
        {
            if (item is ResourceReadoutLeaf leaf && items.OfType<ResourceReadoutLeaf>().Any(l => l != leaf && l.Def == leaf.Def && l.stuff == leaf.stuff))
            {
                return false;
            }
            return true;
        }
    }
}
