using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class MatchGrid
    {
        private GameObjectsSet goSet;
        public List<Column<GridCell>> Columns { get; private set; }
        public List<GridCell> Cells { get; private set; }
        public List<Row<GridCell>> Rows { get; private set; }
        public Transform Parent { get; private set; }
        private int sortingOrder;
        private GameMode gMode;
        private int  vertSize;
        private int horSize;
        private GameObject prefab;
        private float yStart; // Camera.main.orthographicSize - radius
        private float yStep;
        private float xStep;
        private int yOffset;
        private Vector2 cellSize;
        private float cOffset;
        private MatchGroupsHelper matchGroups;

        public bool haveFillPath = false;
        public Vector2 Step { get { return new Vector2(xStep, yStep); } }
        public LevelConstructSet LcSet { get; private set; }

        public MatchGrid(LevelConstructSet lcSet, GameObjectsSet goSet, Transform parent, int sortingOrder, GameMode gMode)
        {
            matchGroups = new MatchGroupsHelper(this);

            this.LcSet = lcSet;
            this.Parent = parent;
            this.gMode = gMode;
            this.sortingOrder = sortingOrder;
            Debug.Log("new grid " + lcSet.name);

            vertSize = lcSet.VertSize;
            horSize = lcSet.HorSize;
            this.goSet = goSet;
            prefab = goSet.gridCellEven;
            cellSize = prefab.GetComponent<BoxCollider2D>().size;

            float deltaX = lcSet.DistX;
            float deltaY = lcSet.DistY;
            float scale = lcSet.Scale;
            parent.localScale = new Vector3(scale, scale, scale);

            Cells = new List<GridCell>(vertSize * horSize);
            Rows = new List<Row<GridCell>>(vertSize);

            yOffset = 0;
            xStep = (cellSize.x + deltaX);
            yStep = (cellSize.y + deltaY);
           
            cOffset = (1 - horSize) * xStep / 2.0f; // offset from center by x-axe
            yStart = (vertSize-1)  * yStep / 2.0f;

            //instantiate cells
            for (int i = 0; i < vertSize; i++)
            {
                AddRow();
            }
            SetObjectsData(lcSet, gMode);
            Debug.Log("create cells: " + Cells.Count);
        }

        public void Rebuild(GameObjectsSet mSet, GameMode gMode)
        {
            Debug.Log("rebuild ");

            this.LcSet = LcSet;
            vertSize = LcSet.VertSize;
            horSize = LcSet.HorSize;

            float deltaX = LcSet.DistX;
            float deltaY = LcSet.DistY;
            float scale = LcSet.Scale;
            Parent.localScale = new Vector3(scale,scale,scale);

            this.goSet = mSet;
            Cells.ForEach((c) => { c.DestroyGridObjects(); });

            List<GridCell> tempCells = Cells;
            Cells = new List<GridCell>(vertSize * horSize + horSize);
            Rows = new List<Row<GridCell>>(vertSize);

            xStep = (cellSize.x + deltaX);
            yStep = (cellSize.y + deltaY);

            cOffset = (1 - horSize) * xStep / 2.0f; // offset from center by x-axe
            yStart = (vertSize - 1) * yStep / 2.0f;

            // create rows 
            GridCell cell;
            Row<GridCell> row;
            int cellCounter = 0;
            int ri = 0;
            Sprite sRE = mSet.gridCellEven.GetComponent<SpriteRenderer>().sprite;
            Sprite sRO = mSet.gridCellOdd.GetComponent<SpriteRenderer>().sprite;

            for (int i = 0; i < vertSize; i++)
            {
                bool isEvenRow = (i % 2 == 0);
                row = new Row<GridCell>(horSize);

                for (int j = 0; j < row.Length; j++)
                {
                    bool isEvenColumn = (j % 2 == 0);
                    Vector3 pos = new Vector3(j * xStep + cOffset, yStart - i * yStep, 0);

                    if (tempCells != null && cellCounter < tempCells.Count)
                    {
                        cell = tempCells[cellCounter];
                        cell.gameObject.SetActive(true);
                        cell.transform.localPosition = pos;
                        cellCounter++;
                        SpriteRenderer sR = cell.GetComponent<SpriteRenderer>();
                        if (sR)
                        {
                            sR.enabled = true;
                            if (isEvenRow) sR.sprite = (!isEvenColumn) ? sRO : sRE;
                            else sR.sprite = (isEvenColumn) ? sRO : sRE;
                        }
                    }
                    else
                    {
                        if (isEvenRow)
                            cell = UnityEngine.Object.Instantiate((!isEvenColumn) ? mSet.gridCellOdd : mSet.gridCellEven).GetComponent<GridCell>();
                        else
                            cell = UnityEngine.Object.Instantiate((isEvenColumn) ? mSet.gridCellOdd : mSet.gridCellEven).GetComponent<GridCell>();
                        cell.transform.parent = Parent;
                        cell.transform.localPosition = pos;
                        cell.transform.localScale = Vector3.one;
                    }


                    Cells.Add(cell);
                    row[j] = cell;
                }
                Rows.Add(row);
                ri++;
            }

            // destroy not used cells
            if (cellCounter < tempCells.Count)
            {
                for (int i = cellCounter; i < tempCells.Count; i++)
                {
                    UnityEngine.Object.Destroy(tempCells[i].gameObject);
                }
            }

            // cache columns
            Columns = new List<Column<GridCell>>(horSize);
            Column<GridCell> column;
            for (int c = 0; c < horSize; c++)
            {
                column = new Column<GridCell>(Rows.Count);
                for (int r = 0; r < Rows.Count; r++)
                {
                    column[r] = Rows[r][c];
                }
                Columns.Add(column);
            }

            for (int r = 0; r < Rows.Count; r++)
            {
                for (int c = 0; c < horSize; c++)
                {
                    Rows[r][c].Init(r, c, Columns[c], Rows[r], this, gMode);
                }
            }
            SetObjectsData(LcSet, gMode);

            Debug.Log("rebuild cells: " + Cells.Count);
        }

        /// <summary>
        /// set objects data from featured list to grid
        /// </summary>
        /// <param name="featCells"></param>
        /// <param name="gMode"></param>
        internal void SetObjectsData(LevelConstructSet lcSet, GameMode gMode)
        {
            if (lcSet.cells != null)
            {
                // first set disabled objects
                foreach (var c in lcSet.cells)
                {
                    if (c != null && c.gridObjects != null)
                    {
                        foreach (var o in c.gridObjects)
                        {
                            if (GameObjectsSet.IsDisabledObject(o.id) && c.row < Rows.Count && c.column < Rows[c.row].Length) Rows[c.row][c.column].SetObject(o.id, o.hits);
                        }
                    }
                }

                // set not disabled objects
                foreach (var c in lcSet.cells)
                {
                    if (c != null && c.gridObjects != null)
                    {
                        foreach (var o in c.gridObjects)
                        {
                            if (!GameObjectsSet.IsDisabledObject(o.id) && c.row < Rows.Count && c.column < Rows[c.row].Length) Rows[c.row][c.column].SetObject(o.id, o.hits);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add row to grid
        /// </summary>
        private void AddRow()
        {
            GridCell cell;
            Row<GridCell> row = new Row<GridCell>(horSize);
            bool isEvenRow = (Rows.Count % 2 == 0);
            for (int j = 0; j < row.Length; j++)
            {
                bool isEvenColumn = (j % 2 == 0);
                Vector3 pos = new Vector3(j * xStep + cOffset, yStart + yOffset * yStep, 0);

                if (isEvenRow)
                    cell = UnityEngine.Object.Instantiate((!isEvenColumn) ? goSet.gridCellOdd : goSet.gridCellEven).GetComponent<GridCell>();
                else
                    cell = UnityEngine.Object.Instantiate((isEvenColumn) ? goSet.gridCellOdd : goSet.gridCellEven).GetComponent<GridCell>();

                cell.transform.parent = Parent;
                cell.transform.localPosition = pos;
                cell.transform.localScale = Vector3.one;
                Cells.Add(cell);
                row[j] = cell;
            }

            Rows.Add(row);

            // cache columns
            Columns = new List<Column<GridCell>>(horSize);
            Column<GridCell> column; 
            for (int c = 0; c < horSize; c++)
            {
                column = new Column<GridCell>(Rows.Count);
                for (int r = 0; r < Rows.Count; r++)
                {
                    column[r] = Rows[r][c];
                }
                Columns.Add(column);
            }

      //      Debug.Log("rows: " + Rows.Count +  " ;columns count: " + columns.Count);
            for (int r = 0; r < Rows.Count; r++)
            {
                for (int c = 0; c < horSize; c++)
                {
                    Rows[r][c].Init(r, c, Columns[c], Rows[r], this, gMode);
                }
            }

            yOffset--;
        }

        public GridCell this[int index0, int index1]
        {
            get { if (ok(index0, index1)) { return Rows[index0][index1]; } else { return null; } }
            set { if (ok(index0, index1)) { Rows[index0][index1] = value; } else {  } }
        }

        private bool ok(int index0, int index1)
        {
            return (index0 >= 0 && index0 < vertSize && index1 >= 0 && index1 < horSize);
        }

        /// <summary>
        ///  return true if cells not simulate physics
        /// </summary>
        /// <returns></returns>
        internal bool NoPhys()
        {
            foreach (GridCell c in Cells)
            {
                if (c.PhysStep) return false;
            }
            return true;
        }

        #region  get data from grid
        public MatchGroupsHelper GetMatches(int minMatch)
        {
            MatchGroupsHelper mgh = new MatchGroupsHelper(this);
            mgh.CreateGroups(minMatch);
            return mgh;
        }

        internal List<GridCell> GetEqualCells(GridCell gCell)
        {
            List<GridCell> gCells = new List<GridCell>();
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].IsMatchObjectEquals(gCell))
                {
                    gCells.Add(Cells[i]);
                }
            }
            return gCells;
        }

        internal List<GridCell> GetNeighCells(GridCell gCell, bool useDiagCells)
        {
            List<GridCell> nCells = new List<GridCell>();
            int row = gCell.Row;
            int column = gCell.Column;

            GridCell c = this[row, column - 1]; if (c) nCells.Add(c); // left
            c = this[row - 1, column]; if (c) nCells.Add(c); //  top
            c = this[row, column + 1]; if (c) nCells.Add(c); // right
            c = this[row + 1, column]; if (c) nCells.Add(c); // bot

            if (useDiagCells)
            {
                c = this[row + 1, column - 1]; if (c) nCells.Add(c); // bot - left
                c = this[row - 1, column - 1]; if (c) nCells.Add(c); // top - left
                c = this[row - 1, column + 1]; if (c) nCells.Add(c); // top right
                c = this[row + 1, column + 1]; if (c) nCells.Add(c); // bot- right
            }
            return nCells;
        }

        /// <summary>
        /// Return not blocked, not disabled cells without dynamic object
        /// </summary>
        /// <returns></returns>
        internal List<GridCell> GetFreeCells()
        {
            List<GridCell> gcL = new List<GridCell>();
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].IsDynamicFree && !Cells[i].Blocked && !Cells[i].IsDisabled)
                {
                    gcL.Add(Cells[i]);
                }
            }
            return gcL;
        }

        /// <summary>
        /// Return not blocked, not disabled cells without dynamic object, with fillPath or with and without
        /// </summary>
        /// <returns></returns>
        internal List<GridCell> GetFreeCells(bool withPath)
        {
            List<GridCell> gcL = new List<GridCell>();
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].IsDynamicFree && !Cells[i].Blocked && !Cells[i].IsDisabled)
                {
                    if (withPath && Cells[i].HaveFillPath() || !withPath)
                        gcL.Add(Cells[i]);
                }
            }
            return gcL;
        }

        /// <summary>
        /// Return objects count on grid with selected ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetObjectsCountByID(int id)
        {
            int res = 0;
            GridObject[] bds = Parent.GetComponentsInChildren<GridObject>();
            foreach (var item in bds)
            {
                if (item.ID == id) res++;
            }

            return res;
        }

        /// <summary>
        /// Return cells with object ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<GridCell> GetAllByID(int id)
        {
            List<GridCell> res = new List<GridCell>();
            foreach (var item in Cells)
            {
                if (item.HaveObjectWithID(id))
                {
                    res.Add(item);
                }
            }
            return res;
        }

        /// <summary>
        /// Return cells with object ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<GridCell> GetAllByTargetID(int targetID)
        {
            List<GridCell> res = new List<GridCell>();
            foreach (var item in Cells)
            {
                if (item.HaveObjectWithTargetID(targetID))
                {
                    res.Add(item);
                }
            }
            return res;
        }

        /// <summary>
        /// Return grid objects with  target ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<GridObject> GetObjectsByTargetID(int targetID)
        {
            List<GridObject> res = new List<GridObject>();
            foreach (var item in Cells)
            {
                GridObject gO = item.GetObjectWithTargetID(targetID);
                if (gO)
                {
                    res.Add(gO);
                }
            }
            return res;
        }

        public void CalcObjects()
        {
            GridObject[] bds = Parent.GetComponentsInChildren<GridObject>();
            Debug.Log("Objects count: " + bds.Length);
        }

        /// <summary>
        /// Get chess distance
        /// </summary>
        /// <returns></returns>
        public static int GetChessDist(GridCell gc1, GridCell gc2)
        {
            return (Mathf.Abs(gc1.Row - gc2.Row) + Mathf.Abs (gc1.Column - gc2.Column));
        }

        /// <summary>
        /// Get chess distance
        /// </summary>
        /// <returns></returns>
        public static GridCell GetChessNear(GridCell gCell, IEnumerable<GridCell> area)
        {
            int dist = Int32.MaxValue;
            GridCell nearItem = null;
            if (gCell && area != null)
            {
                foreach (GridCell c in area)
                {
                    int dist2 = GetChessDist(c, gCell);
                    if (dist2 < dist)
                    {
                        nearItem = c;
                        dist = dist2;
                    }
                }
            }
            return nearItem;
        }

        /// <summary>
        /// Return random match cell list exclude matches with overlay and underlay
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<GridCell> GetRandomMatch(int count)
        {
            List<GridCell> temp = new List<GridCell>(Cells.Count);
            List<GridCell> res = new List<GridCell>(count);

            foreach (var item in Cells)
            {
                if (item.Match && !item.Overlay && !item.Underlay)
                {
                    temp.Add(item);
                }
            }
            temp.Shuffle();
            count = Mathf.Min(count, temp.Count);

            for (int i = 0; i < count; i++)
            {
                res.Add(temp[i]);
            }
            return res;
        }


        /// <summary>
        /// Return random match cell list
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<GridCell> GetRandomMatch(int count, bool excludeOverlayMatch, bool excludeUnderlayMatch)
        {
            List<GridCell> temp = new List<GridCell>(Cells.Count);
            List<GridCell> res = new List<GridCell>(count);

            foreach (var item in Cells)
            {
                if (item.Match)
                {
                    if (item.Overlay && excludeOverlayMatch) continue;
                    else if (item.Underlay && excludeUnderlayMatch) continue;
                    else temp.Add(item);
                }
            }
            temp.Shuffle();
            count = Mathf.Min(count, temp.Count);

            for (int i = 0; i < count; i++)
            {
                res.Add(temp[i]);
            }
            return res;
        }

        public GridCell GetBomb()
        {
            foreach (var item in Cells)
            {
                if (item.HasBomb)
                {
                    if(item.Match && item.IsMatchable)
                        return item;
                    if (!item.Match)
                        return item;
                }
            }
            return null;
        }


        public List<GridCell> GetAllBombs(bool noExplodeProtected)
        {
            List<GridCell> res = new();
            foreach (var item in Cells)
            {
                if (item.HasBomb)
                {
                    if (!item.Overlay)
                    {
                        res.Add(item);
                    }
                    else if (!item.Overlay.BlockMatch)
                    {
                        res.Add(item);
                    }
                }
            }
            return res;
        }

        public List<GridCell> GetBottomEnabledCells()
        {
            List<GridCell> res = new List<GridCell>();
            foreach (var item in Columns)
            {
                res.Add(item.GetMaxUsed());
            }

            return res;
        }

        public List<GridCell> GetBottomDynCells()
        {
            // get all mather cells
            List<GridCell> mathers = new List<GridCell>();
            Cells.ForEach((c) =>
            {
                if (c.GravityMather && !mathers.Contains(c.GravityMather))
                {
                    mathers.Add(c.GravityMather);
                }
            });

            List<GridCell> res = new List<GridCell>();
            Cells.ForEach((c) =>
            {
                if (c.DynamicObject || (!c.Blocked && !c.IsDisabled))
                {
                    if (!mathers.Contains(c) && !res.Contains(c))
                    {
                        res.Add(c);
                    }
                }
            });

            return res;
        }

        public Row<GridCell> GetRow(int row)
        {
            return (row >= 0 && row < Rows.Count) ? Rows[row] : null;
        }

        public Column<GridCell> GetColumn(int col)
        {
            return (col >= 0 && col < Columns.Count) ? Columns[col] : null;
        }

        public  CellsGroup GetWave(GridCell gCell, int radius)
        {
            radius = Mathf.Max(0,radius);
            CellsGroup res = new CellsGroup();
            int row1 = gCell.Row - radius;
            int row2 = gCell.Row + radius;
            int col1 = gCell.Column - radius;
            int col2 = gCell.Column + radius;
            Row<GridCell> topRow = GetRow(row1);
            Row<GridCell> botRow = GetRow(row2);
            Column<GridCell> leftCol = GetColumn(col1);
            Column<GridCell> rightCol = GetColumn(col2);

            if (topRow != null)
            {
                for (int i  = col1; i <= col2; i++)
                {
                    if (ok(row1, i)) res.Add(topRow[i]);
                }
            }

            if (rightCol != null)
            {
                for (int i = row1; i <= row2; i++)
                {
                    if (ok(i, col2)) res.Add(rightCol[i]);
                }
            }

            if (botRow != null)
            {
                for (int i = col2; i >= col1; i--)
                {
                    if (ok(row2, i)) res.Add(botRow[i]);
                }
            }

            if (leftCol != null)
            {
                for (int i = row2; i >= row1; i--)
                {
                    if (ok(i, col1)) res.Add(leftCol[i]);
                }
            }

            return res;
        }

        public CellsGroup GetAroundArea(GridCell gCell, int radius)
        {
            radius = Mathf.Max(0, radius);
            CellsGroup res = new CellsGroup();
            if (radius > 0)
                for (int i = 1; i <= radius; i++)
                {
                    res.AddRange(GetWave(gCell, i).Cells);
                }
            return res;
        }

        /// <summary>
        /// Return gridcells group with id matched  around gCell
        /// </summary>
        /// <param name="gCell"></param>
        /// <returns></returns>
        public MatchGroup GetMatchIdArea(GridCell gCell)
        {
            MatchGroup res = new MatchGroup();
            if (!gCell.Match || !gCell.IsMatchable) return res;

            MatchGroup equalNeigh = new MatchGroup();
            MatchGroup neighTemp;
            int id = gCell.Match.ID;
            res.Add(gCell);

            equalNeigh.AddRange(gCell.Neighbors.GetMatchIdCells(id, true)); //equalNeigh.AddRange(gCell.EqualNeighBornCells());
            while (equalNeigh.Length > 0)
            {
                res.AddRange(equalNeigh.Cells);
                neighTemp = new MatchGroup();
                foreach (var item in equalNeigh.Cells)
                {
                    neighTemp.AddRange(item.Neighbors.GetMatchIdCells(id, true)); // neighTemp.AddRange(item.EqualNeighBornCells());
                }
                equalNeigh = neighTemp;
                equalNeigh.Remove(res.Cells);
            }
            return res;
        }

        public CellsGroup GetCellsWithMatchObjects(bool onlyMatchable)
        {
            CellsGroup res = new CellsGroup();
            foreach (var item in Cells)
            {
                if (item.Match) 
                { 

                   if(!onlyMatchable) res.Add(item); 
                   else if(item.IsMatchable) res.Add(item);
                }
            }
            return res;
        }

        public Dictionary<int, CellsGroup> GetCellsWithMatchObjectsDict(bool onlyMatchable)
        {
            CellsGroup mCells = GetCellsWithMatchObjects(onlyMatchable);
            Dictionary<int, CellsGroup> res = new Dictionary<int, CellsGroup>();
            foreach (var item in mCells.Cells)
            {
                if (res.ContainsKey(item.Match.ID))
                {
                    res[item.Match.ID].Add(item);
                }
                else
                {
                    res.Add(item.Match.ID, new CellsGroup());
                    res[item.Match.ID].Add(item);
                }
            }
            return res;
        }
        #endregion  get data from grid

        #region fill grid
        /// <summary>
        /// Fill grid with random regular objects, preserve existing dynamic objects (match, click bomb, falling)
        /// </summary>
        /// <param name="noMatches"></param>
        /// <param name="goSet"></param>
        internal void FillGrid(bool noMatches)
        {
            SpawnController sC = SpawnController.Instance;
            Debug.Log("fill grid, remove matches: " + noMatches);
            for (int i = 0; i < Cells.Count; i++)
            {
                if (!Cells[i].Blocked && !Cells[i].IsDisabled && !Cells[i].DynamicObject && Cells[i].HaveFillPath()) // fill grid after creating fill pathes 07.12.2023
                {
                    MatchObject m = sC.GetMainRandomObjectPrefab(LcSet, goSet);
                    Cells[i].SetObject(m);
                }
            }
            if (noMatches)
            {
                RemoveMatches();
            }
        }

        internal void RemoveMatches()
        {
            SpawnController sC = SpawnController.Instance;
            int minMatch = 3;
            GridCell[] gc_row = new GridCell[minMatch];
            GridCell[] gc_col = new GridCell[minMatch];
            System.Func<GridCell[], bool> isEqual = (gcl) =>
            {
                if (gcl == null || gcl.Length == 0) return false;
                foreach (var item in gcl)
                    if (!item || !item.Match) return false;

                int id = gcl[0].Match.ID;

                foreach (var item in gcl)
                    if (item.Match.ID != id) return false;
                return true;
            };
            List<GridObject> mod_list;
            for (int i = 0; i < vertSize; i++)
            {
                for (int j = 0; j < horSize; j++)
                {
                    if (Rows[i][j].Blocked || Rows[i][j].IsDisabled) continue;
                    for (int m = 0; m < minMatch; m++)
                    {
                        gc_row[m] = this[i, j - m];
                        gc_col[m] = this[i - m, j];
                    }
                    mod_list = new List<GridObject>();
                    bool rowHasMatches = false;
                    bool colHasMatches = false;

                    if (isEqual(gc_row)) rowHasMatches = true;
                    if (isEqual(gc_col)) colHasMatches = true;

                    if (rowHasMatches || colHasMatches)
                    {
                        if (gc_col[1] && gc_col[1].Match) mod_list.Add(sC.GetMainObjectPrefab(goSet, gc_col[1].Match.ID));
                        if (gc_row[1] && gc_row[1].Match) mod_list.Add(sC.GetMainObjectPrefab(goSet, gc_row[1].Match.ID));
                    }
                    if (mod_list.Count > 0) Rows[i][j].GetComponent<GridCell>().SetObject((sC.GetMainRandomObjectPrefab(LcSet, goSet, mod_list)));
                }
            }
#if UNITY_EDITOR
            // double test
            for (int i = 0; i < vertSize; i++)
            {
                for (int j = 0; j < horSize; j++)
                {
                    if (Rows[i][j].Blocked || Rows[i][j].IsDisabled) continue;
                    for (int m = 0; m < minMatch; m++)
                    {
                        gc_row[m] = this[i, j - m];
                        gc_col[m] = this[i - m, j];
                    }
                    mod_list = new List<GridObject>();
                    bool rowHasMatches = false;
                    bool colHasMatches = false;

                    if (isEqual(gc_row)) rowHasMatches = true;
                    if (isEqual(gc_col)) colHasMatches = true;

                    if (rowHasMatches || colHasMatches)
                    {
                        if (gc_col[1] && gc_col[1].Match) mod_list.Add(sC.GetMainObjectPrefab(goSet, gc_col[1].Match.ID));
                        if (gc_row[1] && gc_row[1].Match) mod_list.Add(sC.GetMainObjectPrefab(goSet, gc_row[1].Match.ID));
                        Debug.Log("----------------------------Found matches--------------------------------------");
                    }
                    if (mod_list.Count > 0) Rows[i][j].GetComponent<GridCell>().SetObject((sC.GetMainRandomObjectPrefab(LcSet, goSet, mod_list)));
                }
            }
#endif
        }
        #endregion fill grid

        #region test grid
        // наличие матчей
        // наличие предматчей
        /// <summary>
        /// Fill grid with random regular objects, preserve existing dynamic objects (match, click bomb, falling)
        /// </summary>
        /// <param name="noMatches"></param>
        /// <param name="goSet"></param>
        internal void FillGrid_exclude(bool noMatches)
        {
            SpawnController sC = SpawnController.Instance;
            List<MatchObject> gridObjects = LcSet.GetMatchObjects(goSet);
            if (gridObjects.Count < 2)
            {
                // Debug.Log("very few objects to fill the playing field: " + gridObjects.Count);
                return;
            }

            // Debug.Log("fill grid new, remove matches: " + noMatches + "; match objects for level: " + gridObjects.Count);
            MatchObject matchObjectExclude = gridObjects.GetRandomPos();
            // Debug.Log("matchObjectExclude: " + matchObjectExclude);

            List<GridObject> gridObjectsExclude = new List<GridObject>(new GridObject[] { matchObjectExclude });
            List<GridCell> filledCells = new ();
            int matchHierarchy = 0;

            // check hier cells
            List<GridCell> cellsToFill = new ();
            for (int i = 0; i < Cells.Count; i++)
            {
                if (!Cells[i].GetHierarchyObject(matchHierarchy) && Cells[i].HaveFillPath()) // fill grid after creating fill pathes 07.12.2023
                {
                    cellsToFill.Add(Cells[i]);
                }
            }

            if (cellsToFill.Count == 0) return;                                             // 

            Action fill = () =>
            {
                for (int i = 0; i < cellsToFill.Count; i++)
                {
                  //  if (!Cells[i].GetHierarchyObject(matchHierarchy) && Cells[i].HaveFillPath()) // fill grid after creating fill pathes 07.12.2023
                    {
                        MatchObject m = sC.GetMainRandomObjectPrefab(LcSet, goSet, gridObjectsExclude);
                      //  Debug.Log("fill set object: " + m);
                        cellsToFill[i].SetObject(m); // Cells[i].SetObject(m);
                        filledCells.Add(cellsToFill[i]);
                    }
                }
            };

            Action reFill = () =>
            {
                for (int i = 0; i < filledCells.Count; i++)
                {
                    MatchObject m = sC.GetMainRandomObjectPrefab(LcSet, goSet, gridObjectsExclude);
                    filledCells[i].SetObject(m);
                }
            };

            if (gridObjects.Count == 2) { ChestFill(cellsToFill, gridObjects[0], gridObjects[1]);  }
            else
            {
                fill();
            }


            if (noMatches)
            {
                bool matchesOk = false;
                bool estMatchesOk = false;
                int loops = 0;

                while (!matchesOk || !estMatchesOk)
                {
                    matchGroups.CreateGroups(3);

                    // remove all matches
                    if (matchGroups.MatchesLength > 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                          DestroyMatchGroups(matchObjectExclude);
                          matchGroups.CreateGroups(3);
                          if(matchGroups.MatchesLength == 0)
                          {
                                matchesOk = true;
                                break;
                          }
                        }
                    }

                    // check and make estimate 
                    if (matchesOk)
                    {
                        matchGroups.CreateEstimateGroups();
                        if (matchGroups.EstimateMatchesLength > 0)
                        {
                            estMatchesOk = true;
                        }
                        else
                        {
                            // make estimate groups 
                            // * x *
                            //   *
                            if (loops >= 5)
                            {
                                foreach (var item in filledCells)
                                {
                                    Debug.Log("Try to make estimate group");
                                    GridCell gC = item;
                                    GridCell gCRight = this[gC.Row, gC.Column + 1];
                                    GridCell gCRightRight = this[gC.Row, gC.Column + 2];
                                    GridCell gCRightBottom = this[gC.Row+1, gC.Column + 1];

                                    if (gCRight && gCRightRight && gCRightBottom)
                                    {
                                        Debug.Log("Make estimate group");
                                        MatchObject m = sC.GetMainRandomObjectPrefab(LcSet, goSet, gridObjectsExclude);
                                        gC.SetObject(m);
                                        gCRightRight.SetObject(m);
                                        gCRightBottom.SetObject(m);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    matchGroups.CreateGroups(3);
                    matchGroups.CreateEstimateGroups();

                    matchesOk = matchGroups.MatchesLength == 0;
                    estMatchesOk = matchGroups.EstimateMatchesLength > 0;
                    Debug.Log("matchgroups count: " + matchGroups.MatchesLength + "; estimates count:  " + matchGroups.EstimateMatchesLength);

                    if (!matchesOk || !estMatchesOk)
                    {
                        reFill();
                    }
                    else
                    {
                        Debug.Log("fill loops: " + loops);
                    }

;                   loops++;
                    if (loops >= 10)
                    {
                        ChestFill(cellsToFill, gridObjects[0], gridObjects[1]); // use chest fill to avoid errors
                        Debug.LogError("fill with mathes: " + matchGroups.MatchesLength + "; estimate: " + matchGroups.EstimateMatchesLength);
                        break;
                    }
                }
            }
        }

        internal void DestroyMatchGroups( MatchObject matchObject)
        {
            foreach (var item in matchGroups.mgList)
            {
                GridCell gCell = item.Cells.GetRandomPos();
             //   Debug.Log("destroy match goup, old match: " + gCell.Match + "; new match: " + matchObject);
                gCell.SetObject(matchObject);
            }
        }

        internal void ChestFill(List<GridCell> cells, MatchObject m0, MatchObject m1)
        {
            foreach (var item in cells)
            {
                bool isEvenRow = (item.Row % 2 == 0);
                bool isEvenCol = (item.Column % 2 == 0);

                if (isEvenRow )
                {
                    item.SetObject(isEvenCol ? m0 : m1);
                }
                else
                {
                    item.SetObject(isEvenCol ? m1 : m0);
                }
            }
            
        }

        internal void ChestFill(MatchObject m1, MatchObject m2, MatchObject m3)
        {

        }
        #endregion test grid
    }
}