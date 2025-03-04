using System;
using UnityEngine;

namespace Mkey
{
    public class HiddenObject : GridObject
    {
        #region properties
        [SerializeField]
        public int Protection
        {
            get { return 1 - Hits; }
        }
        #endregion properties

        public void SetProtection(int protection)
        {
            //    Protection = protection;
            // ChangProtectionEvent?.Invoke();
        }

        #region override
        public override void Hit(GridCell gCell, Action completeCallBack)
        {
            if (Protection <= 0)
            {
                completeCallBack?.Invoke();
                return;
            }

            Hits++;
            SetProtectionImage();

            if (Protection <= 0)
            {
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
                SRenderer.sortingOrder = SortingOrder.HiddenToFront;
            else
                SRenderer.sortingOrder = SortingOrder.Hidden;
        }

        public override string ToString()
        {
            return "Hidden: " + ID;
        }

        public override GridObject Create(GridCell parent)
        {
            if (!parent) return null;
            if (parent.IsDisabled ) return null; // || parent.Blocked
            //if (parent.Hidden)
            //{
            //    GameObject old = parent.Hidden.gameObject;
            //    Destroy(old);
            //}
            DestroyHierCompetitor(parent);
            HiddenObject gridObject = Instantiate(this, parent.transform);
            if (!gridObject) return null;
            gridObject.transform.localScale = Vector3.one;
            gridObject.transform.localPosition = Vector3.zero;
            gridObject.SRenderer = gridObject.GetComponent<SpriteRenderer>();
#if UNITY_EDITOR
            gridObject.name = "hidden " + parent.ToString();
#endif
           // gridObject.TargetCollectEvent = TargetCollectEvent;
            gridObject.SetToFront(false);
            gridObject.Enumerate(ID);
            return gridObject;
        }
        #endregion override

        private void SetProtectionImage()
        {
            if (!SRenderer) SRenderer = GetComponent<SpriteRenderer>();
            if (!SRenderer) return;
            SRenderer.sprite = ObjectImage;
            // SRenderer.enabled = ((MatchBoard.GMode == GameMode.Play) && (Protection == 0)) || (MatchBoard.GMode == GameMode.Edit);
        }
    }
}
