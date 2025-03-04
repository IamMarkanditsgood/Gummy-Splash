#define NOBOOSTERLIST
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Mkey
{
    public class DynamicBlockerObject : GridObject
    {
        public bool canSwap = false;
        [Header("Add match objects that can destroy the blocker (side match hit). \nOtherwise, any match object will be able to destroy the blocker.")]
        public List<MatchObject> matchObjects;
#if !NOBOOSTERLIST
        [Header("Add boosters that can destroy the blocker (booster hit). \nOtherwise, any booster will be able to destroy the blocker.")]
        public List<Booster> boosters;
#endif
        [SerializeField]
        private bool sideMatchHit;
        [SerializeField]
        private bool explodeHit;
        [SerializeField]
        private bool boosterHit;
        public bool fieldBoosterReplaceable;
        public Sprite[] protectionStateImages;
        public Sprite targetImage;
        public GameObject hitAnimPrefab;

        #region properties
        public bool Destroyable { get { return sideMatchHit || explodeHit || boosterHit; } }
        public bool SideHit => sideMatchHit;
        public bool ExplodeHit => explodeHit;
        public bool BoosterHit => boosterHit;
        public virtual int Protection
        {
            get { return protectionStateImages.Length + 1 - Hits; }
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

        public override void SideMatchHit(GridCell gCell, int matchID, Action completeCallBack)
        {
            if (Protection <= 0 || !CanSideHitWithMatch(matchID))
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

        public override void CancellTweensAndSequences()
        {
            base.CancellTweensAndSequences();
        }

        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (!SRenderer) return;
            if (set)
                SRenderer.sortingOrder = SortingOrder.Blocked;
            else
                SRenderer.sortingOrder = SortingOrder.Blocked;
        }

        public override string ToString()
        {
            return "DynamicBlocker: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
           //parent.DestroyGridObjects(); // new

            DestroyHierCompetitor(parent);

            if (Hits > protectionStateImages.Length) return null;

            DynamicBlockerObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = ToString() + parent.ToString();
#endif
           // gridObject.TargetCollectEvent = TargetCollectEvent;
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

        public override bool CanSelfMove()
        {
            return true;
        }

        public override Sprite GetTargetImage()
        {
            if (targetImage) return targetImage;
            return base.GetTargetImage();
        }
        #endregion override

        protected bool CanSideHitWithMatch(int matchID)
        {
            if (matchObjects == null || matchObjects.Count == 0) return true;

            foreach (var item in matchObjects)
            {
                if (item && item.ID == matchID) return true;
            }
            return false;
        }

        protected bool CanHitWithBooster(Booster booster)
        {
#if !NOBOOSTERLIST
            if (boosters == null || boosters.Count == 0 || booster == null) return true;

            foreach (var item in boosters)
            {
                if (item && item.ID == booster.ID) return true;
            }
            return false;
#else
            return true;
#endif
        }

        public void ForceCollect()
        {
            if (hitAnimPrefab)
            {
                Creator.InstantiateAnimPrefab(hitAnimPrefab, transform.parent, transform.position, SortingOrder.MainExplode);
            }
            transform.parent = null;
            CollectEventRaise();
            Destroy(gameObject);
        }
    }
}
