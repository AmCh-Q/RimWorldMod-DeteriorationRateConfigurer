using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Deterioration_Rate_Configurer
{
	public class Settings : ModSettings
	{
		public Dictionary<ThingDef, float> dflt_detValues = [];
		public Dictionary<string, float> detValues = [];
		public bool initialized;

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref detValues, "deteriorationRateValues", LookMode.Value, LookMode.Value);
			base.ExposeData();
			ApplyDeteriorationValues();
		}

		public void LoadDefaultValues()
		{
			dflt_detValues.Clear();
			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
			{
				if (def is null ||
					!def.useHitPoints ||
					def.category != ThingCategory.Item)
					continue;
				dflt_detValues[def] = def.GetStatValueAbstract(StatDefOf.DeteriorationRate);
			}
		}

		public void ApplyDeteriorationValues(bool init = false)
		{
			initialized |= init;
			if (!initialized)
				return;

			StatDef statDef = StatDefOf.DeteriorationRate;
			foreach ((ThingDef def, float dfltRate) in dflt_detValues)
			{
				float currRate = def.GetStatValueAbstract(statDef);
				float confRate = detValues.GetValueOrDefault(def.defName, dfltRate);
				if (confRate != currRate)
				{
					def.SetStatBaseValue(statDef, confRate);
#if DEBUG
					Log.Message("[Deterioration_Rate_Configurer]: " +
						def.defName + ": " + dflt_detValues[def] +
						" -> " + confRate);
#endif
				}
			}
		}

		public List<ThingDef> SearchDefs(string str)
		{
			IEnumerable<ThingDef> defs;
			if (str is null || str.Trim() == string.Empty)
				defs = dflt_detValues.Keys;
			else
				defs = dflt_detValues.Keys.Where(def
#if v1_3 || v1_4 || v1_5
					=> def.defName.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) >= 0
					|| def.label.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) >= 0);
#else
					=> def.defName.Contains(str, StringComparison.InvariantCultureIgnoreCase)
					|| def.label.Contains(str, StringComparison.InvariantCultureIgnoreCase));
#endif
			return [.. defs.OrderBy(def => def.label).ThenBy(def => def.defName)];
		}
	}
}
