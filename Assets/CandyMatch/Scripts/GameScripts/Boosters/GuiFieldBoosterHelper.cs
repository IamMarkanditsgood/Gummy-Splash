using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Mkey
{
    public class GuiFieldBoosterHelper : MonoBehaviour
    {
        public Text boosterCounter;
        public Image boosterImage;
        public GameObject zeroFlag;
        public GameObject useFlag;
        public GameObject counterObject;
        public FieldBooster booster { get; private set; }

        public bool Use { get; private set; }


        #region regular
        private IEnumerator Start()
        {
            while (booster == null) yield return new WaitForEndOfFrame();
            if (booster != null)
            {
                booster.ChangeCountEvent += ChangeCountEventHandler;
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (gameObject) SimpleTween.Cancel(gameObject, true);
            if (booster != null)
            {
                booster.ChangeCountEvent -= ChangeCountEventHandler;
            }
        }
        #endregion regular

        /// <summary>
        /// Refresh booster count and booster visibilty
        /// </summary>
        private void Refresh()
        {
            if (zeroFlag) zeroFlag.SetActive(false);
            if (useFlag) useFlag.SetActive(false);
            if (counterObject) counterObject.SetActive(false);

            if (booster != null)
            {
                if (boosterCounter) boosterCounter.text = booster.Count.ToString();
                if(booster.Count <= 0)
                {
                    if (zeroFlag) zeroFlag.SetActive(true);
                    if (Use) Use = false;
                }
                else
                {
                    if (useFlag) useFlag.SetActive(Use);
                    if (counterObject) counterObject.SetActive(!Use);
                }
            }
        }

        public GuiFieldBoosterHelper Create(FieldBooster booster, RectTransform parent)
        {
            if (parent == null) return null;
            GuiFieldBoosterHelper guiBoosterHelper = Instantiate(this, parent);
            if (guiBoosterHelper)
            {
                guiBoosterHelper.booster = booster;
            }
            return guiBoosterHelper;
        }

        public void ChangeUseOrShop(PopUpsController boosterShop)
        {
            if (booster.Count == 0 && boosterShop) boosterShop.CreateWindow();
            else
            {
                Use = !Use;
                Refresh();
            }
        }

        #region handlers
        private void ChangeCountEventHandler(int count)
        {
            Refresh();
        }
        #endregion handlers
    }
}