using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

[assembly: AssemblyVersion("1.0.1.0")]
namespace Deterioration_Rate_Configurer
{
	public partial class DeteriorationRateConfigurer : Mod
	{
		private readonly Settings settings;
		public DeteriorationRateConfigurer(ModContentPack content)
			: base(content)
		{
			settings = GetSettings<Settings>();
			LongEventHandler.QueueLongEvent(delegate
			{
				settings.LoadDefaultValues();
				settings.ApplyDeteriorationValues(true);
			}, "Config_Deterioration_Rate", false, null, true);
		}
		public override string SettingsCategory()
			=> "DRConf.Name".Translate();
	}
}
