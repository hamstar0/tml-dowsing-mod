using Dowsing.Projectiles;
using HamstarHelpers.MiscHelpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;


namespace Dowsing.Buffs {
	class PsychokineticChargeDebuff : ModBuff {
		public static PlayerDeathReason GetDeathMessage( Player player ) {
			return PlayerDeathReason.ByCustomReason( player.name + "'s head exploded to psychokinetic backfire." );
		}


		////////////////

		private int FxTimer = 0;


		public static void ApplyForTilesIfAnew( DowsingMod mymod, Player player ) {
			int buff_type = mymod.BuffType<PsychokineticChargeDebuff>();
			int buff_idx = player.FindBuffIndex( buff_type );
			if( buff_idx == -1 ) {
				player.AddBuff( buff_type, mymod.Config.Data.PsychokineticChargeDurationForTiles );
			}
		}
		public static void ApplyForTargetIfAnew( DowsingMod mymod, Player player ) {
			int buff_type = mymod.BuffType<PsychokineticChargeDebuff>();
			int buff_idx = player.FindBuffIndex( buff_type );
			if( buff_idx == -1 ) {
				player.AddBuff( buff_type, mymod.Config.Data.PsychokineticChargeDurationForTargets );
			}
		}


		////////////////

		public override void SetDefaults() {
			this.DisplayName.SetDefault( "Psychokinetic Charge" );
			this.Description.SetDefault( "A mysterious energy buildup lingers ominously" );

			Main.debuff[this.Type] = true;
		}


		////////////////

		public override void Update( Player player, ref int buff_idx ) {
			var mymod = (DowsingMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }
			if( player.whoAmI != Main.myPlayer ) { return; }

			bool has_tiles = this.UpdateForTiles( player, buff_idx );
			bool has_npcs = this.UpdateForTarget( player, buff_idx );

			if( !has_tiles && !has_npcs ) {
				player.DelBuff( buff_idx );
			} else {
				if( player.buffTime[buff_idx] == 1 ) {
					var tiles = this.ResetTileSources( player );
					this.ResetTargetSources( player );

					this.HurtForTiles( player, buff_idx, tiles );
					this.HurtForTarget( player, buff_idx );
				}
			}
		}


		private bool UpdateForTiles( Player player, int buff_idx ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			int count = modplayer.TileData.CountDowsings();

			if( count > 0 ) {
				if( player.whoAmI == Main.myPlayer ) {
					int range = mymod.Config.Data.PsychokineticDegaussingTileRange;
					var tiles = modplayer.TileData.DegaussWithinRange( range );
					PsiBoltProjectile.Fire( mymod, player, tiles, false );
				}

				this.RenderPlayerChargeFX( player, count );
			}

			return count > 0;
		}
		
		private bool UpdateForTarget( Player player, int buff_idx ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();

			if( modplayer.WitchingData.IsNpcDegaussing( mymod, player ) ) {
				modplayer.WitchingData.ClearTargetNpc();
			}
			if( modplayer.DiviningData.IsNpcDegaussing( mymod, player ) ) {
				modplayer.DiviningData.ClearTargetNpc();
			}

			bool has_target = modplayer.WitchingData.HasTarget() || modplayer.DiviningData.HasTarget();

			if( has_target ) {
				this.RenderPlayerChargeFX( player, 20 );
			}

			return has_target;
		}


		////////////////

		public void HurtForTiles( Player player, int buff_idx, IDictionary<int, ISet<int>> from_tiles ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			int count = modplayer.TileData.CountDowsings();
			
			PsiBoltProjectile.Fire( mymod, player, from_tiles, true );

			int damage = mymod.Config.Data.PsychokineticBacklashTileDamageBase;
			damage += (count - 1) * mymod.Config.Data.PsychokineticBacklashDamageStack;

			player.Hurt( PsychokineticChargeDebuff.GetDeathMessage( player ), damage, 0 );
		}


		public void HurtForTarget( Player player, int buff_idx ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();

			int damage = mymod.Config.Data.PsychokineticBacklashTargetDamageBase;

			Main.PlaySound( SoundID.Item70, player.Center );

			player.Hurt( PsychokineticChargeDebuff.GetDeathMessage( player ), damage, 0 );
		}

		////////////////
		
		public IDictionary<int, ISet<int>> ResetTileSources( Player player ) {
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			return modplayer.TileData.DegaussWithinRange( -1 );
		}

		public void ResetTargetSources( Player player ) {
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			modplayer.WitchingData.ClearTargetNpc();
		}


		////////////////

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

		public void RenderPlayerChargeFX( Player player, int intensity ) {
			if( this.FxTimer == 0 ) {
				this.FxTimer = (int)Math.Max( 24d * (1f - (double)intensity/30d), 0 );

				var mymod = (DowsingMod)this.mod;
				var pos = new Vector2( player.position.X, player.position.Y - 12 );
				float scale = 0.7f + (0.01f * intensity);
				int dust_type = 110;

				int puff_who = Dust.NewDust( player.position, player.width, player.height, dust_type, player.velocity.X, player.velocity.Y, 0, Color.YellowGreen, scale );
				Main.dust[puff_who].noGravity = true;

				if( (mymod.Config.Data.DEBUGFLAGS & 1) != 0 ) {
					DebugHelpers.Display["player charge"] = "FxTimer " + this.FxTimer+ ", scale " + scale+ ", intensity " + intensity;
				}
			} else {
				this.FxTimer--;
			}
		}
	}
}
