using Dowsing.Buffs;
using HamstarHelpers.PlayerHelpers;
using HamstarHelpers.TileHelpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Dowsing.Items {
	abstract class RodItem : ModItem {
		public static readonly Color AttunedColor = new Color( 50, 255, 130 );

		protected int CurrentBeamTravelDistance = 0;



		public override bool PreDrawInInventory( SpriteBatch sb, Vector2 position, Rectangle frame, Color draw_color, Color item_color, Vector2 origin, float scale ) {
			int x = (int)(position.X - (0.125f * (float)frame.Width * scale));
			int y = (int)(position.Y - (0.125f * (float)frame.Height * scale));
			int width = (int)(1.25f * frame.Width * scale);
			int height = (int)(1.25f * frame.Height * scale);
			var dest = new Rectangle( x, y, width, height );

			sb.Draw( Main.itemTexture[this.item.type], dest, frame, draw_color );
			return false;
		}

		public override void PostDrawInInventory( SpriteBatch sb, Vector2 position, Rectangle frame, Color draw_color, Color item_color, Vector2 origin, float scale ) {
			var item_info = this.item.GetGlobalItem<RodItemInfo>();
			if( item_info.DowsingBlockType == -1 ) { return; }
			if( item_info.DowsingBlockType >= Main.tileTexture.Length ) { return; }

			var tile_tex = Main.tileTexture[ item_info.DowsingBlockType ];
			position.Y -= 8f * scale;
			position.X += (float)((frame.Width / 2) - 16) * scale;

			var dest = new Rectangle( (int)position.X, (int)position.Y, (int)(32f * scale), (int)(32f * scale) );

//sHamstarHelpers.MiscHelpers.DebugHelpers.Display["inv"] = frame+" "+position+" tex: "+tile_tex.Width+","+tile_tex.Height;
			//sb.Draw( tile_tex, position, this.GetFrame(), draw_color, 0, Vector2.Zero, scale * 2, SpriteEffects.None, 0 );
			sb.Draw( tile_tex, dest, this.GetFrame(), draw_color, 0f, Vector2.Zero, SpriteEffects.None, 0 );
		}


		public override bool Shoot( Player player, ref Vector2 position, ref float speed_x, ref float speed_y, ref int type, ref int damage, ref float knock_back ) {
			var myitem = this.item.GetGlobalItem<RodItemInfo>();
			if( myitem.CooldownTimer > 0 ) { return false; }
			
			if( Main.rand.NextFloat() < 0.46f ) {
				player.statMana++;
			}

			if( this.Dowse( player, Main.MouseWorld ) ) {
				this.RenderRodCastFX( player );
				myitem.CooldownTimer = 12;
			}

			return false;
		}


		public override void ModifyTooltips( List<TooltipLine> tooltips ) {
			var item_info = this.item.GetGlobalItem<RodItemInfo>();

			if( item_info.DowsingBlockType != -1 ) {
				string tile_name = TileIdentityHelpers.GetTileName( item_info.DowsingBlockType );

				if( tile_name != "" ) {
					var tip = new TooltipLine( this.mod, "dowse_target", "Attuned to any " + tile_name );
					tip.overrideColor = RodItem.AttunedColor;

					tooltips.Add( tip );
				}
			}
		}


		////////////////

		abstract public Rectangle GetFrame();
		

		abstract protected bool Dowse( Player player, Vector2 aiming_at );


		abstract public void ChooseDowsingTypeAtMouse( Player player );


		protected bool CastBlockDowse( Player player, Vector2 aiming_at, int tile_type ) {
			var mymod = (DowsingMod)this.mod;
			var myworld = this.mod.GetModWorld<DowsingWorld>();
			bool dowsed = false;

			this.CurrentBeamTravelDistance = 0;

			this.CastDowseBeamWithinCone( player, aiming_at, new Utils.PerLinePoint( delegate( int tile_x, int tile_y ) {
				if( this.CurrentBeamTravelDistance >= mymod.Config.Data.MaxDowsingRangeInTiles ) {
					return false;
				}

				dowsed = myworld.ApplyDowseIfBlockIsTarget( tile_x, tile_y, tile_type );

				if( dowsed ) {
					PsychokineticChargeDebuff.ApplyForTilesIfAnew( mymod, player );
					this.RenderRodHitFX( player, tile_x, tile_y );
				} else {
					this.CurrentBeamTravelDistance++;
					if( TileHelpers.IsSolid( Framing.GetTileSafely( tile_x, tile_y ), false, false ) ) {
						this.CurrentBeamTravelDistance++;
					}
				}

				if( (mymod.DEBUGFLAGS & 1) != 0 ) {
					var dust = Dust.NewDustPerfect( new Vector2(tile_x * 16, tile_y * 16), 259, Vector2.Zero, 0, Color.Red, 0.75f );
					dust.noGravity = true;
				}
				return !dowsed;
			} ) );

			return dowsed;
		}


		protected void CastDowseBeamWithinCone( Player player, Vector2 aim_at, Utils.PerLinePoint plot ) {
			var mymod = (DowsingMod)this.mod;
			int max_range = mymod.Config.Data.MaxDowsingRangeInTiles;
			double ray_angle = mymod.Config.Data.DowsingRayAngle;
			Vector2 center = player.MountedCenter;

			double rads = Math.Atan2( aim_at.Y - center.Y, aim_at.X - center.X );
			double rad_off = ((Main.rand.NextDouble() - 0.5d) * ray_angle) * (Math.PI / 180d);
			double rad_rand = rads + rad_off;
			Vector2 end = center + (Vector2.UnitX.RotatedBy( rad_rand ) * max_range * 16);

			Utils.PlotTileLine( player.position, end, 2, plot );
		}


		////////////////

		public void RenderRodCastFX( Player player ) {
			var mymod = (DowsingMod)this.mod;
			Vector2 pos = PlayerItemHelpers.TipOfHeldItem( player );
			int particles = Math.Max( 2, ((mymod.Config.Data.MaxDowsingRangeInTiles / 3) - this.CurrentBeamTravelDistance) / 2);
			int dust_type = 264;

			for( int i = 0; i < particles; i++ ) {
				var dust = Dust.NewDustDirect( pos, 1, 1, dust_type, 0f, 0f, 0, Color.YellowGreen, 0.8f );
				dust.noGravity = true;
				dust.noLight = true;
				dust.fadeIn = 0.8f;
			}

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				HamstarHelpers.MiscHelpers.DebugHelpers.Display["cast"] = "particles: " + particles+", dist: "+ this.CurrentBeamTravelDistance;
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
				HamstarHelpers.MiscHelpers.DebugHelpers.Display["cast hit"] = "at: " + tile_x + "," + tile_y;
			}
		}
	}
}
