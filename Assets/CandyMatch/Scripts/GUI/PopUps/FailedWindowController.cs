using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class FailedWindowController : PopUpsController
    {
        [SerializeField]
        private Text levelText;
        [SerializeField]
        private GameObject scoreComplete;
        [SerializeField]
        private GameObject scoreUnComplete;

        [Space(8)]
        [Header("Targets")]
        [SerializeField]
        private Text scoreText;
        [SerializeField]
        private GUIObjectTargetHelper targetPrefab;
        [SerializeField]
        private RectTransform targetsContainer;

        private RectTransform BoostersParent;
        [Space(8)]
        [Header("FieldBoosters")]
        [SerializeField]
        private List<FieldBooster> fieldBosterPrefabs;
        [SerializeField]
        private RectTransform FieldBoostersParent;
        private int boostersCount = 3;
        [SerializeField]
        private bool useAds = true;

        #region temp vars
        private static int failsCounter = 0;
        private GameBoard MBoard => GameBoard.Instance;
        private GuiController MGui => GuiController.Instance;
        private SoundMaster MSound => SoundMaster.Instance;
        private GameConstructSet GCSet => GameConstructSet.Instance;
        private GameObjectsSet GOSet => (GCSet) ? GCSet.GOSet : null;
        private LevelConstructSet LCSet => (GCSet) ? GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel) : null;
        private List<GuiFieldBoosterHelper> guiFieldBoosterHelpers;
        private List<FieldBooster> fieldBoosters;
        #endregion temp vars

        #region regular
        private void Start()
        {
            failsCounter++;
            if (useAds && failsCounter % 2 == 0)
            {
               if(AdsControl.Instance) AdsControl.Instance.ShowInterstitial(()=> { MSound.ForceStopMusic(); }, ()=> { MSound.PlayCurrentMusic(); });
            }
        }
        #endregion regular

        public override void RefreshWindow()
        {
            // CreateTargets();
            if (levelText) levelText.text = " Level #" + (GameLevelHolder.CurrentLevel + 1).ToString();
            RefreshScore();
            // CreateBoostersPanel();
            CreateFieldBoostersPanel();
            base.RefreshWindow();
        }

        private void RefreshScore()
        {
            int score = ScoreHolder.Count;
            Debug.Log("score: " + score + " : " + MBoard.WinContr.ScoreTarget.ToString());
            if (scoreText) scoreText.text = score.ToString();

            if (scoreComplete)
            {
                scoreComplete.SetActive(MBoard.WinContr.HasScoreTarget && (score >= MBoard.WinContr.ScoreTarget));
            }

            if (scoreUnComplete)
            {
                scoreUnComplete.SetActive(MBoard.WinContr.HasScoreTarget && (score < MBoard.WinContr.ScoreTarget));
            }
            
        }

        public void Map_Click()
        {
            CloseWindow();
            SceneLoader.Instance.LoadScene(1);
        }

        public void Replay_Click()
        {
            if (LifesHolder.Count > 0 || GameConstructSet.Instance.UnLimited)
            {
                GameBoard.showMission = false;
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
                SceneLoader.Instance.LoadScene(2);

            }
            else
            {
               // CloseWindow();
                MGui.ShowMessage("Sorry!", "You have no lifes.", 1.5f, () => { }); // SceneLoader.Instance.LoadScene(0); 
            }
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

        private void CreateTargets()
        {
            if (!targetsContainer) return;
            if (!targetPrefab) return;

            GUIObjectTargetHelper[] ts = targetsContainer.GetComponentsInChildren<GUIObjectTargetHelper>();
            foreach (var item in ts)
            {
                Destroy(item.gameObject);
            }

            foreach (var item in MBoard.Targets)
            {
                GUIObjectTargetHelper th = Instantiate(targetPrefab, targetsContainer);
                th.SetData(item.Value, false, true);
                th.SetIcon(GOSet.GetTargetObject(item.Value.ID).GuiImage);
                th.gameObject.SetActive(item.Value.NeedCount > 0);
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
    }
}
