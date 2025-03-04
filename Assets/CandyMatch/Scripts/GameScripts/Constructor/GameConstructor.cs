using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
    02.12.2019 - first
    13.12.2019 - add spawner brush, path brush
    18.12.2019 - additional level 
    21.12.2019 - fix asset creating methods (utils)
    24.12.2019 - fix continuos dirty scene (construct panel set active false)
    01.02.2020 - improved buttons creating
    03.02.2021 - update 
    23.11.2023 - update 
*/

namespace Mkey
{
    public class GameConstructor : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField]
        private bool showFullProtectors;
        [SerializeField]
        private bool showTargetsFromBoard;
        [SerializeField]
        private bool showMatchTargets;
        [SerializeField]
        private bool showTimeConstrain;
        [SerializeField]
        private bool showMissionDescription;
        [SerializeField]
        private bool showDistAdjust;
        [SerializeField]
        private bool showScoreTarget;

        private List<RectTransform> openedPanels;

        [SerializeField]
        private Text editModeText;

        #region selected brush
        [Space(8)]
        [SerializeField]
        private Image selectedDisabledBrushImage;

        [Space(8)]
        [SerializeField]
        private Image selectedSpawnBrushImage;
        #endregion selected brush

        [SerializeField]
        private GridObject currentBrush;

        [SerializeField]
        private IncDecInputPanel IncDecObjectPanelPrefab;
        [SerializeField]
        private IncDecInputPanel IncDecTextPanelPrefab;


        [SerializeField]
        private PanelContainerController brushPanelContainerPrerfab;
        [SerializeField]
        private Transform brushContainersParent;

        [SerializeField]
        private RectTransform panelsParent;

        #region mission
        [Space(8, order = 0)]
        [Header("Mission", order = 1)]
        [SerializeField]
        private PanelContainerController MissionPanelContainer;
        [SerializeField]
        private IncDecInputPanel InputTextPanelMissionPrefab;
        [SerializeField]
        private IncDecInputPanel IncDecTogglePanelMissionPrefab;
        [SerializeField]
        private IncDecInputPanel TogglePanelMissionPrefab;
        #endregion mission

        #region grid construct
        [Space(8, order = 0)]
        [Header("Grid", order = 1)]
        [SerializeField]
        private PanelContainerController GridPanelContainer;
        [SerializeField]
        private IncDecInputPanel IncDecGridPrefab;
        #endregion grid construct

        #region game construct
        [Space(8, order = 0)]
        [Header("Game construct", order = 0)]
        [SerializeField]
        private Button levelButtonPrefab;
        [SerializeField]
        private Button smallButtonPrefab;
        [SerializeField]
        private GameObject constructPanel;
        [SerializeField]
        private Button openConstructButton;
        [SerializeField]
        private RectTransform LevelButtonsParent;
        [SerializeField]
        private InputField LevelInputField;
        [SerializeField]
        private ScrollRect AddLevelButtonsContainer;
        [SerializeField]
        private Button newAddButton;
        [SerializeField]
        private Button removeAddButton;
        #endregion game construct

        #region temp vars
        private MissionConstruct levelMission;
        private Dictionary<int, TargetData> targets;
        private GameBoard MBoard { get { return GameBoard.Instance; } }
        private MatchGrid MGrid { get { return MBoard.MainGrid; } }
        private GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        private GameObjectsSet GOSet { get { return GCSet.GOSet; } }
        private LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } } 
        private int currentBrushHits = 0;
        #endregion temp vars

        #region properties
        public int ScoreTarget { get { return (LCSet) ? LCSet.levelMission.ScoreTarget : 0; } }
        #endregion properties

        #region default data
        private string levelConstructSetSubFolder = "LevelConstructSets";  //resource folders
        private string pathToSets = "Assets/CandyMatch/Resources/";
        private int minVertSize = 5;
        private int maxVertSize = 15;
        private int minHorSize = 5;
        private int maxHorSize = 15;
        #endregion default data

        #region events
        public Action<int> ChangeTargetScoreEvent;
        #endregion events

        public void InitStart()
        {
            if (GameBoard.GMode == GameMode.Edit)
            {
                if (!MBoard) return;

                Debug.Log("gc init");
                if (!GCSet)
                {
                    Debug.Log("Game construct set not found!!!");
                    return;
                }
                if (!GOSet)
                {
                    Debug.Log("GameObjectSet not found!!! - ");
                    return;
                }

                currentBrush = null;
                List<GridObject> blockers = new List<GridObject>(GOSet.BlockedObjects);
                blockers.AddRange(new List<GridObject>(GOSet.DynamicBlockerObjects));

                // create brush panels
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Blocked brush panel", blockers);
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "BlockedBox brush panel", new List<GridObject>(GOSet.BlockedBoxObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Falling brush panel", new List<GridObject>(GOSet.FallingObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Bombs brush panel", new List<GridObject>(GOSet.DynamicClickBombObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Main brush panel", new List<GridObject>(GOSet.MatchObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Overlay brush panel", new List<GridObject>(GOSet.OverlayObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Underlay brush panel", new List<GridObject>(GOSet.UnderlayObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Hidden brush panel", new List<GridObject>(GOSet.HiddenObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "Treasure brush panel", new List<GridObject>(GOSet.TreasureObjects));
                CreateBrushContainer(brushContainersParent, brushPanelContainerPrerfab, "SubUnderlay brush panel", new List<GridObject>(GOSet.SubUnderlayMCObjects));
                if (GameLevelHolder.CurrentLevel > GCSet.levelSets.Count - 1) GameLevelHolder.CurrentLevel = GCSet.levelSets.Count - 1;

                if (editModeText) editModeText.text = "EDIT MODE" + '\n' + "Level " + (GameLevelHolder.CurrentLevel + 1) + '\n' + LCSet.name;
                ShowLevelData(false);

                DeselectAllBrushes();
                CreateLevelButtons();
                ShowConstructMenu(true);
            }
        }

        #region show board
        private void ShowLevelData()
        {
            ShowLevelData(true);
        }

        private void ShowLevelData(bool rebuild)
        {
            GCSet.Clean();
            LCSet.Clean(GOSet);
            MissionConstruct currMiss = LCSet.levelMission;
            currMiss.CleanObjectTargets(GOSet);

           // Debug.Log("Show level data: " + (GameLevelHolder.CurrentLevel));
            if (rebuild) MBoard.CreateGameBoard();

            levelMission = MBoard.FullLevelMission;
            targets = MBoard.Targets;
            foreach (var item in targets)
            {
                item.Value.SetCurrCount(0);
                int iCount = levelMission.Targets.CountByID(item.Key);
                if (iCount > 0)
                    item.Value.SetNeedCount(levelMission.Targets.CountByID(item.Key));
                else
                    item.Value.SetNeedCount(0);
            }

            LevelButtonsRefresh();
            if (editModeText) editModeText.text = "EDIT MODE" + '\n' + "Level " + (GameLevelHolder.CurrentLevel + 1) + '\n' + LCSet.name;

            ChangeTargetScoreEvent?.Invoke(levelMission.ScoreTarget);

            if (HeaderGUIController.Instance)
            {
                HeaderGUIController.Instance.RefreshTimeMoves();
                HeaderGUIController.Instance.RefreshLevel();
            }

            ShowSpawners(MBoard.MainGrid);
        }

        private void ShowSpawners(MatchGrid mGrid)
        {
            LevelConstructSet lCSet = mGrid.LcSet;

            foreach (var item in mGrid.Cells)
            {
                if (item.GCSpawner)
                {
                    GameObject old = item.GCSpawner.gameObject;
                    item.GCSpawner = null;
                    DestroyImmediate(old);
                }
            }

            if (lCSet.spawnCells != null)
            {
                int i = 0;
                foreach (var item in lCSet.spawnCells)
                {
                    GridCell gC = mGrid.Rows[item.Row].cells[item.Column];
                    if (lCSet.spawnOffsets != null && lCSet.spawnOffsets.Count == lCSet.spawnCells.Count)
                    {
                        gC.CreateSpawner(MBoard.spawnerPrefab, lCSet.spawnOffsets[i]);
                    }
                    else
                    {
                        gC.CreateSpawner(MBoard.spawnerPrefab, Vector2.zero);
                    }
                    i++;
                    gC.GCSpawner.Show(true);
                }
            }
            SaveSpawnOffsets();
        }

        public void SaveSpawnOffsets()
        {
            LCSet.SaveSpawnOfsets(MGrid);
        } 
        #endregion show board

        #region construct menus +
        bool openedConstr = false;

        public void OpenConstructPanel()
        {
            SetConstructControlActivity(false);
            constructPanel.SetActive(true);

            RectTransform rt = constructPanel.GetComponent<RectTransform>();//Debug.Log(rt.offsetMin + " : " + rt.offsetMax);
            float startX = (!openedConstr) ? 0 : 1f;
            float endX = (!openedConstr) ? 1f : 0;

            SimpleTween.Value(constructPanel, startX, endX, 0.2f).SetEase(EaseAnim.EaseInCubic).
                               SetOnUpdate((float val) =>
                               {
                                   rt.transform.localScale = new Vector3(val, 1, 1);
                                   // rt.offsetMax = new Vector2(val, rt.offsetMax.y);
                               }).AddCompleteCallBack(() =>
                               {
                                   SetConstructControlActivity(true);
                                   openedConstr = !openedConstr;
                                   // LevelButtonsRefresh();
                               });


        }

        private void SetConstructControlActivity(bool activity)
        {
            Button[] buttons = constructPanel.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = activity;
            }
        }

        private void ShowConstructMenu(bool show)
        {
            constructPanel.SetActive(show);
            openConstructButton.gameObject.SetActive(show);
        }

        int scrollPosition = 0;
        public void CreateLevelButtons()
        {
            GCSet.Clean();

            int count = 5;
            int currLevel = GameLevelHolder.CurrentLevel;
            int minLevel = Mathf.Max(currLevel - count / 2, 0);
            RebuilLeveldButtonsPanel(minLevel, count);
            UpdateLevelInputField();
        }

        public void RemoveLevel()
        {
            Debug.Log("Click on Button <Remove level...> ");
            if (GCSet.LevelCount < 2)
            {
                Debug.Log("Can't remove the last level> ");
                return;
            }
            GCSet.RemoveLevel(GameLevelHolder.CurrentLevel);
            CreateLevelButtons();
            GameLevelHolder.CurrentLevel = (GameLevelHolder.CurrentLevel <= GCSet.LevelCount - 1) ? GameLevelHolder.CurrentLevel : GameLevelHolder.CurrentLevel - 1;
            ShowLevelData();
            UpdateLevelInputField();
        }

        public void InsertBefore()
        {
            Debug.Log("Click on Button <Insert level before...> ");
            LevelConstructSet lcs = ScriptableObjectUtility.CreateResourceAsset<LevelConstructSet>(pathToSets, levelConstructSetSubFolder, "", " " + 1.ToString());
            GCSet.InsertBeforeLevel(GameLevelHolder.CurrentLevel, lcs);
            CreateLevelButtons();
            ShowLevelData();
        }

        public void InsertAfter()
        {
            Debug.Log("Click on Button <Insert level after...> ");
            LevelConstructSet lcs = ScriptableObjectUtility.CreateResourceAsset<LevelConstructSet>(pathToSets, levelConstructSetSubFolder, "", " " + 1.ToString());
            GCSet.InsertAfterLevel(GameLevelHolder.CurrentLevel, lcs);
            CreateLevelButtons();
            GameLevelHolder.CurrentLevel += 1;
            ShowLevelData();
            UpdateLevelInputField();
        }

        private void LevelButtonsRefresh()
        {
            Action<Button, bool> selectButton = (b, select) =>
            {
                b.GetComponent<Image>().color = (select) ? new Color(0.5f, 0.5f, 0.5f, 1) : new Color(1, 1, 1, 1);
            };

            Button[] levelButtons = LevelButtonsParent.gameObject.GetComponentsInChildren<Button>();
            for (int i = 0; i < levelButtons.Length; i++)
            {
                DataComponent description = levelButtons[i].GetOrAddComponent<DataComponent>();
                selectButton(levelButtons[i], (description.intValue == GameLevelHolder.CurrentLevel));
            }
        }

       
        public void ScrollLevelButtons(bool left)
        {
            int count = 5;
            if (!left)
            {
                if (scrollPosition + count < GCSet.levelSets.Count) scrollPosition++;
                else return;
            }
            else
            {
                if (scrollPosition > 0) scrollPosition--;
                else return;
            }

            int minLevel = Mathf.Max(scrollPosition, 0);
            RebuilLeveldButtonsPanel(minLevel, count);
        }

        /// <summary>
        /// parse input field
        /// </summary>
        /// <param name="val"></param>
        public void GotoLevel(string val)
        {
            int res;
            bool good = int.TryParse(val, out res);
            if (good)
            {
                res--;
                GameLevelHolder.CurrentLevel = Mathf.Clamp(res, 0, GCSet.levelSets.Count - 1);
                // MBoard.ShowGrid(MBoard.MainGrid, 0, null);
                CloseOpenedPanels();
                ShowLevelData();

                int count = 5;
                int currLevel = GameLevelHolder.CurrentLevel;
                int minLevel = Mathf.Max(currLevel - count / 2, 0);
                RebuilLeveldButtonsPanel(minLevel, count);
                UpdateLevelInputField();
            }
        }

        private void RebuilLeveldButtonsPanel(int minLevel, int count)
        {
            Transform parent = LevelButtonsParent.transform;
            DestroyGOInChildrenWithComponent<Button>(parent);
            int maxLevel = minLevel;

            for (int i = minLevel; i < minLevel + count; i++)
            {
                if (i < GCSet.levelSets.Count) maxLevel = i;
            }

            for (int i = maxLevel; i > maxLevel - count; i--)
            {
                if (i >= 0) minLevel = i;
            }
            scrollPosition = minLevel;

            for (int i = minLevel; i <= maxLevel; i++)
            {
                int level = i + 1;
                Button button = CreateButton(levelButtonPrefab, parent, "" + level.ToString(), () =>
                {
                    GameLevelHolder.CurrentLevel = level - 1;
                    CloseOpenedPanels();
                    ShowLevelData();
                    UpdateLevelInputField();
                });
                DataComponent description =  button.GetOrAddComponent<DataComponent>();
                description.intValue = i;
            }

            LevelButtonsRefresh();
        }

        private void UpdateLevelInputField()
        {
            if (LevelInputField) LevelInputField.text = (GameLevelHolder.CurrentLevel + 1).ToString();
        }

        //private void AddLevelButtonsBeh()
        //{
        //   if(removeAddButton) removeAddButton.gameObject.SetActive(!MBoard.IsMainGridActive);
        //}

        //public void CreateAddLevel()
        //{
        //    Debug.Log("Click on Button <Create additional level for...> " + MainLCSet.name);
        //    LevelConstructSet lcs = ScriptableObjectUtility.CreateResourceAsset<LevelConstructSet>(levelConstructSetSubFolder, "", MainLCSet.name, "_add");
        //    Debug.Log("new asset created: " + lcs);
        //    GCSet.AddAdditionalLevel(lcs);
        //    ShowLevelData();
        //}

        //public void RemoveAddLevel()
        //{
        //    Debug.Log("Click on Button <Remove additional level...> ");
        //    GCSet.RemoveAddLevel(MGrid.LcSet);
        //    ShowLevelData();
        //}

        //public void CreateAddLevelButtons()
        //{
        //    if (!AddLevelButtonsContainer) return;
        //    Transform parent = AddLevelButtonsContainer.content;
        //    DestroyGOInChildrenWithComponent<Button>(parent);

        //    List<MatchGrid> aL = new List<MatchGrid>( MBoard.AdditionalGrids.Values);
        //    for (int i = 0; i < aL.Count; i++)
        //    {
        //        int level = i + 1;
        //        Button button = CreateButton(levelButtonPrefab, parent, null, "AddLevel " + level.ToString(), ()=>
        //        {
        //            MBoard.ShowGrid(aL[level - 1], 0 , null);
        //            CloseOpenedPanels();
        //            ShowLevelData();
        //        });
        //    }
        //}

        //private void AddLevelButtonsRefresh()
        //{
        //    if (!AddLevelButtonsContainer) return;
        //    Button[] levelButtons = AddLevelButtonsContainer.content.GetComponentsInChildren<Button>();
        //    for (int i = 0; i < levelButtons.Length; i++)
        //    {
        //        SelectButton(levelButtons[i], (i == MBoard.AddGridIndex));
        //    }
        //}
        #endregion construct menus

        #region grid settings
        private void ShowLevelSettingsMenu(bool show)
        {
            constructPanel.SetActive(show);
            openConstructButton.gameObject.SetActive(show);
        }

        public void OpenSettingsPanel_Click()
        {
            Debug.Log("open grid settings click");

            ScrollPanelController sRC = GridPanelContainer.ScrollPanel;
            if (sRC) // 
            {
                if (sRC) sRC.CloseScrollPanel(true, null);
            }
            else
            {
                CloseOpenedPanels();
                //instantiate ScrollRectController
                sRC = GridPanelContainer.InstantiateScrollPanel();
                sRC.textCaption.text = "Grid panel";

                //create  vert size block
                IncDecInputPanel.Create(sRC.scrollContent, IncDecGridPrefab, "VertSize", MGrid.LcSet.VertSize.ToString(),
                    () => { IncVertSize(); },
                    () => { DecVertSize(); },
                    (val) => { },
                    () => { return MGrid.LcSet.VertSize.ToString(); },
                    null);

                //create hor size block
                IncDecInputPanel.Create(sRC.scrollContent, IncDecGridPrefab, "HorSize", MGrid.LcSet.HorSize.ToString(),
                    () => { IncHorSize(); },
                    () => { DecHorSize(); },
                    (val) => { },
                    () => { return MGrid.LcSet.HorSize.ToString(); },
                    null);

                //create background block
                IncDecInputPanel.Create(sRC.scrollContent, IncDecGridPrefab, "BackGrounds", MGrid.LcSet.BackGround.ToString(),
                    () => { IncBackGround(); },
                    () => { DecBackGround(); },
                    (val) => { },
                    () => { return MGrid.LcSet.BackGround.ToString(); },
                    null);

                //create dist X block
                if (showDistAdjust)
                {
                    IncDecInputPanel.Create(sRC.scrollContent, IncDecGridPrefab, "Dist X", MGrid.LcSet.DistX.ToString(),
                        () => { IncDistX(); },
                        () => { DecDistX(); },
                        (val) => { },
                        () => { return MGrid.LcSet.DistX.ToString(); },
                        null);

                    //create dist Y block
                    IncDecInputPanel.Create(sRC.scrollContent, IncDecGridPrefab, "Dist Y", MGrid.LcSet.DistY.ToString(),
                        () => { IncDistY(); },
                        () => { DecDistY(); },
                        (val) => { },
                        () => { return MGrid.LcSet.DistY.ToString(); },
                        null);
                }

                //create scale block
                IncDecInputPanel.Create(sRC.scrollContent, IncDecGridPrefab, "Scale", MGrid.LcSet.Scale.ToString(),
                    () => { IncScale(); },
                    () => { DecScale(); },
                    (val) => { },
                    () => { return MGrid.LcSet.Scale.ToString(); },
                    null);

                sRC.OpenScrollPanel(null);
            }
        }

        public void IncVertSize()
        {
            Debug.Log("Click on Button <VerticalSize...> ");
            int vertSize = MBoard.MainGrid.LcSet.VertSize;
            vertSize = (vertSize < maxVertSize) ? ++vertSize : maxVertSize;
            MBoard.MainGrid.LcSet.VertSize = vertSize;
            ShowLevelData();
        }

        public void DecVertSize()
        {
            Debug.Log("Click on Button <VerticalSize...> ");
            int vertSize = MGrid.LcSet.VertSize;
            vertSize = (vertSize > minVertSize) ? --vertSize : minVertSize;
            MGrid.LcSet.VertSize = vertSize;
            ShowLevelData();
        }

        public void IncHorSize()
        {
            Debug.Log("Click on Button <HorizontalSize...> ");
            int horSize = MGrid.LcSet.HorSize;
            horSize = (horSize < maxHorSize) ? ++horSize : maxHorSize;
            MGrid.LcSet.HorSize = horSize;
            ShowLevelData();
        }

        public void DecHorSize()
        {
            Debug.Log("Click on Button <HorizontalSize...> ");
            int horSize = MGrid.LcSet.HorSize;
            horSize = (horSize > minHorSize) ? --horSize : minHorSize;
            MGrid.LcSet.HorSize = horSize;
            ShowLevelData();
        }

        public void IncDistX()
        {
            Debug.Log("Click on Button <DistanceX...> ");
            int dist = Mathf.RoundToInt(MGrid.LcSet.DistX * 100f);
            dist += 5;
            MGrid.LcSet.DistX = (dist > 100) ? 1f : dist / 100f;
            ShowLevelData();
        }

        public void DecDistX()
        {
            Debug.Log("Click on Button <DistanceX...> ");
            int dist = Mathf.RoundToInt(MGrid.LcSet.DistX * 100f);
            dist -= 5;
            MGrid.LcSet.DistX = (dist > 0f) ? dist / 100f : 0f;
            ShowLevelData();
        }

        public void IncDistY()
        {
            Debug.Log("Click on Button <DistanceY...> ");
            int dist = Mathf.RoundToInt(MGrid.LcSet.DistY * 100f);
            dist += 5;
            MGrid.LcSet.DistY = (dist > 100) ? 1f : dist / 100f;
            ShowLevelData();
        }

        public void DecDistY()
        {
            Debug.Log("Click on Button <DistanceY...> ");
            int dist = Mathf.RoundToInt(MGrid.LcSet.DistY * 100f);
            dist -= 5;
            MGrid.LcSet.DistY = (dist > 0f) ? dist / 100f : 0f;
            ShowLevelData();
        }

        public void IncScale()
        {
            Debug.Log("Click on Button <Scale...> ");
            int scale = Mathf.RoundToInt(MGrid.LcSet.Scale * 100f);
            scale += 5;
            MGrid.LcSet.Scale = scale / 100f;
            ShowLevelData();
        }

        public void DecScale()
        {
            Debug.Log("Click on Button <Scale...> ");
            int scale = Mathf.RoundToInt(MGrid.LcSet.Scale * 100f);
            scale -= 5;
            MGrid.LcSet.Scale = (scale > 0f) ? scale / 100f : 0f;
            ShowLevelData();
        }

        public void IncBackGround()
        {
            Debug.Log("Click on Button <BackGround...> ");
            MGrid.LcSet.IncBackGround(GOSet.BackGroundsCount);
            ShowLevelData();
        }

        public void DecBackGround()
        {
            Debug.Log("Click on Button <BackGround...> ");
            MGrid.LcSet.DecBackGround(GOSet.BackGroundsCount);
            ShowLevelData();
        }
        #endregion grid settings

        #region grid brushes
        public void Cell_Click(GridCell cell)
        {
            Debug.Log("Click on cell <" + cell.ToString() + "...> ");
            LevelConstructSet lCSet = MGrid.LcSet;

            if (selectedSpawnBrushImage.enabled)
            {
                Debug.Log("spawn brush enabled");
                lCSet.AddSpawnCell(new CellData(-100, cell.Row, cell.Column));
                ShowLevelData();

            }
            else if (currentBrush)
            {
                if (cell.HaveObjectWithID(currentBrush.ID))
                {
                    Debug.Log("remove object ID: "+ currentBrush.ID );
                    cell.RemoveObject(currentBrush.ID);
                }
                else
                {
                    if (currentBrush.CanSetBySize(cell))
                    {
                        GridCell clickCell = cell;
                        Vector2Int gOSize = currentBrush.GetSize();
                        bool simpleBrush = (gOSize == Vector2.one);
                        int hierarchy = currentBrush.GetHierarchy();
                        currentBrush.DestroyHierCompetitor(clickCell, true);
                        Debug.Log("set object ID: " + currentBrush.ID + "; hits: " + currentBrushHits + "; hierarchy: " + currentBrush.GetHierarchy());
                        clickCell.SetObject(currentBrush.ID, currentBrushHits);
                    }
                }
              
                lCSet.SaveObjects(MGrid.Cells);
                ShowLevelData();
            }

            CloseOpenedPanels();
        }

        public void SelectDisabledBrush()
        {
            DeselectAllBrushes();
            selectedDisabledBrushImage.enabled = true;
            currentBrush = GOSet.Disabled;
        }

        private void CloseOpenedPanels()
        {
            ScrollPanelController[] sRCs = GetComponentsInChildren<ScrollPanelController>();
            foreach (var item in sRCs)
            {
                item.CloseScrollPanel(true, null);
            }
        }

        public void SelectSpawnBrush()
        {
            DeselectAllBrushes();
            selectedSpawnBrushImage.enabled = true;
        }

        private void DeselectAllBrushes()
        {
            currentBrush = null;
            PanelContainerController[] panelContainerControllers = brushContainersParent.GetComponentsInChildren<PanelContainerController>();

            foreach (var item in panelContainerControllers)
            {
              if(item)  item.selector.enabled = false;
            }
            selectedSpawnBrushImage.enabled = false;
        }
        #endregion grid brushes

        #region match select
        [SerializeField]
        private PanelContainerController MatchSelectContainer;

        public void OpenMatchSelectPanel_Click()
        {
            LevelConstructSet lCSet = MGrid.LcSet;

            ScrollPanelController sRC = MatchSelectContainer.ScrollPanel;
            if (sRC) // 
            {
                sRC.CloseScrollPanel(true, null);
            }
            else
            {
                CloseOpenedPanels();

                //instantiate ScrollRectController
                sRC = MatchSelectContainer.InstantiateScrollPanel();
                sRC.textCaption.text = "Match Objects on Level";
                List<MatchObject> mData = new List<MatchObject>(GOSet.MatchObjects);

                Action<Button, bool> selectButton = (b, s) =>
                {
                    Image selector = null;
                    List<GameObject> childs = new List<GameObject>();
                    b.gameObject.GetChilds(ref childs);
                    foreach (var item in childs)
                    {
                        if (item.name.CompareTo("Selector") == 0)
                        {
                            selector = item.GetComponent<Image>();
                        }
                    }
                    if (selector) { selector.enabled = s; }
                };

                //create match selectors
                for (int i = 0; i < mData.Count; i++)
                {
                    MatchObject mD = mData[i];
                    Button b = CreateButton(smallButtonPrefab, sRC.scrollContent, mD.ObjectImage, () =>
                    {
                        Debug.Log("Click on Button <" + mD.ID + "...> ");
                        MGrid.LcSet.AddMatch(mD.ID);
                    });
                    b.onClick.AddListener(() => { selectButton(b, MGrid.LcSet.ContainMatch(mD.ID)); });
                    selectButton(b, MGrid.LcSet.ContainMatch(mD.ID));
                }
                sRC.OpenScrollPanel(null);
            }
        }
        #endregion match select

        #region mission
        public void OpenMissionPanel_Click()
        {
            Debug.Log("open mission click");

            MissionConstruct currMiss = MGrid.LcSet.levelMission;

            ScrollPanelController sRC = MissionPanelContainer.ScrollPanel;
            if (sRC) // 
            {
                sRC.CloseScrollPanel(true, null);
            }
            else
            {
                CloseOpenedPanels();
                //instantiate ScrollRectController
                sRC = MissionPanelContainer.InstantiateScrollPanel();
                sRC.textCaption.text = "Mission panel";


                IncDecInputPanel movesPanel = null;

                //create time constrain
                if (showTimeConstrain)
                {
                    IncDecInputPanel.Create(sRC.scrollContent, IncDecTextPanelPrefab, "Time", currMiss.TimeConstrain.ToString(),
                    () => { currMiss.AddTime(1); HeaderGUIController.Instance.RefreshTimeMoves(); },
                    () => { currMiss.AddTime(-1); HeaderGUIController.Instance.RefreshTimeMoves(); },
                    (val) => { int res; bool good = int.TryParse(val, out res); if (good) { currMiss.SetTime(res); HeaderGUIController.Instance.RefreshTimeMoves(); } },
                    () => { movesPanel?.gameObject.SetActive(!currMiss.IsTimeLevel); return currMiss.TimeConstrain.ToString(); },
                    null);
                }
                //create mission moves constrain
                if (true)
                {
                    movesPanel = IncDecInputPanel.Create(sRC.scrollContent, IncDecTextPanelPrefab, "Moves", currMiss.MovesConstrain.ToString(),
                    () => { currMiss.AddMoves(1); HeaderGUIController.Instance.RefreshTimeMoves(); },
                    () => { currMiss.AddMoves(-1); HeaderGUIController.Instance.RefreshTimeMoves(); },
                    (val) => { int res; bool good = int.TryParse(val, out res); if (good) { currMiss.SetMovesCount(res); HeaderGUIController.Instance.RefreshTimeMoves(); } },
                    () => { return currMiss.MovesConstrain.ToString(); },
                    null);
                    movesPanel.gameObject.SetActive(!currMiss.IsTimeLevel);
                }

                //description input field
                if (showMissionDescription)
                {
                    IncDecInputPanel.Create(sRC.scrollContent, InputTextPanelMissionPrefab, "Description", currMiss.Description,
                null,
                null,
                (val) => { currMiss.SetDescription(val); },
                () => { return currMiss.Description; },
                null);
                }

                //create score target
                if (showScoreTarget)
                {
                    IncDecInputPanel.Create(sRC.scrollContent, IncDecTextPanelPrefab, "Score", currMiss.ScoreTarget.ToString(),
                () => { currMiss.AddScoreTarget(1); ChangeTargetScoreEvent?.Invoke(currMiss.ScoreTarget); },
                () => { currMiss.AddScoreTarget(-1); ChangeTargetScoreEvent?.Invoke(currMiss.ScoreTarget); },
                (val) => { int res; bool good = int.TryParse(val, out res); if (good) { currMiss.SetScoreTargetCount(res); ChangeTargetScoreEvent?.Invoke(currMiss.ScoreTarget); } },
                () => { return currMiss.ScoreTarget.ToString(); },
                null);
                }

                //create object targets
                foreach (var item in MBoard.Targets)
                {
                    int id = item.Key;
                    bool show = false;
                   
                    if (showTargetsFromBoard)
                    {
                        if (showMatchTargets && IsMatchId(id)) 
                        {
                            show = true;
                        }
                       if (currMiss.GetTargetCount(id) > 0)
                        {
                            show = true;
                        }
                        if (TargetOnBoard(id))
                        {
                            show = true;
                        }
                    }
                    else
                    {
                        show = true;
                    }

                    if (show)
                    {
                        IncDecInputPanel iP = IncDecInputPanel.Create(sRC.scrollContent, IncDecObjectPanelPrefab, "", currMiss.GetTargetCount(id).ToString(),
                        false,
                        () => { currMiss.AddTarget(id, 1); item.Value?.IncNeedCount(1); },
                        () => { currMiss.RemoveTarget(id, 1); item.Value?.IncNeedCount(-1); },
                        (val) => { int res; bool good = int.TryParse(val, out res); if (good) { currMiss.SetTargetCount(id, res); item.Value?.SetNeedCount(res); } },
                        null,
                        () => { return currMiss.GetTargetCount(id).ToString(); }, // grid.GetObjectsCountByID(id).ToString()); },
                        item.Value.GetImage(GOSet));
                        if(iP.objectButton) iP.objectButton.onClick.AddListener(() => { iP.SetObjectButtonText(MGrid.GetAllByTargetID(item.Value.ID).Count.ToString()); });
                    }
                }

                sRC.OpenScrollPanel(null);
            }
        }

        private bool TargetOnBoard(int id)
        {
            // if(MGrid.GetAllByID(id).Count > 0) return true;
            if (MGrid.GetAllByTargetID(id).Count > 0) return true;
            return false;
        }

        private bool IsMatchId(int id)
        {
            return GOSet.ContainMatchID(id);
        }
        #endregion mission

        #region load assets
        T[] LoadResourceAssets<T>(string subFolder) where T : BaseScriptable
        {
            T[] t = Resources.LoadAll<T>(subFolder);
            if (t != null && t.Length > 0)
            {
                string s = "";
                foreach (var m in t)
                {
                    s += m.ToString() + "; ";
                }
                Debug.Log("Scriptable assets loaded," + typeof(T).ToString() + ", count: " + t.Length + "; sets : " + s);
            }
            else
            {
                Debug.Log("Scriptable assets " + typeof(T).ToString() + " not found!!!");
            }
            return t;
        }
        #endregion load assets

        #region utils
        private void DestroyGOInChildrenWithComponent<T>(Transform parent) where T : Component
        {
            if (!parent) return;
            T[] existComp = parent.GetComponentsInChildren<T>();
            for (int i = 0; i < existComp.Length; i++)
            {
                if (parent.gameObject != existComp[i].gameObject) DestroyImmediate(existComp[i].gameObject);
            }
        }

        private void CreateBrushContainer(Transform parent, PanelContainerController containerPrefab, string capital, List<GridObject> gridObjects)
        {
            if(gridObjects==null || gridObjects.Count == 0)
            {
                Debug.Log("Can't create: " + capital);
                return;
            } 
            PanelContainerController c =  Instantiate(containerPrefab, parent);
            c.capital = capital;
            c.gridObjects = gridObjects;
            c.OpenCloseButton.onClick.RemoveAllListeners();
            c.OpenCloseButton.onClick.AddListener(()=> { CreateBrushPanel(c); });
            c.BrushSelectButton.onClick.RemoveAllListeners();
            c.BrushSelectButton.onClick.AddListener(()=> 
            {
                GridObject gO = c.GetOrAddComponent<GridObject>();
                DeselectAllBrushes();
                currentBrush = GOSet.GetObject(gO.ID); 
                currentBrushHits = gO.Hits;
                c.selector.enabled = true;
                //Debug.Log("current brush: " + currentBrush.ID + " ;hits: " + currentBrush.Hits);
            });
            c.brushImage.sprite = gridObjects[0].ConsructorImage;
            c.GetOrAddComponent<GridObject>().Enumerate(gridObjects[0].ID);
            if (!string.IsNullOrEmpty(capital) && c.BrushName) c.BrushName.text = capital[0].ToString();
        }

        private void CreateBrushPanel(PanelContainerController container)
        {
            ScrollPanelController sRC = container.ScrollPanel;
            if (sRC)
            {
                sRC.CloseScrollPanel(true, null);
            }
            else
            {
                CloseOpenedPanels();

                sRC = (container.gridObjects != null && container.gridObjects.Count > 1) ? container.InstantiateScrollPanel() : container.InstantiateScrollPanelSmall();
                sRC.textCaption.text = container.capital;

                List<GridObject> mData = new List<GridObject>();
                if (container.gridObjects != null) mData.AddRange(container.gridObjects);
                CreateBrushButtons(mData, smallButtonPrefab, container, sRC.scrollContent, container.brushImage, container.selector);
                sRC.OpenScrollPanel(null);
            }
        }

        private void CreateBrushButtons(List<GridObject> mData, Button prefab, PanelContainerController container, Transform parent, Image objectImage, Image selector)
        {
            //create brushes
            if (mData == null || mData.Count == 0) return;

            for (int i = 0; i < mData.Count; i++)
            {
                GridObject mD = mData[i];
                Sprite[] protectionStateImages = mD.GetProtectionStateImages();

                CreateButton(smallButtonPrefab, parent, mD.ConsructorImage, () =>
                {
                    Debug.Log("Click on Button <" + mD.ID + "...> ");
                    DeselectAllBrushes();
                    currentBrush = GOSet.GetObject(mD.ID);
                    objectImage.sprite = currentBrush.ConsructorImage;
                    GridObject cGO = container.GetOrAddComponent<GridObject>();
                    cGO.Enumerate(currentBrush.ID);
                    cGO.Hits = 0;
                    currentBrushHits = 0;
                    selector.enabled = true;
                });

                if (!showFullProtectors && protectionStateImages != null)
                {
                    int hits = 0;
                    foreach (var item in protectionStateImages)
                    {
                        hits += 1;
                        var tHits = hits;
                        CreateButton(smallButtonPrefab, parent, item, () =>
                        {
                            Debug.Log("Click on Button <" + mD.ID +" ;hits: "+ tHits +  "...> " );
                            DeselectAllBrushes();
                            currentBrush = GOSet.GetObject(mD.ID);
                            objectImage.sprite = item;
                            GridObject cGO = container.GetOrAddComponent<GridObject>();
                            cGO.Enumerate(currentBrush.ID);
                            cGO.Hits = tHits;
                            currentBrushHits = tHits;
                            selector.enabled = true;
                        });
                    }
                }
            }
        }

        private Button CreateButton(Button prefab, Transform parent, Sprite sprite, System.Action listener)
        {
            Button button = CreateButton(prefab, parent, listener);
            button.GetComponent<Image>().sprite = sprite;
            return button;
        }

        private Button CreateButton(Button prefab, Transform parent, System.Action listener)
        {
            Button button = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            button.transform.SetParent(parent);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.onClick.RemoveAllListeners();

            if (listener != null) button.onClick.AddListener(() =>
            {
                listener();
            });

            return button;
        }

        private Button CreateButton(Button prefab, Transform parent, Sprite sprite, string text, System.Action listener)
        {
            Button button = CreateButton(prefab, parent, sprite, listener);
            Text t = button.GetComponentInChildren<Text>();
            if (t && text != null) t.text = text;
            return button;
        }

        private Button CreateButton(Button prefab, Transform parent, string text, System.Action listener)
        {
            Button button = CreateButton(prefab, parent, listener);
            Text t = button.GetComponentInChildren<Text>();
            if (t && text != null) t.text = text;
            return button;
        }

        private void SelectButton(Button b)
        {
            Text t = b.GetComponentInChildren<Text>();
            if (!t) return;
            t.enabled = true;
            t.gameObject.SetActive(true);
            t.text = "selected";
            t.color = Color.black;
        }

        private void DeselectButton(Button b)
        {
            Text t = b.GetComponentInChildren<Text>();
            if (!t) return;
            t.enabled = true;
            t.gameObject.SetActive(true);
            t.text = "";
        }
        #endregion utils
#endif
    }
}
