using HamstarHelpers.Components.Config;
using HamstarHelpers.Helpers.DebugHelpers;
using System;
using System.IO;
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
			try {
				if( !this.Config.LoadFile() ) {
					this.Config.SaveFile();
				}
			} catch( Exception e ) {
				LogHelpers.Log( e.Message );
				this.Config.SaveFile();
			}

			if( this.Config.Data.UpdateToLatestVersion() ) {
				ErrorLogger.Log( "Dowsing mod updated to " + ConfigurationData.CurrentVersion.ToString() );
				this.Config.SaveFile();
			}

			this.DEBUGFLAGS = this.Config.Data.DEBUGFLAGS;
		}

		////////////////

		public override void HandlePacket( BinaryReader reader, int whoAmI ) {
			DowsingNetProtocol.RoutePacket( this, reader );
		}


		////////////////

		public override void AddRecipeGroups() {
			if( !this.Config.Data.Enabled ) { return; }

			RecipeGroup group = new RecipeGroup( () => Lang.misc[37] + " Evil Biome Wood", new int[] { ItemID.Ebonwood, ItemID.Shadewood } );
			RecipeGroup.RegisterGroup( "Dowsing:EvilBiomeWood", group );
		}
	}
}
