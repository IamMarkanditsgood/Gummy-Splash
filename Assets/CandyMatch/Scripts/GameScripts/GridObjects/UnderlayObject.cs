using System;
using UnityEngine;

namespace Mkey
{
    public class UnderlayObject : GridObject
    {
        public Sprite[] protectionStateImages;
        public GameObject hitAnimPrefab;
        public bool self_healing;

        #region properties
        public int Protection
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

            Hits++;
            SetProtectionImage();

            if (hitAnimPrefab)
            {
                Creator.InstantiateAnimPrefab(hitAnimPrefab, transform.parent, transform.position, SortingOrder.MainExplode);
            }

            if (Protection <= 0)
            {
                hitDestroySeq = new TweenSeq();

                SetToFront(true);

                hitDestroySeq.Add((callBack) => // play preexplode animation
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

        public override void CancellTweensAndSequences()
        {
            base.CancellTweensAndSequences();
        }

        public override void SetToFront(bool set)
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (set)
                SRenderer.sortingOrder = SortingOrder.UnderToFront;
            else
                SRenderer.sortingOrder = SortingOrder.Under;
        }

        public override string ToString()
        {
            return "Underlay: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled) return null;
            //if (parent.Underlay)
            //{
            //    GameObject old = parent.Underlay.gameObject;
            //    Destroy(old);
            //}
            DestroyHierCompetitor(parent);

            if (Hits > protectionStateImages.Length) return null;

            UnderlayObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "underlay " + parent.ToString();
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

        public override int GetHierarchy()
        {
            return -10;
        }
        #endregion override


        private void SetProtectionImage()
        {
            if (protectionStateImages.Length > 0)
            {
                int i = Mathf.Min(Hits - 1, protectionStateImages.Length - 1);
                GetComponent<SpriteRenderer>().sprite = protectionStateImages[i];
            }
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
    }
}
