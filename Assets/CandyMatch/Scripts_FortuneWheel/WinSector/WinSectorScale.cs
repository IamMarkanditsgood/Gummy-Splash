using Mkey;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MkeyFW {
    public class WinSectorScale : WinSectorBehavior
    {
        [SerializeField]
        private float tweenTime = 1f;
        [SerializeField]
        private float targetScale = 1.1f;
        [SerializeField]
        private float tweenDelay = 1f;

        #region override
        protected override void PlayWin()
        {
            base.PlayWin();
            SimpleTween.Value(gameObject, 1, targetScale, tweenTime).SetOnUpdate((float f) => { ScaleSector(WinSector, f); }).SetEase(EaseAnim.EaseOutBounce).SetDelay(tweenDelay);
        }

        protected override void Cancel()
        {
            base.Cancel();
            if (!this) return;
            SimpleTween.Cancel(gameObject, false);
            ScaleSector(WinSector, 1);
        }
        #endregion override


        private void ScaleSector(Sector sector, float scale)
        {
            if (sector) sector.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}