using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class BoosterWand: BoosterFunc
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
            List<GridCell> area = MGrid.GetAllByID(hitGridCell.Match.ID);
            cG.Add(hitGridCell);
            foreach (var item in area)
            {
                if (hitGridCell.IsMatchObjectEquals(item) && item.IsMatchable) cG.Add(item);
            }

            return cG;
        }

        public override void Apply(GridCell gCell, Action completeCallBack)
        {
            ParallelTween par0 = new ParallelTween();
            ParallelTween par1 = new ParallelTween();
            TweenSeq bTS = new TweenSeq();
            CellsGroup area = GetArea(gCell);
            Transform tempParent = new GameObject("temp wand parent").transform;
            tempParent.position = transform.position;
            transform.parent = tempParent;

            float dist = Vector2.Distance(transform.position, gCell.transform.position);
            //move activeBooster
            bTS.Add((callBack) =>
            {
                SimpleTween.Move(gameObject, transform.position, gCell.transform.position, dist / speed).AddCompleteCallBack(() =>
                {
                    callBack();
                }).SetEase(EaseAnim.EaseInSine);
            });

            // duplicate and move
            foreach (var c in area.Cells)
            {
                if (c != gCell)
                    par0.Add((callBack) =>
                    {
                        GameObject boost = Instantiate(gameObject, transform.position, Quaternion.identity, tempParent); // duplicate
                        SimpleTween.Move(boost, gCell.transform.position, c.transform.position, 0.25f).AddCompleteCallBack(() =>
                        {
                            callBack();
                        }).SetEase(EaseAnim.EaseInSine);
                    });
            }

            //apply effect for each cell parallel
            float delay = 0.0f;
            foreach (var c in area.Cells)
            {
                delay += 0.15f;
                float d = delay;
                par1.Add((callBack) =>
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
            }

                // destroy boosters
                delay += 0.15f;
                float dd = delay;
                par1.Add((callBack) =>
                {
                    TweenExt.DelayAction(gameObject, dd,
                        () =>
                        {
                            if (tempParent) Destroy(tempParent.gameObject);
                        }
                        );
                    callBack();
                });

            // collect match objects
            delay = 0.15f;
            int length = area.Length;
            foreach (var c in area.Cells)
            {
                delay += 0.15f;
                float d = delay;
                par1.Add((callBack) =>
                {
                    c.CollectMatch(d, true, true, true, false, MBoard.showScore, MBoard.MatchScore, callBack);
                });
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
                par1.Start(() =>
                {
                    callback();
                });
            });

            bTS.Add((callback) =>
            {
                completeCallBack?.Invoke();
                callback();
            });

            bTS.Start();
        }
        #endregion override

        private BoosterFunc Duplicate(int sortingOrder, Vector3 position, Transform parent)
        {
            BoosterFunc sceneObject = Instantiate(this, position, Quaternion.identity, parent);
            if (sceneObject)
            {
                SpriteRenderer sr = sceneObject.GetOrAddComponent<SpriteRenderer>();
                sr.sortingOrder = sortingOrder;
            }
            return sceneObject;
        }
    }
}


