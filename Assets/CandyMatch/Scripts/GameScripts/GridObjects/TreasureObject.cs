using System;
using UnityEngine;

namespace Mkey
{
    public class TreasureObject : GridObject
    {
        [Header("Addit properties")]
        [Space(8)]
        public AudioClip privateClip;
        public GameObject collectAnimPrefab;

        #region properties
        #endregion properties

        /// <summary>
        /// Collect match object, hit overlays, hit underlays
        /// </summary>
        /// <param name="completeCallBack"></param>
        internal void Collect(float delay, bool showPrefab, bool fly, Action completeCallBack)
        {
            Debug.Log(ToString() + " collect");

            transform.parent = null;

            MBoard.BeforeFillBoardEvent -= BeforeFillBoardEventHandler;

            TweenSeq cSequence = new TweenSeq();
            if (delay > 0)
            {
                cSequence.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, 0, 1, delay).AddCompleteCallBack(callBack);
                });
            }

            cSequence.Add((callBack) =>
            {
                MSound.PlayClip(0, privateClip);
                callBack();
            });

            // sprite seq animation
            if (showPrefab)
            {
                cSequence.Add((callBack) =>
                {
                    Creator.InstantiateAnimPrefab(collectAnimPrefab, transform, transform.position, SortingOrder.MainExplode);
                    delayAction(gameObject, 0.01f, () =>
                            {
                                if (this) SetToFront(true);
                                callBack();
                            });
                });
            }
            //fly
            if (fly)
            {
                cSequence.Add((callBack) =>
                {
                    SimpleTween.Move(gameObject, transform.position, GameBoard.Instance.FlyTarget, 0.4f).AddCompleteCallBack(() =>
                    {
                        //  callBack();
                    });
                    callBack(); // not wait
                });
                cSequence.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, 0, 1, 0.15f).AddCompleteCallBack(callBack);
                });
            }
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
                SRenderer.sortingOrder = SortingOrder.TreasureToFront;
            else
                SRenderer.sortingOrder = SortingOrder.Treasure;
        }

        public override string ToString()
        {
            return "Treasure: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled) { return null; }

            //if (parent.DynamicObject)
            //{
            //    GameObject old = parent.DynamicObject;
            //    DestroyImmediate(old);
            //}
            DestroyHierCompetitor(parent);

            TreasureObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "treasure: " + parent.ToString();
#endif
          //  gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            if (GameBoard.GMode == GameMode.Play)
            {
                MBoard.BeforeFillBoardEvent += gridObject.BeforeFillBoardEventHandler;
            }
            gridObject.Enumerate(ID);
            return gridObject;
        }
        #endregion override

        private void BeforeFillBoardEventHandler(GameBoard mBoard)
        {
            GridCell p = GetComponentInParent<GridCell>();
            if (p && !p.Overlay) Collect(0, true, true, ()=> { mBoard.WinContr.CheckResult(); });
        }
    }
}