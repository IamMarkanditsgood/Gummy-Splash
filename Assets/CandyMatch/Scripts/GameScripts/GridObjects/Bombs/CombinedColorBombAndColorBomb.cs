using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class CombinedColorBombAndColorBomb : CombinedBomb
    {
        [SerializeField]
        private int radius = 10;
        private float explodeSpeed = 15f;
        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            Prepare(delay, gCell);

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
               ExplodeArea(gCell, 0, true, true, false, true, completeCallBack);
            });
           
        }

        public override void ExplodeArea(GridCell gCell, float delay, bool sequenced, bool showPrefab, bool fly, bool hitProtection, Action completeCallBack)
        {
            Destroy(gameObject);
            pT = new ParallelTween();
            TweenSeq expl = new TweenSeq();
            float eSpeedInv = 1f / explodeSpeed;
            expl.Add((callBack) =>
            {
                TweenExt.DelayAction(gameObject, delay, callBack);
            });

            foreach (GridCell mc in GetArea(gCell).Cells) //parallel explode all cells
            {
                float t = 0;
                if (sequenced)
                {
                    float distance = Vector2.Distance(mc.transform.position, gCell.transform.position);
                    t = distance * eSpeedInv;
                }
                pT.Add((callBack) => {BombObject.ExplodeCell(mc, t, showPrefab, hitProtection, callBack); });
            }

            expl.Add((callBack) => { pT.Start(callBack); });
            expl.Add((callBack) =>
            {
                completeCallBack?.Invoke(); callBack();
            });

            expl.Start();
        }

        public override CellsGroup GetArea(GridCell gCell)
        {
            CellsGroup cG = new();
            List<GridCell> area = MGrid.GetAroundArea(gCell, radius).Cells;
            cG.Add(gCell);
            cG.AddRange(area);
            return cG;
        }
        #endregion override
    }
}

