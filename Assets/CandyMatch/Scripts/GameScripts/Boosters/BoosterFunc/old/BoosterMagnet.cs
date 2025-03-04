using UnityEngine;
using System;

namespace Mkey
{
    public class BoosterMagnet : BoosterFunc
    {
        [SerializeField]
        private float speed = 20f;
        [SerializeField]
        private GameObject animPrefab;

        private ScoreController SC => MBoard.GetComponent<ScoreController>();

        #region override
        public override CellsGroup GetArea(GridCell hitGridCell)
        {
            CellsGroup cG = new CellsGroup();
            GridCell[] area = hitGridCell.GRow.cells;
            foreach (var item in area)
            {
                if (item.IsMatchable) cG.Add(item);
            }
            area = hitGridCell.GColumn.cells;
            foreach (var item in area)
            {
                if (item.IsMatchable) cG.Add(item);
            }

            return cG;
        }

        public override void Apply(GridCell gCell, Action completeCallBack)
        {
            ParallelTween par0 = new ParallelTween();
            ParallelTween par1 = new ParallelTween();
            ParallelTween par2 = new ParallelTween();
            TweenSeq bTS = new TweenSeq();
            CellsGroup area = GetArea(gCell);

            //move activeBooster
            float dist = Vector2.Distance(transform.position, gCell.transform.position); // avoid z coordinat
            bTS.Add((callBack) =>
            {
                SimpleTween.Move(gameObject, transform.position, gCell.transform.position, dist / speed).AddCompleteCallBack(() =>
                {
                    SetActive(gameObject, false, 0.25f);
                    callBack();
                }).SetEase(EaseAnim.EaseInSine);
            });

            //apply effect for each cell parallel
            float delay = 0.0f;
            foreach (var c in area.Cells)
            {
                delay += 0.1f;
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

                par0.Add((callBack) =>
                {
                    TweenExt.DelayAction(gameObject, d,
                        () =>
                        {
                            c.Match.SideHitCells(c, callBack);
                        }
                         );

                });

                par0.Add((callBack) =>
                {
                    TweenExt.DelayAction(gameObject, d,
                        () =>
                        {
                            c.DirectMatchHit(callBack);
                        }
                         );

                });
            }

            //move to magnet
            delay = 0.0f;
            foreach (var c in area.Cells)
            {
                float d = delay;
                par1.Add((callBack) =>
                {
                    SimpleTween.Move(c.Match.gameObject, c.Match.transform.position, gCell.transform.position, 0.2f).AddCompleteCallBack(() =>
                    {
                        callBack();
                    }).SetDelay(d);
                });
                delay += 0.05f;
            }

            // collect
            delay = 0.0f;
            int length = area.Length;
            foreach (var c in area.Cells)
            {
                float d = delay;
                par2.Add((callBack) =>
                {
                    c.CollectMatch(d, true, false, false, false, MBoard.showScore, MBoard.MatchScore, callBack);
                });
                delay += 0.15f;
            }

            bTS.Add((callback) =>
            {
                par0.Start(() =>
                {
                    callback();
                });
            });

            bTS.Add((callback) =>
            {
                par1.Start(() =>
                {
                    callback();
                });
            });

            bTS.Add((callback) =>
            {
                par2.Start(() =>
                {
                    ScoreHolder.Add(length * SC.BaseMatchScore);
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

