using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mkey {
    public class SettingsWindowController : PopUpsController
    {
        [SerializeField] private GameObject _gameView;
        [SerializeField] private GameObject _homeView;
        [SerializeField]
        private Toggle easyToggle;
        [SerializeField]
        private Toggle hardToggle;

        private SoundMaster MSound => SoundMaster.Instance; 

        #region regular
        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "1_Map")
            {
                _gameView.SetActive(false);
                _homeView.SetActive(true);
            }
            else if (SceneManager.GetActiveScene().name == "2_Game")
            {
                _gameView.SetActive(true);
                _homeView.SetActive(false);
            }
            if (easyToggle) easyToggle.onValueChanged.AddListener((value) =>
            {
                MSound.SoundPlayClick(0, null);
                if (value) { HardModeHolder.Instance.SetMode(HardMode.Easy); }
                else { HardModeHolder.Instance.SetMode(HardMode.Hard); }
            });

            RefreshWindow();
        }
        #endregion regular

        public void GoHome()
        {
            SceneManager.LoadScene("1_Map");
        }

        public override void RefreshWindow()
        {
            RefreshHardMode();
            base.RefreshWindow();
        }

        private void RefreshHardMode()
        {
            if(hardToggle)   hardToggle.isOn = (HardModeHolder.Mode == HardMode.Hard);
            if(easyToggle)  easyToggle.isOn = (HardModeHolder.Mode != HardMode.Hard);
        }
    }
}
