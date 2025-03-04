using System;
using UnityEngine;

namespace Mkey
{
    public class Whirlwind : MonoBehaviour
    {
        public void Create(Vector3 endPosition, Action completeCallBack)
        {
            TweenSeq ts = new TweenSeq();
            float locScale = transform.localScale.x;
            float dist = Vector3.Magnitude(endPosition - transform.position);

            ts.Add((callBack) =>
            {
                SimpleTween.Value(gameObject, 0, locScale, 0.15f).
                SetOnUpdate((float val)=> { transform.localScale = new Vector3(val,val,val); }).
                AddCompleteCallBack(() =>
                {
                    callBack();
                }).SetEase(EaseAnim.EaseLinear);
            });

            ts.Add((callBack) =>
            {
                SimpleTween.Move(gameObject, transform.position, endPosition, 0.02f * dist).AddCompleteCallBack(() =>
                   {
                       callBack();
                   }).SetEase(EaseAnim.EaseLinear);
            });

            ts.Add((callBack) =>
            {
                SimpleTween.Value(gameObject, locScale, 0, 0.15f).
                SetOnUpdate((float val) => { transform.localScale = new Vector3(val, val, val); }).
                AddCompleteCallBack(() =>
                {
                    callBack();
                }).SetEase(EaseAnim.EaseLinear);
            });

            ts.Add((callBack) =>
            {
                Destroy(gameObject);
                completeCallBack?.Invoke();
                callBack();
            });

            ts.Start();
        }
    }
}