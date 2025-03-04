using UnityEngine;
using UnityEngine.UI;

namespace Mkey {
    public class SettingsWindowController : PopUpsController
    {
        [SerializeField]
        private Toggle easyToggle;
        [SerializeField]
        private Toggle hardToggle;

        private SoundMaster MSound => SoundMaster.Instance; 

        #region regular
        private void Start()
        {
           if(easyToggle) easyToggle.onValueChanged.AddListener((value) =>
            {
                MSound.SoundPlayClick(0, null);
                if (value) { HardModeHolder.Instance.SetMode(HardMode.Easy); }
                else { HardModeHolder.Instance.SetMode(HardMode.Hard); }
            });

            RefreshWindow();
        }
        #endregion regular

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
