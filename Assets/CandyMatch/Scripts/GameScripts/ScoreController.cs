using UnityEngine;

namespace Mkey
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField]
        private int baseMatchScore = 10;

        [Header("Match object score for 2 matches")]
        [SerializeField]
        private int matchScoreM2 = 15;

        [Header("Bomb creating score for match objects count")]
        [SerializeField]
        private int bombScoreC4 = 50;
        [SerializeField]
        private int bombScoreC5 = 70;
        [SerializeField]
        private int bombScoreC6 = 90;
        [SerializeField]
        private int bombScoreC7 = 110;

        public int BaseMatchScore { get { return baseMatchScore; } }

        public int GetScoreForMatches(int count)
        {
            if (count >= 2) return matchScoreM2;
            return baseMatchScore;
        }

        public int GetBombScore(int count)
        {
            if (count == 4) return bombScoreC4;
            // ****

            if (count == 5) return bombScoreC5;
            //1)
            // *****

            //2)
            //    *
            //    *
            //   ***
            if (count == 6) return bombScoreC6;

            //    *
            //    *
            //   ****
            if (count >= 7) return bombScoreC7;

            //     *
            //     *
            //   *****
            return 0;
        }
    }
}