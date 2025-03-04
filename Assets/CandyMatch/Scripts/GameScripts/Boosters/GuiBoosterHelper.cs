using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class GuiBoosterHelper : MonoBehaviour
    {
        public Text boosterCounter;
        public Image boosterImage;
        public string description;

        public Booster booster { get; private set; }

        #region regular
        private IEnumerator Start()
        {
            while (booster == null) yield return new WaitForEndOfFrame();
            if (booster != null)
            {
                booster.ChangeCountEvent += ChangeCountEventHandler;
                booster.ActivateEvent += ShowActive;
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (gameObject) SimpleTween.Cancel(gameObject, true);
            if (booster != null)
            {
                booster.ChangeCountEvent -= ChangeCountEventHandler;
                booster.ActivateEvent -= ShowActive;
            }
        }
        #endregion regular

        /// <summary>
        /// Refresh booster count and booster visibilty
        /// </summary>
        private void Refresh()
        {
            if (booster != null)
            {
                if (boosterCounter) boosterCounter.text = booster.Count.ToString();
            }
        }

        /// <summary>
        /// Show active footer booster with another color
        /// </summary>
        /// <param name="active"></param>
        private void ShowActive(Booster b)
        {
            if (gameObject) SimpleTween.Cancel(gameObject, true);
            if (boosterImage)
            {
                Color c = boosterImage.color;
                boosterImage.color = new Color(1, 1, 1, 1);
                if (booster.IsActive)
                {
                    SimpleTween.Value(gameObject, 1.0f, 0.5f, 0.5f).SetEase(EaseAnim.EaseLinear).
                                SetOnUpdate((float val) =>
                                {
                                    if (booster.IsActive) boosterImage.color = new Color(1, val, val, 1);
                                    else
                                    {
                                        boosterImage.color = new Color(1, 1, 1, 1);
                                        SimpleTween.Cancel(gameObject, true);
                                    }

                                }).SetCycled();
                }
            }
        }

        public GuiBoosterHelper Create(Booster booster, RectTransform parent)
        {
            if (parent == null) return null;
            GuiBoosterHelper guiBoosterHelper = Instantiate(this, parent);
            if (guiBoosterHelper)
            {
                guiBoosterHelper.booster = booster;
            }
            return guiBoosterHelper;
        }

        #region handlers
        private void ChangeCountEventHandler(int count)
        {
            Refresh();
        }
        #endregion handlers
    }
}