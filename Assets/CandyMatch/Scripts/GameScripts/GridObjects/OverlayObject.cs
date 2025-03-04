using System;
using UnityEngine;

namespace Mkey
{
    public enum BlockProps { OnlyTouch, BlockMatch, BlockMovement }
    public class OverlayObject : GridObject
    {
        public BlockProps blockProps;
        public Sprite[] protectionStateImages;
        public GameObject hitAnimPrefab;

        #region properties
        public int Protection
        {
            get { return  protectionStateImages.Length + 1 - Hits; }
        }

        public bool BlockMatch
        {
            get { return blockProps != BlockProps.OnlyTouch; }
        }

        public bool BlockMovement
        {
            get { return blockProps == BlockProps.BlockMovement; }
        }
        #endregion properties

        #region override
        public override void Hit(GridCell gCell, Action completeCallBack)
        {
            if (Protection <= 0)
            {
                completeCallBack?.Invoke();
                return;
            }

            Hits++;
            if (protectionStateImages.Length > 0)
            {
                int i = Mathf.Min(Hits - 1, protectionStateImages.Length - 1);
                SRenderer.sprite = protectionStateImages[i];
            }

            if (hitAnimPrefab)
            {
                Creator.InstantiateAnimPrefab(hitAnimPrefab, transform.parent, transform.position, SortingOrder.MainExplode);
            }

            if (Protection <= 0)
            {
              //  gCell.ReleaseOverlay();
                transform.parent = null;
                // Debug.Log("destroyed " + ToString());
                hitDestroySeq = new TweenSeq();

                SetToFront(true);

                hitDestroySeq.Add((callBack) => 
                {
                    delayAction(gameObject, 0.05f, callBack);
                });

                hitDestroySeq.Add((callBack) =>
                {
                    CollectEvent?.Invoke(TargetGroupID);
                    callBack();
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

        public override void SideMatchHit(GridCell gCell,int matchID, Action completeCallBack)
        {
            if (Protection <= 0)
            {
                completeCallBack?.Invoke();
                return;
            }

            Hits++;
            if (protectionStateImages.Length > 0)
            {
                int i = Mathf.Min(Hits - 1, protectionStateImages.Length - 1);
                SRenderer.sprite = protectionStateImages[i];
            }

            if (hitAnimPrefab)
            {
                Creator.InstantiateAnimPrefab(hitAnimPrefab, transform.parent, transform.position, SortingOrder.MainExplode);
            }

            if (Protection <= 0)
            {
                //  gCell.ReleaseOverlay();
                transform.parent = null;
                Debug.Log("destroyed " + ToString());
                hitDestroySeq = new TweenSeq();

                SetToFront(true);

                hitDestroySeq.Add((callBack) =>
                {
                    delayAction(gameObject, 0.05f, callBack);
                });

                hitDestroySeq.Add((callBack) =>
                {
                    CollectEvent?.Invoke(TargetGroupID);
                    callBack();
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

        public override void CancellTweensAndSequences()
        {
            base.CancellTweensAndSequences();
        }

        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (!SRenderer) return;
            if (set)
                SRenderer.sortingOrder = SortingOrder.OverToFront;
            else
                SRenderer.sortingOrder = SortingOrder.Over;
        }

        public override string ToString()
        {
            return "Overlay: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;

            if (parent.IsDisabled) return null;
            //if (parent.Overlay)
            //{
            //    GameObject old = parent.Overlay.gameObject;
            //    Destroy(old);
            //}
            DestroyHierCompetitor(parent);
            if (Hits > protectionStateImages.Length) return null;

            OverlayObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "overlay " + parent.ToString();
#endif
          //  gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);

            gridObject.Hits = Mathf.Clamp(Hits, 0, protectionStateImages.Length);
            if (protectionStateImages.Length > 0 && gridObject.Hits > 0)
            {
                int i = Mathf.Min(gridObject.Hits - 1, protectionStateImages.Length - 1);
                gridObject.SRenderer.sprite = protectionStateImages[i];
            }
            gridObject.Enumerate(ID);
            return gridObject;
        }

        public override Sprite[] GetProtectionStateImages()
        {
            return protectionStateImages;
        }

        public override int GetHierarchy()
        {
            return 10;
        }
        #endregion override
    }
}
