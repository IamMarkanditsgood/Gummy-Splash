using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class CombinedRandRocketAndRandRocket : CombinedBomb
    {
        [SerializeField]
        private DynamicClickBombRandRocket randRocketPrefab;

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            Prepare(delay, gCell);

            anim.Add((callBack) => // explode #1 rocket
            {
                pT.Add((cB) =>
                    {
                        DynamicClickBombRandRocket rR = Instantiate(randRocketPrefab, gCell.transform.position, Quaternion.identity);
                        rR.SetToFront(true);
                        ExplodeBomb(rR, gCell, 0.0f, cB);
                    });
                callBack();
            });

            anim.Add((callBack) => // delay
            {
                TweenExt.DelayAction(gameObject, 0.1f, callBack);
            });

            anim.Add((callBack) => // explode #2 rocket
            {
                pT.Add((cB) =>
                {
                    DynamicClickBombRandRocket rR = Instantiate(randRocketPrefab, gCell.transform.position, Quaternion.identity);
                    rR.SetToFront(true);
                    ExplodeBomb(rR, gCell, 0.15f, cB);
                });
                callBack();
            });

            anim.Add((callBack) =>
            {
                completeCallBack?.Invoke();
                callBack();
            });

            anim.Start();
        }

        public override void ApplyToGrid(GridCell gCell, float delay,  Action completeCallBack)
        {
            if (gCell.Blocked || gCell.IsDisabled)
            {
                completeCallBack?.Invoke();
                return;
            }

            PlayExplodeAnimation(gCell, delay, () =>
            {
                Destroy(gameObject);
                pT.Start(completeCallBack);
            });
           
        }
        #endregion override
    }
}

