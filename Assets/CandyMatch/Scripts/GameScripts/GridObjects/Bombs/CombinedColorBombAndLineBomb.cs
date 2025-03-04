using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class CombinedColorBombAndLineBomb : CombinedBomb
    {
        [SerializeField]
        private BombObject bombLineVertPrefab;
        [SerializeField]
        private BombObject bombLineHorPrefab;
        [SerializeField]
        private GameObject additAnimPrefab;

        #region temp vars
        private CellsGroup eArea;
        private List<BombObject> bombs;
        #endregion temp vars

        #region override
        internal override void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            if (!gCell || !bombLineVertPrefab || !bombLineHorPrefab) completeCallBack?.Invoke(); // || !source 
            bombs = new List<BombObject>();

            Prepare(delay, gCell);

            eArea = GetArea(gCell);
            ParallelTween pT1 = new ParallelTween();
            float incDelay = 0f;
            if (eArea.Cells.Count > 0)
            {
                foreach (var item in eArea.Cells)
                {
                    incDelay += 0.0f;
                    float t = incDelay;
                    pT1.Add((cB) =>
                    {
                        TweenExt.DelayAction(item.gameObject, t, () =>  // delay tween
                        {
                            Vector2 relativePos = (item.transform.position - gCell.transform.position).normalized;
                            Quaternion rotation = Quaternion.FromToRotation(new Vector2(-1, 0), relativePos); // Debug.Log("Dir: " +(item.transform.position - gCell.transform.position) + " : " + rotation.eulerAngles );
                        GameObject cb = Instantiate(additAnimPrefab, transform.position, rotation);
                            cb.transform.localScale = transform.lossyScale * 1.0f;
                            SimpleTween.Move(cb, cb.transform.position, item.transform.position, 0.2f).AddCompleteCallBack(cB).SetEase(EaseAnim.EaseOutSine);
                        });
                    });
                }

                anim.Add((callBack) =>
                {
                    pT1.Start(callBack);
                });

                anim.Add((callBack) => // create line bombs
                {
                    foreach (var item in eArea.Cells)
                    {
                        BombObject r = UnityEngine.Random.Range(0, 2) == 0 ? Instantiate(bombLineVertPrefab, item.transform.position, Quaternion.identity) : Instantiate(bombLineHorPrefab, item.transform.position, Quaternion.identity);
                        r.SetToFront(true);
                        pT.Add((cB) =>
                        {
                            ExplodeBomb(r, item, 0.05f, cB);
                        });
                        bombs.Add(r);
                    }
                    callBack();
                });
            }
            else // no match cells
            {
                anim.Add((callBack) => // create bomb
                {
                    DynamicClickBombObject r = DynamicClickBombObject.CreateOverBoard(UnityEngine.Random.Range(0, 2) == 0 ?(DynamicClickBombObject)bombLineHorPrefab : (DynamicClickBombObject)bombLineVertPrefab, transform.position, transform.lossyScale);
                    pT.Add((cB) =>
                    {
                        ExplodeBomb(r, gCell, 0.05f, cB);
                    });
                    bombs.Add(r);
                    callBack();
                });
            }
                anim.Add((callBack) => // delay
            {
                TweenExt.DelayAction(gameObject, 0.25f, callBack);
            });

            anim.Add((callBack) =>
            {
                completeCallBack?.Invoke();
                callBack(); 
            });

            anim.Start();
        }

        public override void ApplyToGrid(GridCell gCell, float delay,  Action completeCallBack)
        {
            if (gCell.Blocked || gCell.IsDisabled)
            {
                completeCallBack?.Invoke();
                return;
            }
            
            PlayExplodeAnimation(gCell, delay, () =>
            {
                Destroy(gameObject);
                pT.Start(completeCallBack);
            });
        }

        public override CellsGroup GetArea(GridCell gCell)
        {
            CellsGroup cG = new CellsGroup(); // if (!gCell) return cG; cG.AddRange(MGrid.GetAllByID(source.matchID).SortByDistanceTo(gCell));
            if (!gCell) return cG;
            Dictionary<int, CellsGroup> mDict = MGrid.GetCellsWithMatchObjectsDict(true);
            List<CellsGroup> cellsGroups = new List<CellsGroup>(mDict.Values);
            cellsGroups.Sort((a, b) => { return b.Cells.Count.CompareTo(a.Cells.Count); }); // greater first
            if (cellsGroups.Count > 0 && cellsGroups[0].Cells.Count > 0) cG.AddRange(cellsGroups[0].Cells.SortByDistanceTo(gCell));
            return cG;
        }
        #endregion override
    }
}

