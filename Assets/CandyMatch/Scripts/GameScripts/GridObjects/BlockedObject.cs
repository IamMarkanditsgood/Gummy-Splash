#define NOBOOSTERLIST
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Mkey
{
    public class BlockedObject : GridObject
    {
        [Header("Add match objects that can destroy the blocker (side match hit). \nOtherwise, any match object will be able to destroy the blocker.")]
        public List<MatchObject> matchObjects;
#if !NOBOOSTERLIST
        [Header("Add boosters that can destroy the blocker (booster hit). \nOtherwise, any booster will be able to destroy the blocker.")]
        public List<Booster> boosters;     
#endif
        public bool sideMatchHit;
        public bool explodeHit;
        public bool boosterHit;
        public bool fieldBoosterReplaceable;
        public Sprite[] protectionStateImages;
        public Sprite targetImage;
        public GameObject hitAnimPrefab;
        public GUIFlyer targetAnimPrefab;
        [Header("After reaching the impact limit, the object self-repairs")]
        public bool self_healing;

        [SerializeField]
        private UnityEvent ObjectTargetAchieved;

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

        #region temp Vars
        private Sprite sourceSprite;
        private int sourceHits = -1;
        #endregion temp Vars

        #region override
        public override void Hit(GridCell gCell, Action completeCallBack)
        {
            CacheSourceData();

            if (Protection <= 0)
            {
                completeCallBack?.Invoke();
                return;
            }

            if (!explodeHit && !boosterHit)
            {
                completeCallBack?.Invoke();
                return;
            }

            ApplyHit(gCell, completeCallBack);
        }

        public override void SideMatchHit(GridCell gCell,int matchID, Action completeCallBack)
        {
            CacheSourceData();

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
                SRenderer.sortingOrder = SortingOrder.Blocked;
            else
                SRenderer.sortingOrder = SortingOrder.Blocked;
        }

        public override string ToString()
        {
            return "Blocked: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            //parent.DestroyGridObjects(); // new
            DestroyHierCompetitor(parent);

            if (Hits > protectionStateImages.Length) return null;

            BlockedObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = ToString() + parent.ToString();
#endif
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

        public override void TargetCollectEventHandler(TargetData targetData)
        {
        }

        public override void TargetReachedEventHandler(TargetData targetData)
        {
            ObjectTargetAchieved?.Invoke();
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
                if (item && item.ID == matchID) 
                { 
                    // Debug.Log("side found match for hit: " + item.name + "item.id:" + item.ID); 
                    return true; 
                }
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

        private void CacheSourceData()
        {
            if (sourceHits < 0)
            {
                sourceHits = Hits;
                sourceSprite = ObjectImage;
            }
        }

        private void RestoreState()
        {
            if (sourceHits >= 0)
            {
                Hits = sourceHits;
                SetSprite(sourceSprite);
            }
        }

        private void ApplyHit(GridCell gCell, Action completeCallBack)
        {
            Hits++;

            if (protectionStateImages.Length > 0)
            {
                int i = Mathf.Min(Hits - 1, protectionStateImages.Length - 1);
                SetSprite(protectionStateImages[i]);
            }

            if (hitAnimPrefab)
            {
                Creator.InstantiateAnimPrefab(hitAnimPrefab, transform.parent, transform.position, SortingOrder.MainExplode);
            }

            if (Protection <= 0)
            {
                if (!self_healing) transform.parent = null;
                //  Debug.Log("destroyed " + ToString());
                hitDestroySeq = new TweenSeq();

                SetToFront(true);

                hitDestroySeq.Add((callBack) =>
                {
                    delayAction(gameObject, 0.05f, callBack);
                });

                hitDestroySeq.Add((callBack) =>
                {
                    if (targetAnimPrefab)
                    {
                        InstantiateGuiTargetFlyer(targetAnimPrefab);
                    }
                    CollectEventRaise();
                    callBack();
                });

                hitDestroySeq.Add((callBack) =>
                {
                    completeCallBack?.Invoke();
                    if (self_healing)
                    {
                        RestoreState();
                        SetToFront(false);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }

                    callBack();
                });

                hitDestroySeq.Start();
            }
            else
            {
                completeCallBack?.Invoke();
            }
        }

        public void DisableHits()
        {
            sideMatchHit = false;
            explodeHit = false;
            boosterHit = false;
        }

        public void SetSourceSprite(Sprite sprite)
        {
            SetSprite(sprite);
            sourceSprite = sprite;
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
