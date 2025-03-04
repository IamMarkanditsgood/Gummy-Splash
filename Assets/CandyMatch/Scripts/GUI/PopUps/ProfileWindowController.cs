using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class ProfileWindowController : PopUpsController
    {
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Text playerName;

        [SerializeField]
        private InputField inputField;

        [SerializeField]
        private Text coinsText;

        [SerializeField]
        private Text starsText;

        [SerializeField]
        private Image avatarImage;

        [SerializeField]
        private Image[] lifes;

        [SerializeField]
        private Text lifesText;

        [SerializeField]
        private Button changeButton;

        [Space(8)]
        [Header("Boosters")]
        [SerializeField]
        private RectTransform BoostersParent;

        #region temp vars
        private Sprite defaultAvatar;

        private GameBoard MBoard => GameBoard.Instance;
        private GuiController MGui => GuiController.Instance;
        private FBholder FB  => FBholder.Instance;
        private GameConstructSet GCSet => GameConstructSet.Instance;
        private GameObjectsSet GOSet => (GCSet) ? GCSet.GOSet : null;
        private GameLevelHolder MGLevel => GameLevelHolder.Instance;
        private StarsHolder MStars => StarsHolder.Instance;
        private LifesHolder MLife => LifesHolder.Instance;
        private CoinsHolder MCoins => CoinsHolder.Instance;
        #endregion temp vars

        #region regular
        private void Start()
        {
            if (inputField)
            {
                inputField.gameObject.SetActive(false);
                inputField.onEndEdit.AddListener((name) =>
                {
                    PlayerDataHolder.Instance.SetFullName(name);
                    inputField.gameObject.SetActive(false);
                    if (changeButton) changeButton.gameObject.SetActive(true);
                    if (playerName) playerName.enabled = true;
                    SetControlActivity(true);
                });
                inputField.enabled = false;
            }
            if (FBholder.IsLogined && FB.playerPhoto)
            {
                avatarImage.sprite = FB.playerPhoto;
            }

            // set fb events
            FBholder.LoadPhotoEvent += SetFBPhoto;
            FBholder.LogoutEvent += RemoveFBPhoto;

            PlayerDataHolder.Instance.ChangeEvent += RefreshFullName;
            MLife.ChangeEvent.AddListener(RefreshLife);
            MCoins.ChangeEvent.AddListener(RefreshCoins);
            if (avatarImage) defaultAvatar = avatarImage.sprite;
        }

        private void OnDestroy()
        {
            FBholder.LoadPhotoEvent -= SetFBPhoto;
            FBholder.LogoutEvent -= RemoveFBPhoto;
            PlayerDataHolder.Instance.ChangeEvent -= RefreshFullName;
            MLife.ChangeEvent.RemoveListener(RefreshLife);
            MCoins.ChangeEvent.RemoveListener(RefreshCoins);
        }
        #endregion regular

        public override void RefreshWindow()
        {
            RefreshFullName(PlayerDataHolder.FullName);
            if (levelText) levelText.text = (GameLevelHolder.TopPassedLevel + 1).ToString();
            CreateBoostersPanel();
            RefreshLife(LifesHolder.Count);
            if (starsText) starsText.text = MStars.GetAllStars().ToString();
            //  if (inputField) inputField.text = MPlayer.FullName;
            if (playerName) playerName.text = PlayerDataHolder.FullName;
			RefreshCoins(CoinsHolder.Count);
            base.RefreshWindow();
        }

        private void CreateBoostersPanel()
        {
            GuiBoosterHelper[] ms = BoostersParent.GetComponentsInChildren<GuiBoosterHelper>();
            foreach (GuiBoosterHelper item in ms)
            {
                DestroyImmediate(item.gameObject);
            }
            List<Booster> bList = new List<Booster>();

            foreach (var b in GOSet.BoosterObjects)
            {
                if (b.Count > 0) bList.Add(b);
            }

            Debug.Log("blist count: " + bList.Count);

         //   bList.Shuffle();
            for (int i = 0; i < bList.Count; i++)
            {
                Booster b = bList[i];
                GuiBoosterHelper bM = b.CreateGuiHelper(BoostersParent, "profile");
            }
        }

        public void Change_Click()
        {
            if (!inputField) return;
            if (changeButton) changeButton.gameObject.SetActive(false);
            inputField.gameObject.SetActive(true);
            if (playerName) playerName.enabled = false;
            SetControlActivity(false);

            inputField.enabled = true;
            inputField.interactable = true;
            inputField.ActivateInputField();
            inputField.Select();
            Debug.Log("change : " + inputField);
        }

        #region event handlers
        private void RefreshCoins(int coins)
        {
            if (coinsText) coinsText.text = coins.ToString();
        }

        private void RefreshLife(int life)
        {
            if (lifesText) lifesText.text = life.ToString();
            if (lifes != null)
            {
                for (int i = 0; i < lifes.Length; i++)
                {
                    lifes[i].gameObject.SetActive(i < life);
                }
            }
        }

        private void SetFBPhoto(bool logined, Sprite photo)
        {
            if (logined && photo && avatarImage) avatarImage.sprite = FB.playerPhoto;
        }

        private void RemoveFBPhoto()
        {
            if (avatarImage) avatarImage.sprite = defaultAvatar;
        }

        private void RefreshFullName(string pName)
        {
            if (playerName) playerName.text = pName;
            if (inputField) inputField.text = PlayerDataHolder.FullName;
        }
        #endregion event handlers 
    }
}