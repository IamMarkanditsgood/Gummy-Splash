using UnityEngine;
using UnityEngine.Events;
using System;
using System.Globalization;
using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif

/*
  player game stars holder
  15.06.2021
  8.10.2021
 */
namespace Mkey
{
    [CreateAssetMenu(menuName = "ScriptableObjects/StarsHolder")]
    public class StarsHolder : SingletonScriptableObject<StarsHolder>
    {
        #region default data
        [Space(10, order = 0)]
        [Header("Default data", order = 1)]

        [Tooltip("Max stars count")]
        [SerializeField]
        private int maxCount = 3;

        [SerializeField]
        private bool saveBestResult = true;
        #endregion default data

        #region keys
        [SerializeField]
        private string saveKey = "mk_stars";
        #endregion keys

        #region temp vars
        private static bool loaded = false;
        private static int _count;
        private static List<int> levelsStars;  // temporary array for store levels stars
        #endregion temp vars

        public static int Count
        {
            get { if (!loaded) Instance.Load(); return _count; }
            private set { _count = value; }
        }
        public static IList<int> LevelsStarsStore =>levelsStars.AsReadOnly(); 

        public UnityEvent<int> ChangeEvent;
        public UnityEvent<List<int>> LoadEvent;

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
            count = Mathf.Clamp(count, 0, maxCount);
            bool changed = (Count != count);
            Count = count;
            if (changed)
            {
                ChangeEvent?.Invoke(Count);
            }
        }

        /// <summary>
        /// Get stars for level. If level not passed return 0;
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int GetLevelStars(int level)
        {
            if (!loaded) Instance.Load();
          //  Debug.Log("level: " + level + "; levelsStars.Count: " + levelsStars.Count);
            if (levelsStars == null || levelsStars.Count == 0 || levelsStars.Count <= level) return 0;
            return levelsStars[level];
        }

        public int GetAllStars()
        {
            if (!loaded) Instance.Load();
            int res = 0;
            int length = levelsStars.Count;
            for (int i = 0; i < length; i++)
            {
                res += levelsStars[i];
            }
            return res;
        }


        /// <summary>
        /// Return list of scores for all passed levels
        /// </summary>
        /// <returns></returns>
        private List<int> GetPassedLevelsStars(int topPassedLevel)
        {
            if (!loaded) Instance.Load();
            List<int> result = new List<int>(topPassedLevel + 1);

            for (int i = 0; i <= topPassedLevel; i++)
            {
                result.Add(GetLevelStars(i));
            }
            return result;
        }

        public void SetStarsByScore(int levelScore)
        {
            int stars = 0;
            float starPos_1 = 0.4f;
            float starPos_2 = 0.63f;
            float starPos_3 = 1;
            int maxScore = ScoreHolder.AverageScore;

            if (levelScore > (maxScore * 2f * starPos_3)) stars = 3;
            else if (levelScore > (maxScore * 2f * starPos_2)) stars = 2;
            else if (levelScore > (maxScore * 2f * starPos_1)) stars = 1;
            SetCount(stars);
        }

        /// <summary>
        /// Load serialized data or set defaults
        /// </summary>
        public void Load()
        {
            levelsStars = new List<int>();
            loaded = true;
            ListWrapperStruct<int> lW = PlayerPrefsExtension.GetObject<ListWrapperStruct<int>>(saveKey, new ListWrapperStruct<int>(levelsStars));
            levelsStars = new List<int>(lW.list);
            LoadEvent?.Invoke(lW.list);
        }

        /// <summary>
        /// save stars after passing the game level
        /// </summary>
        /// <param name="passedLevel"></param>
        public void Save(int passedLevel)
        {
            if (levelsStars == null) levelsStars = new List<int>();
            int count = levelsStars.Count;
            if (count <= passedLevel) // increase stars list
            {
                levelsStars.AddRange(new int[passedLevel - count + 10]); // add 10 for next levels stars
            }

            int stars = (saveBestResult) ? Mathf.Max(Count, levelsStars[passedLevel]) : Count;
            levelsStars[passedLevel] = stars;
           
            ListWrapperStruct <int> lW = new ListWrapperStruct <int> (levelsStars);
            PlayerPrefsExtension.SetObject<ListWrapperStruct<int>>(saveKey, lW);
        }

        public void SetDefaultData()
        {
            PlayerPrefs.DeleteKey(saveKey);
            SetCount(0);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StarsHolder))]
    public class StarsHolderEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            StarsHolder tH = (StarsHolder)target;
            EditorGUILayout.LabelField("Stars: " + StarsHolder.Count);

            #region test
            if (test = EditorGUILayout.Foldout(test, "Test"))
            {
                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Add star"))
                {
                    StarsHolder.Add(1);
                }
                if (GUILayout.Button("Set 3 stars"))
                {
                    tH.SetCount(3);
                }
                if (GUILayout.Button("Dec stars"))
                {
                    StarsHolder.Add(-1);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Reset to default"))
                {
                    tH.SetDefaultData();
                }

                if (GUILayout.Button("Log count"))
                {
                    Debug.Log("Stars: " + StarsHolder.Count);

                }

                if (GUILayout.Button("Load data"))
                {
                    tH.Load();
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion test
        }
    }
#endif
}