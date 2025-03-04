using System.Collections.Generic;
using UnityEngine;
using System;

namespace Mkey
{
    public class CombinedBomb : MonoBehaviour
    {
        protected GameBoard MBoard { get { return GameBoard.Instance; } }
        protected GuiController MGui { get { return GuiController.Instance; } }
        protected SoundMaster MSound { get { return SoundMaster.Instance; } }
        protected GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        protected LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        protected GameObjectsSet GOSet { get { return GCSet.GOSet; } }
        protected MatchGrid MGrid { get { return MBoard.MainGrid; } }
        protected SpriteRenderer SRenderer => GetComponent<SpriteRenderer>();
        [SerializeField]
        protected GameObject explodePrefab;
        [SerializeField]
        protected bool explodeWave = true;

        #region temp vars
        protected TweenSeq anim;
        protected ParallelTween pT;
        #endregion temp vars

        #region virtual
        internal virtual void PlayExplodeAnimation(GridCell gCell, float delay, Action completeCallBack)
        {
            completeCallBack?.Invoke();
        }

        public virtual void ApplyToGrid(GridCell gCell, float delay,  Action completeCallBack)
        {
            completeCallBack?.Invoke();

        }

        public virtual void ExplodeArea(GridCell gCell, float delay, bool sequenced, bool showPrefab, bool fly, bool hitProtection, Action completeCallBack)
        {
            completeCallBack?.Invoke();
        }

        public virtual CellsGroup GetArea(GridCell gCell)
        {
            CellsGroup cG = new CellsGroup();
            return cG;
        }

        protected virtual void ExplodeBomb(BombObject bomb, GridCell gCell, float delay, Action completeCallBack)
        {
            bomb.PlayExplodeAnimation(gCell, delay, () =>
            {
                bomb.ExplodeArea(gCell, 0, true, true, completeCallBack);
            });
        }

        protected virtual void Prepare(float delay, GridCell gCell)
        {
            anim = new TweenSeq();
            pT = new ParallelTween();

            anim.Add((callBack) => // delay
            {
                TweenExt.DelayAction(gameObject, delay, callBack);
            });

            anim.Add((callBack) => // scale out
            {
                SimpleTween.Value(gameObject, 1, 1.5f, 0.2f).SetOnUpdate((float val) => { transform.localScale = gCell.transform.lossyScale * val; }).AddCompleteCallBack(callBack);
            });

            anim.Add((callBack) => // scale in and explode prefab
            {
                SimpleTween.Value(gameObject, 1.5f, 1.0f, 0.15f).SetOnUpdate((float val) => { transform.localScale = gCell.transform.lossyScale * val; }).AddCompleteCallBack(callBack);
                if (explodePrefab)
                {
                    GameObject g = Instantiate(explodePrefab, transform.position, Quaternion.identity);
                    g.transform.localScale = transform.localScale * .50f;
                }
                if (SRenderer) SRenderer.enabled = false;
            });

            if (explodeWave)
            {
                anim.Add((callBack) => // explode wave
                {
                    MBoard.ExplodeWave(0, transform.position, 5, null);
                    callBack();
                });
            }
        }
        #endregion virtual
    }
}

