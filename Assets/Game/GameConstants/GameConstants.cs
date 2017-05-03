using System;
using System.Collections;
using UnityEngine;

using DTAnimatorStateMachine;
using DTEasings;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle.AI;
using DT.Game.Battle.Players;
using DT.Game.GameModes;

namespace DT.Game {
	public class GameConstants : Singleton<GameConstants> {
		// PRAGMA MARK - Public Interface
		public GameMode[] GameModes {
			get { return gameModes_; }
		}

		public void ConfigureWithSubstitutePlayerAI(BattlePlayer player) {
			AIStateMachine ai = ObjectPoolManager.Create<AIStateMachine>(GamePrefabs.Instance.AIPrefab, parent: player.gameObject);
			ai.Init(player, substitutePlayerAIConfiguration_);
		}

		public int ScoreToWin = 5;
		public int PlayersToFill = 4;

		[Space]
		public float ColliderDisappearDelay = 0.2f;

		[Header("Player")]
		public BattlePlayerSkin[] PlayerSkins;
		public BattlePlayerSkin EnemySkin;

		public Material PlayerOpaqueMaterial;
		public Material PlayerTransparentMaterial;


		// PRAGMA MARK - Internal
		[Header("Properties")]
		[SerializeField]
		private GameMode[] gameModes_;

		[SerializeField]
		private AIConfiguration substitutePlayerAIConfiguration_;
	}
}