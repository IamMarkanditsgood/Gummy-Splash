using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Mkey
{
	public class StarChestGUIController : MonoBehaviour
	{
        [SerializeField]
        private RectTransform chest;
        [SerializeField]
        private Image actionFlagImage;
        [SerializeField]
        private Text chestStarsText;
        [SerializeField]
        private PopUpsController starChestPUPrefab;

        public UnityEvent<float> RefresProgressEvent;

        #region temp vars
        private TweenSeq zoomSequence;
        private GuiController MGui => GuiController.Instance;
        private StarChestController ChestController => StarChestController.Instance;
        #endregion temp vars

        #region regular
        private IEnumerator Start()
		{
            while (!ChestController || !ChestController.Started) yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            Refresh(ChestController.StarsInChest, ChestController.StarsTarget);
            ChestController.LoadStarsEvent += Refresh;
            ChestController.ChangeStarsEvent += Refresh;
            StartCoroutine(AnimateChestC());
        }

        private void OnDestroy()
        {
            if (zoomSequence != null)
            {
                zoomSequence.Break();
            }

            SimpleTween.Cancel(gameObject, false);
            if (ChestController)
            {
                ChestController.LoadStarsEvent -= Refresh;
                ChestController.ChangeStarsEvent -= Refresh;
            }
        }
        #endregion regular

        private IEnumerator AnimateChestC()
        {
            while (true)
            {
                if (ChestController.TargetAchieved)
                {
                    Zoom(null);
                    yield return new WaitForSeconds(5); // yeach 5 sec
                }
                yield return new WaitForSeconds(1);
            }
        }

        private void Refresh(int stars, int target)
        {
            if (chestStarsText)
            {
                chestStarsText.text = Mathf.Min(stars, target) + "/" + target;
            }
            if (actionFlagImage) actionFlagImage.enabled = stars >= target;
            RefresProgressEvent?.Invoke((float) stars/(float) target);
        }

        /// <summary>
        /// Show simple zoom sequence on main object
        /// </summary>
        /// <param name="completeCallBack"></param>
        private void Zoom(Action completeCallBack)
        {
            if (zoomSequence != null)
            {
                zoomSequence.Break();
            }

            zoomSequence = new TweenSeq();
            for (int i = 0; i < 2; i++)
            {
                zoomSequence.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, 1.0f, 1.2f, 0.07f).SetOnUpdate((float val) =>
                    {
                        if (chest) chest.localScale = new Vector3(val, val, val);
                    }).AddCompleteCallBack(() =>
                    {
                        callBack();
                    });
                });
                zoomSequence.Add((callBack) =>
                {
                    SimpleTween.Value(gameObject, 1.2f, 1.0f, 0.09f).SetOnUpdate((float val) =>
                    {
                        if (chest) chest.localScale = new Vector3(val, val, val);

                    }).AddCompleteCallBack(() =>
                    {
                        callBack();
                    });
                });
            }

            zoomSequence.Add((callBack) => { completeCallBack?.Invoke(); callBack(); });
            zoomSequence.Start();
        }

        public void Chest_Click()
        {
            if (ChestController.TargetAchieved)
            {
                MGui.ShowPopUp(starChestPUPrefab);
            }
        }
    }
}
