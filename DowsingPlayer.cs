using Dowsing.Buffs;
using Dowsing.Items;
using Terraria;
using Terraria.ModLoader;


namespace Dowsing {
	class DowsingPlayer : ModPlayer {
		private bool IsRightClick = false;
		private int DowseFxTimer = 0;



		////////////////
		
		public override void clientClone( ModPlayer clone ) {
			base.clientClone( clone );
			var myclone = (DowsingPlayer)clone;

			myclone.IsRightClick = this.IsRightClick;
			myclone.DowseFxTimer = this.DowseFxTimer;
		}


		////////////////

		public override void OnEnterWorld( Player player ) {
			if( (((DowsingMod)this.mod).DEBUGFLAGS & 2) != 0 ) {
				int idx = this.player.FindBuffIndex( this.mod.BuffType<PsychokineticChargeDebuff>() );
				if( idx != -1 ) {
					this.player.DelBuff( idx );
				}
			}
		}


		public override void PreUpdate() {
			if( this.player.whoAmI != Main.myPlayer ) { return; }

			this.RunRodCooldownTimer();

			if( !Main.playerInventory && Main.mouseRight && Main.mouseRightRelease ) {
				if( !this.IsRightClick ) {
					this.IsRightClick = true;
					this.CheckDowseChoice();
				}
			} else {
				this.IsRightClick = false;
			}

			this.UpdatePsychokineticState();
			
			this.RunDowseFxTimer();
		}

		////////////////

		private void RunRodCooldownTimer() {
			Item curr_item = this.player.HeldItem;
			if( curr_item.IsAir ) { return; }

			var item_info = curr_item.GetGlobalItem<RodItemInfo>();

			if( item_info.CooldownTimer > 0 ) {
				item_info.CooldownTimer--;
			}
		}

		private void CheckDowseChoice() {
			Item curr_item = this.player.HeldItem;
			if( curr_item.IsAir ) { return; }

			var moditem = curr_item.modItem as RodItem;
			if( moditem != null ) {
				moditem.ChooseDowsingTypeAtMouse( this.player );
			}
		}

		private void UpdatePsychokineticState() {
			var mymod = (DowsingMod)this.mod;
			if( this.player.FindBuffIndex(mymod.BuffType<PsychokineticChargeDebuff>()) >= 0 ) { return; }

			var modworld = mymod.GetModWorld<DowsingWorld>();

			if( modworld.CountDowsings() > 0 ) {
				PsychokineticChargeDebuff.ApplyForTilesIfAnew( mymod, this.player );
			}
		}

		private void RunDowseFxTimer() {
			var modworld = this.mod.GetModWorld<DowsingWorld>();

			if( this.DowseFxTimer == 0 ) {
				this.DowseFxTimer = 15;

				modworld.HighlightDowsedBlocks();
			} else {
				this.DowseFxTimer--;
			}
		}
	}
}
