using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace Dowsing.Items {
	class RodItemInfo : GlobalItem {
		public override bool InstancePerEntity { get { return true; } }

		public override GlobalItem Clone( Item item, Item item_clone ) {
			var clone = (RodItemInfo)base.Clone( item, item_clone );
			clone.DowsingBlockType = this.DowsingBlockType;
			clone.CooldownTimer = this.CooldownTimer;
			return clone;
		}

		public override bool NeedsSaving( Item item ) {
			return item.type == this.mod.ItemType<DowsingRodItem>() ||
				item.type == this.mod.ItemType<ViningRodItem>() ||
				item.type == this.mod.ItemType<WitchingRodItem>() ||
				item.type == this.mod.ItemType<DiviningRodItem>();
		}

		public override void Load( Item item, TagCompound tag ) {
			this.DowsingBlockType = tag.GetInt( "block_type" );
		}

		public override TagCompound Save( Item item ) {
			return new TagCompound {
				{ "block_type", this.DowsingBlockType }
			};
		}



		public int DowsingBlockType = -1;
		public int CooldownTimer = 0;
	}
}
