using System;


namespace Dowsing {
	public class ConfigurationData {
		public readonly static Version CurrentVersion = new Version( 1, 1, 0 );


		public string VersionSinceUpdate = "";

		public bool Enabled = true;
		public int DEBUGFLAGS = 0;	// +1: info, +2: reset dowsings

		public bool DowsingRodIsCraftable = true;
		public bool ViningRodIsCraftable = true;
		public bool WitchingRodIsCraftable = true;
		public bool DiviningRodIsCraftable = true;

		public int PsychokineticChargeDurationForTiles = 90 * 60; // 1.5m
		public int PsychokineticChargeDurationForTargets = 90 * 60; // 1.5m

		public int PsychokineticBacklashTileDamageBase = 100;
		public int PsychokineticBacklashTileDamageStack = 20;
		public int PsychokineticBacklashTargetDamageBase = 400;

		public int PsychokineticDegaussingTileRange = 8;
		public int PsychokineticDegaussingNpcRangeInTiles = 12;

		public double DowsingRayAngle = 10;
		public int MaxDowsingRangeInTiles = 250;
		public int MaxViningRangeInTiles = 250;
		public int MaxWitchingRangeInTiles = 200;
		public int MaxDiviningRangeInTiles = 200;

		public int VirtualTargetApproachTriggerInTiles = 10;
		public int MaxVirtualTargetRangeInTiles = 150;

		public int VirtualTargetPsychChargeAddedTime = 3 * 60;



		////////////////

		public bool UpdateToLatestVersion() {
			var new_config = new ConfigurationData();
			var vers_since = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( vers_since >= ConfigurationData.CurrentVersion ) {
				return false;
			}

			if( vers_since < new Version( 1, 1, 0 ) ) {
				if( ConfigurationData._1_0_0_MaxDowsingRangeInTiles == this.MaxDowsingRangeInTiles ) {
					this.MaxDowsingRangeInTiles = new_config.MaxDowsingRangeInTiles;
				}
			}

			this.VersionSinceUpdate = ConfigurationData.CurrentVersion.ToString();

			return true;
		}

		////////////////

		public string _OLD_SETTINGS_BELOW = "";

		public int PsychokineticBacklashDamageBase = 100;
		public int PsychokineticBacklashDamageStack = 20;
		public int PsychokineticChargeDuration = 90 * 60;
		public int PsychokineticChargeDurationForDowsing = 90 * 60; // 1.5m
		public readonly static int PsychokineticDischargeTileRange = 8;

		public readonly static int _1_0_0_MaxDowsingRangeInTiles = 200;
	}
}
