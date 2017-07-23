using Dowsing.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;


namespace Dowsing.Buffs {
	class PsychokineticChargeDebuff : ModBuff {
		public static PlayerDeathReason GetDeathMessage( Player player ) {
			return PlayerDeathReason.ByCustomReason( player.name + "'s head exploded to psychokinetic backfire." );
		}



		private int FxTimer = 0;


		public static void ApplyForTilesIfAnew( DowsingMod mymod, Player player ) {
			int buff_type = mymod.BuffType<PsychokineticChargeDebuff>();
			int buff_idx = player.FindBuffIndex( buff_type );
			if( buff_idx == -1 ) {
				player.AddBuff( buff_type, mymod.Config.Data.PsychokineticChargeDuration );
			}
		}



		public override void SetDefaults() {
			this.DisplayName.SetDefault( "Psychokinetic Charge" );
			this.Description.SetDefault( "A mysterious energy buildup lingers ominously" );

			Main.debuff[this.Type] = true;
		}


		public override void Update( Player player, ref int buff_idx ) {
			var mymod = (DowsingMod)this.mod;
			var modworld = mymod.GetModWorld<DowsingWorld>();
			int count = modworld.CountDowsings();

			if( player.whoAmI == Main.myPlayer ) {
				int range = mymod.Config.Data.PsychokineticDischargeTileRange;
				var tiles = modworld.DegaussNearbyTiles( range );
				PsiBoltProjectile.Fire( mymod, player, tiles, false );
			}

			if( count == 0 ) {
				player.DelBuff( buff_idx );
				return;
			}

			this.RenderChargeFX( player, count );

			if( player.buffTime[buff_idx] == 1 ) {
				var tiles = modworld.DegaussNearbyTiles( -1 );
				PsiBoltProjectile.Fire( mymod, player, tiles, true );

				int damage = mymod.Config.Data.PsychokineticBacklashDamageBase;
				damage += (count - 1) * mymod.Config.Data.PsychokineticBacklashDamageStack;

				player.Hurt( PsychokineticChargeDebuff.GetDeathMessage(player), damage, 0 );
			}
		}


		public void PsiBolt( Player player, Vector2 from_pos ) {
			if( Main.netMode == 1 ) { return; }

			float base_speed = 16f;
			float dist_x = player.position.X + (float)player.width * 0.5f - from_pos.X;
			float dist_y = player.position.Y + (float)player.height * 0.5f - from_pos.Y;
			float hypot = (float)Math.Sqrt( (double)dist_x * (double)dist_x + (double)dist_y * (double)dist_y );
			float num7 = base_speed / hypot;

			float speed_x = dist_x * num7;
			float speed_y = dist_y * num7;
			int damage = 0;
			int proj_type = 293;

			int idx = Projectile.NewProjectile( from_pos.X, from_pos.Y, speed_x, speed_y, proj_type, damage, 0.0f, Main.myPlayer, 0.0f, 0.0f );
			Projectile proj = Main.projectile[idx];
			proj.timeLeft = 300;
		}

		////////////////

		public void RenderChargeFX( Player player, int intensity ) {
			if( intensity == 0 ) { return; }

			if( this.FxTimer == 0 ) {
				this.FxTimer = (int)Math.Max( 24d * (1f - (double)intensity/30d), 0 );

				var mymod = (DowsingMod)this.mod;
				var modworld = mymod.GetModWorld<DowsingWorld>();
				var pos = new Vector2( player.position.X, player.position.Y - 12 );
				float scale = 0.7f + (0.01f * intensity);
				int dust_type = 110;

				int puff_who = Dust.NewDust( player.position, player.width, player.height, dust_type, player.velocity.X, player.velocity.Y, 0, Color.YellowGreen, scale );
				Main.dust[puff_who].noGravity = true;

				if( (mymod.Config.Data.DEBUGFLAGS & 1) != 0 ) {
					HamstarHelpers.MiscHelpers.DebugHelpers.Display["charge"] = "FxTimer " + this.FxTimer+ ", scale " + scale+ ", intensity " + intensity;
				}
			} else {
				this.FxTimer--;
			}
		}
	}
}
