using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class BoosterCannon : BoosterFunc
    {
        [SerializeField]
        private float speed = 20f;
        [SerializeField]
        private GameObject animPrefab;
        [SerializeField]
        private GameObject usePrefab;
        [SerializeField]
        private GameObject shotPrefab;
        [SerializeField]
        private float useDelay = 1f;
        [SerializeField]
        private EaseAnim moveInEase; // EaseAnim.EaseInSine
        [SerializeField]
        private bool moveOut;
        [SerializeField]
        private EaseAnim moveOutEase;
        [SerializeField]
        private float sequenceDelay = 0.1f;

        [SerializeField]
        private Vector3 useOffset;

        //private ScoreController SC => MBoard.GetComponent<ScoreController>();

        #region override
        public override CellsGroup GetArea(GridCell hitGridCell)
        {
            CellsGroup cG = new CellsGroup();
            if (!hitGridCell) return cG;
            cG.AddRange(hitGridCell.GColumn.GetUsedArea());
            return cG;
        }

        public override void Apply(GridCell gCell, Action completeCallBack)
        {
            ParallelTween par0 = new ParallelTween();
            TweenSeq bTS = new TweenSeq();
            CellsGroup area = GetArea(gCell);
            GridCell botCell = area.GetLowermostY();
            area.Cells.SortByDistanceTo(botCell);
            area.Cells.Reverse();
            GameObject gUse;
            Vector3 startPos = new Vector3(botCell.transform.position.x, transform.position.y, transform.position.z); 

            //move activeBooster
            float dist = Vector2.Distance(startPos, botCell.transform.position + useOffset);
            bTS.Add((callBack) =>
            {
                SimpleTween.Move(gameObject, startPos, botCell.transform.position + useOffset, dist / speed).AddCompleteCallBack(() =>
                {
                    callBack();
                }).SetEase(moveInEase);
            });

            // play use cannon animation
            if (usePrefab)
            {
                bTS.Add((callBack) =>
                {
                    GetComponent<SpriteRenderer>().enabled = false;
                    gUse = Creator.InstantiateAnimPrefab(usePrefab, transform, transform.position, SortingOrder.Booster);
                    SimpleTween.Value(gameObject, 0, 1, useDelay).AddCompleteCallBack(() =>
                    {
                        // GetComponent<SpriteRenderer>().enabled = true; 
                        // Destroy(gUse); 
                        // Creator.InstantiateAnimPrefab(shotPrefab, transform, transform.position, SortingOrder.Booster + 2);
                        if (shotPrefab) 
                        {
                            GameObject g = Instantiate(shotPrefab);
                            g.transform.position = transform.position + new Vector3(0,1,0);
                            g.transform.localScale = transform.localScale * 1.0f;
                        }
                        callBack(); 
                    }); // delay
                });
            }

            if (moveOut)
            {
                bTS.Add((callBack) =>
                {
                    SimpleTween.Move(gameObject, transform.position, new Vector3(transform.position.x, -10, 0), 0.5f).AddCompleteCallBack(() =>
                    {
                        callBack();
                    }).SetEase(moveOutEase);
                });
            }

            //apply effect for each cell parallel
            float delay = 0.0f;
            foreach (var c in area.Cells)
            {
                float d = delay;
                par0.Add((callBack) =>
                {
                    TweenExt.DelayAction(gameObject, d,
                        () =>
                        {
                            GameObject g = Creator.InstantiateAnimPrefab(animPrefab, c.transform, c.transform.position, SortingOrder.Booster + 1);
                            if (g) Destroy(g, 2);
                            callBack?.Invoke();
                        }
                        );

                });
                delay += sequenceDelay;
            }

            delay = 0.03f;
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
                delay += sequenceDelay;
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

