using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
	public class DaysRowAchievement : Achievement
	{
        #region events
        [HideInInspector]
        public UnityEvent TimePassEvent;
        public Action<int, int, int, float> TickRestDaysHourMinSecEvent;
        #endregion events

        #region temp vars
        private int hours = 0;
        private int minutes = 1; // for test
        private GlobalTimer gTimer;
        private string timerName = "daysrowachievement_timer";
        [SerializeField]
        private bool dLog = true;
        #endregion temp vars

        #region properties
        public float RestDays { get; private set; }
        public float RestHours { get; private set; }
        public float RestMinutes { get; private set; }
        public float RestSeconds { get; private set; }
        public bool IsWork { get; private set; }
        #endregion properties

        #region regular
        public override void Load()
		{
            LoadRewardReceived();
            LoadCurrentCount();

            IsWork = false;

            if (!TargetAchieved)
            {
                if (GlobalTimer.Exist(timerName))  // check existing timer and  last tick
                {
                    if (dLog) Debug.Log(GetUniqueName() + " timer exist: " + timerName);
                    DateTime lT = GlobalTimer.GetLastTick(timerName);
                    TimeSpan tS = DateTime.Now - lT;
                    TimeSpan dRTS = new TimeSpan(hours, minutes, 0);

                    if (tS > dRTS) // interrupted game
                    {
                        if (dLog) Debug.Log(GetUniqueName() + " interrupted, timespan: " + tS);
                        ResetCurrentCount();
                        StartNewTimer();
                    }
                    else
                    {
                        if (dLog) Debug.Log(GetUniqueName() + " not interrupted, timespan: " + tS);
                        StartExistingTimer();
                    }
                }
                else
                {
                    if (dLog) Debug.Log(GetUniqueName() + "timer not exist: " + timerName);
                    StartNewTimer();
                }
            }
            RewardReceivedEvent +=(r)=> 
            {
                CoinsHolder.Add(r);
            };
          
            ChangeCurrentCountEvent += (cc, tc)=>{};
        }
        #endregion regular

        public override string GetUniqueName()
        {
            return "daysrow";
        }

        #region timerhandlers
        private void TickRestDaysHourMinSecHandler(int d, int h, int m, float s)
        {
            RestDays = d;
            RestHours = h;
            RestMinutes = m;
            RestSeconds = s;
            TickRestDaysHourMinSecEvent?.Invoke(d, h, m, s);
        }

        private void TimePassedHandler(double initTime, double realyTime)
        {
            if (dLog) Debug.Log(GetUniqueName() + " time passed");
            IsWork = false;
            IncCurrentCount();
            TimePassEvent?.Invoke();
            if(!TargetAchieved) StartNewTimer();
        }
        #endregion timerhandlers

        #region timers
        private void StartNewTimer()
        {
            if (dLog) Debug.Log("start new");
            TimeSpan ts = new TimeSpan(hours, minutes, 0);
            gTimer = new GlobalTimer(timerName, ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

        private void StartExistingTimer()
        {
            gTimer = new GlobalTimer(timerName);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }
        #endregion timers
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DaysRowAchievement))]
    public class DaysRowAchievementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DaysRowAchievement t = (DaysRowAchievement)target;
            t.DrawInspector();
        }
    }
#endif
}
