using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey {
    public class LoginWindowController : PopUpsController
    {

        public Toggle rememberTogle;
        private string rememberKey = "mk_remember";

        public bool Remember
        {
            get
            {
                if (!PlayerPrefs.HasKey(rememberKey)) PlayerPrefs.SetInt(rememberKey, 0);
                return PlayerPrefs.GetInt(rememberKey) > 0;
            }
            set { PlayerPrefs.SetInt(rememberKey, (value) ? 1 : 0); }
        }

        void Start()
        {
            rememberTogle.onValueChanged.AddListener((value) =>
            {
                Remember = value;
            });
            rememberTogle.isOn = Remember;
        }


        public void Login_Click()
        {

        }

        public void Signup_Click()
        {

        }

    }
}
