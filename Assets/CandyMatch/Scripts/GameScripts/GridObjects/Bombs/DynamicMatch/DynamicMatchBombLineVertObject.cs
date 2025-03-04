using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class DynamicMatchBombLineVertObject : DynamicMatchBombObject
    {
        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            Column<GridCell> c = gCell.GColumn;
            playExplodeTS = new TweenSeq();
            GameObject g = null;

            playExplodeTS.Add((callBack) => { delayAction(gameObject, delay, callBack); });

            playExplodeTS.Add((callBack) =>
            {
                GridCell bot = null;
                GridCell top = null;
                float scale = 0;
                if (c != null)
                {
                    bot = c.GetBottomDynamic();
                    top = c.GetTopDynamic();
                }
                if (bot != null && top != null)
                {
                    int colCount = bot.Row - top.Row;
                    Vector3 pos = c.GetDynamicCenter();
                    scale = (colCount > 0) ? colCount / 6.0f : 1f;
                    g = Creator.InstantiateAnimPrefab(explodeAnimPrefab, gCell.transform, pos, SortingOrder.MainExplode);
                }

                if (g)
                {
                    g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y * scale, 1);
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
            cG.AddRange(gCell.GColumn.GetDynamicArea());
            return cG;
        }

        public override void ExplodeArea(GridCell gCell, float delay,  bool showPrefab, bool hitProtection, Action completeCallBack)
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
                    t = (sequenced) ? distance / 15f : 0;
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
            return "DynamicMatchBombVert: " + ID;
        }
        #endregion override
    }
}