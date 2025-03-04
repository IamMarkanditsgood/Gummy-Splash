using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class changerenderorder : MonoBehaviour
    {
        public void IncreaseRenderOrder(int val)
        {
            SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var item in spriteRenderers)
            {
                item.sortingOrder += val;
            }
        }
    }
}