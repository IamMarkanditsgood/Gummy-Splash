using System;
using UnityEngine;

namespace Mkey
{
    public class DisabledObject : GridObject
    {
        #region create
        /// <summary>
        /// Create new DisabledObject for gridcell
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="prefab"></param>
        /// <param name="TargetCollectEvent"></param>
        /// <returns></returns>
        public static DisabledObject Create(GridCell parent, DisabledObject prefab, Action<int> TargetCollectEvent)
        {
            if (!parent || !prefab) return null;

            parent.DestroyGridObjects(); // new

            if (GameBoard.GMode == GameMode.Play)
            {
                parent.gameObject.SetActive(false);
            }
            else
            {
                parent.GetComponent<SpriteRenderer>().enabled = false;
            }

            DisabledObject gridObject = Instantiate(prefab, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "disabled " + parent.ToString();
#endif
           // gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            return gridObject;
        }
        #endregion create

        #region override
        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (set)
                SRenderer.sortingOrder = SortingOrder.Blocked;
            else
                SRenderer.sortingOrder = SortingOrder.Blocked;
        }

        public override string ToString()
        {
            return "Disabled: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;

            parent.DestroyGridObjects(); // new

            if (GameBoard.GMode == GameMode.Play)
            {
                parent.gameObject.SetActive(false);
            }
            else
            {
                parent.GetComponent<SpriteRenderer>().enabled = false;
            }

            DisabledObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            SpriteRenderer sR = gridObject.GetOrAddComponent<SpriteRenderer>();
            sR.sprite = ObjectImage;

#if UNITY_EDITOR
            gridObject.name = "disabled " + parent.ToString();
#endif
            gridObject.SRenderer = sR;
          //  gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            gridObject.Enumerate(ID);
            return gridObject;
        }
            #endregion override
        }
}
