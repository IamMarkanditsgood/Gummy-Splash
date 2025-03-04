using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class BoosterBomb : BoosterFunc
    {
        [SerializeField]
        private float speed = 20f;
        [SerializeField]
        private int radius = 2;
        [SerializeField]
        private GameObject animPrefab;

        private ScoreController SC => MBoard.GetComponent<ScoreController>();

        #region override
        public override CellsGroup GetArea(GridCell hitGridCell)
        {
            CellsGroup cG = new CellsGroup();
            List<GridCell> area = MBoard.MainGrid.GetAroundArea(hitGridCell, radius).Cells;
            cG.Add(hitGridCell);

            foreach (var item in area)
            {
                if (item.IsMatchable) cG.Add(item);
            }

            return cG;
        }

        public override void Apply(GridCell gCell, Action completeCallBack)
        {
            ParallelTween par0 = new ParallelTween();
            TweenSeq bTS = new TweenSeq();
            CellsGroup area = GetArea(gCell);

            //move activeBooster
            float dist = Vector2.Distance(transform.position, gCell.transform.position);
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
                delay += 0.1f;
            }

            delay = 0.1f;
            int length = area.Length;
            foreach (var c in area.Cells)
            {
                float d = delay;
                par0.Add((callBack) =>
                {
                    c.CollectMatch(d, true, true, true, GameBoard.showBombExplode, MBoard.showScore, MBoard.MatchScore, callBack);
                });
                delay += 0.1f;
            }

            bTS.Add((callback) =>
            {
                par0.Start(() =>
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

