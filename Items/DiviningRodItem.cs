using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Dowsing.Items {
	class DiviningRodItem : RodItem {
		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Divining Rod" );
			//this.Tooltip.SetDefault( "Detects a mob with a rare item up to 250 blocks"
			//	+ '\n' + "Right-click a mob to detect its type"
			//	+ '\n' + "You're in for a shock if you don't find them fast!" );
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
			var recipe = new DiviningRodItemRecipe( (DowsingMod)this.mod );
			recipe.AddRecipe();
		}

		////////////////

		public override Rectangle GetFrame() {
			return new Rectangle();
		}

		protected override bool Dowse( Player player, Vector2 aiming_at ) {
			throw new NotImplementedException();
		}

		public override void ChooseDowsingTypeAtMouse( Player player ) {
			// TODO: Implement mob choosing
		}
	}



	class DiviningRodItemRecipe : ModRecipe {
		public DiviningRodItemRecipe( DowsingMod mymod ) : base( mymod ) {
			this.AddTile( 16 );   // Anvil
			this.AddIngredient( mymod.GetItem<ViningRodItem>(), 1 );
			this.AddIngredient( mymod.GetItem<WitchingRodItem>(), 1 );
			this.AddIngredient( ItemID.Throne, 1 );
			this.SetResult( mymod.ItemType<DiviningRodItem>() );
		}

		public override bool RecipeAvailable() {
			var mymod = (DowsingMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return false; }

			return false;
			//return mymod.Config.Data.DiviningRodIsCraftable;
		}
	}
}
