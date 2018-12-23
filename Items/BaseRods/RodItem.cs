using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.PlayerHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Dowsing.Items {
	abstract class RodItem : ModItem {
		public static readonly Color AttunedColor = new Color( 50, 255, 130 );

		public static void RenderDowseEffect( Vector2 position, int stack, Color color ) {
			int alpha = Math.Max( 192 - (stack * 10), 32 );

			int puff_who = Dust.NewDust( position, 6, 6, 16, 0f, 0f, alpha, color, 2f );
			Main.dust[puff_who].velocity *= 0.1f * stack;
		}


		////////////////

		protected int CurrentBeamTravelDistance = 0;


		////////////////

		public override bool PreDrawInInventory( SpriteBatch sb, Vector2 position, Rectangle frame, Color draw_color, Color item_color, Vector2 origin, float scale ) {
			int x = (int)(position.X - (0.125f * (float)frame.Width * scale));
			int y = (int)(position.Y - (0.125f * (float)frame.Height * scale));
			int width = (int)(1.25f * frame.Width * scale);
			int height = (int)(1.25f * frame.Height * scale);
			var dest = new Rectangle( x, y, width, height );

			sb.Draw( Main.itemTexture[this.item.type], dest, frame, draw_color );
			return false;
		}


		public override bool Shoot( Player player, ref Vector2 position, ref float speed_x, ref float speed_y, ref int type, ref int damage, ref float knock_back ) {
			var mymod = (DowsingMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return false; }

			var myitem = this.item.GetGlobalItem<RodItemInfo>();
			if( myitem.CastCooldownTimer > 0 ) { return false; }
			
			if( Main.rand.NextFloat() < 0.46f ) {
				player.statMana++;
			}

			if( this.Dowse( player, Main.MouseWorld ) ) {
				Main.PlaySound( SoundID.Item54, player.Center );
				this.RenderRodCastFX( player );
				myitem.CastCooldownTimer = 12;
			} else {
				Main.PlaySound( SoundID.Item32, player.Center );
			}

			return false;
		}


		////////////////

		abstract protected bool Dowse( Player player, Vector2 aiming_at );


		public virtual void ChooseDowsingTypeAtMouse( Player player ) { }


		////////////////

		protected void CastDowseBeamWithinCone( Player player, Vector2 aim_at, Utils.PerLinePoint plot ) {
			var mymod = (DowsingMod)this.mod;
			int max_range_tiles = mymod.Config.Data.MaxDowsingRangeInTiles;
			double ray_angle = mymod.Config.Data.DowsingRayAngle;
			Vector2 center = player.MountedCenter;

			double rads = Math.Atan2( aim_at.Y - center.Y, aim_at.X - center.X );
			double rad_off = ((Main.rand.NextDouble() - 0.5d) * ray_angle) * (Math.PI / 180d);
			double rad_rand = rads + rad_off;
			Vector2 end = center + (Vector2.UnitX.RotatedBy(rad_rand) * max_range_tiles * 16);

			Utils.PlotTileLine( center, end, 2, plot );
		}


		////////////////

		public void RenderRodCastFX( Player player ) {
			var mymod = (DowsingMod)this.mod;
			Vector2 pos = PlayerItemHelpers.TipOfHeldItem( player );
			int particles = Math.Max( 2, ((mymod.Config.Data.MaxDowsingRangeInTiles / 3) - this.CurrentBeamTravelDistance) / 2 );
			int dust_type = 264;

			for( int i = 0; i < particles; i++ ) {
				var dust = Dust.NewDustDirect( pos, 1, 1, dust_type, 0f, 0f, 0, Color.YellowGreen, 0.8f );
				dust.noGravity = true;
				dust.noLight = true;
				dust.fadeIn = 0.8f;
			}

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				DebugHelpers.Print( "cast from", "particles: " + particles+", dist: "+ this.CurrentBeamTravelDistance, 20 );
			}
		}


		public void RenderRodHitFX( Player player, int tile_x, int tile_y ) {
			var mymod = (DowsingMod)this.mod;
			var pos = new Vector2( tile_x * 16, tile_y * 16 );
			int dust_type = 259;

			for( int i = 0; i < 20; i++ ) {
				int dust_idx = Dust.NewDust( pos, 16, 16, dust_type, 0f, 0f, 0, Color.YellowGreen, 1f );
				Main.dust[ dust_idx ].noGravity = true;
			}

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				DebugHelpers.Print( "cast hit", "at: " + tile_x + ", " + tile_y, 20 );
			}
		}
	}
}
