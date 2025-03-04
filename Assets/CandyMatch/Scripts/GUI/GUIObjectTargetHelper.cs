using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Mkey {
    public class GUIObjectTargetHelper : MonoBehaviour {

        [SerializeField]
        private Image icon;
        [SerializeField]
        private Text countText;
        [SerializeField]
        private GameObject unComplete;

        #region events
        public UnityEvent CompleteEvent;
        public UnityEvent <string> ChangeCountStringLeftEvent;
        public UnityEvent<string> ChangeCountStringCollectedEvent;
        public UnityEvent<string> ChangeCountStringCollAndNeedEvent;
        #endregion events

        #region properties
        public int TargetID { get; private set; }
        public TargetData TData { get; private set; }
        #endregion properties

        #region temp vars
        private string collectedAndNeeded;
        private string lefToCollect;
        private string collected;
        #endregion temp vars

        public void SetData(TargetData tData, bool showCount)
        {
            collected = (tData.CurrCount >= tData.NeedCount) ? tData.NeedCount.ToString() : tData.CurrCount.ToString();
            collectedAndNeeded = collected + "/" + tData.NeedCount.ToString();
            lefToCollect = (tData.CurrCount >= tData.NeedCount) ? "0" : (tData.NeedCount - tData.CurrCount).ToString();

            TargetID = tData.ID;
            TData = tData;

            if (countText)
            {
                countText.enabled = showCount;
            }

            TData.ChangeCountEvent += (t) => 
            {
                collected = (t.CurrCount >= t.NeedCount) ? t.NeedCount.ToString() : t.CurrCount.ToString();
                collectedAndNeeded = collected + "/" + t.NeedCount.ToString();
                lefToCollect = (t.CurrCount >= t.NeedCount) ? "0" : (t.NeedCount - t.CurrCount).ToString();
                if(this && gameObject) gameObject.SetActive(t.NeedCount > 0);

                ChangeCountStringLeftEvent?.Invoke(lefToCollect);
                ChangeCountStringCollectedEvent?.Invoke(collected);
                ChangeCountStringCollAndNeedEvent?.Invoke(collectedAndNeeded);

                if (GameBoard.GMode == GameMode.Play && t.CurrCount >= t.NeedCount) CompleteEvent?.Invoke();
            };

            ChangeCountStringLeftEvent?.Invoke(lefToCollect);
            ChangeCountStringCollectedEvent?.Invoke(collected);
            ChangeCountStringCollAndNeedEvent?.Invoke(collectedAndNeeded);
            if (GameBoard.GMode == GameMode.Play && tData.CurrCount >= tData.NeedCount) CompleteEvent?.Invoke();
        }

        public void SetData(TargetData tData, bool showCount, bool showUnComplete)
        {
            SetData(tData, showCount);
            if (unComplete && showUnComplete) unComplete.SetActive(tData.CurrCount < tData.NeedCount);
        }

        public void SetIcon(Sprite sprite)
        {
            if (icon) icon.sprite = sprite;
        }
    }
}