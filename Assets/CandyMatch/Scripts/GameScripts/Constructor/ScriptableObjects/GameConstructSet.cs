using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/*
    02.12.2019 - first
    7.02.2020 - changes
 */
namespace Mkey
{
    [CreateAssetMenu]
    public class GameConstructSet : BaseScriptable
    {
        [SerializeField]
        private GameObjectsSet gOSet;
        [Space(8, order = 0)]
        [Header("Constructed Levels", order = 1)]
        public List<LevelConstructSet> levelSets;
        public List<LevelConstructSet> levelSetsAdd;

        public bool testMode = false;
        public int testLevel = 0;

        [SerializeField]
        private bool unLimited = false;

        #region properties
        public GameObjectsSet GOSet { get { return gOSet; } }

        public int LevelCount
        {
            get { if (levelSets != null) return levelSets.Count; else return 0; }
        }

        public bool UnLimited { get { return unLimited; } }
        #endregion properties

        static GameConstructSet _instance = null;
        public static GameConstructSet Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = Resources.FindObjectsOfTypeAll<GameConstructSet>().FirstOrDefault();
                }

#if UNITY_EDITOR
                if (!_instance)
                {
                    string[] guids2 = UnityEditor.AssetDatabase.FindAssets("GameConstructSet", null);
                    foreach (var item in guids2)
                    {
                        Debug.Log(item);
                    }
                    if (guids2 != null && guids2.Length > 0)
                    {
                        _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConstructSet>(guids2[0]); // UnityEditor.AssetDatabase.LoadAssetAtPath<GameConstructSet>("Assets/Resources/GameConstaructSet/GameConstructSet_1.asset");
                    }
                }
#endif
                return _instance;
            }
        }


        /// <summary>
        /// Return LevelConstructSet for levelNumber. If levelNumber out of range - return LevelConstruct for 1 levelNumber.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public LevelConstructSet GetLevelConstructSet(int level)
        {
            if (InRange(level)) return levelSets[level];
            else if (levelSets != null) return levelSets[levelSets.Count - 1];
            return null;
        }

        #region regular
        private void OnEnable()
        {

        }

        #endregion regular

        public void Clean()
        {
            bool needClean = false;
            if (levelSets == null) { levelSets = new List<LevelConstructSet>(); needClean = true; };

            if (!needClean)
                foreach (var item in levelSets)
                {
                    if (item == null)
                    {
                        needClean = true;
                        break;
                    }
                }

            if (needClean)
            {
                levelSets = levelSets.Where(item => item != null).ToList();
                SetAsDirty();
            }
            Debug.Log("levels count " + levelSets.Count);
        }

        public void AddLevel(LevelConstructSet lc)
        {
            if (levelSets == null) { levelSets = new List<LevelConstructSet>(); }
            levelSets.Add(lc);
            SetAsDirty();
        }

        public void InsertBeforeLevel(int levelIndex, LevelConstructSet lcs)
        {
            if (!InRange(levelIndex)) return;
            levelSets.Insert(levelIndex, lcs);
            SetAsDirty();
        }

        public void InsertAfterLevel(int levelIndex, LevelConstructSet lcs)
        {
            Debug.Log("insert level after: " + levelIndex);
            if (!InRange(levelIndex)) return;
            if (levelIndex + 1 == levelSets.Count)
            {
                levelSets.Add(lcs);
                Debug.Log("add after : " + levelIndex);
            }
            else
            {
                levelSets.Insert(levelIndex + 1, lcs);
                Debug.Log("insert after : " + levelIndex);
            }
            SetAsDirty();
        }

        public void RemoveLevel(int levelIndex)
        {
            RemoveAllAddLevels(levelIndex);
            if (!InRange(levelIndex)) return;
#if UNITY_EDITOR
                ScriptableObjectUtility.DeleteResourceAsset(levelSets[levelIndex]);
#endif
            levelSets.RemoveAt(levelIndex);
            SetAsDirty();
        }

        private bool InRange(int level)
        {
            return (levelSets != null && levelSets.Count > level && level >= 0);
        }

        public void AddAdditionalLevel(LevelConstructSet lc)
        {
            if (levelSetsAdd == null) { levelSetsAdd = new List<LevelConstructSet>(); }
            Debug.Log("add additional level: " + lc);
            if (lc == null) return;
            levelSetsAdd.Add(lc);
            SetAsDirty();
        }

        public void RemoveAddLevel(LevelConstructSet lcSet)
        {
            if (lcSet == null) return;

            levelSetsAdd.Remove(lcSet);
            
#if UNITY_EDITOR
            ScriptableObjectUtility.DeleteResourceAsset(lcSet);
#endif
            SetAsDirty();
        }

        public void RemoveAllAddLevels(int levelIndex)
        {
            if (!InRange(levelIndex)) return;
            List<LevelConstructSet> toDel = new List<LevelConstructSet>();

            string name = levelSets[levelIndex].name + "_add";

            foreach (var item in levelSetsAdd)
            {
                if (item && item.name.Contains(name))
                {
                    toDel.Add(item);
                }
            }

            foreach (var item in toDel)
            {
                levelSetsAdd.Remove(item);
            }

#if UNITY_EDITOR
            foreach (var item in toDel)
            {
                ScriptableObjectUtility.DeleteResourceAsset(item);
            }
#endif
            SetAsDirty();
        }

        /// <summary>
        /// Return LevelConstructSet for levelNumber. If levelNumber out of range - return LevelConstruct for 1 levelNumber.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public List<LevelConstructSet> GetAddLevelConstructSets(int levelIndex)
        {
            if (!InRange(levelIndex)) return null;
            if (levelSets[levelIndex] == null) return null;

            List<LevelConstructSet> res = new List<LevelConstructSet>();

            string name = levelSets[levelIndex].name + "_add";
            foreach (var item in levelSetsAdd)
            {
                if (item && item.name.IndexOf(name)!=-1)
                {
                    res.Add(item);
                }
            }
            return res;
        }

    }
}

