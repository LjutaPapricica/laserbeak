using System;
using System.Collections;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.ScrollableMenuPopups;

namespace DT.Game.LevelEditor {
	public class LevelEditorCursor : MonoBehaviour, IRecycleCleanupSubscriber {
		// PRAGMA MARK - Public Interface
		public event Action OnMoved = delegate {};

		public void Init(InputDevice inputDevice) {
			inputDevice_ = inputDevice;

			ScrollableMenuPopup.OnShown += LockInPlace;
			ScrollableMenuPopup.OnHidden += UnlockInPlace;

			MenuView.OnShown += LockInPlace;
			MenuView.OnHidden += UnlockInPlace;
		}


		// PRAGMA MARK - IRecycleCleanupSubscriber Implementation
		void IRecycleCleanupSubscriber.OnRecycleCleanup() {
			inputDevice_ = null;
			locked_ = false;

			ScrollableMenuPopup.OnShown -= LockInPlace;
			ScrollableMenuPopup.OnHidden -= UnlockInPlace;

			MenuView.OnShown -= LockInPlace;
			MenuView.OnHidden -= UnlockInPlace;
		}


		// PRAGMA MARK - Internal
		private const float kCursorSpeed = 0.3f;

		private InputDevice inputDevice_;
		private bool locked_ = false;

		private void Update() {
			if (inputDevice_ == null) {
				return;
			}

			if (locked_) {
				return;
			}

			Vector3 newPosition = this.transform.position + (inputDevice_.LeftStick.Value.Vector3XZValue() * kCursorSpeed);
			newPosition = newPosition.SetX(Mathf.Clamp(newPosition.x, -LevelEditorConstants.kArenaHalfWidth, LevelEditorConstants.kArenaHalfWidth));
			newPosition = newPosition.SetZ(Mathf.Clamp(newPosition.z, -LevelEditorConstants.kArenaHalfHeight, LevelEditorConstants.kArenaHalfHeight));

			Vector3 oldPosition = this.transform.position;
			this.transform.position = newPosition;
			if (oldPosition != this.transform.position) {
				OnMoved.Invoke();
			}
		}

		private void LockInPlace() {
			locked_ = true;
		}

		private void UnlockInPlace() {
			locked_ = false;
		}
	}
}