using UnityEngine;
using UnityEngine.Events;
using System;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
  player game lifes holder
  07.06.2021
  17.08.2021
 */
namespace Mkey
{
    [CreateAssetMenu(menuName = "ScriptableObjects/LifesHolder")]
    public class LifesHolder : SingletonScriptableObject<LifesHolder>
    {
        #region default data
        [Space(10, order = 0)]
        [Header("Default data", order = 1)]
        [Tooltip("Default lifes at start")]
        [SerializeField]
        private int defCount = 5;

        [Tooltip("Max lifes count")]
        [SerializeField]
        private int maxCount = 5;
        #endregion default data

        #region keys
        [SerializeField]
        private string saveKey = "mk_match_lifes";
        private string saveInfLifeTimeStampEnd = "inflifesend"; // saved time stamp for infinite life end
        #endregion keys

        #region temp vars
        private static bool loaded = false;
        private static int _count;
        #endregion temp vars

        public static int Count
        {
            get { if (!loaded) Instance.Load(); return _count; }
            private set { _count = value; }
        }
        public int DefaultCount => defCount;
        public int MaxCount => maxCount;

        public UnityEvent<int> ChangeEvent;
        public UnityEvent<int> LoadEvent;
        public static Action StartInfiniteLifeEvent;
        public static Action EndInfiniteLifeEvent;

        private void Awake()
        {
            Load();
            Debug.Log("Awake: " + this + " ;count: " + Count);
        }

        public static void Add(int count)
        {
            if (Instance)
            {
                Instance.SetCount(Count + count);
            }
        }

        /// <summary>
        /// Set count and save result
        /// </summary>
        /// <param name="count"></param>
        public void SetCount(int count)
        {
            if (HasInfiniteLife())
            {
                Count = maxCount;
                return;
            }
            count = Mathf.Clamp(count, 0, maxCount);
            bool changed = (Count != count);
            Count = count;
            if (changed)
            {
                PlayerPrefs.SetInt(saveKey, Count);
                ChangeEvent?.Invoke(Count);
            }
        }

        /// <summary>
        /// Load serialized data or set defaults
        /// </summary>
        public void Load()
        {
            loaded = true;
            Count = PlayerPrefs.GetInt(saveKey, defCount);
            LoadEvent?.Invoke(Count);
        }

        public void StartInfiniteLife(float hours)
        {
            SetCount(maxCount);
            PlayerPrefs.SetString(saveInfLifeTimeStampEnd, DateTime.Now.AddHours(hours).ToString(CultureInfo.InvariantCulture));
            StartInfiniteLifeEvent?.Invoke();
        }

        public void CleanInfiniteLife()
        {
            PlayerPrefs.SetString(saveInfLifeTimeStampEnd, DateTime.Now.ToString(CultureInfo.InvariantCulture));
            Load();
            EndInfiniteLifeEvent?.Invoke();
        }

        public void EndInfiniteLife()
        {
            Load();
            EndInfiniteLifeEvent?.Invoke();
        }

        public bool HasInfiniteLife()
        {
            if (!PlayerPrefs.HasKey(saveInfLifeTimeStampEnd)) return false;
            DateTime end = GlobalTimer.DTFromSring(PlayerPrefs.GetString(saveInfLifeTimeStampEnd));
            return (DateTime.Now < end);
        }

        public TimeSpan GetInfLifeTimeRest()
        {
            if (!PlayerPrefs.HasKey(saveInfLifeTimeStampEnd)) return new TimeSpan(0, 0, 0);
            DateTime end = GlobalTimer.DTFromSring(PlayerPrefs.GetString(saveInfLifeTimeStampEnd));
            return end - DateTime.Now;
        }

        public void SetDefaultData()
        {
            EndInfiniteLife();
            SetCount(defCount);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LifesHolder))]
    public class LifesHolderEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            LifesHolder lH = (LifesHolder)target;
            EditorGUILayout.LabelField("Lifes: " + LifesHolder.Count);
            EditorGUILayout.LabelField("Infinite: " + lH.HasInfiniteLife());

            #region test
            if (test = EditorGUILayout.Foldout(test, "Test"))
            {
                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Add life"))
                {
                    LifesHolder.Add(1);
                }
                if (GUILayout.Button("Set 5 lifes"))
                {
                    lH.SetCount(5);
                }
                if (GUILayout.Button("Dec life"))
                {
                    LifesHolder.Add(-1);
                }

                if (GUILayout.Button("Inf life"))
                {
                    lH.StartInfiniteLife(12);
                }

                if (GUILayout.Button("End inf life"))
                {
                    lH.CleanInfiniteLife();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Reset to default"))
                {
                    lH.SetDefaultData();
                }

                if (GUILayout.Button("Log count"))
                {
                    Debug.Log("Lifes: " + LifesHolder.Count);

                }

                if (GUILayout.Button("Load data"))
                {
                    lH.Load();
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion test
        }
    }
#endif
}