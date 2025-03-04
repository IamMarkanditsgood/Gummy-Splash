using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
  22.10.2020 
 */ 
namespace Mkey
{
    public class ApplicationQuit : MonoBehaviour
    {
        public void Quit()
        {
#if !UNITY_IOS && !UNITY_WEBGL
            Application.Quit();
#endif
        }

#if !UNITY_IOS && !UNITY_WEBGL
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
#endif
    }
}