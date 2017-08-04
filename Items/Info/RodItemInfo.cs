using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace Dowsing.Items {
	class RodItemInfo : GlobalItem {
		public override bool InstancePerEntity { get { return true; } }

		public string Id;


		public override GlobalItem Clone( Item item, Item item_clone ) {
			var clone = (RodItemInfo)base.Clone( item, item_clone );
			clone.Id = this.Id;
			clone.CastCooldownTimer = this.CastCooldownTimer;
			clone.TargetTileType = this.TargetTileType;
			return clone;
		}

		public override bool NeedsSaving( Item item ) {
			return item.type == this.mod.ItemType<DowsingRodItem>() ||
				item.type == this.mod.ItemType<ViningRodItem>() ||
				item.type == this.mod.ItemType<WitchingRodItem>() ||
				item.type == this.mod.ItemType<DiviningRodItem>();
		}

		public override void Load( Item item, TagCompound tag ) {
			this.Id = tag.GetString( "id" );
			this.TargetTileType = tag.GetInt( "tile_type" );
		}

		public override TagCompound Save( Item item ) {
			return new TagCompound {
				{ "id", this.Id },
				{ "tile_type", this.TargetTileType }
			};
		}

		public override void NetSend( Item item, BinaryWriter writer ) {
			writer.Write( this.Id );
		}

		public override void NetReceive( Item item, BinaryReader reader ) {
			this.Id = reader.ReadString();
		}



		////////////////

		public int CastCooldownTimer = 0;
		public int TargetTileType = -1;
	}
}
