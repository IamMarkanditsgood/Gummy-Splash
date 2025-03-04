using System;
using UnityEngine;

namespace Mkey
{
    public class BoosterShuffle : BoosterFunc
    {
        [SerializeField]
        private float speed = 20f;
        [SerializeField]
        private GameObject usePrefab;
        [SerializeField]
        private Vector3 offset;
        [SerializeField]
        private float sweepSpeed = 60f;
        [SerializeField]
        private EaseAnim sweepEase = EaseAnim.EaseInSine;

        #region override
        public override void Apply(GridCell gCell, Action completeCallBack)
        {
            TweenSeq bTS = new TweenSeq();

            //move activeBooster
            Vector3 pos = transform.position;
            Row<GridCell> gRow = MGrid.Rows[MGrid.Rows.Count / 2];
            GridCell gCellL = gRow.cells[0];
            GridCell gCellR = gRow.cells[gRow.cells.Length - 1];

            Vector3 lPos = gCellL.transform.position + offset;
            Vector3 rPos = gCellR.transform.position + offset;

            float dist = Vector2.Distance(pos, lPos);

            bTS.Add((callBack) =>
            {
                GetComponent<SpriteRenderer>().enabled = false;
                SimpleTween.Move(gameObject, pos, lPos, 0.1f).AddCompleteCallBack(() =>
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
                    GameObject g = Creator.InstantiateAnimPrefab(usePrefab, transform, lPos, SortingOrder.Booster);
                    TweenExt.DelayAction(gameObject, 0.7f, () =>
                    {
                        callBack();
                    });// delay
                });
            }
           

            // sweep
            //bTS.Add((callBack) =>
            //{
            //    dist = Vector2.Distance(transform.position, rPos);
            //    SimpleTween.Move(gameObject, transform.position, rPos, dist / sweepSpeed).AddCompleteCallBack(() =>
            //    {
            //        SetActive(gameObject, false, 0.0f);
            //        callBack();
            //    }).SetEase(sweepEase);
            //});

            bTS.Add((callback) =>
            {
                if (gameObject) Destroy(gameObject);
                MBoard.MixGrid(null);
                completeCallBack?.Invoke();
                callback();
            });

            bTS.Start();
        }
        #endregion override
    }
}

