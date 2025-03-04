using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
	public class StarChestController : MonoBehaviour
	{
        [SerializeField]
        private int targetStarsCount = 20;

        #region properties
        public int StarsInChest { get; private set; }
        public int StarsTarget => targetStarsCount;
        public bool TargetAchieved => (targetStarsCount <= StarsInChest);
        public bool Started { get; private set; }
        #endregion properties

        #region temp vars
        private string starsCommitSaveKey = "cheststarsforest";
        private GuiController MGui => GuiController.Instance;
        private GameLevelHolder MGLevel => GameLevelHolder.Instance;
        #endregion temp vars

        #region events
        public Action<int, int> LoadStarsEvent;
        public Action<int, int> ChangeStarsEvent;
        #endregion events

        public static StarChestController Instance;

        #region regular
        void Awake()
        {
            if (Instance) Destroy(gameObject);
            else
            {
                Instance = this;
            }
        }

        private void Start()
		{
            Validate();
            LoadStarsInChest();
            MGLevel.PassLevelEvent.AddListener(PassLevelEventHandler);
            Started = true;
        }

        private void OnDestroy()
        {
            MGLevel.PassLevelEvent.RemoveListener(PassLevelEventHandler);
        }

        private void LoadStarsInChest()
        {
            StarsInChest = PlayerPrefs.GetInt(starsCommitSaveKey, 0);
            LoadStarsEvent?.Invoke(StarsInChest, StarsTarget);
        }

        private void OnValidate()
        {
            Validate();
        }
        #endregion regular

        public void AddLevelStarsInChest(int stars)
        {
            StarsInChest += stars;
            StarsInChest = Mathf.Clamp(StarsInChest, 0, targetStarsCount);
            PlayerPrefs.SetInt(starsCommitSaveKey, StarsInChest);
            ChangeStarsEvent?.Invoke(StarsInChest, StarsTarget);
        }

        private void PassLevelEventHandler(int level)
        {
            AddLevelStarsInChest(StarsHolder.Count);
        }

        private void Validate()
        {
            targetStarsCount = Mathf.Max(targetStarsCount, 3);
        }

        public void ResetData()
        {
            StarsInChest = 0;
            PlayerPrefs.SetInt(starsCommitSaveKey, StarsInChest);
            ChangeStarsEvent?.Invoke(StarsInChest, StarsTarget);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StarChestController))]
    public class StarChestControllerEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            StarChestController t = (StarChestController)target;

            if (!EditorApplication.isPlaying)
            {
                if (test = EditorGUILayout.Foldout(test, "Test"))
                {
                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Reset chest"))
                    {
                        t.ResetData();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                if (test = EditorGUILayout.Foldout(test, "Test"))
                {
                    EditorGUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("Add star in chest"))
                    {
                        t.AddLevelStarsInChest(1);
                    }
                    if (GUILayout.Button("Remove star from chest"))
                    {
                        t.AddLevelStarsInChest(-1);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
#endif
}
