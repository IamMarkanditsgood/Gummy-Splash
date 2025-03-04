using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class StarChestWindowController : PopUpsController
	{
        [SerializeField]
        private RectTransform chest;
        [SerializeField]
        private Sprite openedChest;
        [SerializeField]
        private Sprite closedChest;
        [SerializeField]
        private List<StarChestLine> chestLines;
        [SerializeField]
        private StarChestLine lifeChestLine;
        [SerializeField]
        private Image chestLight;
        [SerializeField]
        private GameObject buttonOpen;
        [SerializeField]
        private GameObject buttonClaim;
        [SerializeField]
        private bool autoOpen;

        #region temp vars
        private StarChestController SCC { get { return StarChestController.Instance; } }
      //  private int rIndex = 0;
        [SerializeField]
        private bool move = false;
        private int colorTweenID;
        private StarChestLine randomLine;
        #endregion temp vars

        public Action OpenChestEvent;

        #region regular
        void Start()
        {
            bool lifeFull = LifesHolder.Count >= LifesHolder.Instance.MaxCount;
            List<StarChestLine> _lines = new List<StarChestLine>(chestLines);
            if (lifeFull) _lines.Remove(lifeChestLine);
            randomLine = _lines.GetRandomPos();
            //if (lifeFull)  //try to avoid life gift
            //{
            //    for (int i = 0; i < 5; i++)
            //    {
            //        rIndex = UnityEngine.Random.Range(0, chestLines.Count);
            //        if (chestLines[rIndex] != lifeChestLine) break;
            //    }
                
            //}
            //else
            //{
            //    rIndex = UnityEngine.Random.Range(0, chestLines.Count);
            //}

            if(autoOpen) Open_Click();
        }

        private void OnDestroy()
        {
            SimpleTween.Cancel(colorTweenID, false);
        }
		#endregion regular

        public void Open_Click()
        {
            Open(()=> { if (buttonClaim) buttonClaim.gameObject.SetActive(true); SetControlActivity(true); });     // Open(CloseWindow);
            SCC.ResetData();
            SetControlActivity(false);
            if (buttonOpen) buttonOpen.SetActive(false);
            if (buttonClaim) buttonClaim.SetActive(false);
            OpenChestEvent?.Invoke();
        }

        public void Claim_Click()
        {
           // Open(() => { });     // Open(CloseWindow);
            foreach (var item in chestLines)
            {
                if (item && item.IsActive) item.ApplyReward();
            }

            SetControlActivity(false);
            if (buttonOpen) buttonOpen.SetActive(false);
            if (buttonClaim) buttonClaim.SetActive(false);
            TweenExt.DelayAction(gameObject, 0.5f, CloseWindow);
        }

        public void ScaleOut(Action completeCallBack, float delay)
        {
            if (chest) chest.localScale = Vector3.zero;
            SimpleTween.Value(gameObject, Vector3.zero, Vector3.one, 0.5f).SetOnUpdate((Vector3 val) =>
            {
                if (chest) chest.localScale = val;
            })
            .SetDelay(delay)
            .SetEase(EaseAnim.EaseOutBounce)
            .AddCompleteCallBack(completeCallBack);
        }

        public void Open(Action completeCallBack)
        {
            if(!chest)
            {
                completeCallBack?.Invoke();
                return;
            }

            TweenSeq ts = new TweenSeq();

            Image im = chest.GetComponent<Image>();

            chest.localScale = Vector3.zero;

            ts.Add((callBack) =>
            {
                SimpleTween.Value(gameObject, Vector3.one, new Vector3(1.5f, 0.5f, 1f), 0.1f).SetOnUpdate((Vector3 val) =>
                {
                    if (chest) chest.localScale = val;
                })
          .AddCompleteCallBack(callBack);
            });

            ts.Add((callBack) =>
            {
                if (move)
                    SimpleTween.Value(gameObject, 0, 5, 0.15f).SetOnUpdate((float val) =>
                    {
                        if (chest) { chest.anchoredPosition -= new Vector2(0, val); }
                    });
                SimpleTween.Value(gameObject, new Vector3(1.55f, 0.5f, 1f), new Vector3(1.00f, 1.00f, 1.00f), 0.25f).SetOnUpdate((Vector3 val) =>
                {
                    if (chest) chest.localScale = val;
                })
          .SetEase(EaseAnim.EaseOutBounce)
          .AddCompleteCallBack(callBack);
            });

            ts.Add((callBack) =>
            {
                if (openedChest && im)
                {
                    im.sprite = openedChest;
                }
                if (chestLight)
                {
                    chestLight.gameObject.SetActive(true);
                    colorTweenID = SimpleTween.Value(gameObject, -Mathf.PI / 4f, Mathf.PI / 4f, 1f).SetOnUpdate((float val) =>
                    {
                        if (chestLight) chestLight.color = new Color(1, 1, 1, Mathf.Cos(val));
                    }).SetCycled().ID;
                }
                //if (coinsFountain)
                //{
                //    if (coinsFountainPosition) coinsFountain.transform.position = coinsFountainPosition.position;
                //    coinsFountain.Jump();
                //}
                //if (coinsText)
                //{
                //    coinsText.gameObject.SetActive(true);
                //    coinsText.text = Coins.ToString("# ### ### ### ###");
                //}

                
                callBack?.Invoke();
            });

            ts.Add((callBack) =>
            {
                randomLine.Show(callBack);
            });

            ts.Add((callBack) =>
            {
                SimpleTween.Value(gameObject, 0, 1, 2).AddCompleteCallBack(callBack);
                if (buttonClaim) buttonClaim.SetActive(true);
            });

            ts.Add((callBack) =>
            {
                completeCallBack?.Invoke();
            });
            ts.Start();
        }
    }
}
