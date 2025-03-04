using Mkey;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MkeyFW
{
    public class WinLightSector : WinSectorBehavior
    {
        private SpriteRenderer sR;
        ColorFlasher cF;
        #region override
        protected override void PlayWin()
        {
            base.PlayWin();
            sR = GetComponent<SpriteRenderer>();
            if (!sR)
            {
                return;
            }
            FlashAlpha();

        }

        protected override void Cancel()
        {
            base.Cancel();
            if (!this) return;
            if (cF != null) cF.Cancel();
            SimpleTween.Cancel(gameObject, false);
            if (sR) sR.color = new Color(1, 1, 1, 0);
        }
        #endregion override

        private void FlashAlpha()
        {
            cF = new ColorFlasher(gameObject, null, null, new SpriteRenderer[] { sR }, null, 1);
            cF.FlashingAlpha();
        }
    }
}
