using System;
using UnityEngine;

namespace Mkey
{
    public class BoosterFunc : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        private new string name;

        protected RectTransform guiHelper;

        #region properties
        public SpriteRenderer SRenderer { get { return (sRenderer) ? sRenderer : sRenderer = GetComponent<SpriteRenderer>(); } }
        public Sprite ObjectImage { get { return (SRenderer) ? SRenderer.sprite : null; } }
        protected GameBoard MBoard => GameBoard.Instance;
        protected GuiController MGui => GuiController.Instance;
        protected SoundMaster MSound => SoundMaster.Instance;
        protected MatchGrid MGrid => MBoard.MainGrid;
        #endregion properties

        #region private
        private SpriteRenderer sRenderer;
        #endregion private

        #region virtual
        public virtual CellsGroup GetArea(GridCell hitGridCell)
        {
            Debug.Log("base get shoot area");
            CellsGroup cG = new CellsGroup();
            return cG;
        }

        public virtual void Apply(GridCell gCell, Action completeCallBack)
        {
            completeCallBack?.Invoke();
        }

        public virtual bool ActivateApply()
        {
            return false;
        }
		
		public virtual bool CanApply(GridCell gCell)
        {
            if (gCell.IsDisabled)
            {
                return false;
            }

            //if (gCell.Blocked)
            //{
            //    return gCell.Blocked.BoosterHit;
            //}

            //if (gCell.DynamicBlocker)
            //{
            //    return gCell.DynamicBlocker.BoosterHit;
            //}

            //if (!gCell.IsMatchable)
            //{
            //    return false;
            //}
          
            return true;
        }
        #endregion virtual

        #region common
        protected void SetActive(GameObject gO, bool active, float delay)
        {
            if (gO)
            {
                if (delay > 0)
                {
                    TweenExt.DelayAction(gO, delay, () => { if (gO) gO.SetActive(active); });
                }
            }
        }
        #endregion common
    }
}