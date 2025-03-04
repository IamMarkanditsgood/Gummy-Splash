using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public class Expanded : MonoBehaviour
	{
        public int ID { get; private set; } //parent grid object ID

        #region regular
        private void Start()
        {
            GridObject gridObject = GetComponent<GridObject>();
            ID = gridObject.ID;
        }
        #endregion regular
    }
}
