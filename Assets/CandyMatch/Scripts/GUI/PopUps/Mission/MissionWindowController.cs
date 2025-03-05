using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Mkey
{
    public class MissionWindowController : PopUpsController
    {
        [SerializeField]
        private TMP_Text levelText;

        [Space(8)]       
        [Header ("Targets")]
        [SerializeField]
        private Text scoreText;
        [SerializeField]
        private Text getScoreText;
        [SerializeField]
        private RectTransform targetsContainer;
        [SerializeField]
        private MissionTarget targetPrefab;

        [Space(8)]
        [Header("FieldBoosters")]
        [SerializeField]
        private List<FieldBooster> fieldBosterPrefabs;
        [SerializeField]
        private RectTransform FieldBoostersParent;

        private RectTransform BoostersParent;
        private int boostersCount = 3;


        #region temp wars
        private GameBoard MBoard => GameBoard.Instance;
        private GuiController MGui => GuiController.Instance;
        private WinController WController => MBoard ? MBoard.WinContr : null;
        private GameConstructSet GCSet => GameConstructSet.Instance;
        private GameObjectsSet GOSet => (GCSet) ? GCSet.GOSet : null;
        private LevelConstructSet LCSet => (GCSet) ? GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel) : null;
        private List<GuiFieldBoosterHelper> guiFieldBoosterHelpers;
        private List<FieldBooster> fieldBoosters;
        #endregion temp wars

        public override void RefreshWindow()
        {
            levelText.text = " Level " + (GameLevelHolder.CurrentLevel + 1).ToString();
            getScoreText.gameObject.SetActive(MBoard.WinContr.HasScoreTarget);
            scoreText.text = MBoard.WinContr.ScoreTarget.ToString();
            CreateTargets();
            // CreateBoostersPanel();
            CreateFieldBoostersPanel();
            base.RefreshWindow();
        }

        public void CreateTargets()
        {
            if (!targetsContainer) return;
            if (!targetPrefab) return;

            MissionTarget[] ts = targetsContainer.GetComponentsInChildren<MissionTarget>();
            foreach (var item in ts)
            {
                DestroyImmediate(item.gameObject);
            }

            foreach (var item in MBoard.Targets)
            {
                targetPrefab.SetIcon(GOSet.GetTargetObject(item.Value.ID).GuiImage);    // unity 2019 fix
                
                RectTransform t = Instantiate(targetPrefab, targetsContainer).GetComponent<RectTransform>();
                MissionTarget th = t.GetComponent<MissionTarget>();
                th.SetData(item.Value, true);
                th.SetIcon(GOSet.GetTargetObject(item.Value.ID).GuiImage);
                th.gameObject.SetActive(item.Value.NeedCount > 0);
            }
        }

        public void Play_Click()
        {
            // set field boosters
            fieldBoosters = new List<FieldBooster>();
            if (guiFieldBoosterHelpers != null && guiFieldBoosterHelpers.Count > 0)
            {
                foreach (var item in guiFieldBoosterHelpers)
                {
                    if (item.Use)
                    {
                      // item.booster.AddCount(-1);
                        fieldBoosters.Add(item.booster);
                    }
                }
            }
            GameBoard.SetFieldBoosters(fieldBoosters);
            CloseWindow();
        }

        private void CreateBoostersPanel()
        {
            GuiBoosterHelper[] ms = BoostersParent.GetComponentsInChildren<GuiBoosterHelper>();
            foreach (var item in ms)
            {
                DestroyImmediate(item.gameObject);
            }
            List<Booster> bList = new List<Booster>();
            List<Booster> bListToShop = new List<Booster>();

            bool selectFromAll = true;

            if (!selectFromAll)
            {
                foreach (var b in GOSet.BoosterObjects)
                {
                    if (b.Count > 0) bList.Add(b);
                    else bListToShop.Add(b);
                }

                bList.Shuffle();
                int bCount = Mathf.Min(bList.Count, boostersCount);
                for (int i = 0; i < bCount; i++)
                {
                    Booster b = bList[i];
                    GuiBoosterHelper bM = b.CreateGuiHelper(BoostersParent, "mission");
                }

                int shopCount = boostersCount - bList.Count;
                if (shopCount > 0)
                {
                    shopCount = Mathf.Min(shopCount, bListToShop.Count);
                    bListToShop.Shuffle();

                    for (int i = 0; i < shopCount; i++)
                    {
                        Booster b = bListToShop[i];
                        GuiBoosterHelper bM = b.CreateGuiHelper(BoostersParent, "mission");
                    }
                }
            }
            else
            {
                foreach (var b in GOSet.BoosterObjects)
                {
                    bList.Add(b);
                }

                bList.Shuffle();
                int bCount = Mathf.Min(bList.Count, boostersCount);
                for (int i = 0; i < bCount; i++)
                {
                    Booster b = bList[i];
                    GuiBoosterHelper bM = b.CreateGuiHelper(BoostersParent, "mission");
                }
            }
        }

        private void CreateFieldBoostersPanel()
        {
            guiFieldBoosterHelpers = new List<GuiFieldBoosterHelper>();
            GuiFieldBoosterHelper[] ms = FieldBoostersParent.GetComponentsInChildren<GuiFieldBoosterHelper>();
            foreach (var item in ms)
            {
                DestroyImmediate(item.gameObject);
            }

            foreach (var b in fieldBosterPrefabs)
            {
                GuiFieldBoosterHelper bM = b.CreateGuiHelper(FieldBoostersParent);
                guiFieldBoosterHelpers.Add(bM);
            }
        }

        public void ToMap_Click()
        {
            CloseWindow();
            SceneLoader.Instance.LoadScene(1);
        }
    }
}