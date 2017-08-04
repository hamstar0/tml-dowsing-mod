using HamstarHelpers.TileHelpers;
using HamstarHelpers.UIHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Dowsing.Items {
	class DowsingRodItem : TileRodItem {
		public override void SetStaticDefaults() {
			var mymod = (DowsingMod)this.mod;

			this.DisplayName.SetDefault( "Dowsing Rod" );
			this.Tooltip.SetDefault( "Detects types of matter up to "+mymod.Config.Data.MaxDowsingRangeInTiles+" blocks in a line"
				+'\n'+"Right-click block to detect its type"
				+'\n'+"You're in for a shock if you don't find it fast!" );
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
			this.item.rare = 2;
			this.item.value = 22000;
			this.item.magic = true;
		}

		public override void AddRecipes() {
			var recipe = new DowsingRodItemRecipe( (DowsingMod)this.mod );
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

			sb.Draw( tile_tex, dest, new Rectangle( 16, 16, 16, 16 ), draw_color, 0f, Vector2.Zero, SpriteEffects.None, 0 );
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
			
			if( !TileHelpers.IsAir(tile) ) {
				if( !TileIdentityHelpers.IsObject( tile.type ) ) {
					string text = Lang.GetMapObjectName( Main.Map[tile_x, tile_y].Type );

					text = TileIdentityHelpers.GetTileName( item_info.TargetTileType );

					if( text == "" ) {
						Main.NewText( "Dowsing Rod now attuned to some kind of material...", RodItem.AttunedColor );
					} else {
						Main.NewText( "Dowsing Rod now attuned to any " + text, RodItem.AttunedColor );
					}

					item_info.TargetTileType = tile.type;

					RodItem.RenderDowseEffect( new Vector2(tile_x * 16, tile_y * 16), 5, Color.GreenYellow );
				} else {
					Main.NewText( "Dowsing Rod may only attune to basic materials.", Color.Yellow );
				}
			}
		}
	}



	class DowsingRodItemRecipe : ModRecipe {
		public DowsingRodItemRecipe( DowsingMod mymod ) : base( mymod ) {
			this.AddTile( 16 );   // Anvil
			this.AddIngredient( ItemID.ActiveStoneBlock, 4 );
			this.AddIngredient( ItemID.Feather, 1 );
			this.AddIngredient( ItemID.Lens, 2 );
			this.AddRecipeGroup( "Dowsing:EvilBiomeWood", 16 );
			this.SetResult( mymod.ItemType<DowsingRodItem>() );
		}

		public override bool RecipeAvailable() {
			var mymod = (DowsingMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return false; }

			return mymod.Config.Data.DowsingRodIsCraftable;
		}
	}
}
