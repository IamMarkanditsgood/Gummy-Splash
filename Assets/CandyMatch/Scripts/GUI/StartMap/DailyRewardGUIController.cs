using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class DailyRewardGUIController : MonoBehaviour
    {
        [SerializeField]
        private PopUpsController dailyRewardPUPrefab;

        #region temp vars
        private GuiController MGui => GuiController.Instance; 
        private DailyRewardController DRC => DailyRewardController.Instance; 
        private int rewDay = -1;
        #endregion temp vars

        #region regular
        private IEnumerator Start()
        {
            while (!DRC) yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            rewDay = DRC.RewardDay;
            if (rewDay >= 0)
            {
                StartCoroutine(ShowRewardPopup(1.5f));
            }
        }
        #endregion regular

        private IEnumerator ShowRewardPopup(float delay)
        {
            yield return new WaitForSeconds(delay);
            while (!MGui.HasNoPopUp) { Debug.Log("daily reward wait gui"); yield return null; }

            // wait closing all popups
            MkeyFW.FortuneWheelInstantiator fortuneWheelInstantiator = FindObjectOfType<MkeyFW.FortuneWheelInstantiator>();
            while(fortuneWheelInstantiator && fortuneWheelInstantiator.MiniGame) { Debug.Log("daily reward wait mini game"); yield return null; }

            MGui.ShowPopUp(dailyRewardPUPrefab);
        }
    }
}
