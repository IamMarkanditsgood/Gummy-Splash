using UnityEngine;
using System;

namespace Mkey
{
    public class BombCreator : MonoBehaviour
    {
        [SerializeField]
        private Sprite glow;
        [SerializeField]
        private GUIFlyer scoreFlyer;
        private GameObject createPrefab;

        #region temp vars
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private GameBoard MBoard { get { return GameBoard.Instance; } }
        private GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        private LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        private GameObjectsSet GOSet { get { return GCSet.GOSet; } }
        #endregion temp vars

        private void Start()
        {
           
        }

        public void Create(BombType bombType, MatchGroup m, bool showScore, int score, Action completeCallBack)
        {
            float delay = 0;
            ParallelTween collectTween = new ParallelTween();

            foreach (GridCell c in m.Cells) // move and collect
            {
                Creator.CreateSpriteAtPosition(c.Match.transform, glow, c.Match.transform.position, SortingOrder.BombCreator - 1);
                collectTween.Add((callBack) => { c.MoveMatchAndCollect(m.lastMatchedCell, delay, false, true, false,  callBack); });
            }

            collectTween.Start(() =>
            {
                BombCreate(m, bombType, showScore, score);
                // Debug.Log("create bomb: " + bombType);
                completeCallBack?.Invoke();
            });
        }

        private void SetStaticMatchBomb(GridCell c, BombDir bombDir, int matchID, bool showScore, int score)
        {
            StaticMatchBombObject b = GOSet.GetStaticMatchBombObject(bombDir, matchID);
            c.SetObject(b);
        }

        private void SetDynamicMatchBomb(GridCell c, BombDir bombDir, int matchID, bool showScore, int score)
        {
            DynamicMatchBombObject b = GOSet.GetDynamicMatchBombObject(bombDir, matchID);
            c.SetObject(matchID);
            c.SetObject(b);
        }

        private void SetDynamicClickBomb(GridCell c, BombDir bombDir, int matchID, bool showScore, int score)
        {
            DynamicClickBombObject b = GOSet.GetDynamicClickBombObject(bombDir, matchID);
            c.SetObject(b);
        }

        private void SetBomb(GridCell c, BombType bombType, BombDir bombDir, int matchID, bool showScore, int score)
        {
            // Debug.Log("SetBomb bomb : " + bombType);
            switch (bombType)
            {
                case BombType.StaticMatch:
                    SetStaticMatchBomb(c, bombDir, matchID, showScore, score);
                    break;
                case BombType.DynamicMatch:
                    SetDynamicMatchBomb(c, bombDir, matchID, showScore, score);
                    break;
                case BombType.DynamicClick:
                    SetDynamicClickBomb(c, bombDir, matchID, showScore, score);
                    break;
            }
            BombObject b = c.GetBomb();
            if (b && showScore && score > 0) b.InstantiateScoreFlyer(scoreFlyer, score);
        }

        private void BombCreate(MatchGroup m, BombType bombType, bool showScore, int score)
        {
            MatchGroupType mT = m.GetGroupType();
            // Debug.Log("BombCreate, MatchGroupType mT: " + mT);
            switch (mT)
            {
                case MatchGroupType.Rect:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Random, m.lastMatchedID, showScore, score);
                    break;
                case MatchGroupType.ExtRect:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Random, m.lastMatchedID, showScore, score);
                    break;
                case MatchGroupType.Vert4:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Horizontal, m.lastMatchedID, showScore, score);
                    break;
                case MatchGroupType.Hor4:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Vertical, m.lastMatchedID, showScore, score);
                    break;
                case MatchGroupType.LT:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Radial, m.lastMatchedID, showScore, score);
                    break;
                case MatchGroupType.BigLT:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Radial, m.lastMatchedID, showScore, score);
                    break;
                case MatchGroupType.MiddleLT:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Radial, m.lastMatchedID, showScore, score);
                    break;
                case MatchGroupType.Hor5:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Color, m.lastMatchedID, showScore, score); // BombDir.Color
                    break;
                case MatchGroupType.Vert5:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Color, m.lastMatchedID, showScore, score); // BombDir.Color
                    break;
                default:
                    SetBomb(m.lastMatchedCell, bombType, BombDir.Color, m.lastMatchedID, showScore, score); // BombDir.Color
                    break;

            }
        }
    }
}
