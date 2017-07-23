using System;


namespace Dowsing {
	public class ConfigurationData {
		public readonly static Version CurrentVersion = new Version( 0, 5, 0 );


		public string VersionSinceUpdate = "";

		public bool Enabled = true;
		public int DEBUGFLAGS = 0;	// +1: info, +2: reset dowsings

		public bool DowsingRodIsCraftable = true;
		public bool ViningRodIsCraftable = true;
		public bool WitchingRodIsCraftable = true;
		public bool DiviningRodIsCraftable = true;

		public int PsychokineticChargeDuration = 90 * 60;
		public int PsychokineticBacklashDamageBase = 100;
		public int PsychokineticBacklashDamageStack = 20;

		public int MaxDowsingRangeInTiles = 200;
		public double DowsingRayAngle = 10;

		public int PsychokineticDischargeTileRange = 8;



		////////////////

		public bool UpdateToLatestVersion() {
			var new_config = new ConfigurationData();
			var vers_since = this.VersionSinceUpdate != "" ?
				new Version( this.VersionSinceUpdate ) :
				new Version();

			if( vers_since >= ConfigurationData.CurrentVersion ) {
				return false;
			}

			this.VersionSinceUpdate = ConfigurationData.CurrentVersion.ToString();

			return true;
		}
	}
}
