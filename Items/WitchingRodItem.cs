using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Dowsing.Items {
	class WitchingRodItem : TargetRodItem {
		public override void SetStaticDefaults() {
			var mymod = (DowsingMod)this.mod;

			this.DisplayName.SetDefault( "Witching Rod" );
			this.Tooltip.SetDefault( "Detects rare mobs up to " + mymod.Config.Data.MaxWitchingRangeInTiles + " blocks in a line"
				+ '\n' + "Several approach attempts may be needed."
			+ '\n' + "You're in for a shock if you don't find it fast!" );
		}

		public override void SetDefaults() {
			this.item.mana = 6;
			this.item.autoReuse = true;
			this.item.useStyle = 5;
			this.item.useAnimation = 16;
			this.item.useTime = 8;
			this.item.width = 38;
			this.item.height = 10;
			this.item.scale = 1f;
			this.item.shoot = 1;
			this.item.shootSpeed = 1;
			//this.item.UseSound = SoundID.Item13;
			this.item.noMelee = true;
			this.item.rare = 2;
			this.item.value = 22000;
			this.item.magic = true;
		}

		public override void AddRecipes() {
			var recipe = new WitchingRodItemRecipe( (DowsingMod)this.mod );
			recipe.AddRecipe();
		}


		////////////////

		protected override bool Dowse( Player player, Vector2 aiming_at ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			int range = mymod.Config.Data.MaxWitchingRangeInTiles;

			if( !modplayer.WitchingData.HasNpcTarget() ) {
				if( this.CastRareNpcDowse( player, aiming_at, range ) ) { return true; }
				return this.CastVirtualTargetDowse( player, aiming_at, range );
			} else {
				return this.CastNpcTargetDowse( player, aiming_at, modplayer.WitchingData.TargetNpcWho, range );
			}
		}

		public override void VirtualTargetIsDowsed( Player player ) {
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			modplayer.WitchingData.IsVirtualTargetDowsed = true;
		}
	}



	class WitchingRodItemRecipe : ModRecipe {
		public WitchingRodItemRecipe( DowsingMod mymod ) : base( mymod ) {
			this.AddTile( 16 );   // Anvil
			this.AddIngredient( mymod.GetItem<DowsingRodItem>(), 1 );
			this.AddIngredient( ItemID.GuideVoodooDoll, 1 );
			this.AddIngredient( ItemID.PurificationPowder, 10 );
			this.SetResult( mymod.ItemType<WitchingRodItem>() );
		}

		public override bool RecipeAvailable() {
			var mymod = (DowsingMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return false; }
			
			return mymod.Config.Data.WitchingRodIsCraftable;
		}
	}
}
