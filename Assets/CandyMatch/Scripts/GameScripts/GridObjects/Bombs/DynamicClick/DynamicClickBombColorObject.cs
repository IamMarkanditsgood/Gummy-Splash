using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class DynamicClickBombColorObject : DynamicClickBombObject
    {
        [SerializeField]
        private GameObject additAnimPrefab;
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

            playExplodeTS.Add((callBack) =>
            {
                float incDelay = 0f;
                foreach (var c in GetArea(gCell).Cells)
                {
                    incDelay += 0.05f;
                    float t = incDelay;
                    par0.Add((cB) =>
                    {
                        TweenExt.DelayAction(c.gameObject, t, () =>  // delay tween
                        {
                            Vector2 relativePos = (c.transform.position - gCell.transform.position).normalized;
                            Quaternion rotation = Quaternion.FromToRotation(new Vector2(-1, 0), relativePos);
                            GameObject cb = Instantiate(additAnimPrefab, transform.position, rotation);
                            cb.transform.localScale = transform.lossyScale * 1.0f;
                            SimpleTween.Move(cb, cb.transform.position, c.transform.position, 0.2f).AddCompleteCallBack(cB).SetEase(EaseAnim.EaseOutSine);
                        });
                    });
                }
                callBack();
            });

            playExplodeTS.Add((callBack) =>
            {
                par0.Start(callBack);
            });
            playExplodeTS.Add((callBack)=> { TweenExt.DelayAction(gameObject, 0.0f, callBack); }); // 0.25f

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
            Dictionary<int, CellsGroup> mDict = MGrid.GetCellsWithMatchObjectsDict(true);

            // try to get match id from swapped cell, if the explosion is caused by a swap
            if (Swapped && Swapped.Match)
            {
                int id = Swapped.Match.ID;
                if (mDict.ContainsKey(id) && mDict[id].Cells.Count > 0)
                {
                    cG.AddRange(mDict[id].Cells);
                    return cG;
                }
            }
          
            List<CellsGroup> cellsGroups = new (mDict.Values);
            if (cellsGroups.Count == 0) return cG;

            cellsGroups.Sort((a, b)=> { return b.Cells.Count.CompareTo(a.Cells.Count); }); // greater first
            cG.AddRange(cellsGroups[0].Cells.SortByDistanceTo(gCell));
            return cG;
        }

        public override void ExplodeArea(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            Destroy(gameObject);
            explodePT = new ();
            TweenSeq explodeTS = new ();
             
            explodeTS.Add((callBack) => { delayAction(gCell.gameObject, delay, callBack); });

            // set hidden
            List<GridCell> area = GetArea(gCell).Cells;
            List<GridCell> areaFull = new (area);
            areaFull.Add(gCell);
            MBoard.SetHiddenObject(areaFull);

            foreach (GridCell mc in area) //parallel explode all cells
            {
                float t = 0;
                if (sequenced)
                {
                    float distance = Vector2.Distance(mc.transform.position, gCell.transform.position);
                    t = distance / 15f;
                }

                explodePT.Add((callBack) => 
                { 

                    ExplodeCell(mc, t, showPrefab, hitProtection, applySideMatchHit, callBack); 
                });
            }

            explodeTS.Add((callBack) => {
                explodePT.Start(callBack); 
            });
            explodeTS.Add((callBack) => { 
                completeCallBack?.Invoke(); 
                callBack(); 
            });

            explodeTS.Start();
        }

        public override string ToString()
        {
            return "DynamicClickBombColor: " + ID;
        }

        #endregion override
    }
}