using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using DTEasings;
using DTObjectPoolManager;

namespace DT.Game.Transitions {
	public class TransitionAlpha : TransitionUI, ITransition {
		// PRAGMA MARK - Internal
		[Header("Alpha Outlets")]
		[SerializeField, DTValidator.Optional]
		private Image image_;

		[Header("Alpha Properties")]
		[SerializeField]
		private float minAlpha_ = 0.0f;
		[SerializeField]
		private float maxAlpha_ = 1.0f;

		protected override void Refresh(TransitionType transitionType, float percentage) {
			float startAlpha = (transitionType == TransitionType.In) ? minAlpha_ : maxAlpha_;
			float endAlpha = (transitionType == TransitionType.In) ? maxAlpha_ : minAlpha_;

			SetAlpha(Mathf.Lerp(startAlpha, endAlpha, percentage));
		}

		private void SetAlpha(float alpha) {
			if (image_ != null) {
				image_.color = image_.color.WithAlpha(alpha);
			}
		}
	}
}