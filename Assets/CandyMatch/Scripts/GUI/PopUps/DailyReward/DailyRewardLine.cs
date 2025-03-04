using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Mkey
{
    public class DailyRewardLine : MonoBehaviour
    {
        [SerializeField]
        private Image rewardReceivedImage;
        [SerializeField]
        private Image currentDayImage;
        [SerializeField]
        private UnityEvent ApplyRewardEvent;
        #region temp vars

        #endregion temp vars

        public void SetData(int day, int rewardDay)
        {
            if (rewardReceivedImage)
            {
                rewardReceivedImage.gameObject.SetActive(day < rewardDay);
            }
            if (currentDayImage)
            {
                currentDayImage.gameObject.SetActive(day == rewardDay);
            }
        }

        public void Apply()
        {
            ApplyRewardEvent?.Invoke();
        }
    }
}
