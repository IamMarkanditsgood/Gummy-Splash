using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class LevelButton : MonoBehaviour
    {
        public Image LeftStar;
        public Image MiddleStar;
        public Image RightStar;
        public Sprite ActiveButtonSprite;
        public Sprite LockedButtonSprite;
        public Image lockImage;
        public Button button;
        public Text numberText;

        public Sprite hardButtonImage;
        public Sprite hardButtonHover;

        public bool Interactable { get; private set; }

        internal void SetActive(bool active, int activeStarsCount, bool isPassed)
        {
            SetActive(active, activeStarsCount, isPassed, false);
        }

        internal void SetActive(bool active, int activeStarsCount, bool isPassed, bool hard)
        {
            LeftStar.gameObject.SetActive(activeStarsCount > 0 && isPassed);
            MiddleStar.gameObject.SetActive(activeStarsCount > 1 && isPassed);
            RightStar.gameObject.SetActive(activeStarsCount > 2 && isPassed);

            Interactable = active || isPassed;
            button.interactable = active || isPassed;


            lockImage.gameObject.SetActive(!isPassed);
            lockImage.sprite = (!active) ? LockedButtonSprite : ActiveButtonSprite;

            if (active)
            {
                MapController.Instance.ActiveButton = this;
            }

            if (hard)
            {
                Image image = GetComponent<Image>();
                if (image && hardButtonImage) image.sprite = hardButtonImage;

                Button button = GetComponent<Button>();
                if(button && hardButtonHover)
                {
                    SpriteState sT = new SpriteState();
                    sT.pressedSprite = hardButtonHover;
                    button.spriteState = sT;
                }
            }
        }
    }
}