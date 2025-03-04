using UnityEngine.UI;
using UnityEngine;

namespace Mkey
{
    public class MissionTarget : MonoBehaviour {

        [SerializeField]
        private Image icon;
        [SerializeField]
        private Text countText;
        [SerializeField]

        public void SetData(TargetData tData, bool showCount)
        {
            string collectedString = (tData.CurrCount > tData.NeedCount) ? tData.NeedCount.ToString() : tData.CurrCount.ToString();
            if (countText)
            {
                countText.text =  tData.NeedCount.ToString();
                countText.enabled = showCount;
            }
        }

        public void SetIcon(Sprite sprite)
        {
            if (icon) icon.sprite = sprite;
        }
    }
}