using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public class AlmostWindowController : PopUpsController
    {
        [SerializeField]
        private bool showOnlyOnce = false;
        [SerializeField]
        private Text coinsText;
        [SerializeField]
        private Button playOnButton;
        private GameBoard MBoard => GameBoard.Instance;

        private int coins;  
        private int defaultCoins = 900;    // default coins


        private void Start()
        {
            int almostCoins = defaultCoins;
            if (MBoard) almostCoins = MBoard.almostCoins;
            SetCoins(almostCoins);
            if (playOnButton) playOnButton.gameObject.SetActive(CoinsHolder.Count >=almostCoins);
        }

        private void SetCoins(int coins)
        {
            this.coins = coins;
            if (coinsText) coinsText.text = this.coins.ToString();
        }

        public void Close_Click()
        {
            CloseWindow();
            if (MBoard)
            {
                MBoard.showAlmostMessage = false;
                MBoard.WinContr.CheckResult();
            }
        }

        public void Play_Click()
        {
            CloseWindow();
            if (MBoard && showOnlyOnce) MBoard.showAlmostMessage = false;
            CoinsHolder.Add(-coins);
            AddMoves(5);
        }

        public void AddMoves(int moves)
        {
            if (MBoard) MBoard.WinContr.AddMoves(moves);
        }
    }
}
