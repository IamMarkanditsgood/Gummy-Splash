using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using Mkey;

namespace MkeyFW
{
    [ExecuteInEditMode]
    public class Sector : MonoBehaviour
    {
        [SerializeField]
        private bool bigWin;
        [SerializeField]
        private UnityEvent  hitEvent;
        [SerializeField]
        public AudioClip hitSound;

        public TextMesh Text { get; private set; }

        public bool BigWin
        {
            get { return bigWin; }
        }

        #region regular
        void Start()
        {
            Text = GetComponent<TextMesh>();
        }

        void OnValidate()
        {

        }
        #endregion regular

        /// <summary>
        /// raise hit event
        /// </summary>
        /// <param name="position"></param>
        public void PlayHit()
        {
            if (hitEvent != null) hitEvent.Invoke();
        }
    }
}