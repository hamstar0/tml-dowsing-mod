using Dowsing.Items;
using HamstarHelpers.TileHelpers;
using HamstarHelpers.Utilities.Config;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;


namespace Dowsing.Data {
	class DowsedTiles {
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



	class TileData {
		private IDictionary<int, DowsedTiles> DowsedTiles = new Dictionary<int, DowsedTiles>();

		

		public void SaveTo( TagCompound tags ) {
			tags["tile_data"] = JsonConfig<IDictionary<int, DowsedTiles>>.Serialize( this.DowsedTiles );
		}

		public void LoadFrom( TagCompound tags ) {
			if( tags.ContainsKey( "tile_data" ) ) {
				this.DowsedTiles = JsonConfig<IDictionary<int, DowsedTiles>>.Deserialize( tags.GetString( "tile_data" ) );
			}
		}



		#region Tile dowse stuff
		public bool ApplyDowseIfTileIsTarget( int tile_x, int tile_y, int tile_type ) {
			Tile tile = Framing.GetTileSafely( tile_x, tile_y );
			if( tile.type != tile_type || TileHelpers.IsAir( tile ) ) { return false; }

			if( !this.DowsedTiles.ContainsKey( tile_type ) ) {
				this.DowsedTiles[tile_type] = new DowsedTiles();
			}
			this.DowsedTiles[tile_type].AddDowseAt( tile_x, tile_y );

			return true;
		}


		public int CountDowsings() {
			if( this.DowsedTiles.Count == 0 ) { return 0; }
			int count = 0;

			foreach( var kv in this.DowsedTiles ) {
				foreach( var kv2 in kv.Value.PositionsOfStacks ) {
					foreach( var kv3 in kv2.Value ) {
						count += kv3.Value;
					}
				}
			}
			return count;
		}

		public void ResetDowsings() {
			this.DowsedTiles = new Dictionary<int, DowsedTiles>();
		}


		public IDictionary<int, ISet<int>> DegaussWithinRange( int range ) {
			IDictionary<int, ISet<int>> tiles = new Dictionary<int, ISet<int>>();
			var player = Main.LocalPlayer;
			int pos_x = (int)player.position.X / 16;
			int pos_y = (int)player.position.Y / 16;

			foreach( var kv in this.DowsedTiles ) {
				var x_to_y_stacks = kv.Value.PositionsOfStacks;

				foreach( int tile_x in x_to_y_stacks.Keys.ToArray() ) {
					if( range > 0 && Math.Abs( pos_x - tile_x ) > range ) { continue; }

					var y_to_stack = x_to_y_stacks[tile_x];

					foreach( int tile_y in y_to_stack.Keys.ToArray() ) {
						if( range > 0 && Math.Abs( pos_y - tile_y ) > range ) { continue; }

						y_to_stack.Remove( tile_y );

						if( !tiles.ContainsKey( tile_x ) ) {
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
		#endregion



		public void HighlightDowsedTiles() {
			foreach( var kv in this.DowsedTiles ) {
				int block_type = kv.Key;

				foreach( var kv2 in kv.Value.PositionsOfStacks ) {
					int tile_x = kv2.Key;

					foreach( var kv3 in kv2.Value ) {
						int tile_y = kv3.Key;
						int stack = kv3.Value;
						if( stack == 0 ) { continue; }

						RodItem.RenderDowseEffect( new Vector2( tile_x * 16, tile_y * 16 ), stack, Color.White );
					}
				}
			}
		}
	}
}
