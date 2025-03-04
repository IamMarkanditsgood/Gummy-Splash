using UnityEngine;
using UnityEngine.UI;
using System;

namespace Mkey
{
    public class VictoryWindowController : PopUpsController
    {
        [Header("Stars")]
        [SerializeField]
        private SceneCurve curveLeft;
        [SerializeField]
        private SceneCurve curveMiddle;
        [SerializeField]
        private SceneCurve curveRight;
        [SerializeField]
        private float speed = 5;
        [Space(16)]
        [SerializeField]
        private GameObject starLeftFull;
        [SerializeField]
        private GameObject starMiddleFull;
        [SerializeField]
        private GameObject starRightFull;

        [Space(8)]
        [SerializeField]
        private GameObject starLeftEmpty;
        [SerializeField]
        private GameObject starMiddleEmpty;
        [SerializeField]
        private GameObject starRightEmpty;

        [Space(8)]
        [SerializeField]
        private Text LevelNumber;
        [SerializeField]
        private Text ScoreCount;

        [Space(8)]
        [Header("Targets")]
        [SerializeField]
        private GUIObjectTargetHelper targetPrefab;
        [SerializeField]
        private RectTransform targetsContainer;

        #region temp
        private bool starLeftSet = false;
        private bool starMiddleSet = false;
        private bool starRightSet = false;
        private TweenSeq ts;
        private int oldCount;
        private GameBoard MBoard => GameBoard.Instance;
        private GameConstructSet GCSet => GameConstructSet.Instance;
        private LevelConstructSet LCSet => GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); 
        private GameObjectsSet GOSet => GCSet.GOSet;
        private GuiController MGui => GuiController.Instance;
        private ScoreHolder MScore => ScoreHolder.Instance;
        private StarsHolder MStars => StarsHolder.Instance;
        private GameLevelHolder MGLevel => GameLevelHolder.Instance;
        #endregion temp

        #region regular
        private void Start()
        {
            MScore.ChangeEvent.AddListener(SetScore);
            MStars.ChangeEvent.AddListener(SetStars);
        }

        private void OnDestroy()
        {
            MScore.ChangeEvent.RemoveListener(SetScore);
            MStars.ChangeEvent.RemoveListener(SetStars);

            if (ts != null) ts.Break();
            SimpleTween.Cancel(gameObject, false);
            SimpleTween.Cancel(starRightFull.gameObject, false);
            SimpleTween.Cancel(starLeftFull.gameObject, false);
            SimpleTween.Cancel(starMiddleFull.gameObject, false);
        }
        #endregion regular

        public override void RefreshWindow()
        {
            CreateTargets();
            SetScore(ScoreHolder.Count);
            SetStars(StarsHolder.Count);
            if (LevelNumber) LevelNumber.text = "Level " + (GameLevelHolder.CurrentLevel + 1).ToString();
            base.RefreshWindow();
        }

        public void Map_Click()
        {
            CloseWindow();
            ShowStory(()=> 
            {
                SceneLoader.Instance.LoadScene(1);
            });
            
        }

        public void Next_Click()
        {
            CloseWindow();
            ShowStory(() =>
            {
                GameLevelHolder.CurrentLevel++;
                SceneLoader.Instance.LoadScene(2);
            });

        }

        private void SetScore(int score)
        {
            if (ScoreCount)
            {
                int newCount = score;
                SimpleTween.Cancel(ScoreCount.gameObject, false);
                SimpleTween.Value(ScoreCount.gameObject, oldCount, newCount, 0.5f).SetOnUpdate((float val) =>
                {
                    oldCount = (int)val;
                    SetTextString(ScoreCount,oldCount.ToString());
                });
            }
        }

        private void SetStars(int count)
        {
            if (!starLeftSet) starLeftFull.SetActive(count >= 1);
            if (!starMiddleSet) starMiddleFull.SetActive(count >= 2);
            if (!starRightSet) starRightFull.SetActive(count >= 3);

            ts = new TweenSeq();
            if (count >= 1 && !starLeftSet)
            {
                starLeftSet = true;

                ts.Add((callBack) =>
                {
                    if (curveLeft)
                    {
                        float time = curveLeft.Length / speed;
                        curveLeft.MoveAlongPath(starLeftFull.gameObject, starLeftEmpty.transform, time, 0f, EaseAnim.EaseInOutSine, callBack);
                    }
                    else
                    {
                        SimpleTween.Move(starLeftFull.gameObject, starLeftFull.transform.position, starLeftEmpty.transform.position, 0.5f).AddCompleteCallBack(() =>
                        {
                            callBack();
                        });
                    }

                });
            }
            if (count >= 2 && !starMiddleSet)
            {
                starMiddleSet = true;
                ts.Add((callBack) =>
                {
                    if (curveMiddle)
                    {
                        float time = curveMiddle.Length / speed;
                        curveMiddle.MoveAlongPath(starMiddleFull.gameObject, starMiddleEmpty.transform, time, 0f, EaseAnim.EaseInOutSine, callBack);
                    }
                    else
                    {
                        SimpleTween.Move(starMiddleFull.gameObject, starMiddleFull.transform.position, starMiddleEmpty.transform.position, 0.5f).AddCompleteCallBack(() =>
                        {
                            callBack();
                        });
                    }
                });
            }
            if (count >= 3 && !starRightSet)
            {
                starRightSet = true;
                ts.Add((callBack) =>
                {
                    if (curveRight)
                    {
                        float time = curveRight.Length / speed;
                        curveRight.MoveAlongPath(starRightFull.gameObject, starRightEmpty.transform, time, 0f, EaseAnim.EaseInOutSine, callBack);
                    }
                    else
                    {
                        SimpleTween.Move(starRightFull.gameObject, starRightFull.transform.position, starRightEmpty.transform.position, 0.5f).AddCompleteCallBack(() =>
                        {
                            callBack();
                        });
                    }
                });
            }
            ts.Start();
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
                targetPrefab.SetIcon(GOSet.GetTargetObject(item.Value.ID).GuiImage); // unity 2019 fix

                RectTransform t = Instantiate(targetPrefab, targetsContainer).GetComponent<RectTransform>();
                GUIObjectTargetHelper th = t.GetComponent<GUIObjectTargetHelper>();
                th.SetData(item.Value, false);
                th.SetIcon(GOSet.GetTargetObject(item.Value.ID).GuiImage);
                item.Value.ChangeCountEvent += (td) => { if (this) { th.SetData(td, false); th.gameObject.SetActive(td.NeedCount > 0); } };
                th.gameObject.SetActive(item.Value.NeedCount > 0);
            }
        }

        private void ShowStory (Action completeCallBack)
        {
            if (LCSet.LevelWinStoryPage && MGui)
            {
                MGui.ShowPopUp(LCSet.LevelWinStoryPage, completeCallBack);
            }

            else completeCallBack?.Invoke();
        }

        #region test
        public void TestLeftStar()
        {
            MStars.SetCount(1);
        }

        public void TestMiddleStar()
        {
            MStars.SetCount(2);
        }

        public void TestRightStar()
        {
            MStars.SetCount(3);
        }
        #endregion test
    }
}