using System;
using System.Collections.Generic;

namespace Mkey
{
    [Serializable]
    /// <summary>
    /// Get neighborns for gridcell 
    /// </summary>
    public class NeighBors
    {
        public GridCell Main { get; private set; }
        public GridCell Left { get; private set; }
        public GridCell Right { get; private set; }
        public GridCell Top { get; private set; }
        public GridCell Bottom { get; private set; }

        public GridCell TopLeft { get; private set; }
        public GridCell BottomLeft { get; private set; }
        public GridCell TopRight { get; private set; }
        public GridCell BottomRight { get; private set; }

        public GridCell TopEnabled { get; private set; } // first enabled cell at top

        public List<GridCell> Cells { get; private set; }

        private List<PFCell> listTELBR;
        private List<PFCell> listTLBR;

        /// <summary>
        /// Create NeighBorns  cells
        /// </summary>
        /// <param name="main"></param>
        /// <param name="id"></param>
        public NeighBors(GridCell main, bool addDiag)
        {
            Main = main;
            Left = main.GRow[main.Column - 1];
            Right = main.GRow[main.Column + 1];
            Top = main.GColumn[main.Row - 1];
            Bottom = main.GColumn[main.Row + 1];

            Cells = new List<GridCell>();
            if (Top) Cells.Add(Top);
            if (Bottom) Cells.Add(Bottom);
            if (Left) Cells.Add(Left);
            if (Right) Cells.Add(Right);

            if (addDiag)
            {
                TopLeft = (Top) ? Top.GRow[Top.Column - 1] : null;
                BottomLeft = (Bottom) ? Bottom.GRow[Bottom.Column - 1] : null; 
                TopRight = (Top) ? Top.GRow[Top.Column + 1] : null;
                BottomRight = (Bottom) ? Bottom.GRow[Bottom.Column + 1] : null;

                Cells = new List<GridCell>();
                if (Top) Cells.Add(Top);
                if (TopLeft) Cells.Add(TopLeft);
                if (Left) Cells.Add(Left);
                if (BottomLeft) Cells.Add(BottomLeft);

                if (Bottom) Cells.Add(Bottom);
                if (BottomRight) Cells.Add(BottomRight);
                if (Right) Cells.Add(Right);
                if (TopRight) Cells.Add(TopRight);
            }

            listTELBR = new List<PFCell>(4);
            listTLBR = new List<PFCell>(4);
        }

        public bool Contain(GridCell gCell)
        {
            return Cells.Contains(gCell);
        }

        public override string ToString()
        {
            return ("All cells : " + ToString(Cells));
        }

        public static string ToString(List<GridCell> list)
        {
            string res = "";
            foreach (var item in list)
            {
                res += item.ToString();
            }
            return res;
        }

        #region path finder
        public List<PFCell> GetNeighBorsPF()
        {
            List<PFCell> res = new List<PFCell>();
            foreach (var item in Cells)
            {
                res.Add(item.PathCell);
            }

            return res;
        }

        /// <summary>
        /// Top, Left, Bottom, Right
        /// </summary>
        /// <returns></returns>
        public List<PFCell> GetNeighBorsPF_TLBR()
        {
            listTLBR.Clear();
            if (Top) listTLBR.Add(Top.PathCell);
            if (Left) listTLBR.Add(Left.PathCell);
            if (Right) listTLBR.Add(Right.PathCell);
            if (Bottom) listTLBR.Add(Bottom.PathCell);
            return listTLBR;
        }

        /// <summary>
        /// TopEnabled, Left, Bottom, Right
        /// </summary>
        /// <returns></returns>
        public List<PFCell> GetNeighBorsPF_TELBR()
        {
            // search first enabled cell at top
            if (Top && !TopEnabled)
            {
                for (int i = Top.Row; i >= 0; i--)
                {
                    if (Main.GColumn[i] && !Main.GColumn[i].IsDisabled)
                    {
                        TopEnabled = Main.GColumn[i];
                        break;
                    }
                }
            }
            listTELBR.Clear();
            if (TopEnabled) listTELBR.Add(TopEnabled.PathCell);
            if (Left) listTELBR.Add(Left.PathCell);
            if (Right) listTELBR.Add(Right.PathCell);
            if (Bottom) listTELBR.Add(Bottom.PathCell);
            return listTELBR;
        }

        /// <summary>
        /// Left, Bottom, Right
        /// </summary>
        /// <returns></returns>
        public List<PFCell> GetNeighBorsPF_LBR(bool withOutTop)
        {
            List<PFCell> res = new List<PFCell>();
            if (Left) res.Add(Left.PathCell);
            if (Bottom) res.Add(Bottom.PathCell);
            if (Right) res.Add(Right.PathCell);
            return res;
        }

        public List<PFCell> GetNeighBorsPF(bool withOutTop)
        {
            List<PFCell> res = new List<PFCell>();
            foreach (var item in Cells)
            {
               if(!withOutTop) res.Add(item.PathCell);
                else
                {
                    if (item == (Bottom || BottomLeft || BottomRight)) res.Add(item.PathCell);
                }
            }
            return res;
        }
        #endregion path finder

        /// <summary>
        /// return neighsors with match id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="matchable"></param>
        /// <returns></returns>
        public List<GridCell> GetMatchIdCells(int id, bool matchable)
        {
            List<GridCell> matchIDcells = new List<GridCell>();
            MatchObject m;
            foreach (var item in Cells)
            {
                m = item.Match;
                if (m &&(m.ID == id) && (item.IsMatchable == matchable))
                {
                    matchIDcells.Add(item);
                }
            }
            return matchIDcells;
        }
    }
}