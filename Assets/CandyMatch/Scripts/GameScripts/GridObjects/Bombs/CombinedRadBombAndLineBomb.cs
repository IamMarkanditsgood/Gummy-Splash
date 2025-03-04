using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class CombinedRadBombAndLineBomb : CombinedBomb
    {
        [SerializeField]
        private DynamicClickBombObject bombLineVertPrefab;
        [SerializeField]
        private DynamicClickBombObject bombLineHorPrefab;

        #region temp vars
        private List<DynamicClickBombObject> bombs;
        #endregion temp vars

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell || !bombLineVertPrefab || !bombLineHorPrefab) completeCallBack?.Invoke();
            bombs = new List<DynamicClickBombObject>();

            Action<DynamicClickBombObject, GridCell> explodeOverBoard = (_prefab, _gCell) =>
            {
                if (!_gCell) return;
                DynamicClickBombObject r = DynamicClickBombObject.CreateOverBoard(_prefab, _gCell.transform.position, _gCell.transform.lossyScale);
                r.SetToFront(true);
                pT.Add((cB) =>
                {
                    ExplodeBomb(r, _gCell, 0, cB);
                });
                bombs.Add(r);
            };

            Prepare(delay, gCell);

            anim.Add((callBack) => // create 6 rockets
            {
                NeighBors nB = gCell.Neighbors;
                explodeOverBoard(bombLineVertPrefab, nB.Left);
                explodeOverBoard(bombLineVertPrefab, nB.Right);
                explodeOverBoard(bombLineHorPrefab, nB.Top);
                explodeOverBoard(bombLineHorPrefab, nB.Bottom);
                explodeOverBoard(bombLineHorPrefab, gCell);
                explodeOverBoard(bombLineVertPrefab, gCell);
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

