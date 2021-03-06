﻿using HamstarHelpers.Helpers.TileHelpers;
using HamstarHelpers.Helpers.UIHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Dowsing.Items {
	class ViningRodItem : TileRodItem {
		public override void SetStaticDefaults() {
			var mymod = (DowsingMod)this.mod;

			this.DisplayName.SetDefault( "Vining Rod" );
			this.Tooltip.SetDefault( "Detects specific objects up to "+mymod.Config.Data.MaxDowsingRangeInTiles+" blocks in a line"
				+ '\n' + "Right-click object to detect its type"
				+ '\n' + "You're in for a shock if you don't find it fast!" );
		}

		public override void SetDefaults() {
			this.item.mana = 1;
			this.item.autoReuse = true;
			this.item.useStyle = 5;
			this.item.useAnimation = 2;
			this.item.useTime = 2;
			this.item.width = 40;
			this.item.height = 12;
			this.item.scale = 1f;
			this.item.shoot = 1;
			this.item.shootSpeed = 2;
			//this.item.UseSound = SoundID.Item13;
			this.item.noMelee = true;
			this.item.rare = 3;
			this.item.value = 30000;
			this.item.magic = true;
		}

		public override void AddRecipes() {
			var recipe = new ViningRodItemRecipe( (DowsingMod)this.mod );
			recipe.AddRecipe();
		}
		
		////////////////

		public override void PostDrawInInventory( SpriteBatch sb, Vector2 position, Rectangle frame, Color draw_color, Color item_color, Vector2 origin, float scale ) {
			var item_info = this.item.GetGlobalItem<RodItemInfo>();
			if( item_info.TargetTileType == -1 ) { return; }
			if( item_info.TargetTileType >= Main.tileTexture.Length ) { return; }

			var tile_tex = Main.tileTexture[item_info.TargetTileType];
			position.Y -= 8f * scale;
			position.X += (float)((frame.Width / 2) - 16) * scale;

			var dest = new Rectangle( (int)position.X, (int)position.Y, (int)(32f * scale), (int)(32f * scale) );

			sb.Draw( tile_tex, dest, new Rectangle(0, 0, 32, 32), draw_color, 0f, Vector2.Zero, SpriteEffects.None, 0 );
		}

		
		////////////////

		protected override bool Dowse( Player player, Vector2 aiming_at ) {
			var mymod = (DowsingMod)this.mod;
			var myitem = this.item.GetGlobalItem<RodItemInfo>();
			if( myitem.TargetTileType == -1 ) { return false; }

			return this.CastBlockDowse( player, aiming_at, mymod.Config.Data.MaxDowsingRangeInTiles, myitem.TargetTileType );
		}


		public override void ChooseDowsingTypeAtMouse( Player player ) {
			var item_info = this.item.GetGlobalItem<RodItemInfo>();
			Rectangle screen_frame = UIHelpers.GetWorldFrameOfScreen();
			Vector2 screen_mouse = UIHelpers.ConvertToScreenPosition( new Vector2( Main.mouseX, Main.mouseY ) + Main.screenPosition );
			Vector2 world_mouse = screen_mouse + new Vector2( screen_frame.X, screen_frame.Y );
			int tile_x = (int)(world_mouse.X / 16);
			int tile_y = (int)(world_mouse.Y / 16);
			Tile tile = Framing.GetTileSafely( tile_x, tile_y );

			if( !TileHelpers.IsAir( tile ) ) {
				if( TileIdentityHelpers.IsObject( tile.type ) ) {
					string text = Lang.GetMapObjectName( Main.Map[tile_x, tile_y].Type );

					text = TileIdentityHelpers.GetVanillaTileName( item_info.TargetTileType );

					if( text == "" ) {
						Main.NewText( "Vining Rod now attuned to some kind of material...", RodItem.AttunedColor );
					} else {
						Main.NewText( "Vining Rod now attuned to any " + text, RodItem.AttunedColor );
					}

					item_info.TargetTileType = tile.type;

					RodItem.RenderDowseEffect( new Vector2(tile_x * 16, tile_y * 16), 5, Color.GreenYellow );
				} else {
					Main.NewText( "Vining Rod may only attune to objects.", Color.Yellow );
				}
			}
		}
	}



	class ViningRodItemRecipe : ModRecipe {
		public ViningRodItemRecipe( DowsingMod mymod ) : base( mymod ) {
			this.AddTile( 16 );   // Anvil
			this.AddIngredient( mymod.GetItem<DowsingRodItem>(), 1 );
			this.AddIngredient( ItemID.Vine, 3 );
			this.AddIngredient( ItemID.Amber, 3 );
			this.SetResult( mymod.ItemType<ViningRodItem>() );
		}

		public override bool RecipeAvailable() {
			var mymod = (DowsingMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return false; }

			return mymod.Config.Data.ViningRodIsCraftable;
		}
	}
}
