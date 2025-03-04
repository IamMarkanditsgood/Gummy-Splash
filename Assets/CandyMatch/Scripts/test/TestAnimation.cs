using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey {
    public class TestAnimation : MonoBehaviour
    {
        public Animator animator;
        public List<string> triggers;
        public bool autoTestTriggers;
        public Vector2 randomTriggerSetTime;

        private IEnumerator Start()
        {
            yield return null;
            while (true)
            {
                if (autoTestTriggers)
                {
                    float time = UnityEngine.Random.Range(randomTriggerSetTime.x, randomTriggerSetTime.y);
                    yield return new WaitForSeconds(time);
                    string trigger = triggers.GetRandomPos();
                    SetTtigger(trigger);
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public void SetTtigger(string trigger)
        {
            if (!animator) animator = GetComponent<Animator>();
            if (animator) animator.SetTrigger(trigger);
            else Debug.LogError("error animator not found");
        }
    }
}