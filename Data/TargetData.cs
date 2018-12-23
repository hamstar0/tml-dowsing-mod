using Dowsing.Buffs;
using Dowsing.Items;
using HamstarHelpers.Helpers.DebugHelpers;
using HamstarHelpers.Helpers.TileHelpers;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;


namespace Dowsing.Data {
	abstract class TargetData {
		public bool IsVirtualTargetDowsed = false;
		public Vector2 VirtualTargetPosition { get; private set; }
		public Vector2 VirtualTargetHeading { get; private set; }
		protected int VirtualTargetUpdateTimer = 0;
		protected int VirtualTargetFakeouts = 3;

		public int TargetNpcWho { get; private set; }


		public TargetData() {
			this.TargetNpcWho = -1;
		}

		
		public void RunTargetUpdate( DowsingMod mymod, Player player ) {
			this.RunVirtualTargetUpdate( mymod, player );

			if( !this.HasNpcTarget() ) { this.ClearTargetNpc(); }

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				if( this.TargetNpcWho != -1 ) {
					DebugHelpers.Print( "targetting", "fakeouts: " + this.VirtualTargetFakeouts + ", npc: " + Main.npc[this.TargetNpcWho].TypeName + " (who:" + this.TargetNpcWho + ")", 20 );
				} else {
					DebugHelpers.Print( "targetting", "fakeouts: " + this.VirtualTargetFakeouts, 20 );
				}
			}
		}


		#region NPC target stuff
		public bool HasTarget() {
			if( this.IsVirtualTargetDowsed ) { return true; }
			return this.HasNpcTarget();
		}


		public bool HasNpcTarget() {
			return this.TargetNpcWho != -1
				&& Main.npc.Length < this.TargetNpcWho
				&& Main.npc[this.TargetNpcWho] != null
				&& Main.npc[this.TargetNpcWho].active;
		}


		public void ClearTargetNpc() {
			this.TargetNpcWho = -1;
		}


		public void SetTargetNpc( NPC npc ) {
			if( npc == null || !npc.active ) { throw new Exception( "Invalid npc." ); }
			this.TargetNpcWho = npc.whoAmI;
		}


		public bool IsNpcDegaussing( DowsingMod mymod, Player player ) {
			if( !this.HasNpcTarget() ) { return true; }

			NPC npc = Main.npc[ this.TargetNpcWho ];
			var dist = Vector2.Distance( npc.position, player.position );

			return (dist / 16) < mymod.Config.Data.PsychokineticDegaussingNpcRangeInTiles;
		}


		abstract public void FindNewTargetNpcTypeAndPosition( out int npc_type, out Vector2 position );
		#endregion



		#region Virtual target setting
		private void RandomizeVirtualTarget( DowsingMod mymod, Player player ) {
			float max_tile_range = (float)Math.Max( mymod.Config.Data.MaxVirtualTargetRangeInTiles, 100 );
			Vector2 pos = Vector2.Zero;
			int i = 0, tile_x, tile_y;

			do {
				if( i++ >= 100 ) { break; }
				
				float rand_range_tiles = (Main.rand.NextFloat() * (max_tile_range - 100f)) + 99f;
				float rand_range = 16f * rand_range_tiles;
				Vector2 rand_heading = Vector2.UnitX.RotatedBy( (double)Main.rand.NextFloat() * (Math.PI * 2d) );

				pos = player.Center + (rand_heading * rand_range);
				tile_x = (int)pos.X / 16;
				tile_y = (int)pos.Y / 16;
			} while( !TileWorldHelpers.IsWithinMap(tile_x, tile_y) || TileHelpers.IsSolid( Framing.GetTileSafely(tile_x, tile_y) ) );

			this.VirtualTargetPosition = TileWorldHelpers.DropToGround( pos );
			this.ReAimVirtualTarget();
		}

		public void ResetVirtualTargetUpdateTimer() {
			this.VirtualTargetUpdateTimer = 60 * 30; // Updates every 30 seconds
		}

		public void ResetVirtualTarget( DowsingMod mymod, Player player ) {
			this.ResetVirtualTargetUpdateTimer();
			this.RandomizeVirtualTarget( mymod, player );
		}

		public void ReAimVirtualTarget() {
			this.VirtualTargetHeading = Vector2.UnitX.RotatedBy( Main.rand.NextFloat() * (Math.PI * 2d) );
		}

		public void RemoveVirtualTarget( DowsingMod mymod, Player player ) {
			this.IsVirtualTargetDowsed = false;
			this.VirtualTargetFakeouts = Math.Max( Main.rand.Next(2, 8), this.VirtualTargetFakeouts );
			this.ResetVirtualTarget( mymod, player );
		}
		#endregion



		#region Virtual target run
		public int TestVirtualTargetMovement( DowsingMod mymod, Player player, out Vector2 newpos ) {
			int max_range = mymod.Config.Data.MaxWitchingRangeInTiles * 16;
			int min_range = mymod.Config.Data.VirtualTargetApproachTriggerInTiles * 16;

			newpos = this.VirtualTargetPosition + (this.VirtualTargetHeading * 2);
			
			int dist = (int)Vector2.Distance( newpos, player.Center );
			int tile_x = (int)newpos.X / 16;
			int tile_y = (int)newpos.Y / 16;

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				DebugHelpers.Print( "dist", min_range + "(" + (dist <= min_range) + ") <= " + dist + " >= " + max_range + "(" + (dist >= max_range) + ")", 20 );
			}

