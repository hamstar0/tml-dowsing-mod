using HamstarHelpers.Utilities.Config;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Dowsing {
    class DowsingMod : Mod {
		public JsonConfig<ConfigurationData> Config { get; private set; }
		public int DEBUGFLAGS { get; private set; } // +1: info, +2: reset dowsings


		////////////////

		public DowsingMod() {
			this.Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
			this.DEBUGFLAGS = 0;

			string filename = "Dowsing Config.json";
			this.Config = new JsonConfig<ConfigurationData>( filename, "Mod Configs", new ConfigurationData() );
		}

		public override void Load() {
			this.LoadConfig();
		}

		private void LoadConfig() {
			if( !this.Config.LoadFile() ) {
				this.Config.SaveFile();
			}

			if( this.Config.Data.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "Dowsing mod updated to " + ConfigurationData.CurrentVersion.ToString() );
				this.Config.SaveFile();
			}

			this.DEBUGFLAGS = this.Config.Data.DEBUGFLAGS;
		}


		////////////////

		public override void AddRecipeGroups() {
			RecipeGroup group = new RecipeGroup( () => Lang.misc[37] + " Evil Biome Wood", new int[] { ItemID.Ebonwood, ItemID.Shadewood } );
			RecipeGroup.RegisterGroup( "DowsingMod:EvilBiomeWood", group );
		}
	}
}
