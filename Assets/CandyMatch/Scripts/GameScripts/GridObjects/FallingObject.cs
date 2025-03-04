using System;
using UnityEngine;

namespace Mkey
{
    public class FallingObject : GridObject
    {
        [Header("Addit properties")]
        [Space(8)]
        public GameObject collectAnimPrefab;
        public GUIFlyer targetAnimPrefab;
        [SerializeField]
        private bool canSwap = false;

        #region properties
        #endregion properties

        /// <summary>
        /// Collect falling object
        /// </summary>
        /// <param name="completeCallBack"></param>
        internal void Collect(float delay, bool showPrefab, bool fly, Action completeCallBack)
        {
            transform.parent = null;
            TweenSeq cSequence = new TweenSeq();
            cSequence.Add((callBack) =>
            {
                TweenExt.DelayAction(gameObject, delay, callBack);
            });

            // sprite seq animation
            if (showPrefab)
            {
                cSequence.Add((callBack) =>
                {
                    Creator.InstantiateAnimPrefab(collectAnimPrefab, transform, transform.position, SortingOrder.MainExplode);
                    TweenExt.DelayAction(gameObject, 1.0f, () =>
                            {
                                if (this) SetToFront(true);
                                callBack();
                            });
                });
            }

            cSequence.Add((callBack) =>
            {
                if (targetAnimPrefab)
                {
                    SRenderer.enabled = false;
                    InstantiateGuiTargetFlyer(targetAnimPrefab);
                }
                callBack();
            });

            //finish
            cSequence.Add((callBack) =>
            {
                CollectEvent?.Invoke(TargetGroupID);
                completeCallBack?.Invoke();
                Destroy(gameObject, (fly) ? 0.6f : 0);
                callBack();
            });

            cSequence.Start();
        }

        #region override
        public override void CancellTweensAndSequences()
        {
            base.CancellTweensAndSequences();
        }

        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (!SRenderer) return;
            if (set)
                SRenderer.sortingOrder = SortingOrder.MainToFront;
            else
                SRenderer.sortingOrder = SortingOrder.Main;
        }

        public override string ToString()
        {
            return "FallingObject: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled) { return null; } // || parent.Blocked
            //if (parent.DynamicObject)
            //{
            //    GameObject old = parent.DynamicObject;
            //    DestroyImmediate(old);
            //}
            DestroyHierCompetitor(parent);
            FallingObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "Falling: " + ID;
#endif
            // gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            gridObject.Enumerate(ID);
            return gridObject;
        }

        public override bool CanSelfMove()
        {
            return true;
        }

        public override bool CanSwap()
        {
            return canSwap;
        }
        #endregion override
    }
}