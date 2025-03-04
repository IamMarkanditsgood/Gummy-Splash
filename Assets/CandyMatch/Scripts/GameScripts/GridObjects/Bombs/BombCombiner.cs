using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class BombCombiner : MonoBehaviour
    {
        [SerializeField]
        private CombinedRadBombAndRadBomb radBombAndRadBombPrefab;
        [SerializeField]
        private CombinedRadBombAndLineBomb radBombAndLineBombPrefab;
        [SerializeField]
        private CombinedLineBombAndLineBomb lineBombAndLineBombPrefab;
        [SerializeField]
        private CombinedColorBombAndLineBomb colorBombAndLineBombPrefab;
        [SerializeField]
        private CombinedColorBombAndRadBomb colorBombAndRadBombPrefab;
        [SerializeField]
        private CombinedColorBombAndColorBomb colorBombAndColorBombPrefab;
        [SerializeField]
        private CombinedRandRocketAndLineBomb randRocketAndLineHorBombPrefab;
        [SerializeField]
        private CombinedRandRocketAndLineBomb randRocketAndLineVertBombPrefab;
        [SerializeField]
        private CombinedRandRocketAndRadBomb randRocketAndRadBombPefab;
        [SerializeField]
        private CombinedColorBombAndRandRocket randRocketAndColorBombPrefab;
        [SerializeField]
        private CombinedRandRocketAndRandRocket randRocketAndRandRocketPrefab;

        #region events
        public static Action CombineBeginEvent;
        public static Action CombineCompleteEvent;
        #endregion events

        [SerializeField]
        private Sprite glow;

        #region temp vars
        [SerializeField]
        private bool dLog = false;
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private GameBoard MBoard { get { return GameBoard.Instance; } }
        private GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        private LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        private GameObjectsSet GOSet { get { return GCSet.GOSet; } }
        private ParallelTween collectTween;
        #endregion temp vars 

        private void Start()
        {
            SwapHelper.BombsCombinedEvent = CombineBombsEventHandler;
        }

        public void CombineAndExplode(GridCell gCell, DynamicClickBombObject bomb, DynamicClickBombObject bomb2, Action completeCallBack)
        {
            if(!gCell || ! bomb)
            {
                completeCallBack?.Invoke();
                return;
            }

            NeighBors nG = gCell.Neighbors;
            BombDir bd1 = bomb.GetBombDir();
            BombCombine bC = BombCombine.None;
            List<DynamicClickBombObject> nBs = new List<DynamicClickBombObject>();

            BombDir bd2 = bomb2.GetBombDir();

            bC = GetCombineType(bd1, bd2);
            nBs.Add(bomb);
            nBs.Add(bomb2);
            // Debug.Log("GetCombineType(bd1, bd2): " + GetCombineType(bd1, bd2));
            collectTween = new ParallelTween();

            Action moveBombs = () => {
                foreach (var item in nBs)
                {
                    item.transform.parent = null;
                    item.SetToFront(true);
                    Creator.CreateSpriteAtPosition(item.transform, glow, item.transform.position, SortingOrder.BombCreator - 1);
                    collectTween.Add((callBack) =>
                    {
                        item.MoveToBomb(gCell, 0, () => { Destroy(item.gameObject); callBack(); });
                    });
                }
            };
            CombineBeginEvent?.Invoke();

            switch (bC)
            {
                case BombCombine.ColorBombAndColorBomb:     // clean full board
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedColorBombAndColorBomb bigBomb = Instantiate(colorBombAndColorBombPrefab);
                        bigBomb.transform.localScale = gCell.transform.lossyScale;
                        bigBomb.transform.position = gCell.transform.position;
                        bigBomb.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });

                    break;

                case BombCombine.RadBombAndRadBomb:               // big bomb explode
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedRadBombAndRadBomb bigBomb =  Instantiate(radBombAndRadBombPrefab);
                        bigBomb.transform.localScale = gCell.transform.lossyScale; 
                        bigBomb.transform.position = gCell.transform.position;
                        bigBomb.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;
                case BombCombine.HV:           // 2 rows or 2 columns
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedLineBombAndLineBomb bigBomb = Instantiate(lineBombAndLineBombPrefab);
                        bigBomb.transform.localScale = gCell.transform.lossyScale;
                        bigBomb.transform.position = gCell.transform.position;
                        bigBomb.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;
                case BombCombine.ColorBombAndRadBomb:          // replace color match with bomb
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedColorBombAndRadBomb colorBombAndBomb = Instantiate(colorBombAndRadBombPrefab);
                        colorBombAndBomb.transform.localScale = gCell.transform.lossyScale;
                        colorBombAndBomb.transform.position = gCell.transform.position;
                        colorBombAndBomb.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;

                case BombCombine.BombAndHV:             // 3 rows and 3 columns
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedRadBombAndLineBomb bombAndRocket = Instantiate(radBombAndLineBombPrefab);
                        bombAndRocket.transform.localScale = gCell.transform.lossyScale;
                        bombAndRocket.transform.position = gCell.transform.position;
                        bombAndRocket.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;
                case BombCombine.ColorBombAndHV:        // replace color bomb with rockets
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedColorBombAndLineBomb colorBombAndRocket = Instantiate(colorBombAndLineBombPrefab);
                        colorBombAndRocket.transform.localScale = gCell.transform.lossyScale;
                        colorBombAndRocket.transform.position = gCell.transform.position;
                        colorBombAndRocket.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;

                case BombCombine.RandRocketAndColorBomb:      
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedColorBombAndRandRocket colorBombAndRocket = Instantiate(randRocketAndColorBombPrefab);
                        colorBombAndRocket.transform.localScale = gCell.transform.lossyScale;
                        colorBombAndRocket.transform.position = gCell.transform.position;
                        colorBombAndRocket.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;

                case BombCombine.RandRocketAndHorBomb:
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedRandRocketAndLineBomb randRocketAndRocket = Instantiate(randRocketAndLineHorBombPrefab);
                        randRocketAndRocket.transform.localScale = gCell.transform.lossyScale;
                        randRocketAndRocket.transform.position = gCell.transform.position;
                        randRocketAndRocket.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;

                case BombCombine.RandRocketAndVertBomb:
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedRandRocketAndLineBomb randRocketAndRocket = Instantiate(randRocketAndLineVertBombPrefab);
                        randRocketAndRocket.transform.localScale = gCell.transform.lossyScale;
                        randRocketAndRocket.transform.position = gCell.transform.position;
                        randRocketAndRocket.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;

                case BombCombine.RandRocketAndRadBomb:
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedRandRocketAndRadBomb randRocketAndBomb = Instantiate(randRocketAndRadBombPefab);
                        randRocketAndBomb.transform.localScale = gCell.transform.lossyScale;
                        randRocketAndBomb.transform.position = gCell.transform.position;
                        randRocketAndBomb.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;

                case BombCombine.RandRocketAndRandRocket:
                    moveBombs();
                    collectTween.Start(() =>
                    {
                        CombinedRandRocketAndRandRocket randRocketAndRandRocket = Instantiate(randRocketAndRandRocketPrefab);
                        randRocketAndRandRocket.transform.localScale = gCell.transform.lossyScale;
                        randRocketAndRandRocket.transform.position = gCell.transform.position;
                        randRocketAndRandRocket.ApplyToGrid(gCell, 0.2f, completeCallBack);
                    });
                    break;
                //case BombCombine.None:                      // simple explode
                //    gCell.ExplodeBomb(0.0f, true, true, bd1 == BombDir.Color, false, () =>
                //    {
                //        completeCallBack?.Invoke();
                //    });
                //    break;
                default:
                    completeCallBack?.Invoke();
                    break;
            }
        }

        private BombCombine GetCombineType(BombDir bd1, BombDir bd2)
        {
            if (bd1 == BombDir.Color)
            {
                if (bd2 == BombDir.Color)  return BombCombine.ColorBombAndColorBomb; 
                if (bd2 == BombDir.Radial) return BombCombine.ColorBombAndRadBomb;
                if (bd2 == BombDir.Horizontal || bd2 == BombDir.Vertical) return BombCombine.ColorBombAndHV;
                if (bd2 == BombDir.Random ) return BombCombine.RandRocketAndColorBomb;
            }
            if (bd1 == BombDir.Radial)
            {
                if (bd2 == BombDir.Color) return BombCombine.ColorBombAndRadBomb;
                if (bd2 == BombDir.Radial) return BombCombine.RadBombAndRadBomb;
                if (bd2 == BombDir.Horizontal || bd2 == BombDir.Vertical) return BombCombine.BombAndHV;
                if (bd2 == BombDir.Random) return BombCombine.RandRocketAndRadBomb;
            }
            if (bd1 == BombDir.Horizontal || bd1 == BombDir.Vertical)
            {
                if (bd2 == BombDir.Color) return BombCombine.ColorBombAndHV;
                if (bd2 == BombDir.Radial) return BombCombine.BombAndHV;
                if (bd2 == BombDir.Horizontal || bd2 == BombDir.Vertical) return BombCombine.HV;
                if (bd2 == BombDir.Random && bd1 == BombDir.Horizontal) return BombCombine.RandRocketAndHorBomb;
                if (bd2 == BombDir.Random && bd1 == BombDir.Vertical) return BombCombine.RandRocketAndVertBomb;
            }

            if (bd1 == BombDir.Random)
            {
                if (bd2 == BombDir.Horizontal) return BombCombine.RandRocketAndHorBomb;
                if (bd2 == BombDir.Vertical) return BombCombine.RandRocketAndVertBomb;
                if (bd2 == BombDir.Radial) return BombCombine.RandRocketAndRadBomb;
                if (bd2 == BombDir.Color)  return BombCombine.RandRocketAndColorBomb;
                if (bd2 == BombDir.Random) return BombCombine.RandRocketAndRandRocket;
            }
            return BombCombine.None;
        }

        private List <DynamicClickBombObject> GetNeighBombs(GridCell gCell)
        {
            List<DynamicClickBombObject> res = new List<DynamicClickBombObject>();
            NeighBors nG = gCell.Neighbors;
            foreach (var item in nG.Cells) // search color bomb
            {
                if (item.DynamicClickBomb)
                {
                    res.Add(item.DynamicClickBomb);
                }
            }
            return res;
        }

        private bool CombineBombsEventHandler(GridCell source, GridCell target)
        {
            if(dLog)  Debug.Log(source.GetBomb().ToString() + " / " + target.GetBomb().ToString());
            BombObject b1 = source.GetBomb();
            BombObject b2 = target.GetBomb();

            DynamicClickBombObject bm1 = (b1) ? b1.GetComponent<DynamicClickBombObject>() : null;
            DynamicClickBombObject bm2 = (b2) ? b2.GetComponent<DynamicClickBombObject>() : null;

           // Debug.Log("enabled: " + enabled + "; " + bm1 +"; " + bm2);

            if (enabled && bm1 && bm2)
            {
               // Debug.Log("start CombineAndExplode");
                CombineAndExplode(target, bm1, bm2, () =>
                {
                   //Debug.Log("end combined swap -> to fill");
                   CombineCompleteEvent?.Invoke();
                });
                return true;
            }
            return false;
        }
    }
}