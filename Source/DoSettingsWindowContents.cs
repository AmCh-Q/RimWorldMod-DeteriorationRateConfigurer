using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Verse;

namespace Deterioration_Rate_Configurer
{
	public partial class DeteriorationRateConfigurer : Mod
	{
		private Vector2 scrollPos = new(0f, 0f);
		private string searchText = string.Empty;
		private string? searchCache;
		private List<ThingDef>? defsFound;
		public override void DoSettingsWindowContents(Rect inRect)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Listing_Standard search_rect = new();
			search_rect.Begin(new Rect(inRect.x, 48f, inRect.width, 48f));
			search_rect.Label("DRConf.Search1".Translate());
			searchText = search_rect.TextEntry(searchText);
			if (defsFound is null || searchText != searchCache)
			{
				searchCache = searchText;
				defsFound = settings.SearchDefs(searchText);
			}
			int defsCount = defsFound.Count;
			int halfCount = defsCount / 2 + defsCount % 2;
			search_rect.End();

			Listing_Standard result_rect = new();
			result_rect.Begin(new Rect(inRect.width - 140f, 48f, 140f, 48f));
			result_rect.Label(string.Concat("DRConf.Search2".Translate(), defsCount.ToString(culture)));
			result_rect.End();

			Rect outRect = new(inRect.x, 108f, inRect.width, inRect.height - 80f);
			Rect scroll_rect = new(inRect.x + 10f, 180f,
				inRect.width - 30f, halfCount * 60f);
			Widgets.BeginScrollView(outRect, ref scrollPos, scroll_rect);
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;

			Listing_Standard left_list = new();
			left_list.Begin(scroll_rect.LeftPart(0.48f).Rounded());
			foreach (ThingDef def in defsFound.Take(halfCount))
			{
				string label = def.LabelCap;
				if (label.NullOrEmpty())
					label = def.defName;
				else
					label = string.Concat(label, " (", def.defName, ")");
				AddDefSetting(left_list,
					settings.detValues,
					def.defName,
					label,
					settings.dflt_detValues[def]);
			}
			left_list.End();

			Listing_Standard right_list = new();
			right_list.Begin(scroll_rect.RightPart(0.48f).Rounded());
			foreach (ThingDef def in defsFound.Skip(halfCount))
			{
				string label = def.LabelCap;
				if (label.NullOrEmpty())
					label = def.defName;
				else// if (!label.Equals(def.defName, StringComparison.InvariantCultureIgnoreCase))
					label = string.Concat(label, " (", def.defName, ")");
				AddDefSetting(right_list,
					settings.detValues,
					def.defName,
					label,
					settings.dflt_detValues[def]);
			}
			right_list.End();

			Text.Anchor = anchor;
			Widgets.EndScrollView();
			base.DoSettingsWindowContents(inRect);
		}

		private static void AddDefSetting(
			Listing_Standard ls,
			Dictionary<string, float> detValues,
			string defName,
			string label,
			float dfltVal)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;
			float val = detValues.GetValueOrDefault(defName, dfltVal);
			string buffer = val.ToString(culture);

			//ls.TextFieldNumericLabeled(label + ":  ", ref val, ref buffer, 0f, 1000000f);
			Rect TextFieldNumericRect = ls.GetRect(Text.LineHeight);
			if (!ls.BoundingRectCached.HasValue ||
				TextFieldNumericRect.Overlaps(ls.BoundingRectCached.Value))
			{
				Rect LeftRect = TextFieldNumericRect.LeftPart(0.85f).Rounded();
				Rect RightRect = TextFieldNumericRect.RightPart(0.15f).Rounded();
				Widgets.LabelFit(LeftRect, label);
				Widgets.TextFieldNumeric(RightRect, ref val, ref buffer, 0f, 1000000f);
			}
			ls.Gap(ls.verticalSpacing);

			TooltipHandler.TipRegion(new Rect(0f, ls.CurHeight - 32f, ls.ColumnWidth, 48f), label);
			float valLog = Mathf.Log10(val);
			val = CustomRound(Mathf.Pow(10f, ls.Slider(valLog, -2f, 3f)));
			if (val != dfltVal)
				detValues[defName] = val;
			else
				detValues.Remove(defName);
			ls.Gap(6f);
		}

		private static float CustomRound(float num)
		{
			float mult = 1f;
			if (num < 0.995f)
			{
				num *= 100f;
				mult = 0.01f;
			}
			else if (num < 9.95f)
			{
				num *= 10f;
				mult = 0.1f;
			}
			return mult * Mathf.Round(num);
		}
	}
}
