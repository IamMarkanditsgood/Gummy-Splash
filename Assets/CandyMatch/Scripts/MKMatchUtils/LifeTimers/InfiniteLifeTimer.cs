using System;
using UnityEngine;
using UnityEngine.Events;

/*
    01.07.2021
    25.01.2022 remove player reference
 */

namespace Mkey
{
    public class InfiniteLifeTimer : MonoBehaviour
    {
        #region temp vars
        private GlobalTimer gTimer;
        private string lifeInfTimerName = "lifeinfinite";
        private bool debug = false;
        private LifesHolder MLife => LifesHolder.Instance;
        public static InfiniteLifeTimer Instance;
        #endregion temp vars

        #region properties
        public float RestDays { get; private set; }
        public float RestHours { get; private set; }
        public float RestMinutes { get; private set; }
        public float RestSeconds { get; private set; }
        public bool IsWork { get; private set; }
        #endregion properties

        public UnityEvent InitStartEvent;

        public UnityEvent TimePassedEvent;


        #region regular
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            IsWork = false;
            LifesHolder.StartInfiniteLifeEvent += StartInfiniteLifeHandler;
            LifesHolder.EndInfiniteLifeEvent += EndInfiniteLifeHandler;
            if (MLife.HasInfiniteLife())
            {
                StartNewTimer();
            }
            InitStartEvent?.Invoke();
        }

        void Update()
        {
            if (IsWork)
                gTimer.Update();
        }

        private void OnDestroy()
        {
            LifesHolder.StartInfiniteLifeEvent -= StartInfiniteLifeHandler;
            LifesHolder.EndInfiniteLifeEvent -= EndInfiniteLifeHandler;
        }
        #endregion regular

        #region timerhandlers
        private void TickRestDaysHourMinSecHandler(int d, int h, int m, float s)
        {
            RestDays = d;
            RestHours = h;
            RestMinutes = m;
            RestSeconds = s;
        }

        private void TimePassedHandler(double initTime, double realTime)
        {
            IsWork = false;
            MLife.EndInfiniteLife();
            TimePassedEvent?.Invoke();
        }
        #endregion timerhandlers

        #region player life handlers
        private void StartInfiniteLifeHandler()
        {
            StartNewTimer();
        }

        private void EndInfiniteLifeHandler()
        {
            IsWork = false;
        }
        #endregion player life handlers

        private void StartNewTimer()
        {
            if (debug) Debug.Log("start new");
            TimeSpan ts = MLife.GetInfLifeTimeRest();
            gTimer = new GlobalTimer(lifeInfTimerName, ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

        private void StartExistingTimer()
        {
            gTimer = new GlobalTimer(lifeInfTimerName);
            gTimer.TickRestDaysHourMinSecEvent += TickRestDaysHourMinSecHandler;
            gTimer.TimePassedEvent += TimePassedHandler;
            IsWork = true;
        }

    }
}