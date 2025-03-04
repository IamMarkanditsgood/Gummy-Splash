using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
    public enum DirMather { None, Top, Right, Bottom, Left }
	public class MatherButton : TouchPadMessageTarget
	{
        public DirMather dirMather;
        public Sprite enabledSprite;
        public Sprite disabledSprite;
        public bool IsActive { get; private set; }
        [SerializeField]
        private SpriteRenderer sRenderer;
        [SerializeField]
        private MatherButton[] matherButtons;

        #region events
        public Action<MatherButton> clickEventAction;
        #endregion events

        #region temp vars

        #endregion temp vars

        #region regular
        private void Start()
		{
            PointerDownEvent += (tpea) =>
            {
                if (!IsActive) DisableButtons();
                SetActive(!IsActive);
                clickEventAction?.Invoke(this);
            };
        }
		#endregion regular

        public void SetActive(bool active)
        {
            IsActive = active;
            if (sRenderer) sRenderer.sprite = (active) ? enabledSprite : disabledSprite;
        }

        private void DisableButtons()
        {
            foreach (var item in matherButtons)
            {
                if (item) item.SetActive(false);
            }
        }
	}
}
