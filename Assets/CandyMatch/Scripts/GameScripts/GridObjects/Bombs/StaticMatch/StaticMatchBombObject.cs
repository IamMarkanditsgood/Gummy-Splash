using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class StaticMatchBombObject : BombObject
    {
        #region properties
        #endregion properties

        #region regular

        #endregion regular

        #region create
        /// <summary>
        /// Create new StaticMatchBombObject
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefab"></param>
        /// <param name="addCollider"></param>
        /// <param name="radius"></param>
        /// <param name="isTrigger"></param>
        /// <returns></returns>
        public static StaticMatchBombObject Create(GridCell parent, StaticMatchBombObject prefab)
        {
            if (!parent || !prefab) return null;
            GridObject b = prefab.Create(parent);
            return (b) ? b.GetComponent<StaticMatchBombObject>() : null;
        }

        /// <summary>
        /// Create new StaticMatchBombObject over board, without parent
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="oData"></param>
        /// <param name="addCollider"></param>
        /// <param name="radius"></param>
        /// <param name="isTrigger"></param>
        /// <returns></returns>
        public static StaticMatchBombObject CreateOverBoard(StaticMatchBombObject prefab, Vector3 position, Vector3 localScale,  Action<int> TargetCollectEvent)
        {
            if (!prefab) return null;

            StaticMatchBombObject gridObject = Instantiate(prefab);
            if (!gridObject) return null;
            gridObject.transform.localScale = localScale;
            gridObject.transform.localPosition = position;
#if UNITY_EDITOR
            gridObject.name = "StaticMatchBomb: " + gridObject.ID;
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
                SRenderer.sortingOrder = SortingOrder.StaticMatchBombToFront;
            else
                SRenderer.sortingOrder = SortingOrder.StaticMatchBomb;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled || parent.Blocked) return null;
            if (parent.StaticMatchBomb)
            {
                GameObject old = parent.StaticMatchBomb.gameObject;
                Destroy(old);
            }
            StaticMatchBombObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "StaticMatchBomb: " + ID;
#endif
           // gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            gridObject.Enumerate(ID);
            return gridObject;
        }
            #endregion override
        }
}
