using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mkey {
    [CreateAssetMenu]
    public class PurchaseBoosterPack : ScriptableObject
    {
        public Booster booster;
        public int count;
        public int price;

        [SerializeField]
        private UnityEvent CompleteEvent;
        [SerializeField]
        private UnityEvent FailedEvent;

        private GuiController MGui => GuiController.Instance;

        public void Purchase()
        {
            if (booster)
            {
                if (CoinsHolder.Count >= price)
                {
                    booster.AddCount(count);
                    CoinsHolder.Add(-price);
                    CompleteEvent?.Invoke();
                }
                else
                {
                    FailedEvent?.Invoke();
                }
            }
            else
            {
                FailedEvent?.Invoke();
            }
        }
    }
}