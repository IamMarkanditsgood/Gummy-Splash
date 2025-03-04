using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class CombinedRandRocketAndRadBomb : CombinedBomb
    {
        [SerializeField]
        private DynamicClickBombRandRocket randRocketPrefab;
        [SerializeField]
        private DynamicClickBombObject bombRadPrefab;
        [SerializeField]
        private DynamicClickBombObject smalRadBombPrefab;

        #region temp vars
        #endregion temp vars

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            Action<DynamicClickBombObject, GridCell, Action> explodeOverBoard = (_prefab, _gCell, callBack) =>
            {
                if (!_gCell) return;
                DynamicClickBombObject r = DynamicClickBombObject.CreateOverBoard(_prefab, _gCell.transform.position, _gCell.transform.lossyScale);

                r.SetToFront(true);
                ExplodeBomb(r, _gCell, 0, callBack);
            };

            Prepare(delay, gCell);

            if (smalRadBombPrefab)
            {
                anim.Add((callBack) =>
                {
                    DynamicClickBombObject rB = Instantiate(smalRadBombPrefab, gCell.transform.position, Quaternion.identity);
                    ExplodeBomb(rB, gCell, 0, null);
                    callBack();
                });
            }
            anim.Add((callBack) =>
            {
                DynamicClickBombRandRocket rR = Instantiate(randRocketPrefab, gCell.transform.position, Quaternion.identity);
                rR.SetToFront(true);
                rR.hitTargetAction += (_gC) => { explodeOverBoard(bombRadPrefab, _gC, callBack); };
                ExplodeBomb(rR, gCell, 0, null);
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
                // pT.Start(completeCallBack);
                completeCallBack?.Invoke();
            });
           
        }
        #endregion override
       
    }
}

