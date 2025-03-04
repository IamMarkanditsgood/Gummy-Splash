using System;
using UnityEngine;

namespace Mkey
{
    public class DynamicMatchBombObject : BombObject
    {
        #region regular
        private void OnDestroy()
        {
            MatchObject m = GetMatch();
            if (m)
            {
                Destroy(m.gameObject);
            }
        }
        #endregion regular

        #region create
        /// <summary>
        /// Create new DynamicMatchBombObject
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefab"></param>
        /// <param name="addCollider"></param>
        /// <param name="radius"></param>
        /// <param name="isTrigger"></param>
        /// <returns></returns>
        public static DynamicMatchBombObject Create(GridCell parent, DynamicMatchBombObject prefab)
        {
            if (!parent || !prefab) return null;
            GridObject b = prefab.Create(parent);
            if (b) b.Enumerate(prefab.ID);
            return (b) ? b.GetComponent<DynamicMatchBombObject>() : null; 
        }

        /// <summary>
        /// Create new DynamicMatchBombObject over board, without parent
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="oData"></param>
        /// <param name="addCollider"></param>
        /// <param name="radius"></param>
        /// <param name="isTrigger"></param>
        /// <returns></returns>
        public static DynamicMatchBombObject CreateOverBoard(DynamicMatchBombObject prefab, Vector3 position, Vector3 localScale)
        {
            if (prefab == null) return null;

            DynamicMatchBombObject gridObject = Instantiate(prefab);
            if (!gridObject) return null;
            gridObject.transform.localScale = localScale;
            gridObject.transform.localPosition = position;
#if UNITY_EDITOR
            gridObject.name = "DynamicMatchBomb: " + gridObject.ID;
#endif
            return gridObject;
        }
        #endregion create

        #region override
        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (!SRenderer) return;
            if (set)
                SRenderer.sortingOrder = SortingOrder.DynamicMatchBombToFront;
            else
                SRenderer.sortingOrder = SortingOrder.DynamicMatchBomb;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled || parent.Blocked || !parent.Match) return null;

            if (parent.DynamicMatchBomb)
            {
                GameObject old = parent.DynamicMatchBomb.gameObject;
                Destroy(old);
            }
            DynamicMatchBombObject gridObject = Instantiate(this, parent.Match.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "DynamicMatchBomb: " + ID;
#endif
           // gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            SpriteRenderer pSR = parent.Match.GetComponent<SpriteRenderer>();
            if (pSR) pSR.enabled = false;
            gridObject.Enumerate(ID);
            return gridObject;
        }
        #endregion override

        internal MatchObject GetMatch()
        {
            return GetComponentInParent<MatchObject>();
        }
    }
}