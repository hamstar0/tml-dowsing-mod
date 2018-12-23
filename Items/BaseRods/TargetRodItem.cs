using Dowsing.Buffs;
using Dowsing.Data;
using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.TileHelpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;


namespace Dowsing.Items {
	abstract class TargetRodItem : RodItem {
		abstract public void VirtualTargetIsDowsed( Player player );


		private IDictionary<int, IDictionary<int, int>> GetNpcPositions() {
			var npc_poses = new Dictionary<int, IDictionary<int, int>>();

			for( int i = 0; i < Main.npc.Length; i++ ) {
				NPC npc = Main.npc[i];
				if( npc == null || !npc.active ) { continue; }
				int x = (int)npc.position.X / 16;
				int y = (int)npc.position.Y / 16;

				if( !npc_poses.ContainsKey(x) ) { npc_poses[x] = new Dictionary<int, int>(); }
				npc_poses[x][y] = i;
			}

			return npc_poses;
		}


		protected bool CastRareNpcDowse( Player player, Vector2 aiming_at, int tile_range ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			bool dowsed = false;
			var npc_poses = this.GetNpcPositions();
			NPC npc = null;
			var rare_npc_type_list = WitchingTargetData.GetCurrentRareNpcTypes();
			if( rare_npc_type_list.Count == 0 ) { return false; }
			var rare_npc_type_set = new HashSet<int>( rare_npc_type_list );
			int traveled = 0;

			this.CurrentBeamTravelDistance = 0;

			this.CastDowseBeamWithinCone( player, aiming_at, new Utils.PerLinePoint( delegate ( int tile_x, int tile_y ) {
				if( !TileWorldHelpers.IsWithinMap( tile_x, tile_y ) || traveled >= tile_range ) {
					return false;
				}
				
				if( npc_poses.ContainsKey( tile_x ) && npc_poses[tile_x].ContainsKey( tile_y ) ) {
					npc = Main.npc[ npc_poses[tile_x][tile_y] ];
					if( rare_npc_type_set.Contains( npc.type ) ) {
						dowsed = true;
					}
				}

				if( dowsed ) {
					this.RenderRodHitFX( player, tile_x, tile_y );
				} else {
					traveled++;
					if( TileHelpers.IsSolid( Framing.GetTileSafely( tile_x, tile_y ), false, false ) ) {
						traveled++;
					}
				}
				this.CurrentBeamTravelDistance = traveled;

				if( (mymod.DEBUGFLAGS & 1) != 0 ) {
					DebugHelpers.Print( "current rare npcs", (this.III++)+" "+string.Join(",", rare_npc_type_set.ToArray()), 20 );
					//var dust = Dust.NewDustPerfect( new Vector2( tile_x * 16, tile_y * 16 ), 259, Vector2.Zero, 0, Color.Red, 0.75f );
					//dust.noGravity = true;
				}
				return !dowsed;
			} ) );
			
			return dowsed;
		}
private int III=0;

		protected bool CastVirtualTargetDowse( Player player, Vector2 aiming_at, int tile_range ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			bool dowsed = false;
			int traveled = 0;

			this.CurrentBeamTravelDistance = 0;

			this.CastDowseBeamWithinCone( player, aiming_at, new Utils.PerLinePoint( delegate ( int tile_x, int tile_y ) {
				if( !TileWorldHelpers.IsWithinMap( tile_x, tile_y ) || traveled >= tile_range ) {
					return false;
				}
				
				Vector2 from = modplayer.WitchingData.VirtualTargetPosition;
				float dist_x = (from.X / 16f) - (float)tile_x;
				float dist_y = (from.Y / 16f) - (float)tile_y;
				float dist = (float)Math.Sqrt( dist_x * dist_x + dist_y * dist_y );

				dowsed = dist <= 8;

				if( dowsed ) {
					PsychokineticChargeDebuff.ApplyForTargetIfAnew( mymod, player );
					this.VirtualTargetIsDowsed( player );
					this.RenderRodHitFX( player, tile_x, tile_y );
				} else {
					traveled++;
					if( TileHelpers.IsSolid( Framing.GetTileSafely(tile_x, tile_y), false, false ) ) {
						traveled++;
					}
				}
				this.CurrentBeamTravelDistance = traveled;

				if( (mymod.DEBUGFLAGS & 1) != 0 ) {
					var dust = Dust.NewDustPerfect( new Vector2(tile_x*16, tile_y*16), 259, Vector2.Zero, 0, Color.Red, 0.75f );
					dust.noGravity = true;
				}
				return !dowsed;
			} ) );

			return dowsed;
		}


		protected bool CastNpcTargetDowse( Player player, Vector2 aiming_at, int npc_who, int tile_range ) {
			var mymod = (DowsingMod)this.mod;
			var modplayer = player.GetModPlayer<DowsingPlayer>();
			bool dowsed = false;
			int traveled = 0;

			this.CurrentBeamTravelDistance = 0;

			this.CastDowseBeamWithinCone( player, aiming_at, new Utils.PerLinePoint( delegate ( int tile_x, int tile_y ) {
				if( !TileWorldHelpers.IsWithinMap( tile_x, tile_y ) || traveled >= tile_range ) {
					return false;
				}

				NPC npc = Main.npc[npc_who];
				if( npc == null || !npc.active ) {
					return false;
				}

				dowsed = npc.getRect().Intersects( new Rectangle((tile_x-1)*16, (tile_y-1)*16, 32, 32) );

				if( dowsed ) {
					PsychokineticChargeDebuff.ApplyForTargetIfAnew( mymod, player );
					this.RenderRodHitFX( player, tile_x, tile_y );
				} else {
					traveled++;
					if( TileHelpers.IsSolid( Framing.GetTileSafely( tile_x, tile_y ) ) ) {
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