			if( dist >= max_range ) {   // Target out-of-range
				return -1;
			}
			if( dist <= min_range ) {   // Target reached
				return 1;
			}
			if( TileHelpers.IsSolid( Framing.GetTileSafely(tile_x, tile_y) ) ) {  // Target collides with solid tile?
				return 2;
			}
			if( !TileWorldHelpers.IsWithinMap( tile_x, tile_y ) ) {  // Target outside map?
				return 3;
			}
			if( !TileFinderHelpers.HasNearbySolid( tile_x, tile_y, 10 ) ) {   // Not near solids?
				return 4;
			}
			return 0;
		}


		private void RunVirtualTargetUpdate( DowsingMod mymod, Player player ) {
			Vector2 newpos;
			int test = this.TestVirtualTargetMovement( mymod, player, out newpos );

			switch( test ) {
			case -1:    // Out-of-range
				this.RemoveVirtualTarget( mymod, player );
				break;
			case 0:     // Normal
				this.RunVirtualTargetTimer( mymod, player );
				this.VirtualTargetPosition = newpos;
				break;
			case 1:     // Collides with player
				this.ResetVirtualTarget( mymod, player );
				if( this.IsVirtualTargetDowsed ) {
					this.VirtualTargetIsApproached( mymod, player );
				}
				break;
			default:     // Blocked
				this.ReAimVirtualTarget();
				break;
			}

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				var dust = Dust.NewDustPerfect( this.VirtualTargetPosition, 259, Vector2.Zero, 0, Color.Purple, 0.75f );
				dust.noGravity = true;
			}
		}


		private void RunVirtualTargetTimer( DowsingMod mymod, Player player ) {
			if( this.VirtualTargetUpdateTimer == 0 ) {
				this.ResetVirtualTarget( mymod, player );

				if( this.IsVirtualTargetDowsed && this.VirtualTargetFakeouts < 3 ) {
					this.VirtualTargetFakeouts++;
					this.IndicateVirtualTargetRange( player );
				}
			}
			this.VirtualTargetUpdateTimer--;
		}
		#endregion



		#region Virtual target interacting
		public void IndicateVirtualTargetRange( Player player ) {
			if( this.VirtualTargetFakeouts > 1 ) {
				int duration = CombatText.NewText( player.getRect(), Color.YellowGreen, "= " + (this.VirtualTargetFakeouts - 1) + " =" );
				Main.combatText[duration].lifeTime = 4 * 60;
			}

			Main.PlaySound( SoundID.Item19, player.Center );
		}


		public void VirtualTargetIsApproached( DowsingMod mymod, Player player ) {
			this.IndicateVirtualTargetRange( player );

			this.VirtualTargetFakeouts--;

			int idx = player.FindBuffIndex( mymod.BuffType<PsychokineticChargeDebuff>() );
			if( idx >= 0 ) { player.buffTime[idx] += mymod.Config.Data.VirtualTargetPsychChargeAddedTime; }
			
			if( this.VirtualTargetFakeouts <= 0 ) {
				this.VirtualTargetArrival( mymod, player );
			}
		}


		public void VirtualTargetArrival( DowsingMod mymod, Player player ) {
			int npc_type;
			Vector2 pos;
			this.FindNewTargetNpcTypeAndPosition( out npc_type, out pos );

			switch( Main.netMode ) {
			case 0: // Single
				int npc_who = -1;
				if( npc_type != -1 ) {
					npc_who = NPC.NewNPC( (int)pos.X, (int)pos.Y, npc_type );
				}
				this.FinalizeTarget( mymod, player, npc_who );
				break;
			case 1: // Client
				if( npc_type != -1 ) {
					DowsingNetProtocol.RequestDowsingNpcFromServer( mymod, player, npc_type, pos, delegate ( int serv_npc_who ) {
						this.FinalizeTarget( mymod, player, serv_npc_who );
					} );
				} else {
					this.FinalizeTarget( mymod, player, -1 );
				}
				break;
			}
		}

		private void FinalizeTarget( DowsingMod mymod, Player player, int npc_who ) {
			NPC npc = null;
			if( npc_who >= 0 && npc_who < Main.npc.Length ) {
				npc = Main.npc[npc_who];
			}

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				if( npc != null ) {
					Main.NewText( "FinalizeTarget npc: " + npc.TypeName + ", pos: " + npc.position );
				} else {
					Main.NewText( "FinalizeTarget npc: " + -1 );
				}
			}

			if( npc != null && npc.active ) {
				this.SetTargetNpc( npc );
			} else {
				this.ClearTargetNpc();
			}
			this.RemoveVirtualTarget( mymod, player );

			if( this.TargetNpcWho == -1 ) {
				int duration = CombatText.NewText( player.getRect(), Color.Red, "Failed..." );
				Main.combatText[duration].lifeTime = 4 * 60;
			} else {
				int duration = CombatText.NewText( player.getRect(), Color.YellowGreen, "Found!" );
				Main.combatText[duration].lifeTime = 4 * 60;
			}

			if( (mymod.DEBUGFLAGS & 1) != 0 ) {
				if( npc != null && npc.active ) {
					var dist = Vector2.Distance( player.position, npc.position );
					Main.NewText( "                    NPC TYPE: " + npc.type + ", WHO: " + this.TargetNpcWho + ", NPC: " + npc.TypeName + " dist: "+ dist+", pos: "+npc.position, Color.Blue );
				}
			}
		}
		#endregion



		public void HighlightDowsedTarget() {
			if( this.TargetNpcWho != -1 ) {
				var npc = Main.npc[ this.TargetNpcWho ];
				if( npc != null && npc.active ) {
					RodItem.RenderDowseEffect( npc.Center, 4, Color.White );
				}
			}
		}
	}
}
