using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using DTAnimatorStateMachine;
using DTObjectPoolManager;
using InControl;

using DT.Game.Battle.Walls;

namespace DT.Game.LevelEditor {
	public class LevelEditor : MonoBehaviour, IRecycleCleanupSubscriber {
		// PRAGMA MARK - Public Interface
		public LevelEditorCursor Cursor {
			get { return cursor_; }
		}

		public void SetObjectToPlace(GameObject prefab, Action<GameObject> instanceInitialization = null) {
			CleanupPlacer();

			if (prefab.GetComponent<Wall>() != null) {
				placerObject_ = ObjectPoolManager.Create(GamePrefabs.Instance.WallPlacerPrefab, parent: this.gameObject);
			} else if (prefab.GetComponent<LevelEditorPlayerSpawnPoint>() != null) {
				placerObject_ = ObjectPoolManager.Create(GamePrefabs.Instance.PlayerSpawnPointPlacerPrefab, parent: this.gameObject);
			} else {
				placerObject_ = ObjectPoolManager.Create(GamePrefabs.Instance.PlatformPlacerPrefab, parent: this.gameObject);
			}
			var placer = placerObject_.GetComponent<IPlacer>();
			placer.Init(prefab, dynamicArenaData_, undoHistory_, inputDevice_, this, instanceInitialization);
		}

		public void Init(InputDevice inputDevice, Action exitCallback) {
			dynamicArenaData_ = new DynamicArenaData();
			if (levelToLoad_ != null) {
				dynamicArenaData_.ReloadFromSerialized(levelToLoad_.text);
			}

			inputDevice_ = inputDevice;

			cursorContextMenu_ = new CursorContextMenu(inputDevice, this);
			levelEditorMenu_ = new LevelEditorMenu(inputDevice, exitCallback, SaveDataToEditor);

			undoHistory_ = new UndoHistory(dynamicArenaData_, inputDevice);

			dynamicArenaView_.Init(dynamicArenaData_);

			cursor_ = ObjectPoolManager.Create<LevelEditorCursor>(GamePrefabs.Instance.LevelEditorCursorPrefab, parent: this.gameObject);
			cursor_.Init(inputDevice);

			SetObjectToPlace(GamePrefabs.Instance.LevelEditorObjects.FirstOrDefault());
		}


		// PRAGMA MARK - IRecycleCleanupSubscriber Implementation
		void IRecycleCleanupSubscriber.OnRecycleCleanup() {
			if (cursor_ != null) {
				ObjectPoolManager.Recycle(cursor_);
				cursor_ = null;
			}

			CleanupPlacer();

			if (undoHistory_ != null) {
				undoHistory_.Dispose();
				undoHistory_ = null;
			}

			if (levelEditorMenu_ != null) {
				levelEditorMenu_.Dispose();
				levelEditorMenu_ = null;
			}

			if (cursorContextMenu_ != null) {
				cursorContextMenu_.Dispose();
				cursorContextMenu_ = null;
			}
		}


		// PRAGMA MARK - Internal
		[Header("Outlets")]
		[SerializeField]
		private DynamicArenaView dynamicArenaView_;

		[SerializeField, DTValidator.Optional]
		private TextAsset levelToLoad_;

		private LevelEditorCursor cursor_;
		private GameObject placerObject_;
		private UndoHistory undoHistory_;
		private LevelEditorMenu levelEditorMenu_;
		private CursorContextMenu cursorContextMenu_;
		private InputDevice inputDevice_;

		private DynamicArenaData dynamicArenaData_ = new DynamicArenaData();

		private void SaveDataToEditor() {
			string directoryPath = Path.Combine(Application.dataPath, "CustomLevels");
			Directory.CreateDirectory(directoryPath);

			string[] filenames = Directory.GetFiles(directoryPath);
			int index = 1;

			string filename = "";
			do {
				filename = string.Format("CustomLevel{0}.txt", index);
				index++;
			} while (filenames.Any(f => f.Contains(filename)));

			File.WriteAllText(Path.Combine(directoryPath, filename), dynamicArenaData_.Serialize());
		}

		private void CleanupPlacer() {
			if (placerObject_ != null) {
				ObjectPoolManager.Recycle(placerObject_);
				placerObject_ = null;
			}
		}
	}
}