using UnityEngine;
using System;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
    public class Achievement : MonoBehaviour
    {
        [SerializeField]
        private int targetCount;
        [SerializeField]
        private int  achReward;
        [SerializeField]
        private AchievementsLine achievementsLinePrefab;
        [SerializeField]
        private readonly bool dLog;

        #region default
        private string prefix = "achievement_";
        private string SaveStageName { get { return prefix + "stage_" + GetUniqueName(); } }
        private string SaveCountName { get { return prefix + "count_" + GetUniqueName(); } }
        private string SaveRewardReceivedName { get { return prefix + "received_" + GetUniqueName(); } }
        #endregion default

        #region properties
        public bool TargetAchieved { get { return CurrentCount >= TargetCount; } }
        public int AchReward { get { return achReward; } }
        public int TargetCount { get { return targetCount; } }
        public int CurrentCount { get; private set; }
        public bool RewardReceived { get; private set; }
        #endregion properties

        #region events
        public Action <int, int> ChangeCurrentCountEvent;
        public Action<int> RewardReceivedEvent;
        public Action ResetReceivedEvent;
        #endregion events

        #region reward
        protected void LoadRewardReceived()
        {
            Debug.Log("load SaveRewardReceivedName: " + SaveRewardReceivedName);
            RewardReceived = (PlayerPrefs.GetInt(SaveRewardReceivedName, 0) == 1);
        }

        protected void SetRewardReceived()
        {
            RewardReceived = true;
            PlayerPrefs.SetInt(SaveRewardReceivedName, 1);
        }

        protected void ResetRewardReceived()
        {
            Debug.Log("Reset reward received");
            RewardReceived = false;
            PlayerPrefs.SetInt(SaveRewardReceivedName, 0);
            ResetReceivedEvent?.Invoke();
        }

        public void OnGetRewardEvent()
        {
            if (!TargetAchieved) return;
            SetRewardReceived();
            RewardReceivedEvent?.Invoke(achReward);
        }
        #endregion reward

        #region current achievement count
        protected void LoadCurrentCount()
        {
            Debug.Log("Load SaveCountName: " + SaveCountName);
            CurrentCount = PlayerPrefs.GetInt(SaveCountName, 0);
        }

        protected void ResetCurrentCount()
        {
            if (CurrentCount == 0) return;
            CurrentCount = 0;
            PlayerPrefs.SetInt(SaveCountName, CurrentCount);
            ChangeCurrentCountEvent?.Invoke(CurrentCount, targetCount);
            Debug.Log("Reset current count");
        }

        protected void IncCurrentCount()
        {
            CurrentCount++;
            CurrentCount = Mathf.Min(CurrentCount, TargetCount);
            ChangeCurrentCountEvent?.Invoke(CurrentCount, targetCount);
            if(dLog)  Debug.Log(GetUniqueName() + " target " + CurrentCount);
            PlayerPrefs.SetInt(SaveCountName, CurrentCount);
        }
        #endregion current achievement count

        public AchievementsLine CreateGuiLine(RectTransform parent)
        {
            return (achievementsLinePrefab) ? achievementsLinePrefab.CreateInstance(parent, this) : null;
        }

        public virtual string GetUniqueName() { return "achievement"; }

        public virtual void Load()
        {

        }

#if UNITY_EDITOR
        public void DrawInspector()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            #region test
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal("box");

                if (GUILayout.Button("Inc Current Count"))
                {
                    IncCurrentCount();
                }
                if (GUILayout.Button("Reset Current Count"))
                {
                    ResetCurrentCount();
                    ResetRewardReceived();
                }
                if (GUILayout.Button("Reset reward received"))
                {
                    ResetRewardReceived();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Goto play mode for test");
            }
            #endregion test
        }
#endif
    }
}