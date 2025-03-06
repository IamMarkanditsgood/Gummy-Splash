using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace Mkey
{
    public class MapController : MonoBehaviour
    {

        [HideInInspector]
        public List<LevelButton> MapLevelButtons { get; private set; }
        [HideInInspector]
        public LevelButton ActiveButton;

        [HideInInspector]
        public Canvas parentCanvas;
        private ScrollRect sRect;
        private RectTransform content;

        public RectTransform avatarGroup;
        public Image avatarImage;

        [Header("If true, then the map will scroll to the Active Level Button", order = 1)]
        [SerializeField]
        private bool scrollToActiveButton = true;

        [SerializeField]
        private int gameSceneOffset = 1;

        [Header("If true then all levels will be open for testing, only for debug", order = 1)]
        [SerializeField]
        private bool openMap = false;

        private List<Biome> biomes;
        private int biomesCount;
        public static MapController Instance;

        private GameBoard MBoard { get { return GameBoard.Instance; } }
        private GuiController MGui { get { return GuiController.Instance; } }
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        private LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        private StarsHolder MStars => StarsHolder.Instance;

        #region regular
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            GameBoard.GMode = GameMode.Play;
        }

        static bool showedWarning = false;
        private void Start()
        {
            Debug.Log("Map started ");
            if (openMap &&!showedWarning) { MGui.ShowMessageWithCloseButton("Test mode!", "All levels \nare available!", () => { }); showedWarning = true; }

            // cache biomes
            biomes = new List<Biome>(GetComponentsInChildren<Biome>());
            if (biomes == null)
            {
                Debug.LogError("No  biomes found.");
                return;
            }

            biomes.RemoveAll((b) => { return b == null; });
            biomes.Reverse();
            biomesCount = biomes.Count;

            content = GetComponent<RectTransform>();
            if (!content)
            {
                Debug.LogError("No RectTransform component. Use RectTransform for MapMaker.");
                return;
            }

            // cache level buttons
            MapLevelButtons = new List<LevelButton>();
            foreach (var b in biomes)
            {
                MapLevelButtons.AddRange(b.levelButtons);
            }

            int topPassedLevel = GameLevelHolder.TopPassedLevel;
            topPassedLevel = Mathf.Clamp(topPassedLevel, -1, MapLevelButtons.Count - 2);

            // set onclick listeners for level buttons
            for (int i = 0; i < MapLevelButtons.Count; i++)
            {
                //1 add listeners
                int buttonNumber = i + 1;
                int currLev = i;

                MapLevelButtons[i].button.onClick.AddListener(() =>
                {
                    MSound.SoundPlayClick(0, null);
                    GameLevelHolder.CurrentLevel = currLev;
                    if (LifesHolder.Count <= 0 && !GCSet.UnLimited) { MGui.ShowMessage("Sorry!", "You have no lifes.", 1.5f, () => { MGui.ShowPopUpByDescription("lifeshop"); }); return; }
                    Debug.Log("load scene : " + gameSceneOffset + " ;CurrentLevel: " + GameLevelHolder.CurrentLevel);
                    GameBoard.showMission = true;
                    if (SceneLoader.Instance) SceneLoader.Instance.LoadScene(gameSceneOffset);
                });
                // activate buttons
                SetButtonActive(buttonNumber, buttonNumber == topPassedLevel + 2, topPassedLevel + 1 >= buttonNumber);
                MapLevelButtons[i].numberText.text = (buttonNumber).ToString();
            }

            // set fb events
            FBholder.LoadPhotoEvent += SetFBPhoto;
            FBholder.LogoutEvent += RemoveFBPhoto;

            parentCanvas = GetComponentInParent<Canvas>();
            sRect = GetComponentInParent<ScrollRect>();
            if (scrollToActiveButton) StartCoroutine(SetMapPositionToAciveButton());

            //update photo
            SetFBPhoto(FBholder.IsLogined, FBholder.Instance.playerPhoto);
        }

        private void OnDestroy()
        {
            FBholder.LoadPhotoEvent -= SetFBPhoto;
            FBholder.LogoutEvent -= RemoveFBPhoto;
        }
        #endregion regular

        private IEnumerator SetMapPositionToAciveButton()
        {
            yield return new WaitForSeconds(0.01f);
            /*if (sRect)
            {
                Debug.Log("scrolling rect");
                int bCount = biomesCount;
                float contentSizeY = content.sizeDelta.y / (bCount) * (bCount - 1.0f);
                float relPos = content.InverseTransformPoint(ActiveButton.transform.position).y; // Debug.Log("contentY : " + contentSizeY +  " ;relpos : " + relPos + " : " + relPos / contentSizeY);
                float vpos = (-contentSizeY / (bCount * 2.0f) + relPos) / contentSizeY; // 

                SimpleTween.Cancel(gameObject, false);
                float start = sRect.verticalNormalizedPosition;

                SimpleTween.Value(gameObject, start, vpos, 0.25f).SetOnUpdate((float f) => { sRect.verticalNormalizedPosition = Mathf.Clamp01(f); });
                // sRect.verticalNormalizedPosition = Mathf.Clamp01(vpos); // Debug.Log("vpos : " + Mathf.Clamp01(vpos));
            }
            else
            {
                Debug.Log("no scrolling rect");
            }*/
        }

        private void SetButtonActive(int buttonNumber, bool active, bool isPassed)
        {
            LevelConstructSet levelConstructSet = GCSet.GetLevelConstructSet(buttonNumber - 1);
            bool hard = levelConstructSet.hardFlag;
            if (hard) Debug.Log("HARD LEVEL: " + buttonNumber + "; " + levelConstructSet.name);

            int activeStarsCount = MStars.GetLevelStars(buttonNumber-1);
            if (openMap)
            {
                MapLevelButtons[buttonNumber - 1].SetActive(true, activeStarsCount, isPassed, hard);
            }
            else
            {
                MapLevelButtons[buttonNumber - 1].SetActive(active, activeStarsCount, isPassed, hard);
            }
        }

        public void SetControlActivity(bool activity)
        {
            for (int i = 0; i < MapLevelButtons.Count; i++)
            {
                if (!activity) MapLevelButtons[i].button.interactable = activity;
                else
                {
                    MapLevelButtons[i].button.interactable = MapLevelButtons[i].Interactable;
                }
            }
        }

        private void SetFBPhoto(bool logined, Sprite photo)
        {
            if (logined && photo && avatarImage) avatarImage.sprite = FBholder.Instance.playerPhoto;
        }

        private void RemoveFBPhoto()
        {
            //  if(avatarImage) avatarImage.sprite = FBholder.Instance.playerPhoto;
        }
    }
}
