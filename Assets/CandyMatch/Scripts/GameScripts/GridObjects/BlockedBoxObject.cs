using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class BlockedBoxObject : BlockedObject
    {
        public int occupiedCols;
        public int occupiedRows;

        [SerializeField]
        private GameObject boxDestroyPrefab;

        [SerializeField]
        private List<InBoxObject> inboxObjects;

        #region override
        public override int Protection
        {
            get { return inboxObjects.Count; }
        }

        public override void Hit(GridCell gCell, Action completeCallBack)
        {
            if (Protection == 0)
            {
                completeCallBack?.Invoke();
                return;
            }

            ApplyHit(gCell, completeCallBack);
        }

        public override void SideMatchHit(GridCell gCell, int matchID, Action completeCallBack)
        {
            // Debug.Log("side match hit: " + matchID);
            if (Protection <= 0 || !CanSideHitWithMatch(matchID))
            {
                completeCallBack?.Invoke();
                return;
            }

            ApplyHit(gCell, completeCallBack);
        }

        public override void CancellTweensAndSequences()
        {
            base.CancellTweensAndSequences();
        }

        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (!SRenderer) return;
            if (set)
            {
                SRenderer.sortingOrder = SortingOrder.BlockedToFront;
            }
            else
            {
                SRenderer.sortingOrder = SortingOrder.Blocked;
            }
            for (int i = 0; i < inboxObjects.Count; i++)
            {
                if (inboxObjects[i]) inboxObjects[i].GetComponent<SpriteRenderer>().sortingOrder = i + SRenderer.sortingOrder + 1;
            }
        }

        public override string ToString()
        {
            return "BlockedBox: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            //parent.DestroyGridObjects(); // new
            DestroyHierCompetitor(parent);

            BlockedBoxObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "blocked box " + parent.ToString();
#endif
          //  gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            gridObject.Enumerate(ID);
            return gridObject;
        }

        public override bool CanSetBySize(GridCell gCell)
        {
            List<GridCell> cells = GetOccupiedCells(gCell);
            // Debug.Log("cells: " + cells.MakeString(";"));
            return (cells.Count == occupiedCols * occupiedRows);
        }

        /// <summary>
        /// Get occupied size (rows, columns)
        /// </summary>
        /// <returns></returns>
        public override Vector2Int GetSize()
        {
            return new Vector2Int(occupiedRows, occupiedCols);
        }

        List<GridCell> resOccupL;
        public override List<GridCell> GetOccupiedCells(GridCell gCell)
        {
            if (resOccupL == null) resOccupL = new List<GridCell>(occupiedRows * occupiedCols);
            else resOccupL.Clear();

            int cRow = gCell.Row;
            int cCol = gCell.Column;
            MatchGrid mGrid = gCell.MGrid;
            GridCell _gCell;
            for (int r = cRow; r > cRow - occupiedRows; r--)
            {
                for (int c = cCol; c < cCol + occupiedCols; c++)
                {
                    _gCell = mGrid[r, c];
                    if (_gCell && !_gCell.IsDisabled) resOccupL.Add(_gCell);
                }
            }
            return resOccupL;
        }

        public override int RemainingHits()
        {
            return Protection;
        }
        #endregion override

        private Vector3 GetCenterPosition()
        {
            List<GridCell> res = GetOccupiedCells();
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < res.Count; i++)
            {
                pos += res[i].transform.position;
            }
            pos = pos / (float)res.Count;

            return pos;
        }

        private void ApplyHit(GridCell gCell, Action completeCallBack)
        {
            Hits++;

            InBoxObject hitObject = inboxObjects[0];
            inboxObjects.RemoveAt(0);

            if (hitAnimPrefab)
            {
                Creator.InstantiateAnimPrefab(hitAnimPrefab, hitObject.transform.parent, hitObject.transform.position, SortingOrder.MainExplode);
            }
            hitObject.transform.parent = null;
            Destroy(hitObject.gameObject, 0.1f);
            CollectEvent?.Invoke(TargetGroupID);


            if (Protection <= 0)
            {
                Vector3 center = GetCenterPosition();
                transform.parent = null;
                hitDestroySeq = new TweenSeq();

                SetToFront(true);

                hitDestroySeq.Add((callBack) =>
                {
                    TweenExt.DelayAction(gameObject, 0.1f, callBack);
                });

                hitDestroySeq.Add((callBack) =>
                {
                    SRenderer.enabled = false;
                    Instantiate(boxDestroyPrefab, center, Quaternion.identity);
                    callBack();
                });

                hitDestroySeq.Add((callBack) =>
                {
                    TweenExt.DelayAction(gameObject, 0.1f, callBack);
                });

                hitDestroySeq.Add((callBack) =>
                {
                    completeCallBack?.Invoke();
                    Destroy(gameObject);
                    callBack();
                });

                hitDestroySeq.Start();
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }
    }
}
