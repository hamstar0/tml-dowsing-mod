using Dowsing.Buffs;
using HamstarHelpers.TileHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Dowsing.Items {
	abstract class TileRodItem : RodItem {
		public override bool PreDrawInInventory( SpriteBatch sb, Vector2 position, Rectangle frame, Color draw_color, Color item_color, Vector2 origin, float scale ) {
			int x = (int)(position.X - (0.125f * (float)frame.Width * scale));
			int y = (int)(position.Y - (0.125f * (float)frame.Height * scale));
			int width = (int)(1.25f * frame.Width * scale);
			int height = (int)(1.25f * frame.Height * scale);
			var dest = new Rectangle( x, y, width, height );

			sb.Draw( Main.itemTexture[this.item.type], dest, frame, draw_color );
			return false;
		}
		

		public override void ModifyTooltips( List<TooltipLine> tooltips ) {
			var item_info = this.item.GetGlobalItem<RodItemInfo>();

			if( item_info.TargetTileType != -1 ) {
				string tile_name = TileIdentityHelpers.GetTileName( item_info.TargetTileType );

				if( tile_name != "" ) {
					var tip = new TooltipLine( this.mod, "dowse_target", "Attuned to any " + tile_name );
					tip.overrideColor = RodItem.AttunedColor;

					tooltips.Add( tip );
				}
			}
		}


		////////////////
		
		protected bool CastBlockDowse( Player player, Vector2 aiming_at, int tile_range, int tile_type ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			bool dowsed = false;
			int traveled = 0;

			this.CurrentBeamTravelDistance = 0;

			this.CastDowseBeamWithinCone( player, aiming_at, new Utils.PerLinePoint( delegate ( int tile_x, int tile_y ) {
				if( !TileHelpers.IsWithinMap( tile_x, tile_y ) || traveled >= tile_range ) {
					return false;
				}

				dowsed = modplayer.TileData.ApplyDowseIfTileIsTarget( tile_x, tile_y, tile_type );

				if( dowsed ) {
					PsychokineticChargeDebuff.ApplyForTilesIfAnew( mymod, player );
					this.RenderRodHitFX( player, tile_x, tile_y );
				} else {
					traveled++;
					if( TileHelpers.IsSolid( Framing.GetTileSafely( tile_x, tile_y ), false, false ) ) {
						traveled++;
					}
				}
				this.CurrentBeamTravelDistance = traveled;

				if( (mymod.DEBUGFLAGS & 1) != 0 ) {
					var dust = Dust.NewDustPerfect( new Vector2( tile_x * 16, tile_y * 16 ), 259, Vector2.Zero, 0, Color.Red, 0.75f );
					dust.noGravity = true;
				}
				return !dowsed;
			} ) );

			return dowsed;
		}
	}
}
