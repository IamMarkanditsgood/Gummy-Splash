using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class SubUnderlayMCObject : GridObject
    {
        public int occupiedCols;
        public int occupiedRows;

        public GameObject collectAnimPrefab;
        public GUIFlyer targetAnimPrefab;
        public Vector3 collectPosOffset;

        #region properties
        //public int Protection
        //{
        //    get { return protectionStateImages.Length + 1 - Hits; }
        //}
        #endregion properties

        #region override
        //public override void Hit(GridCell gCell, Action completeCallBack)
        //{
        //    return;
        //    //if (Protection <= 0)
        //    //{
        //    //    completeCallBack?.Invoke();
        //    //    return;
        //    //}

        //    Hits++;
        //    //SetProtectionImage();

        //    if (hitAnimPrefab)
        //    {
        //        Creator.InstantiateAnimPrefab(hitAnimPrefab, transform.parent, transform.position, SortingOrder.MainExplode);
        //    }

        //    //if (Protection <= 0)
        //    {
        //        hitDestroySeq = new TweenSeq();

        //        SetToFront(true);

        //        hitDestroySeq.Add((callBack) => // play preexplode animation
        //        {
        //            delayAction(gameObject, 0.05f, callBack);
        //        });

        //        hitDestroySeq.Add((callBack) =>
        //        {
        //            TargetCollectEvent?.Invoke(TargetGroupID);
        //            callBack();
        //        });

        //        hitDestroySeq.Add((callBack) =>
        //        {
        //            completeCallBack?.Invoke();
        //            //if (self_healing)
        //            //{
        //            //    Hits = 0;
        //            //    SetProtectionImage();
        //            //    SetToFront(false);
        //            //}
        //            //else
        //            {
        //                Destroy(gameObject);
        //            }

        //            callBack();
        //        });

        //        hitDestroySeq.Start();
        //    }
        //    //else
        //    //{
        //    //    completeCallBack?.Invoke();
        //    //}
        //}

        public override void CancellTweensAndSequences()
        {
            base.CancellTweensAndSequences();
        }

        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (set)
                SRenderer.sortingOrder = SortingOrder.SubUnderToFront;
            else
                SRenderer.sortingOrder = SortingOrder.SubUnder;
        }

        public override string ToString()
        {
            return "Sub Underlay: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled ) return null; //|| parent.Blocked
            DestroyHierCompetitor(parent);
            //if (parent.Underlay)
            //{
            //    GameObject old = parent.Underlay.gameObject;
            //    Destroy(old);
            //}
            //if (Hits > protectionStateImages.Length) return null;

            SubUnderlayMCObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "sub underlay " + parent.ToString();
#endif
          //  gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);

            //gridObject.Hits = Mathf.Clamp(Hits, 0, protectionStateImages.Length);
            //if (protectionStateImages.Length > 0 && gridObject.Hits > 0)
            //{
            //    int i = Mathf.Min(gridObject.Hits - 1, protectionStateImages.Length - 1);
            //    gridObject.SRenderer.sprite = protectionStateImages[i];
            //}
            gridObject.Enumerate(ID);
            return gridObject;
        }

        public override bool CanSetBySize(GridCell gCell)
        {
            List<GridCell> cells = GetOccupiedCells(gCell);
            // Debug.Log("cells: " + cells.MakeString(";"));
            return (cells.Count == occupiedCols * occupiedRows);
        }

        public override int GetHierarchy()
        {
            return -20;
        }

        /// <summary>
        /// Get occupied size (rows, columns)
        /// </summary>
        /// <returns></returns>
        public override Vector2Int GetSize()
        {
            return new Vector2Int(occupiedRows, occupiedCols);
        }

        public override List<GridCell> GetOccupiedCells(GridCell gCell)
        {
            List<GridCell> res = new List<GridCell>();
            int cRow = gCell.Row;
            int cCol = gCell.Column;
            MatchGrid mGrid = gCell.MGrid;
            GridCell _gCell;
            for (int r = cRow; r > cRow - occupiedRows; r--)
            {
                for (int c = cCol; c < cCol + occupiedCols; c++)
                {
                    _gCell = mGrid[r, c];
                    if (_gCell && !_gCell.IsDisabled) res.Add(_gCell);
                }
            }
            return res;
        }
        #endregion override

        /// <summary>
        /// Collect sub underlay object
        /// </summary>
        /// <param name="completeCallBack"></param>
        internal void Collect(float delay, bool showPrefab, bool fly, Action completeCallBack)
        {
            transform.parent = null;
            Vector3 aPos = transform.position + collectPosOffset;

            collectSequence = new TweenSeq();
            collectSequence.Add((callBack) =>
            {
                TweenExt.DelayAction(gameObject, delay, callBack);
            });

            // anim prefab
            if (showPrefab)
            {
                collectSequence.Add((callBack) =>
                {
                    if (SRenderer) SRenderer.enabled = false;
                    Creator.InstantiateAnimPrefab(collectAnimPrefab, transform, aPos, SortingOrder.MainExplode);
                    TweenExt.DelayAction(gameObject, 0.0f, callBack);
                });
            }
         
            collectSequence.Add((callBack) =>
            {
                if (targetAnimPrefab)
                {
                    InstantiateGuiTargetFlyer(targetAnimPrefab, aPos);
                }
                callBack();
            });

            //finish
            collectSequence.Add((callBack) =>
            {
                CollectEvent?.Invoke(TargetGroupID);
                completeCallBack?.Invoke();
                Destroy(gameObject, (fly) ? 0.6f : 0);
                callBack();
            });

            collectSequence.Start();
        }

        public bool IsFree()
        {
            List<GridCell> occupiedCells = GetOccupiedCells();
            foreach (var item in occupiedCells)
            {
                if (item.Overlay) return false;
                if (item.Blocked) return false;
                if (item.Underlay) return false;
            }
            return true;
        }

        /// <summary>
        /// returns the object that occupied this cell
        /// </summary>
        /// <param name="matchGrid"></param>
        /// <param name="gridCell"></param>
        /// <returns></returns>
        public static SubUnderlayMCObject GetOccupied(MatchGrid matchGrid, GridCell gridCell)
        {
            SubUnderlayMCObject[] source = matchGrid.Parent.GetComponentsInChildren<SubUnderlayMCObject>();

            foreach (var item in source)
            {
                List<GridCell> occupiedCells = item.GetOccupiedCells();
                if (occupiedCells.Contains(gridCell)) return item;
            }
            return null;
        }
    }
}
