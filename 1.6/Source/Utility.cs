using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CustomResourceReadout
{
    public static class Utility
    {
        private static readonly Dictionary<ResourceCounter, Dictionary<Tuple<ThingDef, ThingDef>, int>> countedAmountsStuff = new Dictionary<ResourceCounter, Dictionary<Tuple<ThingDef, ThingDef>, int>>();
        private static readonly Hashtable countAsResourceCache = new Hashtable();

        public static Dictionary<Tuple<ThingDef, ThingDef>, int> GetCountedAmountsStuff(this ResourceCounter counter)
        {
            if (!countedAmountsStuff.ContainsKey(counter))
            {
                countedAmountsStuff[counter] = new Dictionary<Tuple<ThingDef, ThingDef>, int>();
            }
            return countedAmountsStuff[counter];
        }

        public static bool CanAccept(this IEnumerable<ResourceReadoutItem> items, ResourceReadoutItem item)
        {
            if (item is ResourceReadoutLeaf leaf && items.OfType<ResourceReadoutLeaf>().Any(l => l != leaf && l.Def == leaf.Def && l.stuff == leaf.stuff))
            {
                return false;
            }
            return true;
        }

        public static bool CountAsResource(ThingDef def)
        {
            if (CustomResourceReadoutSettings.CustomOrPresetMode)
            {
                if (def.CountAsResource)
                {
                    return true;
                }
                if (!countAsResourceCache.ContainsKey(def))
                {
                    countAsResourceCache[def] = CustomResourceReadoutSettings.CurrentMode.Items.Any(i => i.ThingDefs.Contains(def));
                }
                return (bool)countAsResourceCache[def];
            }
            else
            {
                return def.CountAsResource;
            }
        }

        public static void ClearCaches()
        {
            countAsResourceCache.Clear();
        }
    }
}
