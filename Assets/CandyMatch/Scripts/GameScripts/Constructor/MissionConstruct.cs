using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    [Serializable]
    public class MissionConstruct
    {
        #region serialized fields
        [SerializeField]
        private string description = "Mission";

        [SerializeField]
        private int timeConstrain = 0;

        [SerializeField]
        private int movesConstrain = 10;

        [SerializeField]
        private int scoreTarget = 0;

        [SerializeField]
        private ObjectSetCollection targets;
        #endregion serialized fields

        #region properties
        public string Description { get { return description; } }
        public int TimeConstrain { get { return timeConstrain; } }  // priority 0 - remove all bubbles from board
        public int MovesConstrain { get { return movesConstrain; } } // priority 1 - used if time constrain >0
        public ObjectSetCollection Targets { get { return targets; } }
        public bool IsTimeLevel { get { return timeConstrain > 0; } }
        public int ScoreTarget { get { return scoreTarget; } }
        #endregion properties

        public Action ChangeMovesCountEvent;
        public Action ChangeTimeEvent;
        public Action ChangeScoreTargetEvent;
        public Action ChangeDescriptionEvent;
        public Action ChangeMatchObectsListEvent;
        public Action SaveEvent; // need to save object data, used for constructor

        public MissionConstruct()
        {
            targets = new ObjectSetCollection();
        }

        #region movesConstrain
        /// <summary>
        /// Add moves
        /// </summary>
        /// <param name="count"></param>
        public void AddMoves(int count)
        {
            SetMovesCount(movesConstrain + count);
        }

        /// <summary>
        /// Set lifes gift count
        /// </summary>
        /// <param name="count"></param>
        public void SetMovesCount(int count)
        {
            count = Mathf.Max(0, count);
            bool changed = (movesConstrain != count);
            movesConstrain = count;
            if (changed)
            {
                ChangeMovesCountEvent?.Invoke();
                SaveEvent?.Invoke();
            }
        }
        #endregion movesConstrain

        #region score target
        /// <summary>
        /// Add score target 
        /// </summary>
        /// <param name="count"></param>
        public void AddScoreTarget(int count)
        {
            SetScoreTargetCount(scoreTarget + count);
        }

        /// <summary>
        /// Set score target count count
        /// </summary>
        /// <param name="count"></param>
        public void SetScoreTargetCount(int count)
        {
            count = Mathf.Max(0, count);
            bool changed = (scoreTarget != count);
            scoreTarget = count;
            if (changed)
            {
                ChangeScoreTargetEvent?.Invoke();
                SaveEvent?.Invoke();
            }
        }
        #endregion movesConstrain

        #region timeConstrain
        /// <summary>
        /// Add time to timeConstrain
        /// </summary>
        /// <param name="seconds"></param>
        public void AddTime(int seconds)
        {
            SetTime(timeConstrain + seconds);
        }

        /// <summary>
        /// Set time constrain
        /// </summary>
        /// <param name="seconds"></param>
        public void SetTime(int seconds)
        {
            seconds = Mathf.Max(0, seconds);
            bool changed = (timeConstrain != seconds);
            timeConstrain = seconds;
            if (changed)
            {
                ChangeTimeEvent?.Invoke();
                SaveEvent?.Invoke();
            }
        }
        #endregion timeConstrain

        #region description
        /// <summary>
        /// Set description
        /// </summary>
        /// <param name="seconds"></param>
        public void SetDescription(string description)
        {
            bool changed = this.description != description;
            this.description = (string.IsNullOrEmpty(description)) ? string.Empty : description;
            if (changed)
            {
                ChangeDescriptionEvent?.Invoke();
                SaveEvent?.Invoke();
            }
        }
        #endregion lifes

        #region object targets
        /// <summary>
        /// Add target by id
        /// </summary>
        /// <param name="count"></param>
        private void AddTarget(int ID)
        {
                targets.AddById(ID,1);
               // ChangeObjectTargetsEvent?.Invoke();
                SaveEvent?.Invoke();
        }

        /// <summary>
        /// Add target by id
        /// </summary>
        /// <param name="count"></param>
        public void AddTarget(int ID, int count)
        {
            bool changed = (count != 0);
            if (changed)
            {
                for (int i = 0; i < count; i++)
                {
                    targets.AddById(ID, count);
                }
                //ChangeObjectTargetsEvent?.Invoke();
                SaveEvent?.Invoke();
            }
        }

        /// <summary>
        /// Remove booster gift, count
        /// </summary>
        /// <param name="count"></param>
        public void RemoveTarget(int ID, int count)
        {
            bool changed = (count != 0);
            if (changed)
            {
                targets.RemoveById(ID, count);
                //ChangeObjectTargetsEvent?.Invoke();
                SaveEvent?.Invoke();
            }
        }

        /// <summary>
        /// Retun count of gift boosters by id
        /// </summary>
        /// <param name="boosterID"></param>
        /// <returns></returns>
        public int GetTargetCount(int ID)
        {
            return targets.CountByID(ID);
        }

        /// <summary>
        /// Set target need count and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetTargetCount( int ID, int count)
        {
            bool changed = (targets.CountByID(ID) != count);

            if (changed)
            {
                targets.SetCountById(ID, count);
                //ChangeObjectTargetsEvent?.Invoke();
                SaveEvent?.Invoke();
            }
        }

        public void CleanObjectTargets(GameObjectsSet gameObjectsSet)
        {
            if (targets == null || targets.ObjectsList == null || gameObjectsSet == null ) return;
            targets.RemoveAll((item) => { return (item.Count == 0 || !gameObjectsSet.ContainTargetGroupID(item.ID)); });
            SaveEvent?.Invoke();
        }
        #endregion boosters

        #region matchObjects
        ///// <summary>
        ///// Add match object id
        ///// </summary>
        ///// <param name="count"></param>
        //public void AddMatchObject(int id)
        //{
        //    matchObjects.RemoveAll((c)=> { return c == id; });
        //    matchObjects.Add(id);
        //    ChangeMatchObectsListEvent?.Invoke();
        //    SaveEvent?.Invoke();
        //}

        ///// <summary>
        ///// Add match object id
        ///// </summary>
        ///// <param name="count"></param>
        //public void RemoveMatchObject(int id)
        //{
        //    matchObjects.RemoveAll((c) => { return c == id; });
        //    ChangeMatchObectsListEvent?.Invoke();
        //    SaveEvent?.Invoke();
        //}
        #endregion matchObjects

        public static MissionConstruct operator + (MissionConstruct m1, MissionConstruct m2)
        {
            MissionConstruct mC = new MissionConstruct();
            mC.description = m1.description;
            mC.timeConstrain = m1.timeConstrain;
            mC.movesConstrain = m1.movesConstrain;
            mC.scoreTarget = m1.scoreTarget + m2.scoreTarget;
            mC.targets = new ObjectSetCollection(m1.targets);
            mC.targets.Add(m2.targets);
            return mC;
        }
    }
}

