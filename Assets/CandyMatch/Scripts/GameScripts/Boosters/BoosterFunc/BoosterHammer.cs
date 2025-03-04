using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace Mkey
{
    public class BoosterHammer: BoosterFunc
    {
        [SerializeField]
        private float speed = 20f;
        [SerializeField]
        private GameObject usePrefab;
        [SerializeField]
        private GameObject cellHitPrefab;
        [SerializeField]
        private Vector3 offset;

        //private ScoreController SC => MBoard.GetComponent<ScoreController>();

        #region override

        public override void Apply(GridCell gCell, Action completeCallBack)
        {
            TweenSeq bTS = new TweenSeq();

            //move activeBooster
            Vector3 pos = transform.position;
            float dist = Vector2.Distance(pos, gCell.transform.position);

            bTS.Add((callBack) =>
            {
                SimpleTween.Move(gameObject, pos, gCell.transform.position + offset, dist / speed).AddCompleteCallBack(() =>
                {
                    callBack();
                }).SetEase(EaseAnim.EaseInSine);
            });

            // play use animation
            if (usePrefab)
            {
                bTS.Add((callBack) =>
                {
                    GetComponent<SpriteRenderer>().enabled = false;
                    GameObject g = Creator.InstantiateAnimPrefab(usePrefab, transform, transform.position, SortingOrder.Booster);
                    TweenExt.DelayAction(gameObject, 0.5f, () =>
                    {
                        GameObject cH = Creator.InstantiateAnimPrefab(cellHitPrefab, transform, gCell.transform.position, SortingOrder.Booster);
                        callBack();
                    });// delay
                });
            }


            bTS.Add((callBack) =>
            {
                gCell.BoosterHit(callBack);
            });

            bTS.Add((callBack) =>
            {
                TweenExt.DelayAction(gameObject, 0.25f, callBack);  // delay
            });

            bTS.Add((callback) =>
            {
                if (gameObject) Destroy(gameObject);
                completeCallBack?.Invoke();
                callback();
            });

            bTS.Start();
        }
        #endregion override
    }
}
