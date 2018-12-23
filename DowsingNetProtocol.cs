using HamstarHelpers.Helpers.DebugHelpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;


namespace Dowsing {
	public enum DowsingNetProtocolTypes : byte {
		ModSettingsRequest,
		ModSettings,
		DowsingNpcRequest,
		RequestedDowsingNpcWho
	}


	class DowsingNetProtocol {
		public static void RoutePacket( DowsingMod mymod, BinaryReader reader ) {
			DowsingNetProtocolTypes protocol = (DowsingNetProtocolTypes)reader.ReadByte();

			switch( protocol ) {
			case DowsingNetProtocolTypes.ModSettingsRequest:
				DowsingNetProtocol.ReceiveSettingsRequestOnServer( mymod, reader );
				break;
			case DowsingNetProtocolTypes.ModSettings:
				DowsingNetProtocol.ReceiveSettingsOnClient( mymod, reader );
				break;
			case DowsingNetProtocolTypes.DowsingNpcRequest:
				DowsingNetProtocol.ReceiveDowsingNpcRequestOnServer( mymod, reader );
				break;
			case DowsingNetProtocolTypes.RequestedDowsingNpcWho:
				DowsingNetProtocol.ReceiveRequestedDowsingNpcWhoOnClient( mymod, reader );
				break;
			default:
				ErrorLogger.Log( "Invalid packet protocol: " + protocol );
				break;
			}
		}


		private static Queue<Action<int>> DowsingNpcRequestQueue = new Queue<Action<int>>();



		////////////////////////////////
		// Senders (Client)
		////////////////////////////////

		public static void RequestSettingsFromServer( DowsingMod mymod, Player player ) {
			if( Main.netMode != 1 ) { return; }	// Clients only

			ModPacket packet = mymod.GetPacket();
			packet.Write( (byte)DowsingNetProtocolTypes.ModSettingsRequest );
			packet.Write( (int)player.whoAmI );
			packet.Send();
		}

		public static void RequestDowsingNpcFromServer( DowsingMod mymod, Player player, int npc_type, Vector2 position, Action<int> callback ) {
			if( Main.netMode != 1 ) { return; } // Clients only

			DowsingNetProtocol.DowsingNpcRequestQueue.Enqueue( callback );

			ModPacket packet = mymod.GetPacket();
			packet.Write( (byte)DowsingNetProtocolTypes.DowsingNpcRequest );
			packet.Write( (int)player.whoAmI );
			packet.Write( (int)npc_type );
			packet.Write( (float)position.X );
			packet.Write( (float)position.Y );
			packet.Send();
		}
		
		////////////////////////////////
		// Senders (Server)
		////////////////////////////////

		public static void SendSettingsFromServer( DowsingMod mymod, Player player ) {
			if( Main.netMode != 2 ) { return; } // Server only

			ModPacket packet = mymod.GetPacket();
			packet.Write( (byte)DowsingNetProtocolTypes.ModSettings );
			packet.Write( (string)mymod.Config.SerializeMe() );

			packet.Send( (int)player.whoAmI );
		}

		public static void SendRequestedDowsingNpcFromServer( DowsingMod mymod, Player player, int npc_who ) {
			if( Main.netMode != 2 ) { return; } // Server only

			ModPacket packet = mymod.GetPacket();

			packet.Write( (byte)DowsingNetProtocolTypes.RequestedDowsingNpcWho );
			packet.Write( (int)npc_who );

			packet.Send( (int)player.whoAmI );
		}



		////////////////////////////////
		// Recipients (Clients)
		////////////////////////////////

		private static void ReceiveSettingsOnClient( DowsingMod mymod, BinaryReader reader ) {
			if( Main.netMode != 1 ) { return; } // Clients only

			bool _;
			mymod.Config.DeserializeMe( reader.ReadString(), out _ );
		}

		private static void ReceiveRequestedDowsingNpcWhoOnClient( DowsingMod mymod, BinaryReader reader ) {
			if( Main.netMode != 1 ) { return; } // Clients only

			int npc_who = reader.ReadInt32();

			if( npc_who < 0 || npc_who >= Main.npc.Length ) {
				LogHelpers.Log( "ReceiveRequestedDowsingNpcWhoOnClient - Invalid npc_who. " + npc_who );
				return;
			}
			if( Main.npc[npc_who] == null || !Main.npc[npc_who].active ) {
				LogHelpers.Log( "ReceiveRequestedDowsingNpcWhoOnClient - Invalid npc (who: " + npc_who + ")" );
				return;
			}

			if( DowsingNetProtocol.DowsingNpcRequestQueue.Count > 0 ) {
				DowsingNetProtocol.DowsingNpcRequestQueue.Dequeue()( npc_who );
			}
		}

		////////////////////////////////
		// Recipients (Server)
		////////////////////////////////

		private static void ReceiveSettingsRequestOnServer( DowsingMod mymod, BinaryReader reader ) {
			if( Main.netMode != 2 ) { return; } // Server only

			int who = reader.ReadInt32();

			if( who < 0 || who >= Main.player.Length || Main.player[who] == null ) {
				LogHelpers.Log( "ReceiveSettingsRequestOnServer - Invalid player whoAmI. " + who );
				return;
			}

			DowsingNetProtocol.SendSettingsFromServer( mymod, Main.player[who] );
		}

		private static void ReceiveDowsingNpcRequestOnServer( DowsingMod mymod, BinaryReader reader ) {
			if( Main.netMode != 2 ) { return; } // Server only

			int player_who = reader.ReadInt32();
			int npc_type = reader.ReadInt32();
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();

			if( player_who < 0 || player_who >= Main.player.Length || Main.player[player_who] == null ) {
				LogHelpers.Log( "ReceiveRareNpcRequestOnServer - Invalid player whoAmI. " + player_who );
				return;
			}
			if( npc_type < 0 || npc_type >= Main.npcTexture.Length ) {
				LogHelpers.Log( "ReceiveRareNpcRequestOnServer - Invalid npc_type. " + npc_type );
				return;
			}

			Player player = Main.player[player_who];
			int npc_who = NPC.NewNPC( (int)x, (int)y, npc_type );

			DowsingNetProtocol.SendRequestedDowsingNpcFromServer( mymod, player, npc_who );
		}
	}
}
