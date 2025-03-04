using UnityEngine;
using UnityEngine.Events;

/*
  
    10.11.2021 temp test
 */
namespace Mkey
{
    public class SetFloatAnimatorParameter : MonoBehaviour
    {
        public Animator animator;
        public string defaultParameter;
        public float minValue = 1;
        public float maxValue = 2;

        public void SetRandomDefaultParameter()
        {
            SetRandomParameter(defaultParameter);
        }

        public void SetRandomParameter(string pName)
        {
            SetParameter(pName, Random.Range(minValue, maxValue));
        }

        public void SetDefaultParameter(float pValue)
        {
            SetParameter(defaultParameter, pValue);
        }

        public void SetParameter(string pName, float pValue)
        {
            if (!animator) animator = GetComponent<Animator>();
            if (animator && !string.IsNullOrEmpty(pName)) animator.SetFloat(pName, pValue);
        }
    }
}