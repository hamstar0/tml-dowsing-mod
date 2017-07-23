using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Dowsing.Buffs;


namespace Dowsing.Projectiles {
	class PsiBoltProjectileInfo : GlobalProjectile {
		public override bool InstancePerEntity { get { return true; } }
		
		public PsiBoltProjectileInfo() : base() { }

		public override GlobalProjectile Clone() {
			var clone = new PsiBoltProjectileInfo();
			clone.IsHostile = this.IsHostile;
			return clone;
		}

		////////////////


		public bool IsHostile = false;
	}


	class PsiBoltProjectile : ModProjectile {
		public static void Fire( DowsingMod mymod, Player player, IDictionary<int, ISet<int>> from_tiles, bool hostile ) {
			int proj_type = mymod.ProjectileType<PsiBoltProjectile>();

			foreach( var kv in from_tiles ) {
				int x = kv.Key * 16;
				foreach( int tile_y in kv.Value ) {
					int y = tile_y * 16;
					int idx = Projectile.NewProjectile( new Vector2(x, y), Vector2.Zero, proj_type, 0, 0f );

					var proj_info = Main.projectile[idx].GetGlobalProjectile<PsiBoltProjectileInfo>();
					proj_info.IsHostile = hostile;
					
					if( (mymod.DEBUGFLAGS & 1) != 0 ) {
						Main.NewText( "fire "+ x + ", " + y );
					}
				}
			}
		}


		////////////////

		public override string Texture { get { return "Terraria/Projectile_293"; } }
		
		public override void SetStaticDefaults() {
			this.DisplayName.SetDefault( "Psi Bolt" );
		}

		public override void SetDefaults() {
			this.projectile.width = 12;
			this.projectile.height = 12;
			this.projectile.hostile = true;
			this.projectile.alpha = (int)64;
			this.projectile.magic = true;
			this.projectile.tileCollide = false;
			this.projectile.penetrate = -1;
			this.projectile.extraUpdates = 0;
			this.projectile.timeLeft = 60 * 60;
		}

		////////////////

		public override void AI() {
			Projectile proj = this.projectile;
			var proj_info = proj.GetGlobalProjectile<PsiBoltProjectileInfo>();
			Player player = Main.LocalPlayer;
			int alpha = 50; //100
			Color color = proj_info.IsHostile ? Color.Green : Color.White;

			proj.alpha = proj_info.IsHostile ? 32 : 64;

			for( int i = 0; i < 9; ++i ) {
				int idx = Dust.NewDust( new Vector2( proj.position.X, proj.position.Y ), proj.width, proj.height, 175, 0.0f, 0.0f, alpha, color, 1.3f );
				Main.dust[idx].noGravity = true;
				Main.dust[idx].velocity *= 0.0f;
			}

			var target = player.Center;
			var source = proj.Center;
			double rads = Math.Atan2( target.Y - source.Y, target.X - source.X );
			Vector2 diff = target - source;
			float dist = (float)Math.Sqrt( diff.X * diff.X + diff.Y * diff.Y );
			proj.velocity = Vector2.UnitX.RotatedBy(rads) * 14;

			//float target_x = player.position.X + (float)(player.width / 2);
			//float target_y = player.position.Y + (float)(player.height / 2);

			//Vector2 proj_pos = new Vector2( proj.position.X + (float)proj.width * 0.5f, proj.position.Y + (float)proj.height * 0.5f );
			//double target_dist_x = target_x - proj_pos.X;
			//double target_dist_y = target_y - proj_pos.Y;
			//double target_dist = Math.Sqrt( target_dist_x * target_dist_x + target_dist_y * target_dist_y );
			//double homing_x = 24d * target_dist_x / target_dist;
			//double homing_y = 24d * target_dist_y / target_dist;

			//proj.velocity.X = (float)(((double)proj.velocity.X * 100.0 + (double)homing_x) / 101.0);
			//proj.velocity.Y = (float)(((double)proj.velocity.Y * 100.0 + (double)homing_y) / 101.0);

			proj.position += proj.velocity;

			if( player.getRect().Intersects( proj.getRect() ) ) {
				//if( proj_info.IsHostile ) {
				//	var mymod = (DowsingMod)this.mod;
				//	int damage = mymod.Config.Data.PsychokineticBacklashDamageStack;
				//	player.Hurt( PsychokineticChargeDebuff.GetDeathMessage( player ), damage, 0 );
				//}
				proj.Kill();
			}
		}


		public override bool ShouldUpdatePosition() {
			return false;
		}
	}
}

