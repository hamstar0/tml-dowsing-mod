using HamstarHelpers.Utilities.Config;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.IO;
using HamstarHelpers.TileHelpers;
using System.Linq;

namespace Dowsing {
	class DowsedAt {
		public IDictionary<int, IDictionary<int, int>> PositionsOfStacks = new Dictionary<int, IDictionary<int, int>>();


		public void AddDowseAt( int tile_x, int tile_y ) {
			if( !this.PositionsOfStacks.ContainsKey( tile_x ) ) {
				this.PositionsOfStacks[tile_x] = new Dictionary<int, int>();
			}
			if( !this.PositionsOfStacks[tile_x].ContainsKey( tile_y ) ) {
				this.PositionsOfStacks[tile_x][tile_y] = 1;
			} else {
				this.PositionsOfStacks[tile_x][tile_y]++;
			}
		}

		public int GetDowsingsAt( int tile_x, int tile_y ) {
			if( !this.PositionsOfStacks.ContainsKey( tile_x ) ) { return 0; }
			if( !this.PositionsOfStacks[tile_x].ContainsKey( tile_y ) ) { return 0; }
			return this.PositionsOfStacks[tile_x][tile_y];
		}
	}




	class DowsingWorld : ModWorld {
		private IDictionary<int, DowsedAt> MaterialsDowsedAt;



		public bool ApplyDowseIfBlockIsTarget( int tile_x, int tile_y, int tile_type ) {
			Tile tile = Framing.GetTileSafely( tile_x, tile_y );
			if( tile.type != tile_type || TileHelpers.IsAir(tile) ) { return false; }

			if( !this.MaterialsDowsedAt.ContainsKey(tile_type) ) {
				this.MaterialsDowsedAt[tile_type] = new DowsedAt();
			}
			this.MaterialsDowsedAt[tile_type].AddDowseAt( tile_x, tile_y );

			return true;
		}


		public int CountDowsings() {
			if( this.MaterialsDowsedAt.Count == 0 ) { return 0; }
			int count = 0;

			foreach( var kv in this.MaterialsDowsedAt ) {
				foreach( var kv2 in kv.Value.PositionsOfStacks ) {
					foreach( var kv3 in kv2.Value ) {
						count += kv3.Value;
					}
				}
			}
			return count;
		}

		public void ResetDowsings() {
			this.MaterialsDowsedAt = new Dictionary<int, DowsedAt>();
		}

		
		public IDictionary<int, ISet<int>> DegaussNearbyTiles( int range ) {
			IDictionary<int, ISet<int>> tiles = new Dictionary<int, ISet<int>>();
			var player = Main.LocalPlayer;
			int pos_x = (int)player.position.X / 16;
			int pos_y = (int)player.position.Y / 16;

			foreach( var kv in this.MaterialsDowsedAt ) {
				var x_to_y_stacks = kv.Value.PositionsOfStacks;

				foreach( int tile_x in x_to_y_stacks.Keys.ToArray() ) {
					if( range > 0 && Math.Abs(pos_x - tile_x) > range ) { continue; }

					var y_to_stack = x_to_y_stacks[ tile_x ];

					foreach( int tile_y in y_to_stack.Keys.ToArray() ) {
						if( range > 0 && Math.Abs(pos_y - tile_y) > range ) { continue; }

						y_to_stack.Remove( tile_y );

						if( !tiles.ContainsKey(tile_x) ) {
							tiles[tile_x] = new HashSet<int>();
						}
						tiles[tile_x].Add( tile_y );
					}

					if( y_to_stack.Count == 0 ) {
						x_to_y_stacks.Remove( tile_x );
					}
				}
			}

			return tiles;
		}

		////////////////

		public void RenderDowseEffect( Vector2 position, int stack, Color color ) {
			int alpha = Math.Max( 192 - (stack * 10), 32 );

			int puff_who = Dust.NewDust( position, 6, 6, 16, 0f, 0f, alpha, color, 2f );
			Main.dust[puff_who].velocity *= 0.1f * stack;
		}


		public void HighlightDowsedBlocks() {
			foreach( var kv in this.MaterialsDowsedAt ) {
				int block_type = kv.Key;

				foreach( var kv2 in kv.Value.PositionsOfStacks ) {
					int tile_x = kv2.Key;

					foreach( var kv3 in kv2.Value ) {
						int tile_y = kv3.Key;
						int stack = kv3.Value;
						if( stack == 0 ) { continue; }

						this.RenderDowseEffect( new Vector2( tile_x * 16, tile_y * 16 ), stack, Color.White );
					}
				}
			}
		}



		////////////////

		public override void Initialize() {
			this.MaterialsDowsedAt = new Dictionary<int, DowsedAt>();
		}

		public override TagCompound Save() {
			return new TagCompound {
				{ "data", JsonConfig<IDictionary<int, DowsedAt>>.Serialize(this.MaterialsDowsedAt) }
			};
		}

		public override void Load( TagCompound tags ) {
			if( tags.ContainsKey("data") ) {
				this.MaterialsDowsedAt = JsonConfig<IDictionary<int, DowsedAt>>.Deserialize( tags.GetString( "data" ) );
			}

			if( (((DowsingMod)this.mod).DEBUGFLAGS & 2) != 0 ) {
				this.ResetDowsings();
			}
		}


		public override void NetSend( BinaryWriter writer ) {
			writer.Write( (string)JsonConfig<IDictionary<int, DowsedAt>>.Serialize( this.MaterialsDowsedAt ) );
		}

		public override void NetReceive( BinaryReader reader ) {
			this.MaterialsDowsedAt = JsonConfig<IDictionary<int, DowsedAt>>.Deserialize( reader.ReadString() );
		}
	}
}
