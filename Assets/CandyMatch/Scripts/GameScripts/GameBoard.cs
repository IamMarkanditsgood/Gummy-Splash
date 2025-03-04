using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mkey
{
    public class GameBoard : MonoBehaviour
    {
        #region bomb setting
        private const BombType bombType = BombType.DynamicClick;  // only DynamicClick bombs, changed 17.01.2024
        public const bool showBombExplode = true;
        #endregion bomb setting

        #region settings 
        [Space(8)]
        [Header("Game settings")]
        public bool showAlmostMessage = true;
        public static bool showMission = true;
        public int almostCoins = 100;
        public FillType fillType;
        public bool showScore;
        private const AutoWin autoWin = AutoWin.Bombs;      // only bombs changed 17.01.2024
        #endregion settings

        #region references
        [Header("Main references")]
        [Space(8)]
        public Transform GridContainer;
        [SerializeField]
        private RectTransform flyTarget;
        public SpriteRenderer backGround;
        public GameConstructor gConstructor;
        public BombCreator bombCreator;
        [SerializeField]
        private WinController winController;
        [SerializeField]
        private ScoreController scoreController;
        [SerializeField]
        private PopUpsController goodPrefab;
        [SerializeField]
        private PopUpsController greatPrefab;
        [SerializeField]
        private PopUpsController excellentPrefab;
        [SerializeField]
        private PopUpsController timeLeftMessagePrefab;
        [SerializeField]
        private PopUpsController movesLeftMessagePrefab;
        [SerializeField]
        public PopUpsController AutoVictoryPrefab;
        [SerializeField]
        private PopUpsController missionPrefab;
        [SerializeField]
        private PopUpsController missionHardPrefab;
        #endregion references

        #region spawn
        [Space(8)]
        [Header("Spawn")]
        public Spawner spawnerPrefab;
        private SpawnerStyle spawnerStyle = SpawnerStyle.AllEnabled;
        #endregion spawn

        #region grid
        public static float MaxDragDistance;
        public MatchGrid MainGrid { get; private set; }
        #endregion grid

        #region curves
        [SerializeField]
        private AnimationCurve explodeCurve;
        [SerializeField]
        public AnimationCurve arcCurve;
        #endregion curves

        #region states
        public static GameMode GMode = GameMode.Play; // Play or Edit

        public MatchBoardState MbState; // {get; private set; }
        #endregion states

        #region properties
        public Vector3 FlyTarget
        {
            get { return flyTarget.transform.position; } //return Coordinats.CanvasToWorld(flyTarget.gameObject); }
        }

        public Sprite BackGround
        {
            get { return backGround.sprite; }
            set { if (backGround) backGround.sprite = value; }
        }

        public int AverageScore
        {
            get;
            private set;
        }

        private SoundMaster MSound { get { return SoundMaster.Instance; } }

        public WinController WinContr { get { return winController; } }

        public Dictionary<int, TargetData> Targets { get; private set; }

        public GuiController MGui => GuiController.Instance;

        public int MatchScore { get; private set; }

        public int AlmostCoins => almostCoins;
        #endregion properties

        #region sets
        private GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        private LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        private GameObjectsSet GOSet { get { return GCSet.GOSet; } }
        public MissionConstruct FullLevelMission { get; private set; }
        private GameLevelHolder MGLevel => GameLevelHolder.Instance;
        #endregion sets

        #region events
        public static Action<GameBoard> ChangeCurrentBoardEvent;  // use for game with many boards
        public Action<GameBoard> BeforeFillBoardEvent;             // 
        public Action<GameBoard> AfterFillBoardEvent;             // 
        public Action<GameBoard> BeforeCollectBoardEvent;          // 
        public Action<GameBoard> BeforeStepBoardEvent;             // 
        public Action<GameBoard> AfterStepBoardEvent;
        public Action<GameBoard> SkipWinShowEvent;
        public UnityEvent PrelooseEvent;
        public UnityEvent LooseEvent;
        public UnityEvent MovesLeftFiveEvent;
        public UnityEvent GoodStepEvent;
        public UnityEvent GreatStepEvent;
        public UnityEvent ExcellentStepEvent;
        public UnityEvent AutoWinEvent;
        #endregion events

        #region temp
        public MatchGroupsHelper MatchGroups { get; private set; }
        private bool testing = true;
        private float lastiEstimateShowed;
        private float lastPlayTime;
        private bool canPlay = false;
        private bool fieldBoosterOk = false;
        private int collected = 0;                  // collected counter
        private float collectDelay = 0;             // 0.1f; timespan between cells collecting
        private bool manualStep = false;            // flag for tracking the complete manual step in the game - from swap to the complete completion of all collections and fillings
        private int scoreBeforeStep = 0;
        private TouchState touchState = TouchState.None;
        private HeaderGUIController headerGUI;
        private List<Spawner> spawners;
        private bool skipWinShow = false;
        private bool wave = false;
        private static List<FieldBooster> FieldBoosters { get; set; }
        private bool testBombs = false;  // debug bombs test (hold space and click -> set bomb, left control hold and click -> explode wave)
        #endregion temp

        #region debug
        [Header("Debug")]
        [SerializeField]
        private bool anySwap = false;
        #endregion debug

        public static GameBoard Instance { get; private set; }

        #region regular
        private void Awake()
        {
            if (Instance) Destroy(gameObject);
            else
            {
                Instance = this;
            }
#if UNITY_EDITOR
            if (GCSet && GCSet.testMode) GameLevelHolder.CurrentLevel = Mathf.Abs(GCSet.testLevel);
#endif
            ScoreHolder.Instance.SetCount(0);
        }

        private void Start()
        {
            #region game sets 
            if (!GCSet)
            {
                Debug.Log("Game construct set not found!!!");
                return;
            }

            if (!LCSet)
            {
                Debug.Log("Level construct set not found!!! - " + GameLevelHolder.CurrentLevel);
                return;
            }

            if (!GOSet)
            {
                Debug.Log("MatcSet not found!!! - " + GameLevelHolder.CurrentLevel);
                return;
            }
            #endregion game sets 

            FullLevelMission = LCSet.levelMission;

            #region targets
            Targets = new Dictionary<int, TargetData>();
            #endregion targets

            DestroyGrid();
            CreateGameBoard();
            GameLevelHolder.StartLevel();

            if (GMode == GameMode.Edit)
            {
#if UNITY_EDITOR
                Debug.Log("start edit mode");
                foreach (var item in GOSet.TargetObjects) // add all possible targets
                {
                    Targets[item.TargetGroupID] = new TargetData(item.TargetGroupID, FullLevelMission.GetTargetCount(item.TargetGroupID));
                }

                if (gConstructor)
                {
                    gConstructor.gameObject.SetActive(true);
                    gConstructor.InitStart();
                }
                foreach (var item in GOSet.BoosterObjects)
                {
                    // if (item.Use) item.ChangeUse();
                }
#endif
            }

            else if (GMode == GameMode.Play)
            {
                GridObject.CollectEvent = TargetCollectEventHandler;
                MatchScore = 10;
                Debug.Log("start play mode");
                WinContr.InitStart();

                if (gConstructor) DestroyImmediate(gConstructor.gameObject);

                ScoreHolder.Instance.SetAverageScore(WinContr.IsTimeLevel ? Mathf.Max(40, WinContr.MovesRest) * 30 : WinContr.MovesRest * 30);
                WinContr.TimerLeft30Event += () =>
                {
                    if (timeLeftMessagePrefab)
                    {
                        timeLeftMessagePrefab.CreateWindowAndClose(1);
                    }
                };

                WinContr.MovesLeft5Event += () =>
                {
                    if (WinContr.Result != GameResult.WinAuto)
                    {
                        if (movesLeftMessagePrefab) movesLeftMessagePrefab.CreateWindowAndClose(1);
                        MovesLeftFiveEvent?.Invoke();
                    }
                };

                WinContr.LevelWinEvent += () =>
                {
                    MGui.ShowPopUpByDescription("victory");
                    MGLevel.PassLevel();
                    foreach (var item in GOSet.BoosterObjects)
                    {
                        if (item.Use) item.ChangeUse();
                    }
                };

                WinContr.LevelPreLooseEvent += () =>
                {
                    // if (CoinsHolder.Count >= almostCoins) 
                    MGui.ShowPopUpByDescription("almost");
                    PrelooseEvent?.Invoke();
                };

                WinContr.LevelLooseEvent += () =>
                {
                    MGui.ShowPopUpByDescription("failed");
                    if (!GCSet.UnLimited) LifesHolder.Add(-1);
                    foreach (var item in GOSet.BoosterObjects)
                    {
                        if (item.Use) item.ChangeUse();
                    }
                    LooseEvent?.Invoke();
                };
                WinContr.AutoWinEvent += () =>
                {
                    if (AutoVictoryPrefab) AutoVictoryPrefab.CreateWindow();   // CreateWindowAndClose(1) 16.01.2023
                    AutoWinEvent?.Invoke();
                };

                foreach (var item in GOSet.TargetObjects)
                {
                    if (FullLevelMission.Targets.ContainObjectID(item.TargetGroupID) && (FullLevelMission.Targets.CountByID(item.TargetGroupID) > 0))
                    {
                        Targets[item.TargetGroupID] = new TargetData(item.TargetGroupID, FullLevelMission.GetTargetCount(item.TargetGroupID));

                        // notify the remaining objects on the game board
                        Targets[item.TargetGroupID].ChangeCountEvent += (targetData) => { MainGrid.GetObjectsByTargetID(targetData.ID).ApplyAction((gObject) => { gObject.TargetCollectEventHandler(targetData); }); };
                        Targets[item.TargetGroupID].ReachedEvent += (targetData) => { MainGrid.GetObjectsByTargetID(targetData.ID).ApplyAction((gObject) => { gObject.TargetReachedEventHandler(targetData); }); };
                    }
                }

                // collect preloaded hidden
                HiddenObject hObject = null;
                foreach (var item in MainGrid.Cells)
                {
                    HiddenObject hO = item.Hidden;
                    if (hO)
                    {
                        hO.Hit(null, null);
                        hObject = hO;
                    }
                }

                Action missionAction = () => {
                    PopUpsController _missionPrefab = LCSet.hardFlag ? missionHardPrefab : missionPrefab;
                    if (showMission && _missionPrefab)
                    {
                        MGui.ShowPopUp(_missionPrefab, () =>
                        {
                            if (WinContr.IsTimeLevel) WinContr.Timer.Start();
                            MbState = MatchBoardState.Fill;
                            canPlay = true;
                            StartCoroutine(SetFieldBoostersC());
                        });
                    }
                    else
                    {
                        if (WinContr.IsTimeLevel) WinContr.Timer.Start();
                        MbState = MatchBoardState.Fill;
                        canPlay = true;
                        StartCoroutine(SetFieldBoostersC());
                    }
                };

                if (LCSet.LevelStartStoryPage)
                {
                    MGui.ShowPopUp(LCSet.LevelStartStoryPage, missionAction);
                }
                else
                {
                    missionAction?.Invoke();
                }
                showMission = true;
            }

            MainGrid.CalcObjects();
            /*
             // test array math
                        int[,] t6_0 = { { 3, 3, 2, 3, 4 },
                                        { 1, 1, 0, 1, 5 },
                                        { 3, 3, 1, 3, 5 },
                                        };
                        PrintData.BufTostring (t6_0, t6_0.GetLength(1), t6_0.GetLength(0), "t6: source", ';');
                        int[,] t6_0r = t6_0.CWRotateArray2D();
                        PrintData.BufTostring(t6_0r, t6_0r.GetLength(1), t6_0r.GetLength(0), "t6cw: source", ';');
            */
        }

        private void Update()
        {
            // return;
            if (skipWinShow) return;
            if (!canPlay) return;
            if (!fieldBoosterOk) return;
            if (WinContr.Result == GameResult.Win) return;
            if (WinContr.Result == GameResult.Loose) return;

            WinContr.UpdateTimer(Time.time);

            // check board state
            switch (MbState)
            {
                case MatchBoardState.ShowEstimate:  // iddle <-> ShowEstimate
                    ShowEstimateState();
                    break;

                case MatchBoardState.Fill:          // start -> fill, ExplodeBomb -> fill, EndSwap -> fill, MakeStep->fill, MixGrid -> fill;  fill <-> collect, 
                    FillState();
                    break;

                case MatchBoardState.Collect:       // fill <-> collect;  collect -> ShowEstimate;
                    CollectState();
                    break;

                case MatchBoardState.Iddle:         // iddle <-> ShowEstimate
                    IddleState();
                    break;

                case MatchBoardState.Waiting:       // not active state, just waiting coroutines, no handlers
                    break;
            }
        }
        #endregion regular

        #region grid construct
        public void CreateGameBoard()
        {
            Debug.Log("Create gameboard ");
            Debug.Log("level set: " + LCSet.name);
            Debug.Log("current level: " + GameLevelHolder.CurrentLevel);

            BackGround = GOSet.GetBackGround(LCSet.BackGround);

            if (GMode == GameMode.Play)
            {
                Func<LevelConstructSet, Transform, MatchGrid> create = (lC, cont) =>
                {
                    MatchGrid g = new MatchGrid(lC, GOSet, cont, SortingOrder.Base, GMode);

                    // set cells delegates
                    for (int i = 0; i < g.Cells.Count; i++)
                    {
                        g.Cells[i].GCPointerDownEvent = MatchPointerDownHandler;
                        g.Cells[i].GCPointerUpEvent = MatchPointerUpHandler;
                        g.Cells[i].GCDragEnterEvent = MatchDragEnterHandler;
                        g.Cells[i].GCDoubleClickEvent = MatchDoubleClickHandler;
                    }
                    MaxDragDistance = Vector3.Distance(g.Cells[0].transform.position, g.Cells[1].transform.position);

                    // have manually added spawn cells
                    bool haveAddedSpawners = false;

                    if (lC.spawnCells != null)
                    {
                        foreach (var item in lC.spawnCells)
                        {
                            if (g[item.Row, item.Column] && !g[item.Row, item.Column].IsDisabled) { haveAddedSpawners = true; break; }
                        }
                    }

                    if (haveAddedSpawners)
                    {
                        foreach (var item in lC.spawnCells)
                        {
                            if (g[item.Row, item.Column]) g[item.Row, item.Column].CreateSpawner(spawnerPrefab, Vector2.zero);
                        }
                    }
                    else
                    {
                        g.Columns.ForEach((c) =>
                        {
                            c.CreateTopSpawner(spawnerPrefab, spawnerStyle, GridContainer.lossyScale, transform);
                        });
                    }

                    spawners = new List<Spawner>(GetComponentsInChildren<Spawner>()); // avoid columns, use created spawners directly // 20.12.2023
                    // create pathes to spawners
                    CreateFillPath(g);

                    g.FillGrid_exclude(true);       // fill grid after creating fill pathes 07.12.2023

                    g.Cells.ForEach((c) =>
                    {
                        c.CreateBorder();
#if UNITY_EDITOR
                        c.name = c.ToString();    // set complete name with spawner flag
#endif
                    });
                    return g;
                };

                SwapHelper.SwapEndEvent = MatchEndSwapHandler;
                SwapHelper.SwapBeginEvent = MatchBeginSwapHandler;
                BombCombiner.CombineBeginEvent = () => {
                    WinContr.MakeMove();
                };
                BombCombiner.CombineCompleteEvent = () => {
                    SetControlActivity(true, true);
                    MbState = MatchBoardState.Fill;
                };
                MainGrid = create(LCSet, GridContainer);
            }
            else // edit mode
            {
#if UNITY_EDITOR

                FullLevelMission = LCSet.levelMission;

                // main grid
                if (MainGrid != null && MainGrid.LcSet == LCSet)
                {
                    MainGrid.Rebuild(GOSet, GMode);
                }
                else
                {
                    DestroyGrid();
                    MainGrid = new MatchGrid(LCSet, GOSet, GridContainer, SortingOrder.Base, GMode);
                }

                // set cells delegates for constructor
                for (int i = 0; i < MainGrid.Cells.Count; i++)
                {
                    MainGrid.Cells[i].GCPointerDownEvent = (c) =>
                     {
                         gConstructor.GetComponent<GameConstructor>().Cell_Click(c);
                     };

                    MainGrid.Cells[i].GCDragEnterEvent = (c) =>
                    {
                        gConstructor.GetComponent<GameConstructor>().Cell_Click(c);
                    };
                }
#endif
            }

            #region matchgroups
            MatchGroups = new MatchGroupsHelper(MainGrid);
            MatchGroups.ComboCollect = ComboCollectHandler;
            MatchGroups.Collect = CollectHandler;
            #endregion matchgroups
        }

        private int horPathlength;
        private void CreateFillPath(MatchGrid g)
        {
            // Debug.Log("Make gravity fill path");
            Map map = new Map(g);
            PathFinder pF = new PathFinder(LCSet.pathTELBR); // true - use TELBR neighbors
            GridCell c;
            List<GridCell> path;

            for (int i = 0; i < g.Cells.Count; i++)
            {
                c = g.Cells[i];
                /*
                    // debug neighbors
                    if (c.Row == 7 && c.Column == 8)
                        {
                            // Debug.Log("debug neighbors for " + c.name + " neighbors: " + c.Neighbors.GetNeighBorsPF_TELBR().MakeString(";")) ;
                        }
                */

                if (!c.Blocked && !c.IsDisabled && !c.MovementBlocked)
                {
                    horPathlength = int.MaxValue;
                    path = null;
                    foreach (var sp in spawners)
                    {
                        if (sp.gridCell != c)
                        {
                            pF.CreatePath(map, c.PathCell, sp.gridCell.PathCell);

                            /*
                          // debug cell path
                          if (c.Row == 9 && c.Column == 1)
                          {
                              Debug.Log("debug path for " + c.name + " : " + pF.GCPath().MakeString(";") + " to spawner: " + sp.gridCell.name + "; hor length: " + pF.GetHorPathLength(c.pfCell));
                          }
                        */

                            if (pF.GetHorPathLength(c.PathCell) < horPathlength)
                            {
                                path = pF.GCPath();
                                horPathlength = pF.GetHorPathLength(c.PathCell);   // 16.12.2023
                            }
                        }
                        else
                        {
                            horPathlength = int.MaxValue;
                            path = new List<GridCell>();
                            break;
                        }
                    }
                    c.FillPathToSpawner = path;
                    c.HorPathLength = horPathlength;
                }
            }

            //g.Cells.ForEach((c) =>
            //{

            //});
        }

        /// <summary>
        /// destroy default main grid cells
        /// </summary>
        private void DestroyGrid()
        {
            GridCell[] gcs = gameObject.GetComponentsInChildren<GridCell>();
            for (int i = 0; i < gcs.Length; i++)
            {
                Destroy(gcs[i].gameObject);
            }
        }
        #endregion grid construct

        #region bombs
        /// <summary>
        /// async collect matched objects in a group
        /// </summary>
        /// <param name="completeCallBack"></param>
        internal void ExplodeClickBomb(GridCell c)
        {
            if (!c.DynamicClickBomb)
            {
                return;
            }
            MbState = MatchBoardState.Waiting;
            SetControlActivity(false, false);
            c.ExplodeBomb(0.0f, showBombExplode, true, () =>
            {
                MbState = MatchBoardState.Fill;
               // Debug.Log("explode complete");
            });

        }
        #endregion bombs

        #region states
        private List<FallingObject> GetFalling()
        {
            List<GridCell> botCell = MainGrid.GetBottomEnabledCells();
            List<FallingObject> res = new List<FallingObject>();
            foreach (var item in botCell)
            {
                if (item)
                {
                    FallingObject f = item.Falling;
                    if (f)
                    {
                        res.Add(f);
                    }
                }
            }
            return res;
        }

        private void CollectFalling(Action completeCallBack)
        {
            //   Debug.Log("collect falling " + GetFalling().Count);
            ParallelTween pt = new ParallelTween();
            foreach (var item in GetFalling())
            {
                pt.Add((callBack) =>
                {
                    item.Collect(0, false, true, callBack);
                });
            }
            pt.Start(completeCallBack);
        }

        private List<SubUnderlayMCObject> GetSubUnderlays()
        {
            SubUnderlayMCObject[] source = GetComponentsInChildren<SubUnderlayMCObject>();
            List<SubUnderlayMCObject> res = new List<SubUnderlayMCObject>();

            foreach (var item in source)
            {
                if (item && item.IsFree())
                {
                    res.Add(item);
                }
            }
            return res;
        }

        private void CollectSubUnderlays(Action completeCallBack)
        {
            //  Debug.Log("collect subunderlays " + GetSubUnderlays().Count);
            ParallelTween pt = new ParallelTween();
            foreach (var item in GetSubUnderlays())
            {
                pt.Add((callBack) =>
                {
                    item.Collect(0, true, true, callBack);
                });
            }
            pt.Start(completeCallBack);
        }

        private void CollectState()
        {
            MbState = MatchBoardState.Waiting;
            collected = 0;

            // start collecting, disable touch
            SetControlActivity(false, false);       // Debug.Log("collect matches");

            // force collect special objects (falling, subunderlays)
            CollectFalling(() => { });              // free cells may appear, change 05.10.2023 remove callback WinContr.CheckResult(); 
            CollectSubUnderlays(() => { });

            // check for free cells
            CreateFillPath(MainGrid);                                       // 17.01.24
            if (MainGrid.GetFreeCells(true).Count > 0)
            {
                MbState = MatchBoardState.Fill;
                return;
            }

            // search and create matchgroups
            MatchGroups.CancelTweens();
            MatchGroups.CreateGroups(3);

            // break collecting or collect match groups
            if (MatchGroups.MatchesLength == 0)     // no matches
            {
                SetControlActivity(true, true);
                WinContr.CheckResult();

                MbState = MatchBoardState.ShowEstimate;
                if (manualStep)
                {
                    manualStep = false;
                    GreatingMessage(scoreBeforeStep, ScoreHolder.Count);
                    AfterStepBoardEvent?.Invoke(this);
                }
            }
            else
            {
                BeforeCollectBoardEvent?.Invoke(this);
                MatchScore = scoreController.GetScoreForMatches(MatchGroups.MatchesLength);
                MatchGroups.CollectMatchGroups(() =>
                {
                    MbState = MatchBoardState.Fill;
                    MatchScore = scoreController.GetScoreForMatches(0);
                });
            }
        }

        private void FillGridByStep(List<GridCell> freeCells, Action completeCallBack)
        {
            if (freeCells.Count == 0)
            {
                completeCallBack?.Invoke();
                return;
            }

            ParallelTween tp = new ParallelTween();
            foreach (GridCell gc in freeCells)
            {
                tp.Add((callback) =>
                {
                    gc.FillGrab(callback);
                });
            }
            tp.Start(() =>
            {
                completeCallBack?.Invoke();
            });
        }

        private void FillState()
        {
            if (fStarted) return;
            StartCoroutine(FillStateC());
            return;
        }

        bool fStarted = false;
        private IEnumerator FillStateC()
        {
            fStarted = true;
            CreateFillPath(MainGrid);                                       // 26.12.23

            // we get the number of cells that need to be filled
            List<GridCell> gFreeCells = MainGrid.GetFreeCells(true);        // Debug.Log("fill free count: " + gFreeCells.Count + " to collapse" );
            bool filled = false;
            // Debug.Log("FillStateC() gFreeCells : " + gFreeCells.Count);

            // disable touch, raise BeforeFillBoardEvent
            if (gFreeCells.Count > 0)
            {
                SetControlActivity(false, false);                           //  CreateFillPath(MainGrid); // 26.12.23
                // Debug.Log("before fill");
                BeforeFillBoardEvent?.Invoke(this);
            }

            // filling while there are empty cells
            while (gFreeCells.Count > 0)
            {
                FillGridByStep(gFreeCells, () => { });
                yield return new WaitForEndOfFrame();
                //  yield return new WaitForSeconds(0.5f); // debug test
                gFreeCells = MainGrid.GetFreeCells(true);
                filled = true;
            }

            // we are waiting for the completion of filling, raise AfterFillBoardEvent and go to the collecting state
            while (!MainGrid.NoPhys()) yield return new WaitForEndOfFrame();
            if (filled) AfterFillBoardEvent?.Invoke(this);
            MbState = MatchBoardState.Collect;
            fStarted = false;
        }

        private void PlayIddleAnimRandomly()
        {
            if ((Time.time - lastPlayTime) < 5.3f) return;
            int randCell = UnityEngine.Random.Range(0, MainGrid.Cells.Count);
            MainGrid.Cells[randCell].PlayIddle();
            lastPlayTime = Time.time;
        }

        private void IddleState()
        {
            PlayIddleAnimRandomly();
            if (Time.time - lastiEstimateShowed >= 5f)
            {
                lastiEstimateShowed = Time.time;
                MbState = MatchBoardState.ShowEstimate;
            }
        }

        private void ShowEstimateState()
        {
            if (!MainGrid.NoPhys()) return;

            MbState = MatchBoardState.Waiting;
            MatchGroups.CancelTweens();
            MatchGroups.CreateEstimateGroups();

            int clickBombsCount = 0;
            if (bombType == BombType.DynamicClick)
            {
                List<GridCell> bombs = MainGrid.GetAllBombs(true);
                clickBombsCount = bombs.Count;
            }

            if (MatchGroups.EstimateMatchesLength == 0 && clickBombsCount == 0)
            {
                MixGrid(null);
                return;
            }

            if (WinContr.Result != GameResult.WinAuto)
            {
                lastiEstimateShowed = Time.time; //  Debug.Log("show estimate");
                if (HardModeHolder.Mode == HardMode.Easy)
                {
                    MatchGroups.ShowNextEstimateMatchGroups(() =>
                    {
                        MbState = MatchBoardState.Iddle; // go to iddle state
                    });
                }
                else
                {
                    MbState = MatchBoardState.Iddle; // go to iddle state
                }
            }
            else
            {
                MakeStep(); // make auto step
            }
        }

        public void MixGrid(Action completeCallBack)
        {
            ParallelTween pT0 = new ParallelTween();
            ParallelTween pT1 = new ParallelTween();

            TweenSeq tweenSeq = new TweenSeq();
            List<GridCell> cellList = new List<GridCell>();
            List<GameObject> goList = new List<GameObject>();
            MatchGroups.CancelTweens();
            MatchGroups.CancelTweens();

            MainGrid.Cells.ForEach((c) => { if (c.IsMixable) { cellList.Add(c); goList.Add(c.DynamicObject); } });
            cellList.ForEach((c) => { pT0.Add((callBack) => { c.MixJump(transform.position, callBack); }); });

            cellList.ForEach((c) =>
            {
                int random = UnityEngine.Random.Range(0, goList.Count);
                GameObject m = goList[random];
                pT1.Add((callBack) => { c.GrabDynamicObject(m.gameObject, false, callBack); });
                goList.RemoveAt(random);
            });

            tweenSeq.Add((callBack) =>
            {
                pT0.Start(callBack);
            });

            tweenSeq.Add((callBack) =>
            {
                pT1.Start(() =>
                {
                    MbState = MatchBoardState.Fill;
                    completeCallBack?.Invoke();
                    callBack();
                });
            });
            tweenSeq.Start();
        }

        internal void SetControlActivity(bool activityGrid, bool activityMenu)
        {
            if (WinContr.Result == GameResult.None || WinContr.Result == GameResult.PreLoose) // control touch activity only for live game
            {
                TouchManager.SetTouchActivity(activityGrid);
                HeaderGUIController.Instance.SetControlActivity(activityMenu);
                FooterGUIController.Instance.SetControlActivity(activityMenu);
            }
            else
            {
                TouchManager.SetTouchActivity(false);
                HeaderGUIController.Instance.SetControlActivity(false);
                FooterGUIController.Instance.SetControlActivity(false);
            }
        }
        #endregion states

        #region swap helper handlers
        private void MatchBeginSwapHandler(GridCell source, GridCell target)
        {
            if (GMode == GameMode.Play)
            {
                touchState = TouchState.BeginSwap;
                SetControlActivity(false, false);
            }
        }

        private void MatchEndSwapHandler(GridCell source, GridCell target, bool bombSwap)
        {
            if (GMode == GameMode.Play)
            {
                collected = 0; // reset collected count
                touchState = TouchState.EndSwap;
                if (bombSwap)
                {
                    DynamicClickBombObject sDBomb = (source) ? source.DynamicClickBomb : null;
                    DynamicClickBombObject tDBomb = (target) ? target.DynamicClickBomb : null;
                    if (sDBomb && !tDBomb)
                    {
                        sDBomb.Swapped = target;
                        WinContr.MakeMove();
                        ExplodeClickBomb(source);
                    }
                    else if (!sDBomb && tDBomb)
                    {
                        tDBomb.Swapped = source;
                        WinContr.MakeMove();
                        ExplodeClickBomb(target);
                    }
                    else
                    {
                        // Debug.Log("combine bombs");
                    }
                    return;
                }
                MatchGroups.CreateGroups(3);
                if (MatchGroups.MatchesLength == 0)   // no matches
                {
                    if (!anySwap)
                        SwapHelper.UndoSwap(() =>
                        {
                            SetControlActivity(true, true);
                            MbState = MatchBoardState.Fill;
                        });

                    else
                    {
                        SetControlActivity(true, true);
                        WinContr.MakeMove();
                        MbState = MatchBoardState.Fill;
                    }
                }
                else
                {
                    manualStep = true;
                    scoreBeforeStep = ScoreHolder.Count;
                    BeforeStepBoardEvent?.Invoke(this);
                    SetControlActivity(true, true);
                    WinContr.MakeMove();
                    MbState = MatchBoardState.Fill; // Debug.Log("end swap -> to fill");
                }
            }
        }
        #endregion swap helper handlers

        #region gridcell handlers
        private void MatchPointerDownHandler(GridCell c)            // does not affect the game board in this version
        {
            if (GMode == GameMode.Play)
            {
                if (MainGrid.NoPhys())
                {
                    touchState = TouchState.Down;
                    MatchGroups.CancelTweens();
                    // EstimateGroups.CancelTweens();
                    // if(Booster.ActiveBooster != null) 
                    //    ApplyBooster(c);
                    // else
                    //    ExplodeClickBomb(c);
                }
            }
            else if (GMode == GameMode.Edit)
            {
#if UNITY_EDITOR
                //  gConstructor.GetComponent<GameConstructor>().selected = c;
#endif
            }
        }

        int bombNumber = 0;
        private void MatchPointerUpHandler(GridCell c)
        {
            if (GMode == GameMode.Play)
            {
                if (MainGrid.NoPhys())
                {
                    // debug
                    if (Input.GetKey("space") && testBombs)
                    {
                        int count = GOSet.DynamicClickBombObjects.Count;
                        c.SetObject(GOSet.DynamicClickBombObjects[bombNumber]);
                        bombNumber++;
                        if (bombNumber >= count) bombNumber = 0;
                        return;
                    }
                    if (Input.GetKey(KeyCode.LeftControl) && testBombs)
                    {
                        Debug.Log("try wave");
                        ExplodeWave(0, c.transform.position, 3, null);
                        return;
                    }

                    // MatchGroups.CancelTweens();
                    // EstimateGroups.CancelTweens();
                    if (touchState == TouchState.None || touchState == TouchState.Down)
                    {
                        if (Booster.ActiveBooster != null)
                        {
                            ApplyBooster(c);
                        }
                        else if (c.DynamicClickBomb)
                        {
                            WinContr.MakeMove();
                            ExplodeClickBomb(c);
                        }
                    }
                }
                touchState = TouchState.None;
            }
            else if (GMode == GameMode.Edit)
            {
#if UNITY_EDITOR

#endif
            }
        }

        private void MatchDragEnterHandler(GridCell c)
        {
            if (GMode == GameMode.Play)
            {
                SwapHelper.Swap();
            }
        }

        private void MatchDoubleClickHandler(GridCell c)
        {

        }

        /// <summary>
        /// Raise for each collected matchobject
        /// </summary>
        /// <param name="gCell"></param>
        /// <param name="mData"></param>
        public void TargetCollectEventHandler(int id)
        {
            if (GMode == GameMode.Play)
            {
                if (Targets.ContainsKey(id)) Targets[id].IncCurrCount();
                GameEvents.CollectGridObject?.Invoke(id);
            }
        }

        /// <summary>
        /// Raise for each collected matchobject
        /// </summary>
        /// <param name="gCell"></param>
        /// <param name="mData"></param>
        public void TargetAddEventHandler(int id)
        {
            if (GMode == GameMode.Play)
            {
                if (Targets.ContainsKey(id)) Targets[id].IncNeedCount(1);
            }
        }

        public bool IsTarget(int ID)
        {
            return (Targets != null && Targets.ContainsKey(ID));
        }

        public void MatchScoreCollectHandler()
        {
            //collected += 1;
            //if (collected <= 3) MPlayer.AddScore(10);
            //else MPlayer.AddScore(20);
        }
        #endregion gridcell handlers

        #region match group handlers score counter
        private void ComboCollectHandler(MatchGroupsHelper mgH)
        {
            // combo message
        }

        private void GreatingMessage()
        {
            if (WinContr.Result == GameResult.WinAuto) return; //  EffectsHolder.Instance.InstantiateScoreFlyerAtPosition(s, scoreFlyerPos, f);
            int add = (collected - 3) * 10;
            int score = collected * 10 + Math.Max(0, add);
            if (score > 59 && score < 89)
            {
                Debug.Log("GOOD");
                if (goodPrefab) goodPrefab.CreateWindowAndClose(1);
                GoodStepEvent?.Invoke();
            }
            else if (score > 89 && score < 119)
            {
                Debug.Log("GREAT");
                if (greatPrefab) greatPrefab.CreateWindowAndClose(1);
                GreatStepEvent?.Invoke();
            }
            else if (score > 119)
            {
                Debug.Log("EXCELLENT");
                if (excellentPrefab) excellentPrefab.CreateWindowAndClose(1);
                ExcellentStepEvent?.Invoke();
            }
        }

        private void GreatingMessage(int _scoreBeforeStep, int _scoreAfterStep)
        {
            int score = _scoreAfterStep - _scoreBeforeStep;
            int scoreOffset = 30;
            if (score > 59 + scoreOffset && score < 89 + scoreOffset)
            {
                Debug.Log("GOOD");
                if (goodPrefab) goodPrefab.CreateWindowAndClose(1);
                GoodStepEvent?.Invoke();
            }
            else if (score > 89 + scoreOffset && score < 119 + scoreOffset)
            {
                Debug.Log("GREAT");
                if (greatPrefab) greatPrefab.CreateWindowAndClose(1);
                GreatStepEvent?.Invoke();
            }
            else if (score > 119 + scoreOffset)
            {
                Debug.Log("EXCELLENT");
                if (excellentPrefab) excellentPrefab.CreateWindowAndClose(1);
                ExcellentStepEvent?.Invoke();
            }
        }

        /// <summary>
        /// async collect matched objects in a group
        /// </summary>
        /// <param name="completeCallBack"></param>
        internal void CollectHandler(MatchGroup m, Action completeCallBack)
        {
            float delay = 0;
            // Debug.Log("collect match group");

            SetHiddenObject(m.Cells);

            if (m.Length > 3 && m.BombsCount == 0 && bombCreator) // create bomb
            {
                BombCreator bC = Instantiate(bombCreator);
                bC.Create(bombType, m, showScore, scoreController.GetBombScore(m.Length), completeCallBack);
                ScoreHolder.Add(scoreController.GetBombScore(m.Length));
                return;
            }

            ParallelTween collectTween = new ParallelTween();
            foreach (GridCell c in m.Cells)
            {
                delay += collectDelay;
                float d = delay;
                collectTween.Add((callBack) => { c.CollectMatch(d, true, true, true, showBombExplode, showScore, MatchScore, callBack); });
            }
            ScoreHolder.Add(MatchScore * m.Length);
            collectTween.Start(completeCallBack);
        }

        internal void SetHiddenObject(List<GridCell> cells)
        {
            if (GOSet.HiddenObjects.Count == 0) return;
            HiddenObject hO = null;
            foreach (var item in cells)
            {
                hO = item.Hidden;
                if (hO) break;
            }

            if (hO)
            {
                foreach (var item in cells)
                {
                    if (!item.Hidden && !item.Overlay && !item.Underlay)
                    {
                        item.SetObject(GOSet.GetHiddenObject(hO.ID));
                    }
                }
            }
        }
        #endregion match group handlers score counter

        private void MakeStep()
        {
            List<GridCell> bombs = MainGrid.GetAllBombs(true); // 18.12.2023
            int count = bombs.Count;
            if (count > 0)
            {
                SetControlActivity(false, false);
                MbState = MatchBoardState.Waiting;
                ExplodeBombs(bombs, true, true, () =>
                {
                    WinContr.MakeMove(count);
                    MbState = MatchBoardState.Fill;
                    SetControlActivity(true, true);
                });
                return;
            }

            if (autoWin == AutoWin.Slow)        // not used
            {
                if (MatchGroups.EstimateMatchesLength > 0)
                {
                    MatchGroups.SwapEstimate();
                }
            }
            else if (autoWin == AutoWin.Fast)   // not used
            {
                SetControlActivity(false, false);
                int moves = WinContr.MovesRest;
                int bombsCount = Mathf.Min(moves, 3);
                List<GridCell> cells = MainGrid.GetRandomMatch(bombsCount);

                MbState = MatchBoardState.Waiting;
                if (cells.Count > 2)
                    BombObject.ExplodeArea(cells[2].GColumn.cells, 0, true, true, true, () =>
                    {
                        WinContr.MakeMove();
                    });

                if (cells.Count > 1)
                    BombObject.ExplodeArea(cells[1].GRow.cells, 0.05f, true, true, true, () =>
                   {
                       WinContr.MakeMove();
                   });

                BombObject.ExplodeArea(cells[0].GRow.cells, 0.15f, true, true, true, () =>
                {
                    // set 3 bombs
                    WinContr.MakeMove();
                    MbState = MatchBoardState.Fill; //    Debug.Log("end swap -> to fill");
                    SetControlActivity(true, true);
                });

            }
            else if (autoWin == AutoWin.Bombs)
            {
                Rockets();
            }
        }

        private void ExplodeBombs(List<GridCell> bombCells, bool playExplodeAnimation, bool showCollectPrefab, Action completeCallback)
        {
            if (skipWinShow) { completeCallback?.Invoke(); return; }

            ParallelTween pT = new ParallelTween();
            float delay = 0;
            foreach (var item in bombCells)
            {
                float _delay = delay;
                pT.Add((cB) =>
                {
                    if (item.GetBomb()) item.ExplodeBomb(_delay, playExplodeAnimation, showCollectPrefab, cB);
                });
                delay += 0.1f;
            }
            pT.Start(completeCallback);
        }

        public void Rockets()
        {
            if (skipWinShow) return;
            SetControlActivity(false, false);
            int moves = WinContr.MovesRest;
            int bombsCount = Mathf.Min(moves, 10); // ?????
            List<GridCell> cells = MainGrid.GetRandomMatch(bombsCount);
            cells.RemoveAll((c) => { return c.MatchProtected; });
            cells.Shuffle();
            bombsCount = cells.Count;

            MbState = MatchBoardState.Waiting;

            TweenSeq anim = new TweenSeq();
            ParallelTween pT = new ParallelTween();
            ParallelTween pT1 = new ParallelTween();

            anim.Add((callBack) =>
            {
                pT1.Start(callBack);
            });

            anim.Add((callBack) => // create rockets
            {
                foreach (var item in cells)
                {
                    BombDir bd = UnityEngine.Random.Range(0, 2) == 0 ? BombDir.Horizontal : BombDir.Vertical;
                    BombObject r = null;
                    switch (bombType)
                    {
                        case BombType.StaticMatch:
                            r = StaticMatchBombObject.Create(item, GOSet.GetStaticMatchBombObject(bd, item.Match.ID));
                            break;
                        case BombType.DynamicMatch:
                            r = DynamicMatchBombObject.Create(item, GOSet.GetDynamicMatchBombObject(bd, item.Match.ID));
                            r.SetToFront(true);
                            break;
                        case BombType.DynamicClick:
                            r = DynamicClickBombObject.Create(item, GOSet.GetDynamicClickBombObject(bd));
                            r.SetToFront(true);
                            break;
                        default:
                            r = DynamicClickBombObject.Create(item, GOSet.GetDynamicClickBombObject(bd));
                            r.SetToFront(true);
                            break;
                    }

                    pT.Add((cB) =>
                    {
                        item.ExplodeBomb(UnityEngine.Random.Range(0, 2f), true, true, cB);
                    });
                }
                callBack();
            });

            anim.Add((callBack) => // delay
            {
                TweenExt.DelayAction(gameObject, 1.0f, callBack);
            });

            anim.Add((callBack) =>
            {
                pT.Start(callBack);
            });

            anim.Add((callBack) =>
            {
                WinContr.MakeMove(bombsCount);
                MbState = MatchBoardState.Fill; //    Debug.Log("end swap -> to fill");
                SetControlActivity(true, true);
                callBack();
            });

            anim.Start();
        }

        public bool NeedAlmostMessage()
        {
            return showAlmostMessage && MGui.GetPopUpPrefabByDescription("almost");    // && (almostCoins <= CoinsHolder.Count)
        }

        #region undo
        ///// <summary>
        /////  Save Undo state of the match board
        ///// </summary>
        //internal void SaveUndoState()
        //{
        //    if (undoStates == null) undoStates = new List<DataState>();
        //    if (undoStates.Count >= 5)
        //    {
        //        undoStates.RemoveAt(0);
        //    }
        //    DataState ds = new DataState(this, MPlayer);
        //    undoStates.Add(ds);
        //    // Debug.Log("save undo state" + undoStates.Count);
        //    grid.Cells.ForEach((ct) => { ct.SaveUndoState(); });
        //}

        ///// <summary>
        ///// set the prev state on board
        ///// </summary>
        //public void PreviousState()
        //{
        //    if (GMode == GameMode.Edit) return;
        //    if (Time.timeScale == 0) return;
        //    if (undoStates == null || undoStates.Count == 0) return;
        //    currentGrid.Cells.ForEach((ct) => { ct.PreviousUndoState(); });
        //    DataState ds = undoStates[undoStates.Count - 1];
        //    ds.RestoreState(this, MPlayer);
        //    undoStates.RemoveAt(undoStates.Count - 1);
        //    HeaderGUIController.Instance.Refresh();
        //}

        /// <summary>
        /// Set Time.timescale =(Time.timescale!=0)? 0 : 1
        /// </summary>
        public void Pause()
        {
            if (GMode == GameMode.Edit) return;
            if (Time.timeScale == 0.0f)
            {
                Time.timeScale = 1f;
                SetControlActivity(true, true);
            }

            else if (Time.timeScale > 0f)
            {
                Time.timeScale = 0f;
                SetControlActivity(false, true);
            }
        }
        #endregion undo

        #region boosters
        /// <summary>
        /// Aplly active booster to gridcell
        /// </summary>
        /// <param name="gCell"></param>
        private void ApplyBooster(GridCell gCell)
        {
            collected = 0; // reset collected count
            if (Booster.ActiveBooster != null)
            {
                MbState = MatchBoardState.Waiting;
                SetControlActivity(false, false);
                Booster.ActiveBooster.ApplyToGridM(gCell, () => { MbState = MatchBoardState.Fill; });
            }
        }
        #endregion boosters

        public void ExplodeWave(float delay, Vector3 center, float radius, Action completeCallBack)
        {
            if (wave) return;
            wave = true;
            AnimationCurve ac = explodeCurve; //sineCurve;//
            ParallelTween pT = new ParallelTween();
            TweenSeq anim = new TweenSeq();
            float maxDist = radius * MainGrid.Step.x;
            float maxAmpl = 1.0f;
            float speed = 15f;
            float waveTime = 0.15f;
            // Debug.Log("WAVE");

            anim.Add((callBack) => // delay
            {
                SimpleTween.Value(gameObject, 0, 1, delay).AddCompleteCallBack(callBack);
            });

            MainGrid.Cells.ForEach((tc) =>
            {
                if (tc.DynamicObject)
                {
                    Vector3 tcPos = tc.transform.position;
                    Vector3 dir = tcPos - center;
                    float dirM = dir.magnitude;
                    dirM = (dirM < 1) ? 1 : dirM;
                    dirM = (dirM > maxDist) ? maxDist : dirM;
                    Vector3 dirOne = dir.normalized;
                    float b = maxAmpl;
                    float k = -maxAmpl / maxDist;
                    pT.Add((callBack) =>
                    {
                        SimpleTween.Value(gameObject, 0f, 1f, waveTime).SetOnUpdate((float val) =>
                        {
                            float deltaPos = ac.Evaluate(val);
                            if (tc.DynamicObject) tc.DynamicObject.transform.position = tcPos + dirOne * deltaPos * (k * dirM + b);// new Vector3(deltaPos, deltaPos, 0);
                        }).
                                                                AddCompleteCallBack(() =>
                                                                {
                                                                    if (tc.DynamicObject) tc.DynamicObject.transform.localPosition = Vector3.zero;
                                                                    callBack();
                                                                }).SetDelay(dirM / speed);
                    });
                }
            });

            pT.Start(() => { wave = false; completeCallBack?.Invoke(); });
        }

        public static void SetFieldBoosters(List<FieldBooster> fieldBoosters)
        {
            FieldBoosters = fieldBoosters;
        }

        public IEnumerator SetFieldBoostersC()
        {
            if (FieldBoosters == null) FieldBoosters = new List<FieldBooster>();
            List<FieldBooster> fieldBoosters = new List<FieldBooster>(FieldBoosters);
            FieldBoosters = new List<FieldBooster>(); // clean FieldBoostersArray

            // get cells with match object, without overlay and underlay
            List<GridCell> matchGridCells = MainGrid.GetRandomMatch(fieldBoosters.Count);

            // get cells with match object, without overlay but with underlay
            if (matchGridCells.Count < fieldBoosters.Count)
            {
                int count = fieldBoosters.Count - matchGridCells.Count;
                matchGridCells.AddRange ( MainGrid.GetRandomMatch(count, true, false));
            }

            // get cells with match object, overlay and with underlay
            if (matchGridCells.Count < fieldBoosters.Count)
            {
                int count = fieldBoosters.Count - matchGridCells.Count;
                matchGridCells.AddRange(MainGrid.GetRandomMatch(count, false, false));
            }

            List<GridCell> _gCellsNO = new List<GridCell>();
            List<GridCell> _gCellsWO = new List<GridCell>();
            // get blocked cells
            if (matchGridCells.Count < fieldBoosters.Count)
            {
                int count = fieldBoosters.Count - matchGridCells.Count;
                foreach (var item in MainGrid.Cells)
                {
                    if(item.Blocked && item.Blocked.fieldBoosterReplaceable)
                    {
                        if (!item.Overlay) _gCellsNO.Add(item);
                        else _gCellsWO.Add(item);
                    }
                    else if (item.DynamicBlocker && item.DynamicBlocker.fieldBoosterReplaceable)
                    {
                        if (!item.Overlay) _gCellsNO.Add(item);
                        else _gCellsWO.Add(item);
                    }
                }
                _gCellsNO.Shuffle();
                _gCellsWO.Shuffle();

                matchGridCells.AddRange(_gCellsNO);
                matchGridCells.AddRange(_gCellsWO);
            }

            int cCount = (int)MathF.Min(matchGridCells.Count, fieldBoosters.Count);

            // set in match cell, blocked, blocked with overlay
            for (int i = 0; i < cCount; i++)
            {
                int index = i;
                fieldBoosters[index].AddCount(-1);
                fieldBoosters[index].AnimateObject(SortingOrder.BoosterToFront, new Vector3(0, -6, 0), matchGridCells[index].transform.position, null, () => 
                {
                    if (matchGridCells[index].Match) matchGridCells[index].CollectMatch(0, true, false, false, false, false, 0, null);
                    else if (matchGridCells[index].Blocked) matchGridCells[index].Blocked.ForceCollect();
                    else if (matchGridCells[index].DynamicBlocker) matchGridCells[index].DynamicBlocker.ForceCollect();

                    matchGridCells[index].SetObject(fieldBoosters[index].gridObjectPrefab.ID); 
                });
                yield return new WaitForSeconds(0.6f);
            }

            fieldBoosterOk = true;
        }

        public Vector3 GetGuiTargetPosW(int targetID)
        {
            if (!headerGUI) headerGUI = FindAnyObjectByType<HeaderGUIController>();
            if (headerGUI)
            {
                GUIObjectTargetHelper gO = headerGUI.GetTargetGuiObject(targetID);
                if (gO) return gO.transform.position;
            }
            return FlyTarget;
        }

        public Transform GetGuiTarget(int targetID)
        {
            if (!headerGUI) headerGUI = FindAnyObjectByType<HeaderGUIController>();
            if (headerGUI)
            {
                GUIObjectTargetHelper gO = headerGUI.GetTargetGuiObject(targetID);
                if (gO) return gO.transform;
            }
            return (flyTarget) ? flyTarget.transform : null;
        }

        public void SkipWinShow()
        {
            if (!skipWinShow)
            {
                skipWinShow = true;
                StopAllCoroutines();
                SimpleTween.ForceCancelAll();
                SkipWinShowEvent?.Invoke(this);
            }
        }
    }

    public class MatchGroupsHelper
    {
        public List<MatchGroup> mgList;
        public List<EstimateMatchGroup> emgList;
        private MatchGrid grid;
        public Action<MatchGroupsHelper> ComboCollect;
        public Action<MatchGroup, Action> Collect;

        public int MatchesLength
        {
            get { return mgList.Count; }
        }

        public int EstimateMatchesLength
        {
            get { return emgList.Count; }
        }

        /// <summary>
        /// Find match croups on grid and estimate match groups
        /// </summary>
        public MatchGroupsHelper(MatchGrid grid)
        {
            emgList = new List<EstimateMatchGroup>();
            mgList = new List<MatchGroup>();
            this.grid = grid;
            CreateEstimateMasks();
            CreateIsolateMatchMasks();
        }

        public void CreateGroups(int minMatches)
        {
            mgList = new List<MatchGroup>();

            #region search isolate groups
            List<MatchGroup>  isolateList = new List<MatchGroup>();
            foreach (var item in isMasksP00)
            {
                isolateList.AddRange(GetIsolateMatchesForMask(grid, item));
            }
            // Debug.Log("isolateList.Count : " + isolateList.Count);
            foreach (var item in isMasksP10)
            {
                isolateList.AddRange(GetIsolateMatchesForMask(grid, item));
            }
            //Debug.Log("isolateList.Count +p10: " + isolateList.Count);
            /*
            foreach (var item in isolateList)
            {
                Debug.Log("isolate match: " + item.GetGroupType());
            }
            */
            #endregion search isolate groups

            Func<MatchGroup,  bool> intersectWithIsolate = (mg) =>
           {
               foreach (var item in isolateList)
               {
                   if (mg.IsIntersectWithGroup(item)) return true;
               }
               return false;
           };

            foreach (var br in grid.Rows)
            {
                List<MatchGroup> mgList_t = br.GetMatches(minMatches, false);
                if (mgList_t != null && mgList_t.Count > 0)
                {
                    for (int i = 0; i < mgList_t.Count; i++)
                    {
                        if (!intersectWithIsolate(mgList_t[i])) Add(mgList_t[i]);
                    }
                }
            }
            //grid.Rows.ForEach((br) =>
            //{

            //});

            foreach (var bc in grid.Columns)
            {
                List<MatchGroup> mgList_t = bc.GetMatches(minMatches, false);
                if (mgList_t != null && mgList_t.Count > 0)
                {
                    for (int i = 0; i < mgList_t.Count; i++)
                    {
                        if (!intersectWithIsolate(mgList_t[i])) Add(mgList_t[i]);
                    }
                }
            }

            //grid.Columns.ForEach((bc) =>
            //{
               
            //});

            AddRange(isolateList);
        }

        /// <summary>
        /// Add new matchgroup and merge all intersections
        /// </summary>
        public void Add(MatchGroup mG)
        {
            List<MatchGroup> intersections = new List<MatchGroup>();

            for (int i = 0; i < mgList.Count; i++)
            {
                if (mgList[i].IsIntersectWithGroup(mG))
                {
                    intersections.Add(mgList[i]);
                }
            }
            // merge intersections
            if (intersections.Count > 0)
            {
                intersections.ForEach((ints) => { mgList.Remove(ints); });
                intersections.Add(mG);
                mgList.Add(Merge(intersections));
            }
            else
            {
                mgList.Add(mG);
            }
        }

        /// <summary>
        /// Add new estimate matchgroup
        /// </summary>
        public void AddEstimate(EstimateMatchGroup mGe)
        {
            for (int i = 0; i < emgList.Count; i++)
            {
                if (emgList[i].IsEqualTo(mGe))
                {
                    return;
                }
            }
            emgList.Add(mGe);
        }

        /// <summary>
        /// Add new matchgroup List and merge all intersections
        /// </summary>
        public void AddRange(List<MatchGroup> mGs)
        {
            for (int i = 0; i < mGs.Count; i++)
            {
                Add(mGs[i]);
            }
        }

        private MatchGroup Merge(List<MatchGroup> intersections)
        {
            MatchGroup mG = new MatchGroup();
            foreach (var ints in intersections)
            {
                mG.Merge(ints);
            }
            // intersections.ForEach((ints) => {  });
            return mG;
        }

        TweenSeq showSequence;
        public void ShowMatchGroupsSeq(Action completeCallBack)
        {
            showSequence = new TweenSeq();
            if (mgList.Count > 0)
            {
                Debug.Log("show match");
                foreach (MatchGroup mG in mgList)
                {
                    showSequence.Add((callBack) =>
                    {
                        mG.Show(callBack);
                    });
                }
            }
            showSequence.Add((callBack) =>
            {
                if (completeCallBack != null) completeCallBack();
                Debug.Log("show match ended");
                callBack();
            });
            showSequence.Start();
        }

        public void ShowMatchGroupsPar(Action completeCallBack)
        {
            showSequence = new TweenSeq();
            ParallelTween showTweenPar = new ParallelTween();

            if (mgList.Count > 0)
            {
                //  Debug.Log("show match");
                foreach (MatchGroup mG in mgList)
                {
                    showTweenPar.Add((callBack) =>
                    {
                        mG.Show(callBack);
                    });
                }
            }

            showSequence.Add((callBack) =>
            {
                showTweenPar.Start(callBack);
            });

            showSequence.Add((callBack) =>
            {
                if (completeCallBack != null) completeCallBack();
                // Debug.Log("show match ended");
                callBack();
            });
            showSequence.Start();
        }

        TweenSeq showEstimateSequence;
        public void ShowEstimateMatchGroupsSeq(Action completeCallBack)
        {
            showEstimateSequence = new TweenSeq();
            if (emgList.Count > 0)
            {
                foreach (EstimateMatchGroup mG in emgList)
                {
                    showEstimateSequence.Add((callBack) => { mG.Show(callBack); });
                }
            }
            showEstimateSequence.Add((callBack) =>
            {
                completeCallBack?.Invoke();
                callBack();
            });
            showEstimateSequence.Start();
        }

        static int next = 0;
        public void ShowNextEstimateMatchGroups(Action completeCallBack)
        {
            showEstimateSequence = new TweenSeq();
            next = (next < emgList.Count) ? next : 0;
            int n = next;
            // Debug.Log("show next estimate: " + n + "; emgList[n].Cells.Count: " + emgList[n].Cells.Count);
            if (emgList.Count > 0)
            {
                showEstimateSequence.Add((callBack) => { emgList[n].Show(callBack); });
            }
            showEstimateSequence.Add((callBack) =>
            {
                completeCallBack?.Invoke();
                // Debug.Log("end show next estimate: " + n);
                callBack();
            });
            showEstimateSequence.Start();
            next++;
        }

        public void CollectMatchGroups(Action completeCallBack)
        {
            ParallelTween pt = new ParallelTween();

            if (mgList.Count == 0)
            {
                completeCallBack?.Invoke();
                return;
            }

            for (int i = 0; i < mgList.Count; i++)
            {
                if (mgList[i] != null)
                {
                    MatchGroup m = mgList[i];
                    pt.Add((callBack) =>
                    {
                        Collect(m, callBack);
                    });
                }
            }
            pt.Start(() =>
            {
                if (mgList.Count > 1) ComboCollect?.Invoke(this);
                completeCallBack?.Invoke();
            });
        }

        public override string ToString()
        {
            string s = "";
            mgList.ForEach((mg) => { s += mg.ToString(); });
            return s;
        }

        public void CancelTweens()
        {
            if (showSequence != null) { showSequence.Break(); showSequence = null; }
            if (showEstimateSequence != null) { showEstimateSequence.Break(); showEstimateSequence = null; }
            mgList.ForEach((mg) => { mg.CancelTween(); });
            emgList.ForEach((mg) => { mg.CancelTween(); });
        }

        public void SwapEstimate()
        {
            emgList[0].SwapEstimate();
        }

        #region isolate match masks arrays
        // these matches are isolated and have no overlap with others
        // 1 - main match object (kernel)
        // 3 - not match object, 4 - match or not match object,
        // 5 - match, but not collected (imm_00 - 29.01.2024)
        // random rocket 
        private static int[,] imm_0 = { { 4, 3, 3, 3, 4 },      
                                        { 3, 1, 1, 1, 3 },
                                        { 3, 1, 1, 3, 4 },
                                        { 4, 3, 3, 4, 4 },
        };

        // random rocket 
        private static int[,] imm_00 = { { 4, 4, 4, 3, 4 },
                                         { 4, 3, 3, 5, 4 },
                                         { 3, 1, 1, 1, 3 },
                                         { 3, 1, 1, 3, 4 },
                                         { 4, 3, 3, 4, 4 },
        };

        // random rocket 
        private static int[,] imm_10 = { { 4, 3, 3, 4 },
                                         { 3, 1, 1, 3 },
                                         { 3, 1, 1, 3 },
                                         { 4, 3, 3, 4 },
        };
        #endregion isolate match masks arrays

        #region create isolate match masks
        // priority 00
        public static List<IsolateMask> isMasksP00;
        // priority 10
        public static List<IsolateMask> isMasksP10;

        private void CreateIsolateMatchMasks()
        {
            // priority 0
            isMasksP00 = toIsolateList(createMaskVariants(imm_0, 0));
            Debug.Log("add imm_0 -> isMasksP00.Count: " + isMasksP00.Count);
            isMasksP00.AddRange(toIsolateList(createMaskVariants(imm_00, 0)));   // isMasksP00 = toIsolateList(createMaskVariants(imm_00, 0)); // 29.01.2024
            Debug.Log("add imm_00 -> isMasksP00.Count: " + isMasksP00.Count);

            // priority 10
            isMasksP10 = toIsolateList(createMaskVariants(imm_10, 10));
            Debug.Log("add imm_10 -> isMasksP10.Count: " + isMasksP10.Count);
        }

        #endregion create isolate match masks

        #region estimate match masks arrays
        // 0 - to position
        // 1 - main match object (kernel)
        // 2 - from position
        // 4 - possible match postion
        // 3 - not match object, 
        // 5 - not collected match postion
        // these masks only use 0, 1, 2 codes

        private static int[,] t7_00 = { { 3, 3, 2, 3, 3 },  
                                        { 1, 1, 0, 1, 1 },
                                        { 3, 3, 1, 3, 3 },
                                        { 3, 3, 1, 3, 3 },
        };
      
        private static int[,] t6_10 = { { 3, 3, 2, 3 },
                                        { 1, 1, 0, 1 },
                                        { 3, 3, 1, 3 },
                                        { 3, 3, 1, 3 },
        };

        public static int[,] rr6_15 =  { { 3, 1, 1, 3 },  // don't use it yet, may be random rocket or radial bomb 
                                         { 2, 0, 1, 1 },
                                         { 3, 1, 3, 3 },
        };

        public static int[,] h5_20 =  { { 3, 3, 2, 3, 3 },  
                                        { 1, 1, 0, 1, 1 },
                                        { 3, 3, 2, 3, 3 },
        };

        public static int[,] l5_30 = { { 3, 2, 3, 3 },    
                                       { 2, 0, 1, 1 },
                                       { 3, 1, 3, 3 },
                                       { 3, 1, 3, 3 },
        };

        public static int[,] t5_30 = { { 3, 2, 3 },
                                       { 1, 0, 1 },
                                       { 3, 1, 3 },
                                       { 3, 1, 3 },
        };

        public static int[,] rr5_35 = { { 3, 2, 3, 3 },
                                        { 2, 0, 1, 1 },
                                        { 3, 1, 1, 3 },
                                        { 3, 3, 3, 3 },
        };

        public static int[,] h4_40 = { { 3, 2, 3, 3 },    
                                       { 1, 0, 1, 1 },
                                       { 3, 2, 3, 3 },

        };

        public static int[,] rr4_45 = { { 3, 2, 3, 3 },
                                        { 2, 0, 1, 3 },
                                        { 3, 1, 1, 3 },
                                        { 3, 3, 3, 3 },
        };

        public static int[,] h3_50 = { { 3, 3, 2 },      
                                       { 1, 1, 0 },
                                       { 3, 3, 2 },

        };

        public static int[,] v3_50 = { { 3, 1, 3 },      
                                       { 3, 1, 3 },
                                       { 3, 0, 3 },
                                       { 3, 2, 3 },

        };

        public static int[,] hm3_50 = { { 3, 2, 3 },      
                                        { 1, 0, 1 },
                                        { 3, 2, 3 },

        };
        #endregion estimate match masks arrays

        #region create estimate match masks

        // priority 00
        public static List<EstimateMask> estMasksP00;

        // priority 10
        public static List<EstimateMask> estMasksP10;

        // priority 20
        public static List<EstimateMask> estMasksP20;

        // priority 30
        public static List<EstimateMask> estMasksP30;

        // priority 35
        public static List<EstimateMask> estMasksP35;

        // priority 40
        public static List<EstimateMask> estMasksP40;

        // priority 45
        public static List<EstimateMask> estMasksP45;

        // priority 50
        public static List<EstimateMask> estMasksP50;

        private void CreateEstimateMasks()
        {
/*
            Func<EstimateMask, List<EstimateMask>, bool> containEqualMask = (eMask, list) =>
            {
                foreach (var item in list)
                {
                    if (eMask.IsEqualTo(item)) return true;
                }
                return false;
            };

            Func<int[,], int, List<EstimateMask>> createMaskVariants = (eMask, priority) =>
            {
                List<EstimateMask> result = new List<EstimateMask>();

                // mask + 3 rotations
                result.Add(new EstimateMask(eMask, priority));
                for (int i = 0; i < 3; i++)
                {
                    EstimateMask em = new EstimateMask(result.Last().mask.CWRotateArray2D(), priority);
                    if (!containEqualMask(em, result)) result.Add(em);

                }

                // horizontal mirror mask + 3 rotations
                EstimateMask emm = new EstimateMask(eMask.CopyAndReverseColumns2D(), priority);
                if (!containEqualMask(emm, result)) result.Add(emm);
                for (int i = 0; i < 3; i++)
                {
                    EstimateMask em = new EstimateMask(result.Last().mask.CWRotateArray2D(), priority);
                    if (!containEqualMask(em, result)) result.Add(em);

                }

                // vertical mirror mask + 3 rotations
                emm = new EstimateMask(eMask.CopyAndReverseRows2D(), priority);
                if (!containEqualMask(emm, result)) result.Add(emm);
                for (int i = 0; i < 3; i++)
                {
                    EstimateMask em = new EstimateMask(result.Last().mask.CWRotateArray2D(), priority);
                    if (!containEqualMask(em, result)) result.Add(em);

                }

                Debug.Log("priority: " + priority + "; masks.count: " + result.Count);
                return result;
            };
*/
            // priority 0
            estMasksP00 =toEsimateList(createMaskVariants(t7_00, 0)); // 4

            // priority 10
            estMasksP10 = toEsimateList(createMaskVariants(t6_10, 10)); // 8

            // priority 20
            estMasksP20 = toEsimateList(createMaskVariants(h5_20, 20)); // 2

            // priority 30
            estMasksP30 = toEsimateList(createMaskVariants(l5_30, 30)); // 4
            estMasksP30.AddRange(toEsimateList(createMaskVariants(t5_30, 30))); // 4

            // priority 35
            estMasksP35 = toEsimateList(createMaskVariants(rr5_35, 35)); // 8

            // priority 40
            estMasksP40 = toEsimateList(createMaskVariants(h4_40, 40)); // 4

            // priority 45
            estMasksP45 = toEsimateList(createMaskVariants(rr4_45, 45)); // 4

            // priority 50
            estMasksP50 = toEsimateList(createMaskVariants(h3_50, 50)); // 4
            estMasksP50.AddRange(toEsimateList(createMaskVariants(v3_50, 50))); // 4
            estMasksP50.AddRange(toEsimateList(createMaskVariants(hm3_50, 50))); // 2
        }
        #endregion create estimate match masks

        #region helper actions
        static Func<MatchMask, List<MatchMask>, bool> containEqualMask = (eMask, list) =>
        {
            foreach (var item in list)
            {
                if (eMask.IsEqualTo(item)) return true;
            }
            return false;
        };

        static Func<List<MatchMask>, List<EstimateMask>> toEsimateList = (source) =>
        {
            List<EstimateMask> result = new List<EstimateMask>();
            foreach (var item in source)
            {
                result.Add(new EstimateMask(item));
            }
            return result;
        };

        static Func<List<MatchMask>, List<IsolateMask>> toIsolateList = (source) =>
        {
            List<IsolateMask> result = new  List<IsolateMask>();
            foreach (var item in source)
            {
                result.Add(new IsolateMask(item));
            }
            return result;
        };

        static Func<int[,], int, List<MatchMask>> createMaskVariants = (eMask, priority) =>
        {
            List<MatchMask> result = new List<MatchMask>();

            // mask + 3 rotations
            result.Add(new MatchMask(eMask, priority));
            for (int i = 0; i < 3; i++)
            {
                MatchMask em = new MatchMask(result.Last().mask.CWRotateArray2D(), priority);
                if (!containEqualMask(em, result)) result.Add(em);

            }

            // horizontal mirror mask + 3 rotations
            MatchMask emm = new MatchMask(eMask.CopyAndReverseColumns2D(), priority);
            if (!containEqualMask(emm, result)) result.Add(emm);
            for (int i = 0; i < 3; i++)
            {
                MatchMask em = new MatchMask(result.Last().mask.CWRotateArray2D(), priority);
                if (!containEqualMask(em, result)) result.Add(em);

            }
            /*
                            // vertical mirror mask + 3 rotations
                            emm = new MatchMask(eMask.CopyAndReverseRows2D(), priority);
                            if (!containEqualMask(emm, result)) result.Add(emm);
                            for (int i = 0; i < 3; i++)
                            {
                                MatchMask em = new MatchMask(result.Last().mask.CWRotateArray2D(), priority);
                                if (!containEqualMask(em, result)) result.Add(em);

                            }
            */
          //  Debug.Log("priority: " + priority + "; masks.count: " + result.Count);
            return result;
        };

        static Func<EstimateMatchGroup, List<EstimateMatchGroup>, bool> containEqual = (emg, toArray) =>
        {
            foreach (var item in toArray)
            {
                if (emg.IsEqualTo(item)) return true;
            }
            return false;
        };

        static Action<List<EstimateMatchGroup>, List<EstimateMatchGroup>> addEqualPriorityArray = (from, to) =>
        {
            foreach (var item in from)
            {
                if (!containEqual(item, to)) to.Add(item);
            }
        };

        static Func<EstimateMatchGroup, List<EstimateMatchGroup>, bool> containAsPartLowerPriority = (emgLP, toArray) =>
        {
            foreach (var item in toArray)
            {
                if (emgLP.maskPriority > item.maskPriority && item.IncludeAllMainCellsFrom(emgLP)) return true;
            }
            return false;
        };

        static Action<List<EstimateMatchGroup>, List<EstimateMatchGroup>> addLowerPriorityArray = (from, to) =>
        {
            foreach (var item in from)
            {
                if (!containAsPartLowerPriority(item, to)) to.Add(item);
            }
        };
        #endregion helper actions

        public void CreateEstimateGroups()
        {
            emgList = new List<EstimateMatchGroup>();
            List<EstimateMatchGroup> matchGroupsP00 = new List<EstimateMatchGroup>();
            List<List<EstimateMatchGroup>> lowerPriority = new List<List<EstimateMatchGroup>>();
            List<EstimateMatchGroup> matchGroupsLP;

            // priority 0
            foreach (var em in estMasksP00)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid,em), matchGroupsP00);
            }

            // lower priority

            // priority 10
            matchGroupsLP = new List<EstimateMatchGroup>();
            foreach (var em in estMasksP10)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid, em), matchGroupsLP);
            }
            lowerPriority.Add(matchGroupsLP);

            // priority 20
            matchGroupsLP = new List<EstimateMatchGroup>();
            foreach (var em in estMasksP20)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid, em), matchGroupsLP);
            }
            lowerPriority.Add(matchGroupsLP);

            // priority 30
            matchGroupsLP = new List<EstimateMatchGroup>();
            foreach (var em in estMasksP30)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid, em), matchGroupsLP);
            }
            lowerPriority.Add(matchGroupsLP);

            // priority 35
            matchGroupsLP = new List<EstimateMatchGroup>();
            foreach (var em in estMasksP35)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid, em), matchGroupsLP);
            }
            lowerPriority.Add(matchGroupsLP);

            // priority 40
            matchGroupsLP = new List<EstimateMatchGroup>();
            foreach (var em in estMasksP40)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid, em), matchGroupsLP);
            }
            lowerPriority.Add(matchGroupsLP);

            // priority 45
            matchGroupsLP = new List<EstimateMatchGroup>();
            foreach (var em in estMasksP45)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid, em), matchGroupsLP);
            }
            lowerPriority.Add(matchGroupsLP);

            // priority 50
            matchGroupsLP = new List<EstimateMatchGroup>();
            foreach (var em in estMasksP50)
            {
                addEqualPriorityArray(GetEstimateMatchesForMask(grid, em), matchGroupsLP);
            }
            lowerPriority.Add(matchGroupsLP);

            // add created groups to emgList
            emgList.AddRange(matchGroupsP00);

            for (int i = lowerPriority.Count-1; i >=1; i--)
            {
                addLowerPriorityArray(lowerPriority[i], lowerPriority[i - 1]);
            }
            addLowerPriorityArray(lowerPriority[0], emgList);

           // Debug.Log("estimates count: " + emgList.Count);
        }

        private static List<EstimateMatchGroup> GetEstimateMatchesForMask(MatchGrid grid, EstimateMask eMask)
        {
            List<EstimateMatchGroup> result = new List<EstimateMatchGroup>();
            int mRows = eMask.mask.GetLength(0);
            int mCols = eMask.mask.GetLength(1);

            int gRows = grid.Rows.Count;
            int gCols = grid.Columns.Count;

            // check: whether the result already contains this group
            Func<EstimateMatchGroup, bool> contain = (emg) => 
            {
                foreach (var item in result)
                {
                    if (emg.IsEqualTo(item)) return true;
                }
                return false; 
            };

            for (int gr = 0 - mRows + 1; gr < gRows; gr++)
            {
                for (int gc = 0 - mCols + 1; gc < gCols; gc++)
                {
                    EstimateMatchGroup mg = new EstimateMatchGroup();
                    bool failed = false;

                    // check main id cells
                    int _r = eMask.mainMatch[0].x + gr;
                    int _c = eMask.mainMatch[0].y + gc;
                    int _id = 0;
                    if (grid[_r, _c] && grid[_r, _c].IsMatchable)
                    {
                        _id = grid[_r, _c].Match.ID;
                        mg.Add(grid[_r, _c]);
                    }

                    for (int i = 1; i < eMask.mainMatch.Count; i++)
                    {
                        _r = eMask.mainMatch[i].x + gr;
                        _c = eMask.mainMatch[i].y + gc;
                        if (grid[_r, _c] && grid[_r, _c].IsMatchable && grid[_r, _c].Match.ID == _id)
                        {
                            mg.Add(grid[_r, _c]);
                        }
                        else
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (failed) continue;

                    // check to cell
                   
                    _r = eMask.to.x + gr;
                    _c = eMask.to.y + gc;
                    GridCell toCell = grid[_r, _c];
                    if (!toCell || !toCell.IsMatchable || toCell.Match.ID == _id) continue;

                    // check from cells - only once must have main ID
                    List<GridCell> from = new List<GridCell>();
                    for (int i = 0; i < eMask.from.Count; i++)
                    {
                        _r = eMask.from[i].x + gr;
                        _c = eMask.from[i].y + gc;
                        if(grid[_r, _c] && grid[_r, _c].IsMatchable && grid[_r, _c].Match.ID == _id)
                        {
                            from.Add(grid[_r, _c]);
                        }
                    }

                    // create estimate group from "from" cell
                    foreach (var fc in from)
                    {
                        EstimateMatchGroup m = new EstimateMatchGroup();
                        m.AddRange(mg.Cells);
                        m.toCell = toCell;
                        m.fromCell = fc;
                        m.maskPriority = eMask.priority;
                        if(!contain(m)) result.Add(m);
                    }
                }
            }
            return result;
        }

        private static List<MatchGroup> GetIsolateMatchesForMask(MatchGrid grid, IsolateMask eMask)
        {
            List<MatchGroup> result = new List<MatchGroup>();
            int mRows = eMask.mask.GetLength(0);
            int mCols = eMask.mask.GetLength(1);

            int gRows = grid.Rows.Count;
            int gCols = grid.Columns.Count;

            // check: whether the result already contains this group
            Func<MatchGroup, bool> contain = (emg) =>
            {
                foreach (var item in result)
                {
                    if (emg.IsEqualTo(item)) return true;
                }
                return false;
            };

            for (int gr = 0 - mRows + 1; gr < gRows + mRows - 1; gr++)   // 29.01.2024 + mRows - 1
            {
                for (int gc = 0 - mCols + 1; gc < gCols + mCols - 1; gc++)          // 29.01.2024 + mCols - 1;
                {
                    MatchGroup mg = new MatchGroup();
                 
                    bool failed = false;

                    // check main id cells
                    int _r = eMask.mainMatch[0].x + gr;     // row main cell on board
                    int _c = eMask.mainMatch[0].y + gc;     // column main cell on board
                    // get main ID from board
                    int _id = 0; 
                    if (grid[_r, _c] && grid[_r, _c].IsMatchable)
                    {
                        _id = grid[_r, _c].Match.ID;
                        mg.Add(grid[_r, _c]);
                    }
                    else
                    {
                        failed = true;
                    }
                    if (failed) continue;

                    // check main match cells
                    for (int i = 1; i < eMask.mainMatch.Count; i++)
                    {
                        _r = eMask.mainMatch[i].x + gr;
                        _c = eMask.mainMatch[i].y + gc;
                        if (grid[_r, _c] && grid[_r, _c].IsMatchable && grid[_r, _c].Match.ID == _id)
                        {
                            mg.Add(grid[_r, _c]);
                        }
                        else // main match not exist
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (failed) continue;

                    // check not collected match cells
                    for (int i = 0; i < eMask.ncMatch.Count; i++)
                    {
                        _r = eMask.ncMatch[i].x + gr;
                        _c = eMask.ncMatch[i].y + gc;
                        if (grid[_r, _c] && grid[_r, _c].IsMatchable && grid[_r, _c].Match.ID == _id)
                        {
                           
                        }
                        else // nc match not exist
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (failed) continue;


                    // check not Match cells - since the mask must be insulated

                    for (int i = 0; i < eMask.notMatch.Count; i++)
                    {
                        _r = eMask.notMatch[i].x + gr;
                        _c = eMask.notMatch[i].y + gc;
                        if (grid[_r, _c] && grid[_r, _c].IsMatchable && grid[_r, _c].Match.ID == _id)
                        {
                            failed = true;
                            break;
                        }
                    }
                    if (failed) continue;
                    else 
                    {
                        mg.SetGroupType (mg.Length==4 ?  MatchGroupType.Rect : MatchGroupType.ExtRect);
                        result.Add(mg);
                    }
                }
            }
            return result;
        }
    }

    public class MatchGroup : CellsGroup
    {
        private MatchGroupType matchGroupType = MatchGroupType.None;

        public bool IsIntersectWithGroup(MatchGroup mGroup)
        {
            if (mGroup == null || mGroup.Length == 0) return false;
            for (int i = 0; i < Cells.Count; i++)
            {
                if (mGroup.Contain(Cells[i])) return true;
            }
            return false;
        }

        public void Merge(MatchGroup mGroup)
        {
            if (mGroup == null || mGroup.Length == 0) return;
            for (int i = 0; i < mGroup.Cells.Count; i++)
            {
                Add(mGroup.Cells[i]);
            }
        }

        public bool IsEqualTo(MatchGroup mGroup)
        {
            if (Length != mGroup.Length) return false;
            foreach (GridCell c in Cells)
            {
                if (!mGroup.Contain(c)) return false;
            }
            return true;
        }

        public void SetGroupType(MatchGroupType matchGroupType)
        {
            this.matchGroupType = matchGroupType;
        }

        internal MatchGroupType GetGroupType()
        {
            if (matchGroupType != MatchGroupType.None) return matchGroupType;
            if (Length == 4 && IsHorizonal()) // hor4
            {
                return MatchGroupType.Hor4;
            }
            else if (Length == 4 && IsVertical()) // vert4
            {
                return MatchGroupType.Vert4;
            }
            else if (Length == 5 && IsVertical()) // vert5
            {
                return MatchGroupType.Vert5;
            }
            else if (Length == 5 && IsHorizonal()) // hor5
            {
                return MatchGroupType.Hor5;
            }
            else if (Length == 5) // LT
            {
                return MatchGroupType.LT;
            }
            else if (Length == 6) // MiddleLT
            {
                return MatchGroupType.MiddleLT;
            }
            else if (Length == 7) // BigLT
            {
                return MatchGroupType.BigLT;
            }
            return MatchGroupType.Simple;
        }
    }

    public class EstimateMatchGroup : CellsGroup
    {
        public int maskPriority;
        public GridCell toCell;  // to cell
        public GridCell fromCell;  // from cell

        private List<GridCell> _cells; // Cells + est2

        /// <summary>
        /// check intesection main cells array
        /// </summary>
        /// <param name="mGroup"></param>
        /// <returns></returns>
        public bool IsIntersectWithGroup(EstimateMatchGroup mGroup)
        {
            if (mGroup == null || mGroup.Length == 0) return false;
            for (int i = 0; i < Cells.Count; i++)
            {
                if (mGroup.Contain(Cells[i])) return true;
            }
            return false;
        }

        public bool IsMainCellsEqual(EstimateMatchGroup mGroup)
        {
            if (Length != mGroup.Length) return false;
            foreach (GridCell c in Cells)
            {
                if (!mGroup.Contain(c)) return false;
            }
            return true;
        }

        public bool IncludeAllMainCellsFrom(EstimateMatchGroup mGroup)
        {
            foreach (GridCell c in mGroup.Cells)
            {
                if (!Cells.Contains(c)) return false;
            }
            return true;
        }

        public bool IsEqualTo(EstimateMatchGroup mGroup)
        {
            if (Length != mGroup.Length) return false;
            if (mGroup.toCell != toCell || mGroup.fromCell != fromCell) return false;
            foreach (GridCell c in Cells)
            {
                if (!mGroup.Contain(c)) return false;
            }
            return true;
        }

        internal void SwapEstimate()
        {
            if (toCell && fromCell)
            {
                //Debug.Log("swap estimate");
                //est1.Swap(est2.Match);
                SwapHelper.Swap(toCell, fromCell);
            }
        }

        internal MatchGroupType GetGroupType()
        {
            if (Length == 4 && IsHorizonal()) // hor4
            {
                return MatchGroupType.Hor4;
            }
            else if (Length == 4 && IsVertical()) // vert4
            {
                return MatchGroupType.Vert4;
            }
            else if (Length == 5 && IsVertical()) // vert5
            {
                return MatchGroupType.Vert5;
            }
            else if (Length == 5 && IsHorizonal()) // hor5
            {
                return MatchGroupType.Hor5;
            }
            else if (Length == 5) // LT
            {
                return MatchGroupType.LT;
            }
            else if (Length == 6) // MiddleLT
            {
                return MatchGroupType.MiddleLT;
            }
            else if (Length == 7) // BigLT
            {
                return MatchGroupType.BigLT;
            }
            return MatchGroupType.Simple;
        }

        /// <summary>
        /// Scaling sequenced group
        /// </summary>
        /// <param name="completeCallBack"></param>
        internal override void Show(Action completeCallBack)
        {
            ParallelTween showTween = new ParallelTween();
            _cells = new List<GridCell>(Cells);
            _cells.Add(fromCell);
            foreach (GridCell gc in _cells)
            {
                showTween.Add((callBack) =>
                {
                    gc.ZoomMatch(callBack);
                });
            }
            showTween.Start(completeCallBack);
        }

        public override void CancelTween()
        {
            if(_cells!=null)
            _cells.ForEach((c) => { c.CancelTween(); });
        }
    }

    public class CellsGroup
    {
        public List<GridCell> Cells { get; private set; }
        public List<GridCell> Bombs { get; private set; }
        public int lastObjectOrderNumber;
        public int lastMatchedID { get; private set; }
        public GridCell lastAddedCell { get; private set; }
        public GridCell lastMatchedCell { get; private set; }

        public int MinYPos
        {
            get; private set;
        }

        public bool Contain(GridCell mCell)
        {
            return Cells.Contains(mCell);
        }

        public int Length
        {
            get { return Cells.Count; }
        }

        public CellsGroup()
        {
            Cells = new List<GridCell>();
            Bombs = new List<GridCell>();
            MinYPos = -1;
        }

        public void Add(GridCell mCell)
        {
            if (!Cells.Contains(mCell))
            {
                Cells.Add(mCell);
                MinYPos = (mCell.Row < MinYPos) ? mCell.Row : MinYPos;
                lastAddedCell = mCell;
                lastMatchedCell = (lastMatchedCell == null || lastMatchedCell.Match == null) ? mCell : lastMatchedCell;

                if (mCell.Match)
                {
                    lastObjectOrderNumber = mCell.Match.ID;
                    lastMatchedCell = (lastMatchedCell.Match.SwapTime < mCell.Match.SwapTime) ? mCell : lastMatchedCell;
                    lastMatchedID = lastMatchedCell.Match.ID;

                    if (mCell.HasBomb)
                    {
                        { Bombs.Add(mCell); }
                    }
                }
            }
        }

        public void AddRange(IEnumerable<GridCell> mCells)
        {
            if (mCells != null)
            {
                foreach (var item in mCells)
                {
                    Add(item);
                }
            }
        }

        public virtual void CancelTween()
        {
            Cells.ForEach((c) => { c.CancelTween(); });
        }

        public override string ToString()
        {
            string s = "";
            Cells.ForEach((c) => { s += c.ToString(); });
            return s;
        }

        public GridCell GetLowermostX()
        {
            if (Cells.Count == 0) return null;
            GridCell l = Cells[0];
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Column < l.Column) l = Cells[i];
            }
            return l;
        }

        public GridCell GetTopmostX()
        {
            if (Cells.Count == 0) return null;
            GridCell t = Cells[0];
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Column > t.Column) t = Cells[i];
            }
            return t;
        }

        public GridCell GetLowermostY()
        {
            if (Cells.Count == 0) return null;
            GridCell l = Cells[0];
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Row > l.Row) l = Cells[i];
            }
            return l;
        }

        public GridCell GetTopmostY()
        {
            if (Cells.Count == 0) return null;
            GridCell t = Cells[0];
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].Row < t.Row) t = Cells[i];
            }
            return t;
        }

        public List<GridCell> GetDynamicArea()
        {
            List<GridCell> res = new List<GridCell>(Length);
            for (int i = 0; i < Length; i++)
            {
                if (Cells[i].DynamicObject)
                {
                    res.Add(Cells[i]);
                }
            }
            return res;
        }

        internal bool IsHorizonal()
        {
            if (Cells.Count < 2) return false;
            int row = Cells[0].Row;
            for (int i = 1; i < Cells.Count; i++)
            {
                if (row != Cells[i].Row) return false;
            }
            return true;
        }

        internal bool IsVertical()
        {
            if (Cells.Count < 2) return false;
            int column = Cells[0].Column;
            for (int i = 1; i < Cells.Count; i++)
            {
                if (column != Cells[i].Column) return false;
            }
            return true;
        }

        /// <summary>
        /// Scaling sequenced group
        /// </summary>
        /// <param name="completeCallBack"></param>
        internal virtual void Show(Action completeCallBack)
        {
            ParallelTween showTween = new ParallelTween();
            foreach (GridCell gc in Cells)
            {
                showTween.Add((callBack) =>
                {
                    gc.ZoomMatch(callBack);
                });
            }
            showTween.Start(completeCallBack);
        }

        internal int BombsCount
        {
            get { return Bombs.Count; }
        }

        internal void Remove(GridCell mCell)
        {
            if (mCell == null) return;
            if (Contain(mCell))
            {
                Cells.Remove(mCell);
            }
        }

        internal void Remove(List<GridCell> mCells)
        {
            if (mCells == null) return;
            for (int i = 0; i < mCells.Count; i++)
            {
                if (Contain(mCells[i]))
                {
                    Cells.Remove(mCells[i]);
                }
            }
        }
    }

    public class Row<T> : CellArray<T> where T : GridCell
    {
        public Row(int size) : base(size) { }

        public void CreateWestWind(GameObject prefab, Vector3 scale, Transform parent, Action completeCallBack)
        {
            GameObject s = UnityEngine.Object.Instantiate(prefab, cells[0].transform.position, Quaternion.identity);
            s.transform.localScale = scale;
            s.transform.parent = parent;

            Vector3 dPos = new Vector3((cells[0].transform.localPosition - cells[1].transform.localPosition).x * 3.0f, 0, 0);
            s.transform.localPosition += dPos;

            Vector3 endPos = cells[cells.Length - 1].transform.position - dPos * scale.x;
            Whirlwind w = s.GetComponent<Whirlwind>();
            w.Create(endPos, completeCallBack);
        }

        /// <summary>
        /// Get right cells
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<T> GetRightCells(int index)
        {
            List<T> cs = new List<T>();
            if (ok(index))
            {
                int i = Length - 1;
                while (i > index)
                {
                    cs.Add(cells[i]);
                    i--;
                }
            }
            return cs;
        }

        /// <summary>
        /// Get right cell
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetRightCell(int index)
        {
            if (ok(index + 1))
            {
                return cells[index + 1];
            }
            return null;
        }

        /// <summary>
        /// Get left cells
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<T> GetLeftCells(int index)
        {
            List<T> cs = new List<T>();
            if (ok(index))
            {
                int i = 0;
                while (i < index)
                {
                    cs.Add(cells[i]);
                    i++;
                }
            }
            return cs;
        }

        /// <summary>
        /// Get left cell
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetLeftCell(int index)
        {
            if (ok(index - 1))
            {
                return cells[index - 1];
            }
            return null;
        }

        public T GetLeftDynamic()
        {
            return GetMinDynamic();
        }

        public T GetRightDynamic()
        {
            return GetMaxDynamic();
        }
    }

    public class Column<T> : CellArray<T> where T : GridCell
    {
        // public Spawner Spawn { get; private set; } // 20.12.2023

        public Column(int size) : base(size) { }

        public void CreateTopSpawner(Spawner prefab, SpawnerStyle sStyle, Vector3 scale, Transform parent)
        {
            switch (sStyle)
            {
                case SpawnerStyle.AllEnabled:
                    GridCell gc = GetTopUsed();
                    if (gc)
                    {
                        gc.CreateSpawner(prefab, new Vector2(0, -(cells[0].transform.position - cells[1].transform.position).y * 1.3f));
                        // Spawn = gc.spawner; // 20.12.2023
                    }
                    break;
                case SpawnerStyle.AllEnabledAlign:
                    GridCell c = GetTopUsed();// 20.12.2023
                    if (c)
                    {
                        c.CreateSpawner(prefab, new Vector2(0, -(cells[0].transform.position - cells[1].transform.position).y * 1.3f));
                        // Spawn = c.spawner;// 20.12.2023
                    }

                    break;
                case SpawnerStyle.DisabledAligned:
                    if (!cells[0].Blocked && !cells[0].IsDisabled)
                    {
                        cells[0].CreateSpawner(prefab, new Vector2(0, -(cells[0].transform.position - cells[1].transform.position).y * 1.3f));
                       // Spawn = cells[0].spawner; // 20.12.2023
                    }
                    break;
            }

        }

        public void CreateNordWind(GameObject prefab, Vector3 scale, Transform parent, Action completeCallBack)
        {
            GameObject s = UnityEngine.Object.Instantiate(prefab, cells[0].transform.position, Quaternion.identity);
            s.transform.localScale = scale;
            s.transform.parent = parent;
            s.transform.eulerAngles = new Vector3(0, 0, -90);
            Vector3 dPos = new Vector3(0, (cells[0].transform.localPosition - cells[1].transform.localPosition).y * 3.0f, 0);
            s.transform.localPosition += dPos;

            Vector3 endPos = cells[cells.Length - 1].transform.position - dPos * scale.x;
            Whirlwind w = s.GetComponent<Whirlwind>();
            w.Create(endPos, completeCallBack);
        }

        public T GetTopUsed()
        {
            return GetMinUsed();
        }

        public T GetTopDynamic()
        {
            return GetMinDynamic();
        }

        public T GetBottomDynamic()
        {
            return GetMaxDynamic();
        }

        public T GetBottomUsed()
        {
            return GetMaxUsed();
        }

        /// <summary>
        /// Get cells at top
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<T> GetTopCells(int index)
        {
            List<T> cs = new List<T>();
            if (ok(index))
            {
                int i = 0;
                while (i < index)
                {
                    cs.Add(cells[i]);
                    i++;
                }
            }
            return cs;
        }

        /// <summary>
        /// Get cell at top
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetTopCell(int index)
        {
            if (ok(index - 1))
            {
                return cells[index - 1];
            }
            return null;
        }

        /// <summary>
        /// Get bottom cells
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<T> GetBottomCells(int index)
        {
            List<T> cs = new List<T>();
            if (ok(index))
            {
                int i = Length - 1;
                while (i > index)
                {
                    cs.Add(cells[i]);
                    i--;
                }
            }
            return cs;
        }

        /// <summary>
        /// Get bottom cell
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetBottomCell(int index)
        {
            if (ok(index + 1))
            {
                return cells[index - 1];
            }
            return null;
        }
    }

    public class CellArray<T> : GenInd<T> where T : GridCell
    {
        public CellArray(int size) : base(size) { }

        public static bool AllMatchObjectsIsEqual(GridCell[] mcs)
        {
            if (mcs == null || !mcs[0] || mcs.Length < 2) return false;
            for (int i = 1; i < mcs.Length; i++)
            {
                if (!mcs[i]) return false;
                if (!mcs[0].IsMatchObjectEquals(mcs[i])) return false;
            }
            return true;
        }

        public List<MatchGroup> GetMatches(int minMatches, bool X0X)
        {
            List<MatchGroup> mgList = new List<MatchGroup>();
            MatchGroup mg = new MatchGroup();
            mg.Add(cells[0]);
            for (int i = 1; i < cells.Length; i++)
            {
                int prev = mg.Length - 1;
                if (cells[i].IsMatchable && cells[i].IsMatchObjectEquals(mg.Cells[prev]) && mg.Cells[prev].IsMatchable)
                {
                    mg.Add(cells[i]);
                    if (i == cells.Length - 1 && mg.Length >= minMatches)
                    {
                        mgList.Add(mg);
                        mg = new MatchGroup();
                    }
                }
                else // start new match group
                {
                    if (mg.Length >= minMatches)
                    {
                        mgList.Add(mg);
                    }
                    mg = new MatchGroup();
                    mg.Add(cells[i]);
                }
            }

            if (X0X) // [i-2, i-1, i]
            {
                mg = new MatchGroup();

                for (int i = 2; i < cells.Length; i++)
                {
                    mg.Add(cells[i - 2]);
                    if (cells[i].IsMatchable && cells[i].IsMatchObjectEquals(mg.Cells[0]) && !cells[i - 1].IsMatchObjectEquals(mg.Cells[0]) && mg.Cells[0].IsMatchable && cells[i - 1].IsDraggable())
                    {
                        mg.Add(cells[i]);
                        mgList.Add(mg);
                    }
                    mg = new MatchGroup();
                }
            } // end X0X
            return mgList;
        }

        public List<T> GetDynamicArea()
        {
            List<T> res = new List<T>(Length);
            for (int i = 0; i < Length; i++)
            {
                if (cells[i].DynamicObject)
                {
                    res.Add(cells[i]);
                }
            }
            return res;
        }

        public List<T> GetUsedArea()
        {
            List<T> res = new List<T>(Length);
            for (int i = 0; i < Length; i++)
            {
                if (!cells[i].Disabled)
                {
                    res.Add(cells[i]);
                }
            }
            return res;
        }

        public T GetMinDynamic()
        {
            for (int i = 0; i < Length; i++)
            {
                if (cells[i].DynamicObject || (!cells[i].Blocked && !cells[i].IsDisabled))
                {
                    return cells[i];
                }
            }
            return null;
        }

        /// <summary>
        /// return most top enabled cell
        /// </summary>
        /// <returns></returns>
        public T GetMinUsed()
        {
            for (int i = 0; i < Length; i++)
            {
                if (!cells[i].IsDisabled)
                {
                    return cells[i];
                }
            }
            return null;
        }
 
        public T GetMaxDynamic()
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                if (cells[i].DynamicObject || (!cells[i].Blocked && !cells[i].IsDisabled))
                {
                    return cells[i];
                }
            }
            return null;
        }

        /// <summary>
        /// return most bottom enabled cell
        /// </summary>
        /// <returns></returns>
        public T GetMaxUsed()
        {
            for (int i = Length - 1; i >= 0; i--)
            {
                if (!cells[i].IsDisabled)
                {
                    return cells[i];
                }
            }
            return null;
        }

        public Vector3 GetDynamicCenter()
        {
            T l = GetMinDynamic();
            T r = GetMaxDynamic();

            if (l && r) return (l.transform.position + r.transform.position) / 2f;
            else if (l) return l.transform.position;
            else if (r) return r.transform.position;
            else return Vector3.zero;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < cells.Length; i++)
            {
                s += cells[i].ToString();
            }
            return s;
        }
    }

    public class GenInd<T> where T : class
    {
        public T[] cells;
        public int Length;

        public GenInd(int size)
        {
            cells = new T[size];
            Length = size;
        }

        public T this[int index]
        {
            get { if (ok(index)) { return cells[index]; } else { return null; } }
            set { if (ok(index)) { cells[index] = value; } else { } }
        }

        protected bool ok(int index)
        {
            return (index >= 0 && index < Length);
        }

        public T GetMiddleCell()
        {
            int number = Length / 2;

            return cells[number];
        }
    }

    public class EstimateMask : MatchMask
    {
        public Vector2Int to;           // 0
        public List<Vector2Int> from;   // 2

        public EstimateMask(int[,] mask, int priority) : base (mask, priority)
        {
           
        }


        public EstimateMask(MatchMask matchMask) : base(matchMask.mask, matchMask.priority)
        {

        }
        protected override void FillArrays()
        {
            mainMatch = new List<Vector2Int>();
            ncMatch = new List<Vector2Int>();
            from = new List<Vector2Int>();
            long mRows = mask.GetLongLength(0);
            long mCols = mask.GetLongLength(1);

            for (int gr = 0; gr < mRows; gr++)
            {
                for (int gc = 0; gc < mCols; gc++)
                {
                    if (mask[gr, gc] == 1) mainMatch.Add(new Vector2Int(gr, gc));
                    else if (mask[gr, gc] == 5) ncMatch.Add(new Vector2Int(gr, gc));
                    else if (mask[gr, gc] == 2) from.Add(new Vector2Int(gr, gc));
                    else if (mask[gr, gc] == 0) to = new Vector2Int(gr, gc);
                }
            }
        }
    }

    public class IsolateMask : MatchMask
    {
        public List<Vector2Int> notMatch;       // 3
        public List<Vector2Int> anyMatch;       // 4, 
        public IsolateMask (int[,] mask, int priority) : base(mask, priority) { }

        public IsolateMask(MatchMask matchMask) : base(matchMask.mask, matchMask.priority) { }

        public MatchGroupType matchGroupType;

        protected override void FillArrays()
        {
            mainMatch = new List<Vector2Int>();
            ncMatch = new List<Vector2Int>();
            notMatch = new List<Vector2Int>();
            anyMatch = new List<Vector2Int>();

            long mRows = mask.GetLongLength(0);
            long mCols = mask.GetLongLength(1);

            for (int gr = 0; gr < mRows; gr++)
            {
                for (int gc = 0; gc < mCols; gc++)
                {
                    if (mask[gr, gc] == 1) mainMatch.Add(new Vector2Int(gr, gc));
                    else if (mask[gr, gc] == 5) ncMatch.Add(new Vector2Int(gr, gc));
                    else if (mask[gr, gc] == 3) notMatch.Add(new Vector2Int(gr, gc));
                    else if (mask[gr, gc] == 4) anyMatch.Add(new Vector2Int(gr, gc));
                }
            }
        }
    }

    public class MatchMask
    {
        public List<Vector2Int> mainMatch;        // 1
        public List<Vector2Int> ncMatch;          // 5 - not collected match
        public int[,] mask;
        public int priority;

        public MatchMask(int[,] mask, int priority)
        {
            this.mask = mask;
            this.priority = priority;
            FillArrays();
        }

        protected virtual void FillArrays()
        {
            mainMatch = new List<Vector2Int>();
            ncMatch = new List<Vector2Int>();

            long mRows = mask.GetLongLength(0);
            long mCols = mask.GetLongLength(1);

            for (int gr = 0; gr < mRows; gr++)
            {
                for (int gc = 0; gc < mCols; gc++)
                {
                    if (mask[gr, gc] == 1) mainMatch.Add(new Vector2Int(gr, gc));
                    if (mask[gr, gc] == 5) ncMatch.Add(new Vector2Int(gr, gc));
                }
            }
        }

        public virtual bool IsEqualTo(MatchMask toMask)
        {
            int mRows = mask.GetLength(0);
            int mCols = mask.GetLength(1);

            int tRows = toMask.mask.GetLength(0);
            int tCols = toMask.mask.GetLength(1);

            if (mRows != tRows || mCols != tCols) return false;

            for (int gr = 0; gr < mRows; gr++)
            {
                for (int gc = 0; gc < mCols; gc++)
                {
                    if (mask[gr, gc] != toMask.mask[gr, gc]) return false;
                }
            }
            return true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameBoard))]
    public class MatchBoardEditor : Editor
    {
        private bool test = true;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            //EditorGUILayout.Space();
            //EditorGUILayout.Space();
            //#region test
            //if (EditorApplication.isPlaying)
            //{
            //    MatchBoard tg = (MatchBoard)target;
            //    if (MatchBoard.GMode == GameMode.Play)
            //    {
            //        if (test = EditorGUILayout.Foldout(test, "Test"))
            //        {
            //            #region fill
            //            EditorGUILayout.BeginHorizontal("box");
            //            if (GUILayout.Button("Fill(remove matches)"))
            //            {
            //                tg.grid.FillGrid(true);
            //            }
            //            if (GUILayout.Button("Fill"))
            //            {
            //                tg.grid.FillGrid(false);
            //            }
            //            if (GUILayout.Button("Remove matches"))
            //            {
            //                tg.grid.RemoveMatches();
            //            }
            //            EditorGUILayout.EndHorizontal();
            //            #endregion fill

            //            #region mix
            //            EditorGUILayout.BeginHorizontal("box");
            //            if (GUILayout.Button("Mix"))
            //            {
            //                tg.MixGrid(null); 
            //            }

            //            if (GUILayout.Button("Mix"))
            //            {
            //                tg.MixGrid(null);
            //            }
            //            EditorGUILayout.EndHorizontal();
            //            #endregion mix

            //            #region matches
            //            EditorGUILayout.BeginHorizontal("box");
            //            if (GUILayout.Button("Estimate check"))
            //            {
            //                 tg.EstimateGroups.CreateGroups( 2, true);
            //                Debug.Log("Estimate Length:" + tg.EstimateGroups.Length);
            //            }

            //            if (GUILayout.Button("Get free cells"))
            //            {
            //                Debug.Log("Free cells: " + tg.grid?.GetFreeCells().Count);
            //            }
            //            EditorGUILayout.EndHorizontal();
            //            #endregion matches

            //        }

            //        EditorGUILayout.LabelField("Board state: " + tg.MbState);
            //        EditorGUILayout.LabelField("Estimate groups count: " + ((tg.EstimateGroups!=null)? tg.EstimateGroups.Length.ToString(): "none"));
            //        EditorGUILayout.LabelField("Collect groups count: " + ((tg.CollectGroups != null) ? tg.CollectGroups.Length.ToString() : "none"));
            //        EditorGUILayout.LabelField("Free cells count: " + ((tg.grid!= null) ? tg.grid.GetFreeCells().Count.ToString() : "none"));

            //        return;
            //    }
            //}
            //EditorGUILayout.LabelField("Goto play mode for test");
            //#endregion test
        }
    }
#endif
}
