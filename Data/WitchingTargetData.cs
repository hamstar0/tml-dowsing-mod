using HamstarHelpers.Helpers.NPCHelpers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;


namespace Dowsing.Data {
	class WitchingTargetData : TargetData {
		#region Spawn rate gauging stuff
		private static int GaugeTimer = 10 * 60;
		private static IDictionary<int, float> TotalSpawns = new Dictionary<int, float>();
		public static IDictionary<int, float> SpawnsPerSecond = new Dictionary<int, float>();
		public static Vector2 RandomSpawnPos = Vector2.Zero;



		public static void RunSpawnRateGauging( Player player ) {
			var spawns = NPCFinderHelpers.GaugeSpawnRatesForPlayer( player, 29, out WitchingTargetData.RandomSpawnPos );

			foreach( var kv in spawns ) {
				if( !WitchingTargetData.TotalSpawns.ContainsKey( kv.Key ) ) {
					WitchingTargetData.TotalSpawns[kv.Key] = kv.Value / 29f;
				} else {
					WitchingTargetData.TotalSpawns[kv.Key] += kv.Value / 29f;
				}
			}

			if( WitchingTargetData.GaugeTimer <= 0 ) {
				int refill = 10 * 60;
				WitchingTargetData.GaugeTimer = refill;

				WitchingTargetData.SpawnsPerSecond = new Dictionary<int, float>();

				foreach( var kv in WitchingTargetData.TotalSpawns ) {
					WitchingTargetData.SpawnsPerSecond[kv.Key] = kv.Value / refill;
				}
				WitchingTargetData.TotalSpawns = new Dictionary<int, float>();
			}
			WitchingTargetData.GaugeTimer--;
		}


		public static IList<int> GetCurrentRareNpcTypes() {
			var set = new List<int>();
			var spawns = WitchingTargetData.SpawnsPerSecond;
			if( spawns.Count == 0 ) { return set; }
			var sorted_rates = from entry in spawns orderby entry.Value ascending select entry.Key;
			//var rarest = sorted_rates.Take( Math.Max( count / 5, Math.Min(2, count) ) );

			float sum = spawns.Values.Sum();
			float avg = sum / spawns.Count;
			
			int i = 0;
			foreach( int npc_type in sorted_rates ) {
				if( i > 4 ) { break; }
				if( spawns[npc_type] >= (avg/2f) && i > 0 ) { break; }

				set.Add( npc_type );
			}
			return set;
		}
		#endregion



		public override void FindNewTargetNpcTypeAndPosition( out int npc_type, out Vector2 position ) {
			npc_type = -1;
			position = WitchingTargetData.RandomSpawnPos;
			var rarest = WitchingTargetData.GetCurrentRareNpcTypes();
			if( rarest.Count == 0 ) { return; }

			float rarest_sum = rarest.Sum();
			float rand = Main.rand.NextFloat() * rarest_sum;
			npc_type = rarest.First();

			float tally = 0;
			foreach( int rare_npc_type in rarest ) {
				tally += WitchingTargetData.SpawnsPerSecond[ rare_npc_type ];
				if( tally >= rand ) {
					npc_type = rare_npc_type;
					break;
				}
			}
		}
	}
}
