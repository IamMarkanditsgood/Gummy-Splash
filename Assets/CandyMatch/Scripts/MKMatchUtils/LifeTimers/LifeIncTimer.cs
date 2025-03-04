using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif
/*
    05.08.2020 - improve global calc 
                if (r > 1) StartNewTimer((int)(sourceInitTime-r));
                    else StartNewTimer();
    25.07.2021 - use holder

 */
namespace Mkey
{
    public class LifeIncTimer : MonoBehaviour
    {
        private string lifeIncTimerName = "lifeinc";
        [Tooltip("Time span to life increase, minutes")]
        [SerializeField]
        private int lifeIncTime = 20; 
        [Tooltip("Increase lives if count less than value")]
        [SerializeField]
        private uint incIfLessThan = 3;
        [Tooltip("If check, count the time between games")]
        [SerializeField]
        private bool countGlobalTime = false;

        #region temp vars
        private GlobalTimer gTimer;
        private bool debug = true;
        private LifesHolder MLife => LifesHolder.Instance;
        public static LifeIncTimer Instance;
        #endregion temp vars

        #region properties
        public bool IsWork { get; private set; }
        public float RestMinutes { get; private set; }
        public float RestSeconds { get; private set; }
        public float RestDays { get; private set; }
        public float RestHours { get; private set; }
        #endregion properties

        #region events
        public Action<int, int, int, float> TickRestDaysHourMinSecEvent;
        public UnityEvent TimePassedEvent;
        #endregion events

        #region regular
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            // set life handlers
            LifesHolder.StartInfiniteLifeEvent += StartInfiniteLifeHandler;
            LifesHolder.EndInfiniteLifeEvent += EndInfiniteLifeHandler;
            MLife.ChangeEvent.AddListener(ChangeLifeHandler);

            if (!countGlobalTime && !MLife.HasInfiniteLife() && (LifesHolder.Count < incIfLessThan) && !IsWork)
            {
                StartNewTimer();
            }
            else if (countGlobalTime && !MLife.HasInfiniteLife() && (LifesHolder.Count < incIfLessThan) && !IsWork)
            {
                if (GlobalTimer.Exist(lifeIncTimerName)) StartExistingTimer();
                else StartNewTimer();
            }
        }

        void OnDestroy()
        {
                LifesHolder.StartInfiniteLifeEvent -= StartInfiniteLifeHandler;
                LifesHolder.EndInfiniteLifeEvent -= EndInfiniteLifeHandler;
                MLife.ChangeEvent.RemoveListener(ChangeLifeHandler);
        }

        void Update()
        {
            if (IsWork)
                gTimer.Update();
        }
        #endregion regular

        #region timerhandlers
        private void TickRestDaysHourMinSecHandler(int d, int h, int m, float s)
        {
            RestDays = d;
            RestHours = h;
            RestMinutes = m;
            RestSeconds = s;
            TickRestDaysHourMinSecEvent?.Invoke(d, h, m, s);
        }

        private void TimePassedHandler(double initTime, double realTime)
        {
            Debug.Log("Time Passed event : " + initTime + " : " + realTime);
            double r=0;
            double sourceInitTime = lifeIncTime * 60.0;

            if (LifesHolder.Count < incIfLessThan)
            {
                int lifes = 1;
                double i = 1;

                if (realTime > sourceInitTime)
                {
                    i = realTime / sourceInitTime;
                   
                    lifes = (int)(i);
                    r = realTime - lifes * sourceInitTime;
                }
                LifesHolder.Add(lifes);
            }
            TimePassedEvent?.Invoke();
            if (LifesHolder.Count < incIfLessThan)
            {
                if (r > 1) StartNewTimer((int)(sourceInitTime-r));
                else StartNewTimer();
            }
        }
        #endregion timerhandlers

        #region player life handlers
        private void ChangeLifeHandler(int count)
        {
          if(debug) Debug.Log("change life by timer");
            if (count >= incIfLessThan && IsWork)
            {
                RestDays = 0;
                RestHours = 0;
                RestMinutes = 0;
                RestSeconds = 0;
                IsWork = false;
                if (debug) Debug.Log("timer stop");
            }
            else if (count < incIfLessThan && !IsWork)
            {
                StartNewTimer();
            }
        }

        private void StartInfiniteLifeHandler()
        {
            RestDays = 0;
            RestHours = 0;
            RestMinutes = 0;
            RestSeconds = 0;
            IsWork = false;
        }

        private void EndInfiniteLifeHandler()
        {
            if (!MLife.HasInfiniteLife() && (LifesHolder.Count < incIfLessThan) && !IsWork)
            {
                StartNewTimer();
            }
        }
        #endregion player life handlers

        private void StartNewTimer()
        {
            if (debug) Debug.Log("start new");
            TimeSpan ts = new TimeSpan(0, lifeIncTime, 0);
            gTimer = new GlobalTimer(lifeIncTimerName, ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

        private void StartNewTimer(int seconds)
        {
            if (debug) Debug.Log("start new not full");
            TimeSpan ts = new TimeSpan(0, 0, seconds);
            gTimer = new GlobalTimer(lifeIncTimerName, ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

        private void StartExistingTimer()
        {
            gTimer = new GlobalTimer(lifeIncTimerName);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LifeIncTimer))]
    public class LifeIncTimerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (EditorApplication.isPlaying)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Set 0 live"))
                {
                     LifesHolder. Instance.SetCount(0);
                }
                if (GUILayout.Button("Inc life"))
                {
                    LifesHolder.Add(1);
                }
                if (GUILayout.Button("Dec life"))
                {
                    LifesHolder.Add(-1);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("Goto play mode for test");
            }
        }
    }
#endif
}