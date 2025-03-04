using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class DynamicMatchBombColorObject : DynamicMatchBombObject
    {
        [SerializeField]
        private Sprite flyObjectSprite;
        [SerializeField]
        private bool applySideMatchHit = true;

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            playExplodeTS = new TweenSeq();
            ParallelTween par0 = new ParallelTween();
            GameObject g = null;

            playExplodeTS.Add((callBack) => { delayAction(gameObject, delay, callBack); });

            playExplodeTS.Add((callBack) =>
            {
                g = Creator.InstantiateAnimPrefab(explodeAnimPrefab, gCell.transform, gCell.transform.position, SortingOrder.MainExplode);
                if (g)
                {
                    g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, 1);
                    delayAction(g, 0.3f, callBack);
                }
                else
                {
                    callBack?.Invoke();
                }
            });

            // duplicate and move
            foreach (var c in GetArea(gCell).Cells)
            {
                if (c != gCell)
                    par0.Add((callBack) =>
                    {
                        SpriteRenderer w = Creator.CreateSpriteAtPosition(transform, flyObjectSprite, transform.position, SortingOrder.BoosterToFront);
                        if (w && c.Match)
                        {
                            SimpleTween.Move(w.gameObject, gCell.transform.position, c.transform.position, 0.25f).AddCompleteCallBack(callBack)
                            .SetEase(EaseAnim.EaseInSine);
                        }
                        else
                        {
                            callBack?.Invoke();
                        }
                    });
            }

            playExplodeTS.Add((callBack) =>
            {
                par0.Start(callBack);
            });
            playExplodeTS.Add((callBack)=> { delayAction(gameObject, 0.25f, callBack); });

            playExplodeTS.Add((callBack) =>
            {
                if (g) Destroy(g);
                CollectEvent?.Invoke(TargetGroupID);
                completeCallBack?.Invoke();
                callBack();
            });

            playExplodeTS.Start();
        }

        public override CellsGroup GetArea(GridCell gCell)
        {
            CellsGroup cG = new CellsGroup();
            if (!gCell) return cG;
            cG.AddRange(MGrid.GetAllByID(matchID).SortByDistanceTo(gCell));
            return cG;
        }

        public override void ExplodeArea(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            Destroy(gameObject);
            explodePT = new ParallelTween();
            TweenSeq explodeTS = new TweenSeq();
             
            explodeTS.Add((callBack) => { delayAction(gCell.gameObject, delay, callBack); });

            // set hidden
            List<GridCell> area = GetArea(gCell).Cells;
            List<GridCell> areaFull = new List<GridCell>(area);
            areaFull.Add(gCell);
            MBoard.SetHiddenObject(areaFull);

            foreach (GridCell mc in area) //parallel explode all cells
            {
                float t = 0;
                if (sequenced)
                {
                    float distance = Vector2.Distance(mc.transform.position, gCell.transform.position);
                    t = (sequenced) ? distance / 15f : 0;
                }

                explodePT.Add((callBack) => { ExplodeCell(mc, t, showPrefab, hitProtection, applySideMatchHit, callBack); });
            }

            explodeTS.Add((callBack) => { explodePT.Start(callBack); });
            explodeTS.Add((callBack) => { completeCallBack?.Invoke(); callBack(); });

            explodeTS.Start();
        }

        public override string ToString()
        {
            return "DynamicMatchBombColor: " + ID;
        }
        #endregion override
    }
}