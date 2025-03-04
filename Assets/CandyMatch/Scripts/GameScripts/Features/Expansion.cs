using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class Expansion : MonoBehaviour
    {
        #region properties
        protected SoundMaster MSound { get { return SoundMaster.Instance; } }
        protected GameBoard MBoard { get { return GameBoard.Instance; } }
        protected MatchGrid MGrid { get { return MBoard.MainGrid; } }
        #endregion properties

        #region temp vars
        [SerializeField]
        private Dictionary<int, int> expDictOv;
        private Dictionary<int, int> expDictUnd;
        private Dictionary<int, int> collExpDictOv;
        private Dictionary<int, int> collExpDictUnd;
        private bool use = false;
        #endregion temp vars

        #region regular
        private void Start()
        {
            expDictOv = new Dictionary<int, int>();
            collExpDictOv = new Dictionary<int, int>();
            expDictUnd = new Dictionary<int, int>();
            collExpDictUnd = new Dictionary<int, int>();
            StartCoroutine(StartC());
        }

        private IEnumerator StartC()
        {
            yield return new WaitForSeconds(0.1f);
            while (!MBoard) yield return new WaitForEndOfFrame();

            CalcExpandedOnBoard();
            if (use)
            {
                MBoard.BeforeStepBoardEvent += BeforeStepBoardEventHandler;
                MBoard.AfterStepBoardEvent += AfterStepBoardEventHandler;
            }
        }

        private void OnDestroy()
        {
            if (use && MBoard)
            {
                MBoard.BeforeStepBoardEvent -= BeforeStepBoardEventHandler;
                MBoard.AfterStepBoardEvent  -= AfterStepBoardEventHandler;
            }
        }
        #endregion regular

        private void SetNewUnderlay(int id)
        {
            for (int i = MGrid.Cells.Count - 1; i >= 0; i--)
            {
                GridCell gC = MGrid.Cells[i];
                NeighBors gCN = gC.Neighbors;
                bool containExpanded = false;
                foreach ( var item in gCN.Cells)
                {
                    if(item.HaveObjectWithID(id))
                    {
                        containExpanded = true;
                        break;
                    }
                }

                if (!gC.HaveObjectWithID(id) && !gC.IsDisabled && !gC.Blocked && !gC.StaticMatchBomb && !gC.Underlay && containExpanded)
                {
                    gC.SetObject(id);
                    if (MBoard.Targets.ContainsKey(id)) MBoard.TargetAddEventHandler(id);
                    return;
                }
            }
        }

        private void SetNewOverlay(int id)
        {
            for (int i = MGrid.Cells.Count - 1; i >= 0; i--)
            {
                GridCell gC = MGrid.Cells[i];
                NeighBors gCN = gC.Neighbors;
                bool containExpanded = false;
                foreach (var item in gCN.Cells)
                {
                    if (item.HaveObjectWithID(id))
                    {
                        containExpanded = true;
                        break;
                    }
                }
                if (!gC.HaveObjectWithID(id) && !gC.IsDisabled && !gC.Blocked && !gC.StaticMatchBomb && !gC.Overlay && containExpanded)
                {
                    gC.SetObject(id);
                    if(MBoard.Targets.ContainsKey(id)) MBoard.TargetAddEventHandler(id);
                    return;
                }
            }
        }

        private void CalcExpandedOnBoard()
        {
            expDictOv = new Dictionary<int, int>();
            expDictUnd = new Dictionary<int, int>();

            foreach (var item in MGrid.Cells)
            {
                Expanded e = null;
                if (item.Overlay) e = item.Overlay.GetComponent<Expanded>();
                if (e)
                {
                    use = true;
                    if (!expDictOv.ContainsKey(e.ID))
                    {
                        expDictOv.Add(e.ID, item.Overlay.Protection);
                    }
                    else
                    {
                        expDictOv[e.ID] += item.Overlay.Protection;
                    }
                }
                e = null;

                if (item.Underlay) e = item.Underlay.GetComponent<Expanded>();
                if (e)
                {
                    use = true;
                    if (!expDictUnd.ContainsKey(e.ID))
                    {
                        expDictUnd.Add(e.ID, item.Underlay.Protection);
                    }
                    else
                    {
                        expDictUnd[e.ID] += item.Underlay.Protection;
                    }
                }
            }
        }

        /// <summary>
        ///  search expanded objects on board
        /// </summary>
        private void CalcCollectedExpanded()
        {
            collExpDictOv = new Dictionary<int, int>();
            collExpDictUnd = new Dictionary<int, int>();

            foreach (var item in MGrid.Cells)
            {
                Expanded e = null;
                if (item.Overlay) e = item.Overlay.GetComponent<Expanded>();
                if (e)
                {
                    use = true;
                    if (!collExpDictOv.ContainsKey(e.ID))
                    {
                        collExpDictOv.Add(e.ID, item.Overlay.Protection);
                    }
                    else
                    {
                        collExpDictOv[e.ID] += item.Overlay.Protection;
                    }
                }
                e = null;

                if (item.Underlay) e = item.Underlay.GetComponent<Expanded>();
                if (e)
                {
                    use = true;
                    if (!collExpDictUnd.ContainsKey(e.ID))
                    {
                        collExpDictUnd.Add(e.ID, item.Underlay.Protection);
                    }
                    else
                    {
                        collExpDictUnd[e.ID] += item.Underlay.Protection;
                    }
                }
            }
        }

        #region eventhandler
        private void BeforeStepBoardEventHandler(GameBoard mB)
        {
            CalcExpandedOnBoard();
            Debug.Log("ExpDictOv " + expDictOv.Count);
        }

        private void AfterStepBoardEventHandler(GameBoard mB)
        {
            CalcCollectedExpanded();
            Debug.Log("collExpDictOv " + collExpDictOv.Count);

            foreach (var item in expDictOv)
            {
                if (collExpDictOv.ContainsKey(item.Key) && expDictOv[item.Key]<= collExpDictOv[item.Key]) SetNewOverlay(item.Key);
            }

            foreach (var item in expDictUnd)
            {
                if (collExpDictUnd.ContainsKey(item.Key) && expDictUnd[item.Key] <= collExpDictUnd[item.Key]) SetNewUnderlay(item.Key);
            }
        }
        #endregion eventhandler
    }
}