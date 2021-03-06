using System;
using System.Collections;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

namespace DT.Game.Battle.Players {
	public abstract class BattlePlayerInputComponent : MonoBehaviour, IRecycleCleanupSubscriber {
		// PRAGMA MARK - Public Interface
		public bool Enabled {
			get { return enabled_ && inputDelegate_ != null && controller_ != null; }
			set { enabled_ = value; }
		}

		public void Init(BattlePlayer player, BattlePlayerInputController controller, IBattlePlayerInputDelegate inputDelegate) {
			player_ = player;
			controller_ = controller;
			inputDelegate_ = inputDelegate;

			Initialize();
		}


		// PRAGMA MARK - IRecycleCleanupSubscriber Implementation
		void IRecycleCleanupSubscriber.OnRecycleCleanup() {
			controller_ = null;
			inputDelegate_ = null;

			Cleanup();
		}


		// PRAGMA MARK - Internal
		protected BattlePlayerInputController Controller_ {
			get { return controller_; }
		}

		protected IBattlePlayerInputDelegate InputDelegate_ {
			get { return inputDelegate_; }
		}

		protected BattlePlayer Player_ {
			get { return player_; }
		}

		protected virtual void Initialize() {
			// stub
		}

		protected virtual void Cleanup() {
			// stub
		}

		private BattlePlayerInputController controller_;
		private IBattlePlayerInputDelegate inputDelegate_;
		private BattlePlayer player_;

		private bool enabled_;
	}
}