using Dowsing.Buffs;
using Dowsing.Data;
using Dowsing.Items;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace Dowsing {
	class DowsingPlayer : ModPlayer {
		public TileData TileData;
		public WitchingTargetData WitchingData;
		public DiviningTargetData DiviningData;

		private bool IsRightClick = false;
		private int DowseFxTimer = 0;



		////////////////

		public override void clientClone( ModPlayer clone ) {
			base.clientClone( clone );
			var myclone = (DowsingPlayer)clone;

			myclone.TileData = this.TileData;
			myclone.WitchingData = this.WitchingData;
			myclone.DiviningData = this.DiviningData;
			myclone.IsRightClick = this.IsRightClick;
			myclone.DowseFxTimer = this.DowseFxTimer;
		}


		////////////////

		public override void Initialize() {
			this.IsRightClick = false;
			this.DowseFxTimer = 0;
			this.TileData = new TileData();
			this.WitchingData = new WitchingTargetData();
			this.DiviningData = new DiviningTargetData();
		}

		public override TagCompound Save() {
			var tags = new TagCompound();

			this.TileData.SaveTo( tags );
			return tags;
		}

		public override void Load( TagCompound tags ) {
			this.TileData.LoadFrom( tags );

			if( (((DowsingMod)this.mod).DEBUGFLAGS & 2) != 0 ) {
				this.TileData.ResetDowsings();
			}
		}

		////////////////

		public override void OnEnterWorld( Player player ) {
			if( player.whoAmI == this.player.whoAmI ) { // Current player
				var mymod = (DowsingMod)this.mod;

				if( Main.netMode != 2 ) {   // Not server
					if( !mymod.Config.LoadFile() ) {
						mymod.Config.SaveFile();
					}
				}

				if( Main.netMode == 1 ) {   // Client
					DowsingNetProtocol.RequestSettingsFromServer( mymod, player );
				}
			}

			if( (((DowsingMod)this.mod).DEBUGFLAGS & 2) != 0 ) {
				int idx = this.player.FindBuffIndex( this.mod.BuffType<PsychokineticChargeDebuff>() );
				if( idx != -1 ) {
					this.player.DelBuff( idx );
				}
			}
		}

		////////////////

		public override void PreUpdate() {
			var mymod = (DowsingMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }

			if( this.player.whoAmI == Main.myPlayer ) {
				if( !Main.playerInventory && Main.mouseRight && Main.mouseRightRelease ) {
					if( !this.IsRightClick ) {
						this.IsRightClick = true;
						this.CheckDowseChoice();
					}
				} else {
					this.IsRightClick = false;
				}

				this.UpdatePsychokineticState();
				this.RunRodCooldownTimer();
				this.RunDowseFxTimer();
				this.RunRodPassiveBehavior();
				this.RunRodTargetPassiveBehavior( this.WitchingData );
				//this.RunRodTargetPassiveBehavior( this.DiviningData );
			}
		}

		////////////////

		private void CheckDowseChoice() {
			Item curr_item = this.player.HeldItem;
			if( curr_item.IsAir ) { return; }

			var moditem = curr_item.modItem as RodItem;
			if( moditem != null ) {
				moditem.ChooseDowsingTypeAtMouse( this.player );
			}
		}

		private void RunRodCooldownTimer() {
			Item held_item = this.player.HeldItem;
			if( held_item == null || held_item.IsAir ) { return; }

			var item_info = held_item.GetGlobalItem<RodItemInfo>();

			if( item_info.CastCooldownTimer > 0 ) {
				item_info.CastCooldownTimer--;
			}
		}

		private void UpdatePsychokineticState() {
			var mymod = (DowsingMod)this.mod;
			if( this.player.FindBuffIndex( mymod.BuffType<PsychokineticChargeDebuff>() ) >= 0 ) { return; }
			
			Item held_item = this.player.HeldItem;

			if( this.TileData.CountDowsings() > 0 ) {
				PsychokineticChargeDebuff.ApplyForTilesIfAnew( mymod, this.player );
			}
			if( this.WitchingData.HasTarget() || this.DiviningData.HasTarget() ) {
				PsychokineticChargeDebuff.ApplyForTargetIfAnew( mymod, this.player );
			}
		}

		private void RunRodPassiveBehavior() {
			var mymod = (DowsingMod)this.mod;

			if( this.WitchingData.IsVirtualTargetDowsed ) {
				WitchingTargetData.RunSpawnRateGauging( this.player );
			}
		}

		private void RunRodTargetPassiveBehavior( TargetData data ) {
			var mymod = (DowsingMod)this.mod;
			data.RunTargetUpdate( mymod, player );
		}

		private void RunDowseFxTimer() {
			if( this.DowseFxTimer == 0 ) {
				this.DowseFxTimer = 15;

				this.TileData.HighlightDowsedTiles();
				this.WitchingData.HighlightDowsedTarget();
			} else {
				this.DowseFxTimer--;
			}
		}
	}
}
