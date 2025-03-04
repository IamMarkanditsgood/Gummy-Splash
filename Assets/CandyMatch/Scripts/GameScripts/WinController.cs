using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mkey
{
    public enum GameResult {None, WinAuto, Win, PreLoose, Loose}
    public class WinController : MonoBehaviour
    {
        #region events
        public Action <int> ChangeMovesEvent;
        public Action  MovesLeft5Event;
        public Action <int> ChangeTimeEvent;
        public Action <float> TimerTickEvent;
        public Action  TimerLeft30Event;
        public Action  TimerPassEvent;
        public Action LevelWinEvent;
        public Action AutoWinEvent;
        public Action LevelLooseEvent;
        public Action LevelPreLooseEvent;
        private Action CheckTargetResultEvent;          //for current target
        #endregion events

        #region properties
        public GameResult Result { get; private set; }
        public int TimeLimit { get; private set; } // initial
        public int MovesRest { get; private set; }
        public SessionTimer Timer { get; private set; }
        public bool IsTimeLevel { get; private set; }
        public int ScoreTarget { get; private set; }
        public bool HasScoreTarget { get { return ScoreTarget > 0; } }
        public int TimeRest { get; private set; }  //in seconds
        public bool Loaded { get; private set; }
        #endregion properties

        #region private temp
        private int timeCostrain;
        private int movesCostrain;
        private List<GridCell> botDynCells; // for faling object control
        private MatchGrid mGrid;
        private GameBoard mBoard;
        private bool skipWinShow = false;
        #endregion private temp

        #region regular
        public void InitStart()
        {
            Result = GameResult.None;

            mGrid = GameBoard.Instance.MainGrid;
            mBoard = GameBoard.Instance;

            movesCostrain = mBoard.FullLevelMission.MovesConstrain;
            MovesRest = movesCostrain;
            ScoreTarget = mBoard.FullLevelMission.ScoreTarget;
            botDynCells = mBoard.MainGrid.GetBottomDynCells();

            #region time 
            timeCostrain = mBoard.FullLevelMission.TimeConstrain;
            IsTimeLevel = mBoard.FullLevelMission.IsTimeLevel;
            if (IsTimeLevel)
            {
                MovesRest = 0;
                Timer = new SessionTimer(timeCostrain);
                Timer.TickRestFullSecondsEvent += (s) => { TimerTickEvent?.Invoke(s); };
                Timer.TickRestFullSecondsEvent += (sec) => { TimeRest = (int)sec; if (TimeRest == 30) { TimerLeft30Event?.Invoke(); } };
                Timer.TimePassedEvent += () => { if (Result == GameResult.None) { CheckResult(); } };
            }
            #endregion time 

            mBoard.SkipWinShowEvent += (mb) => { 
                float additScore = (float)ScoreHolder.AverageScore * (float)MovesRest / (float)movesCostrain;
              //  Debug.Log("additional score after skip: " + additScore + ";aver: " + ScoreHolder.AverageScore + "; mrest: " + MovesRest + "; constr: " + movesCostrain);
                MovesRest = 0;
                skipWinShow = true;
                ScoreHolder.Add((int)additScore);
                CheckResult();
            };
            Loaded = true;
        }
        #endregion regular

        /// <summary>
        /// Check swap result
        /// </summary>
        /// <param name="completeCallBack"></param>
        public void CheckResult()
        {
            // Debug.Log("ObjectTargetsIsCollected() " + ObjectTargetsIsCollected());
            bool targetAchieved = ObjectTargetsIsCollected() && ScoreTarget <= ScoreHolder.Count; // current grid target achieved
                                                                                                  // Debug.Log("targetAchieved : " + targetAchieved);
            bool changed = false;
            if (!targetAchieved)
            {
                if (IsTimeLevel && Timer.IsTimePassed)
                {
                    changed = Result != GameResult.Loose;
                    if (changed)
                    {
                        Result = GameResult.Loose;
                        LevelLooseEvent?.Invoke();
                    }
                }
                else if ((!IsTimeLevel && MovesRest <= 0) && mBoard.NeedAlmostMessage() && (Result != GameResult.PreLoose))
                {
                    changed = Result != GameResult.PreLoose;
                    if (changed)
                    {
                        Result = GameResult.PreLoose;
                        LevelPreLooseEvent?.Invoke();
                    }
                }
                else if ((!IsTimeLevel && MovesRest <= 0) && !mBoard.NeedAlmostMessage())
                {
                    changed = Result != GameResult.Loose;
                    if (changed)
                    {
                        Result = GameResult.Loose;
                        LevelLooseEvent?.Invoke();
                    }
                }
                else if ((!IsTimeLevel && MovesRest > 0) && (Result == GameResult.PreLoose))
                {
                    Result = GameResult.None;
                }
            }

            if (targetAchieved && Result != GameResult.Win)
            {
                if (MovesRest == 0)
                {
                    Result = GameResult.Win;
                    LevelWinEvent?.Invoke();
                }
                else
                {
                    if (Result != GameResult.WinAuto)
                    {
                        AutoWinEvent?.Invoke();
                    }
                    Result = GameResult.WinAuto;
                }
                return;
            }
        }

        private bool ObjectTargetsIsCollected()
        {
            //foreach (var item in mBoard.CurTargets)
            //{
            //    Debug.Log(item.Key + "->" + item.Value.CurrCount + " : " + item.Value.NeedCount);
            //}
            foreach (var item in mBoard.Targets)
            {
                if (!item.Value.Achieved)
                    return false;
            }
            return true;
        }

        public void UpdateTimer(float time)
        {
            if (IsTimeLevel)
            {
                Timer.Update(time);
            }
        }

        public bool HasTimeToMove()
        {
            return(IsTimeLevel)? TimeRest > 0 : true;
        }

        public bool HasMoves()
        {
            return (!IsTimeLevel) ? MovesRest > 0 : true;
        }

        public void MakeMove()
        {
            AddMoves(-1);
        }

        public void MakeMove(int count)
        {
            AddMoves(-Mathf.Abs(count));
        }

        public void AddMoves(int count)
        {
            SetMoves(MovesRest + count);
        }

        public void AddSeconds(float seconds)
        {
            if (Timer != null) Timer.AddTimeSpan(seconds);
        }

        public void SetMoves(int count)
        {
            count = Mathf.Max(0, count);
            bool changed = (MovesRest != count);
            MovesRest = count;
            if (changed) ChangeMovesEvent?.Invoke(MovesRest);
            if (MovesRest == 5) MovesLeft5Event?.Invoke();
        }
    }
}
