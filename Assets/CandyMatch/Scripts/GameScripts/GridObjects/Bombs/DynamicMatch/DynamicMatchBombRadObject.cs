using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class DynamicMatchBombRadObject : DynamicMatchBombObject
    {
        [SerializeField]
        private BombObject bombLineHorPrefab;
        [SerializeField]
        private BombObject bombLineVertPrefab;

        #region temp vars
        private BombObject r1;
        private BombObject r2;
        #endregion temp vars

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell) completeCallBack?.Invoke();

            playExplodeTS = new TweenSeq();

            playExplodeTS.Add((callBack) => { delayAction(gameObject, delay, callBack); });
 
            playExplodeTS.Add((callBack) => // create line bombs
            {
                r1 = Instantiate(bombLineHorPrefab);
                r2 = Instantiate(bombLineVertPrefab);
                if (r1) r1.transform.position = gCell.transform.position;
                if (r2) r2.transform.position = gCell.transform.position;
                callBack();
            });

            playExplodeTS.Add((callBack) => // play explode anuimations
            {
                if (r1 && r2)
                {
                    r1.PlayExplodeAnimation(gCell, 0, null);
                    r2.PlayExplodeAnimation(gCell, 0, callBack);
                }
                else if (r1)
                {
                    r1.PlayExplodeAnimation(gCell, 0, callBack);
                }
                else if (r2)
                {
                    r2.PlayExplodeAnimation(gCell, 0, callBack);
                }
                else
                {
                    callBack();
                }
            });

            playExplodeTS.Add((callBack) =>
            {
                CollectEvent?.Invoke(TargetGroupID);
                completeCallBack?.Invoke();
            });

            playExplodeTS.Start();
        }

        public override void ExplodeArea(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            Destroy(gameObject);
            if (!r1)
            {
                r1 = Instantiate(bombLineHorPrefab);
            }
            if (!r2)
            {
                r2 = Instantiate(bombLineVertPrefab);
            }

            if (r1 && r2)
            {
                ParallelTween pT = new ParallelTween();
                pT.Add((callBack) => { r2.ExplodeArea(gCell, 0, showPrefab, hitProtection, callBack); });
                pT.Add((callBack) => { r1.ExplodeArea(gCell, 0, showPrefab, hitProtection, callBack); });
                pT.Start(completeCallBack);
            }
            else if (r1)
            {
                r1.ExplodeArea(gCell, 0, showPrefab, hitProtection, completeCallBack);
            }
            else if (r2)
            {
                r2.ExplodeArea(gCell, 0, showPrefab, hitProtection, completeCallBack);
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        public override string ToString()
        {
            return "DynamicMatchBombRad: " + ID;
        }
        #endregion override
    }
}