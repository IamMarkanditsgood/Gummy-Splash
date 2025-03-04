using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class DynamicClickBombObject : BombObject
    {
        public bool canSwap = false;
        #region properties
        public GridCell Swapped { get; set; }  // swapped cell, if the explosion is caused by a swap
        #endregion properties

        #region regular

        #endregion regular

        #region create
        /// <summary>
        /// Create new DynamicClickBombObject for gridcell
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="oData"></param>
        /// <param name="addCollider"></param>
        /// <param name="radius"></param>
        /// <param name="isTrigger"></param>
        /// <returns></returns>
        public static DynamicClickBombObject Create(GridCell parent, DynamicClickBombObject prefab)
        {
            if (!parent || !prefab) return null;
            GridObject b = prefab.Create(parent);
            return (b) ? b.GetComponent<DynamicClickBombObject>() : null;
        }

        /// <summary>
        /// Create new DynamicClickBombObject over board, without parent
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="oData"></param>
        /// <param name="addCollider"></param>
        /// <param name="radius"></param>
        /// <param name="isTrigger"></param>
        /// <returns></returns>
        public static DynamicClickBombObject CreateOverBoard(DynamicClickBombObject prefab, Vector3 position, Vector3 localScale)
        {
            if (!prefab) return null;

            DynamicClickBombObject gridObject = Instantiate(prefab);
            if (!gridObject) return null;
            gridObject.transform.localScale = localScale;
            gridObject.transform.localPosition = position;
#if UNITY_EDITOR
            gridObject.name = "DynamicClickBomb: " + gridObject.ID;
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
                SRenderer.sortingOrder = SortingOrder.DynamicClickBombToFront;
            else
                SRenderer.sortingOrder = SortingOrder.DynamicClickBomb;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled || parent.Blocked) { return null; }
            if (parent.DynamicObject)
            {
                GameObject old = parent.DynamicObject;
                DestroyImmediate(old);
            }

            DynamicClickBombObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "DynamicClickBomb: " + ID;
#endif
            gridObject.SetToFront(false);
            gridObject.Enumerate(ID);
            return gridObject;
        }
        #endregion override
        }
}
