using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mkey
{
    public class DynamicClickBombRandRocket : DynamicClickBombObject
    {
        private float shotDist = 3f;
        private float flyHight = 4f;
        public Action<GridCell> hitTargetAction; // used for combined bomb
        public UnityEvent hitTargetEvent;
        public GridCell RandTarget { get; private set; } // set from RocketFly

        #region temp vars
        private RocketFly rocketFly;
        #endregion temp vars

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            playExplodeTS = new TweenSeq();
            GameObject g = null;

            playExplodeTS.Add((callBack) => { delayAction(gameObject, delay, callBack); });

            playExplodeTS.Add((callBack) =>
            {
                g = Creator.InstantiateAnimPrefab(explodeAnimPrefab, gCell.transform, gCell.transform.position, SortingOrder.MainExplode);
                GetComponent<SpriteRenderer>().enabled = false;

                if (g)
                {
                    rocketFly = g.GetComponent<RocketFly>();
                    rocketFly.SelectTargetAction += (t) => { RandTarget = t; };
                    rocketFly.EndOFlyAction += () => { hitTargetAction?.Invoke(RandTarget); hitTargetEvent?.Invoke(); };   // used for combined bomb
                    rocketFly.EndOFlyAction += callBack;
                    rocketFly.startCell = gCell;
                    rocketFly.randRocketBomb = this;
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

        List<int> targetIDs;
        public override CellsGroup GetArea(GridCell gCell)
        {
            CellsGroup cG = new CellsGroup();
            targetIDs = new List<int>();
            // get targets from board
            foreach (var item in MBoard.Targets)
            {
                if (!item.Value.Achieved && !GOSet.ContainFallingObjectID(item.Value.ID) && !GOSet.ContainUnderlayObjectID(item.Value.ID)) // exclude achieved, falling and underlay
                {
                    cG.AddRange(MGrid.GetAllByTargetID(item.Key));
                    if (!targetIDs.Contains(item.Key)) targetIDs.Add(item.Key);
                }
            }
            return cG;
        }

        public override void ExplodeArea(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {

            Destroy(gameObject);
            explodePT = new ParallelTween();
            explodeTS = new TweenSeq();

            explodeTS.Add((callBack) => { delayAction(gCell.gameObject, delay, callBack); });

            // set hidden objects
            List<GridCell> area = new List<GridCell>() { RandTarget };
            List<GridCell> areaFull = new List<GridCell>() {RandTarget, gCell};
            MBoard.SetHiddenObject(areaFull);

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
                       delayAction(mcGO, t, ()=> { Instantiate(matchExplodePrefab, mcPos, Quaternion.identity, mcTransform); });   
                    }
                    ExplodeCell(mc, t, showPrefab,  hitProtection, callBack);
                });
            }

            explodeTS.Add((callBack) => { explodePT.Start(callBack); });
            explodeTS.Add((callBack) => { completeCallBack?.Invoke(); callBack(); });

            explodeTS.Start();
        }

        public override string ToString()
        {
            return "DynamicClickBombLineHor: " + ID;
        }
        #endregion override

        public static List <Vector3> GetAttackPositions(GridCell gCell, GridCell target, float flyHight)
        {
            Row<GridCell> gRow = gCell.GRow;
            Vector3 minRL = gCell.transform.InverseTransformPoint(gRow[0].transform.position);
            Vector3 maxRL = gCell.transform.InverseTransformPoint(gRow[gRow.Length-1].transform.position);

            Vector3 startPosL = Vector3.zero;
            Vector3 targetPosL = gCell.transform.InverseTransformPoint(target.transform.position);
            List<Vector3> curve = new List<Vector3>();

            float ys = startPosL.y;
            float xs = startPosL.x;
            float yt = targetPosL.y;
            float xt = targetPosL.x;

            float ymaxST = (ys > yt) ? ys : yt;
            float yminST = (ys < yt) ? ys : yt;
            float xmaxST = (xs > xt) ? xs : xt;
            float xminST = (xs < xt) ? xs : xt;

            float xCenter = (xs + xt) / 2f;
            float yCenter = (ys + yt) / 2f;

            float hPos = ymaxST + flyHight;

            bool targetRight = xt > xs;
            float xOffset = targetRight ? -UnityEngine.Random.Range(2f, 3f) : +UnityEngine.Random.Range(2f, 3f);

            Vector3 pos_1 = new Vector3(xCenter, hPos, 0) + new Vector3(xOffset, 0, 0);
            Vector3 pos_2 = new Vector3(xCenter, hPos, 0);

            // X correction
            float xCorrection = 0;
            if (pos_2.x > maxRL.x) xCorrection = -2;
            else if (pos_2.x < minRL.x) xCorrection = 2;
            Vector3 correction = new Vector3(xCorrection, 0, 0);

            // add points 
            curve.Add(startPosL);
            curve.Add(pos_1 + correction);
            curve.Add(pos_2 + correction);
            curve.Add(targetPosL);

            return curve;
        }

        public List<int> GetTargetIds()
        {
            return new List<int>(targetIDs);
        }
    }
}