using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// 20.09.2023 events
namespace Mkey
{
    public class LifeGUIController : MonoBehaviour
    {
        [SerializeField]
        private GameObject lifesGroup;
        [SerializeField]
        private Text lifesText;
        [SerializeField]
        private Image infiniteIcon;
        [SerializeField]
        private Text timerText;

        [SerializeField]
        private UnityEvent<int> ChangeLifesCountEvent;
        [SerializeField]
        private UnityEvent LifesFullEvent;
        [SerializeField]
        private UnityEvent LifesNotFullEvent;

        #region temp vars
        private float restHours = 0;
        private float restMinutes = 0;
        private float restSeconds = 0;
        private GameConstructSet GCSet => GameConstructSet.Instance;
        private LifesHolder MLife => LifesHolder.Instance;

        private bool unLimited = false;
        #endregion temp vars

        #region regular
        private void Start()
        {
            unLimited = GCSet.UnLimited;
            if (lifesGroup) lifesGroup.SetActive(!unLimited);
            if (!unLimited)
            {
                MLife.ChangeEvent.AddListener(RefreshLife);
                MLife.ChangeEvent.AddListener(ChangeLifesCountEventHandler);
                LifesHolder.StartInfiniteLifeEvent += RefreshInfiniteLife;
                LifesHolder.EndInfiniteLifeEvent += RefreshInfiniteLife;
                if (timerText) timerText.text = restMinutes.ToString("00") + ":" + restSeconds.ToString("00");
            }
            ChangeLifesCountEventHandler(LifesHolder.Count);
            Refresh();
        }

        void OnGUI()
        {
            if (!unLimited) RefreshTimerText();
        }

        private void OnDestroy()
        {
            if (!unLimited)
            {
                MLife.ChangeEvent.RemoveListener(RefreshLife);
                MLife.ChangeEvent.RemoveListener(ChangeLifesCountEventHandler);
                LifesHolder.StartInfiniteLifeEvent -= RefreshInfiniteLife;
                LifesHolder.EndInfiniteLifeEvent -= RefreshInfiniteLife;
            }
        }
        #endregion regular

        private void RefreshTimerText()
        {
            LifeIncTimer lifeIncTimer = LifeIncTimer.Instance;
            InfiniteLifeTimer infiniteLifeTimer = InfiniteLifeTimer.Instance;
            if (timerText)
            {
                if (infiniteLifeTimer && infiniteLifeTimer.IsWork)
                {
                    if (restHours != infiniteLifeTimer.RestHours || restMinutes != infiniteLifeTimer.RestMinutes || restSeconds != infiniteLifeTimer.RestSeconds)
                    {
                        restHours = infiniteLifeTimer.RestHours;
                        restMinutes = infiniteLifeTimer.RestMinutes;
                        restSeconds = infiniteLifeTimer.RestSeconds;
                        timerText.text = restHours.ToString("00") + ":" + restMinutes.ToString("00"); // + ":" + restSeconds.ToString("00");
                    }
                    if (lifesText && lifesText.gameObject.activeSelf) lifesText.gameObject.SetActive(false);
                    if (infiniteIcon && !infiniteIcon.gameObject.activeSelf) infiniteIcon.gameObject.SetActive(true);
                    return;
                }

                if (lifeIncTimer)
                {
                    if (restMinutes != lifeIncTimer.RestMinutes || restSeconds != lifeIncTimer.RestSeconds)
                    {
                        restMinutes = lifeIncTimer.RestMinutes;
                        restSeconds = lifeIncTimer.RestSeconds;
                        timerText.text = restMinutes.ToString("00") + ":" + restSeconds.ToString("00");
                    }
                    if (lifesText && !lifesText.gameObject.activeSelf) lifesText.gameObject.SetActive(true);
                    if (infiniteIcon && infiniteIcon.gameObject.activeSelf) infiniteIcon.gameObject.SetActive(false);
                }
            }
        }

        private void Refresh()
        {
            if (!unLimited)
            {
                RefreshLife(LifesHolder.Count);
                RefreshTimerText();
            }
        }

        private void RefreshLife(int life)
        {
            if (lifesText) lifesText.text = life.ToString();
        }

        private void RefreshInfiniteLife()
        {
            if (infiniteIcon) infiniteIcon.gameObject.SetActive(MLife.HasInfiniteLife());
            if (lifesText) lifesText.enabled = !MLife.HasInfiniteLife();
        }

        private void ChangeLifesCountEventHandler(int count)
        {
            ChangeLifesCountEvent?.Invoke(count);
            if(MLife.MaxCount <= LifesHolder.Count) LifesFullEvent?.Invoke();
            else LifesNotFullEvent?.Invoke();
        }
    }
}
