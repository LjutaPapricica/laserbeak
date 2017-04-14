using System;
using System.Collections;
using System.Linq;
using UnityEngine;

using DT.Game.Battle.Player;
using DTAnimatorStateMachine;
using DTObjectPoolManager;

namespace DT.Game.Battle.AI {
	public class AIAttackState : DTStateMachineBehaviour<AIStateMachine> {
		// PRAGMA MARK - Internal
		private const float kNearDistance = 10.0f;

		private CoroutineWrapper delayedAttackAction_;

		private BattlePlayer target_;
		private Vector3? fuzzyTargetPosition_;

		private BattlePlayerInputChargedLaser chargedLaserComponent_;
		private BattlePlayerInputChargedLaser ChargedLaserComponent_ {
			get {
				if (chargedLaserComponent_ == null) {
					chargedLaserComponent_ = StateMachine_.Player.GetComponentInChildren<BattlePlayerInputChargedLaser>();
				}
				return chargedLaserComponent_;
			}
		}

		protected override void OnStateEntered() {
			fuzzyTargetPosition_ = null;
			StateMachine_.InputState.LaserPressed = true;
			if (ChargedLaserComponent_.FullyCharged) {
				HandleFullyChargedLaser();
			} else {
				ChargedLaserComponent_.OnFullCharge += HandleFullyChargedLaser;
			}
		}

		protected override void OnStateExited() {
			ChargedLaserComponent_.OnFullCharge -= HandleFullyChargedLaser;

			if (delayedAttackAction_ != null) {
				delayedAttackAction_.Cancel();
				delayedAttackAction_ = null;
			}
		}

		protected override void OnStateUpdated() {
			UpdateTarget();

			if (target_ == null) {
				StateMachine_.SwitchState(AIStateMachine.State.Idle);
				return;
			}

			UpdateFuzzyTargetPosition();
			HeadTowardsFuzzyTargetPosition();
		}

		private void UpdateTarget() {
			// if target is valid, do nothing
			if (target_ != null && BattlePlayer.ActivePlayers.Contains(target_)) {
				StateMachine_.GizmoOutlet.SetSphere("AttackTarget", target_.transform.position, radius: 0.5f);
				return;
			}

			BattlePlayer closestEnemyPlayer = BattlePlayer.ActivePlayers.Where(p => p != StateMachine_.Player).Min(p => (p.transform.position - StateMachine_.Player.transform.position).magnitude);
			target_ = closestEnemyPlayer;

			fuzzyTargetPosition_ = null;
		}

		private void UpdateFuzzyTargetPosition() {
			Vector3 currentPosition = StateMachine_.Player.transform.position;
			float accuracyInDegrees = StateMachine_.AIConfiguration.AccuracyInDegrees();

			Vector3 targetVector = target_.transform.position - currentPosition;
			Quaternion rotationToTarget = Quaternion.LookRotation(targetVector);

			if (fuzzyTargetPosition_ != null) {
				// check if target has moved out of accurancy cone, if so recompute fuzzyTargetPosition_
				Vector3 targetPositionVector = (Vector3)fuzzyTargetPosition_ - currentPosition;
				Quaternion rotationToTargetPosition = Quaternion.LookRotation(targetPositionVector);

				float angleToTarget = Quaternion.Angle(rotationToTarget, rotationToTargetPosition);
				if (angleToTarget > accuracyInDegrees) {
					fuzzyTargetPosition_ = null;
				}
			}

			if (fuzzyTargetPosition_ != null) {
				// update fuzzyTargetPosition_ to have same distance as target_ currently does
				fuzzyTargetPosition_ = currentPosition + (((Vector3)fuzzyTargetPosition_ - currentPosition).normalized * targetVector.magnitude);
				return;
			}

			Vector3 rotationToTargetEuler = rotationToTarget.eulerAngles;
			// NOTE (darren): divide by three since 3 * standardDeviation is 95% percentile of generated normal deviation
			float newYRotation = MathUtil.SampleGaussian(rotationToTargetEuler.y, accuracyInDegrees / 3.0f);
			rotationToTargetEuler = rotationToTargetEuler.SetY(newYRotation);

			Quaternion rotationToGeneratedTargetPosition = Quaternion.Euler(rotationToTargetEuler);

			fuzzyTargetPosition_ = currentPosition + (rotationToGeneratedTargetPosition * Vector3.forward * targetVector.magnitude);
		}

		private void HeadTowardsFuzzyTargetPosition() {
			if (fuzzyTargetPosition_ == null) {
				StateMachine_.InputState.LerpMovementVectorTo(Vector2.zero);
				return;
			}

			StateMachine_.GizmoOutlet.SetSphere("AttackFuzzyTargetPositionSphere", (Vector3)fuzzyTargetPosition_, radius: 0.2f);
			StateMachine_.GizmoOutlet.SetLineTarget("AttackFuzzyTargetPosition", (Vector3)fuzzyTargetPosition_);
			Vector3 fuzzyTargetPositionVector = (Vector3)fuzzyTargetPosition_ - StateMachine_.Player.transform.position;
			Vector2 xzDirection = fuzzyTargetPositionVector.Vector2XZValue();
			if (xzDirection.magnitude <= kNearDistance) {
				// don't move if already near player position and pointing to fuzzyTargetPosition_
				Quaternion rotation = StateMachine_.Player.transform.rotation;
				Quaternion rotationToTarget = Quaternion.LookRotation(fuzzyTargetPositionVector);

				// if accurate enough then don't move anymore
				float angleToTarget = Quaternion.Angle(rotation, rotationToTarget);
				if (angleToTarget < 1.0f) {
					StateMachine_.InputState.LerpMovementVectorTo(Vector2.zero);
					return;
				}
			}

			StateMachine_.InputState.LerpMovementVectorTo(xzDirection);
		}

		private void HandleFullyChargedLaser() {
			delayedAttackAction_ = CoroutineWrapper.DoAfterDelay(StateMachine_.AIConfiguration.RandomReactionTime(), () => {
				StateMachine_.InputState.LaserPressed = false;

				// NOTE (darren): why delay here? to simulate AI watching laser hit / miss target :)
				delayedAttackAction_ = CoroutineWrapper.DoAfterDelay(StateMachine_.AIConfiguration.RandomReactionTime(), () => {
					StateMachine_.SwitchState(AIStateMachine.State.Idle);
				});
			});
		}
	}
}