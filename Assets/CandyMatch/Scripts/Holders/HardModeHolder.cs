using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

/*
    player hard mode holder
    22.06.2021
 */
namespace Mkey
{
    public enum HardMode { Easy, Hard }

    [CreateAssetMenu(menuName = "ScriptableObjects/HardModeHolder")]
    public class HardModeHolder : SingletonScriptableObject<HardModeHolder>
    {
        #region default data
        [Space(10, order = 0)]
        [Header("Default data", order = 1)]
        [Tooltip("Default hard mode")]
        [SerializeField]
        private HardMode defHardMode = HardMode.Easy;
        #endregion default data

        #region keys
        [SerializeField]
        private string saveKey = "mk_hardmode"; // current hard mode
        #endregion keys

        #region temp vars
        private static bool loaded = false;
        private static HardMode _hardMode;
        #endregion temp vars

        public static HardMode Mode
        {
            get { if (!loaded) Instance.Load(); return _hardMode; }
            private set { _hardMode = value; }
        }
        public HardMode DefaultCount => defHardMode;

        public UnityEvent<HardMode> ChangeEvent;
        public UnityEvent<HardMode> LoadEvent;

        private void Awake()
        {
            Load();
            Debug.Log("Awake: " + this + " ;hard mode: " + Mode);
        }


        /// <summary>
        /// Set hard mode
        /// </summary>
        /// <param name="count"></param>
        public void SetMode(HardMode hMode)
        {
            bool changed = (Mode != hMode);
            Mode = hMode;
            if (changed)
            {
                PlayerPrefs.SetInt(saveKey, (int)Mode);
                ChangeEvent?.Invoke(Mode);
            }
        }


        /// <summary>
        /// Load serialized hard mode
        /// </summary>
        public void Load()
        {
            loaded = true;
            _hardMode = (HardMode)PlayerPrefs.GetInt(saveKey, (int) defHardMode);
            LoadEvent?.Invoke(Mode);
        } 

        public void SetDefaultData()
        {
            SetMode(defHardMode);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HardModeHolder))]
    public class HardModeHolderEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            HardModeHolder tH = (HardModeHolder)target;
            EditorGUILayout.LabelField("Hard mode: " + HardModeHolder.Mode);

            if (test = EditorGUILayout.Foldout(test, "Test"))
            {
                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Set Hard"))
                {
                    tH.SetMode(HardMode.Hard);
                }
                if (GUILayout.Button("Set Easy"))
                {
                    tH.SetMode(HardMode.Easy);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Reset to default"))
                {
                    tH.SetDefaultData();
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Log mode"))
                {
                    Debug.Log("Hard mode: " + HardModeHolder.Mode);

                }
                if (GUILayout.Button("Load saved mode"))
                {
                    tH.Load();
                }

                if (GUILayout.Button("Log coins"))
                {
                    Debug.Log("Coins: " + CoinsHolder.Count);

                }
            }
        }
    }
#endif
}