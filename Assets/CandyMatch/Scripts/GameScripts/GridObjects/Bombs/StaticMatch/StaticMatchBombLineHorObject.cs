using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class StaticMatchBombLineHorObject : StaticMatchBombObject
    {
        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            Row<GridCell> r = gCell.GRow;
            playExplodeTS = new TweenSeq();
            GameObject g = null;

            playExplodeTS.Add((callBack) => { delayAction(gameObject, delay, callBack); });

            playExplodeTS.Add((callBack) =>
            {
                GridCell right = null;
                GridCell left = null;
                float scale = 0;
                if (r != null)
                {
                    right = r.GetRightDynamic();
                    left = r.GetLeftDynamic();
                }
                if (right != null && left != null)
                {
                    int rowCount = right.Column - left.Column;
                    Vector3 pos = r.GetDynamicCenter();
                    scale = (rowCount > 0) ? rowCount / 6.0f : 1f;
                    g = Creator.InstantiateAnimPrefab(explodeAnimPrefab, gCell.transform, pos, SortingOrder.MainExplode);
                }

                if (g)
                {
                    g.transform.localScale = new Vector3(g.transform.localScale.x * scale, g.transform.localScale.y, 1);
                    delayAction(g, 0.3f, callBack);
                }
                else
                {
                    callBack?.Invoke();
                }
            });

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
            cG.AddRange(gCell.GRow.GetDynamicArea());
            return cG;
        }

        public override void ExplodeArea(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            Destroy(gameObject);
            explodePT = new ParallelTween();
            explodeTS = new TweenSeq();

            explodeTS.Add((callBack) => { delayAction(gCell.gameObject, delay, callBack); });

            // set hidden
            List<GridCell> area = GetArea(gCell).Cells;
            MBoard.SetHiddenObject(area);

            foreach (GridCell mc in area) //parallel explode all cells
            {
                if (!mc) continue;
                Vector3 mcPos = mc.transform.position;
                Transform mcTransform = mc.transform;
                GameObject mcGO = mc.gameObject;

                float t = 0;
                if (sequenced)
                {
                    float distance = Vector2.Distance(mcPos, gCell.transform.position);
                    t = distance / 15f;
                }

                explodePT.Add((callBack) =>
                {
                    if (matchExplodePrefab)
                    {
                        delayAction(mcGO, t, () => { Instantiate(matchExplodePrefab, mcPos, Quaternion.identity, mcTransform); });
                    }
                    ExplodeCell(mc, t, showPrefab, hitProtection, callBack);
                });
            }

            explodeTS.Add((callBack) => { explodePT.Start(callBack); });
            explodeTS.Add((callBack) => { completeCallBack?.Invoke(); callBack(); });

            explodeTS.Start();
        }

        public override string ToString()
        {
            return "StaticMatchBombLineHor: " + ID;
        }
        #endregion override
    }
}
