using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mkey {
    public class PurchaseBooster : MonoBehaviour
    {
        [SerializeField]
        private Booster booster;
        [SerializeField]
        private int count = 1;
        [SerializeField]
        private int price = 5;

        [SerializeField]
        private UnityEvent CompleteEvent;
        [SerializeField]
        private UnityEvent FailedEvent;

        private GuiController MGui => GuiController.Instance;

        public void Purchase()
        {
            if(CoinsHolder.Count >= price)
            {
                if (booster)
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