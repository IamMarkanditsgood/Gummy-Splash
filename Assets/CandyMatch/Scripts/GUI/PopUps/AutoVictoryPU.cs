using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class AutoVictoryPU : PopUpsController
    {
        [SerializeField]
        private Text skipText;
        [SerializeField]
        private Text caption;
        [SerializeField]
        private Text message;
        [SerializeField]
        private Button skipButton;
        private GameBoard MBoard => GameBoard.Instance;

        private void Start()
        {
            System.Action<Text, bool> setActive = (txt, active) => { if (txt) txt.gameObject.SetActive(active); };
            System.Action<Button, bool> setInteractable = (btn, interact) => { if (btn) btn.interactable = interact; };

            setInteractable(skipButton, false);
            setActive(skipText, false);
            setActive(caption, true);
            setActive(message, true);

            skipButton.onClick.AddListener (() =>
            {
                setInteractable(skipButton, false);
                if (MBoard) MBoard.SkipWinShow();
                CloseWindow();
            });

            TweenExt.DelayAction(gameObject, 2, ()=> 
            {
                setActive(skipText, true);
                setInteractable(skipButton, true);
                setActive(caption, false);
                setActive(message, false);
            });
        }

    }
}
