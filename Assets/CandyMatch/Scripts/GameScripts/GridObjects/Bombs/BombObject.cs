using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class BombObject : GridObject
    {
        [SerializeField]
        protected bool sequenced = false;
        [HideInInspector]
        [SerializeField]
        public BombDir bombDirection;
        [HideInInspector]
        public int matchID;

        [SerializeField]
        protected GameObject explodeAnimPrefab;
        [SerializeField]
        protected GameObject matchExplodePrefab;

        protected TweenSeq playExplodeTS;
        protected ParallelTween explodePT;
        protected TweenSeq explodeTS;

        #region static
        public static void ExplodeCell(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            ExplodeCell(gCell, delay, showPrefab, hitProtection, false, completeCallBack);
        }

        public static void ExplodeCell(GridCell gCell, float delay, bool showPrefab, bool hitProtection, bool sideMatchHit, Action completeCallBack)
        {
            if (gCell.GetBomb())
            {
                gCell.ExplodeBomb(delay, true, true, completeCallBack);
                return;
            }

            if (gCell.Overlay && gCell.Overlay.BlockMatch)
            {
                gCell.Overlay.Hit(gCell, null);
                completeCallBack?.Invoke();
                return;
            }

            if (gCell.Blocked && gCell.Blocked.ExplodeHit)  // hit blocker
            {
                gCell.Blocked.Hit(gCell, null);
                completeCallBack?.Invoke();
                return;
            }

            if (gCell.DynamicBlocker && gCell.DynamicBlocker.ExplodeHit)  // hit dyn blocker
            {
                gCell.DynamicBlocker.Hit(gCell, null);
                completeCallBack?.Invoke();
                return;
            }

            if (gCell.Match)
            {
                gCell.Match.Explode(gCell, showPrefab, hitProtection, sideMatchHit, delay, completeCallBack);
                return;
            }

            if (gCell.Underlay)
            {
                gCell.Underlay.Hit(gCell, null);
                completeCallBack?.Invoke();
                return;
            }

            completeCallBack?.Invoke();
        }

        public static void ExplodeArea(IEnumerable<GridCell> area, float delay, bool sequenced, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            ParallelTween pt = new ParallelTween();
            TweenSeq expl = new TweenSeq();
            GameObject temp = new GameObject();
            if (delay > 0)
            {
                expl.Add((callBack) => {
                    SimpleTween.Value(temp, 0, 1, delay).AddCompleteCallBack(callBack);
                });
            }
            float incDelay = 0;
            foreach (GridCell mc in area) //parallel explode all cells
            {
                if (sequenced) incDelay += 0.05f;
                float t = incDelay;
                pt.Add((callBack) => { ExplodeCell(mc, t, showPrefab, hitProtection, callBack); });
            }

            expl.Add((callBack) => { pt.Start(callBack); });
            expl.Add((callBack) =>
            {
                Destroy(temp);
                completeCallBack?.Invoke();
            });

            expl.Start();
        }
        #endregion static

        #region virtual
        internal virtual void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            completeCallBack?.Invoke();
        }

        public virtual void ExplodeArea(GridCell gCell, float delay, bool showPrefab, bool hitProtection, Action completeCallBack)
        {
            completeCallBack?.Invoke();
        }

        /// <summary>
        /// Return set of cells to be blown up, around gCell
        /// </summary>
        /// <param name="gCell"></param>
        /// <returns></returns>
        public virtual CellsGroup GetArea(GridCell gCell)
        {
            return new CellsGroup();
        }
        #endregion virtual

        #region override
        public override void CancellTweensAndSequences()
        {
            //if (playExplodeTS != null)
            //{
            //    playExplodeTS.Break();
            //    playExplodeTS = null;
            //}

            //if (explodePT!=null)
            //{
            //    explodePT = null;
            //}

            //if (explodeTS != null)
            //{
            //    explodeTS.Break();
            //    explodeTS = null;
            //}

            base.CancellTweensAndSequences();
        }
        #endregion override

        #region common
        public BombDir GetBombDir()
        {
            return bombDirection;
        }

        /// <summary>
        /// If matched > = 4 cretate bomb from items
        /// </summary>
        /// <param name="bombCell"></param>
        /// <param name="completeCallBack"></param>
        internal void MoveToBomb(GridCell toCell, float delay, Action completeCallBack)
        {
            SetToFront(true);
            //scale
            SimpleTween.Value(gameObject, gameObject.transform.localScale, gameObject.transform.localScale * 1.05f, 0.1f).SetOnUpdate((val) => { gameObject.transform.localScale = val; });

            // move
            SimpleTween.Move(gameObject, transform.position, toCell.transform.position, 0.25f).AddCompleteCallBack(completeCallBack).SetEase(EaseAnim.EaseInBack).SetDelay(delay);
        }
        #endregion common
    }
}