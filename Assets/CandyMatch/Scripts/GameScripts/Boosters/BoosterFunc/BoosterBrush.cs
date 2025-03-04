using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class BoosterBrush : BoosterFunc
    {
        [SerializeField]
        private float speed = 20f;
        [SerializeField]
        private EaseAnim moveEase = EaseAnim.EaseInSine;
        [SerializeField]
        private float sweepSpeed = 60f;
        [SerializeField]
        private EaseAnim sweepEase = EaseAnim.EaseInSine;
        [SerializeField]
        private GameObject usePrefab;
        [SerializeField]
        private float useTime = 0.4f;
        [SerializeField]
        private Vector3 offset;



        //private ScoreController SC => MBoard.GetComponent<ScoreController>();

        #region override
        public override CellsGroup GetArea(GridCell hitGridCell)
        {
            CellsGroup cG = new CellsGroup();
            if (!hitGridCell) return cG;
            cG.AddRange(hitGridCell.GRow.GetUsedArea());
            return cG;
        }

        public override void Apply(GridCell gCell, Action completeCallBack)
        {
            ParallelTween par0 = new ParallelTween();
            TweenSeq bTS = new TweenSeq();
            CellsGroup area = GetArea(gCell);
            GridCell leftGC = area.GetLowermostX();
            GridCell rightGC = area.GetTopmostX();
            Vector3 lPos = leftGC.transform.position + offset;
            Vector3 rPos = rightGC.transform.position + offset;

            //move activeBooster
            float dist = Vector2.Distance(transform.position, lPos);

            bTS.Add((callBack) =>
            {
                SimpleTween.Move(gameObject, transform.position, lPos, dist / speed).AddCompleteCallBack(() =>
                {
                    GetComponent<SpriteRenderer>().enabled = false;
                    callBack();
                }).SetEase(moveEase);
            });

            // play use animation
            if (usePrefab)
            {
                bTS.Add((callBack) =>
                {
                   
                    GameObject g = Creator.InstantiateAnimPrefab(usePrefab, transform, transform.position, SortingOrder.Booster);
                    TweenExt.DelayAction(gameObject, useTime, () =>
                    {
                        callBack();
                    });// delay
                });
            }

            // sweep
            bTS.Add((callBack) =>
            {
                dist = Vector2.Distance(transform.position, rPos);
                SimpleTween.Move(gameObject, transform.position, rPos, dist / sweepSpeed).AddCompleteCallBack(() =>
                {
                    SetActive(gameObject, false, 0.0f);
                    callBack();
                }).SetEase(sweepEase);
            });

            float delay = 0.0f;
            // delay = 0.1f;
            int length = area.Length;
            foreach (var c in area.Cells)
            {
                float d = delay;
                par0.Add((callBack) =>
                {
                    TweenExt.DelayAction(gameObject, d,
                       () =>
                       {
                           //if (c.IsMatchable) c.CollectMatch(0, true, true, false, MBoard.showBombExplode, MBoard.showScore, MBoard.MatchScore, callBack);
                           //else c.BoosterHit(callBack);
                           c.BoosterHit(callBack);
                       }
                       );
                });
                delay += 0.1f;
            }

            bTS.Add((callback) =>
            {
                par0.Start(() =>
                {
                    //Debug.Log("NEED FIXING");
                    //ScoreHolder.Add(length * SC.BaseMatchScore);  
                    callback();
                });
            });

            bTS.Add((callback) =>
            {
                if (gameObject) Destroy(gameObject);
                completeCallBack?.Invoke();
                callback();
            });

            bTS.Start();
        }
        #endregion override
    }
}

