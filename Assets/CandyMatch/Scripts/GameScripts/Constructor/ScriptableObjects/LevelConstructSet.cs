using UnityEngine;
using System;
using System.Collections.Generic;

/*
    02.12.2019 - first
    30.12.2019
    17.03.2020
    15.07.2020
 */
namespace Mkey
{
   // [CreateAssetMenu]
    public class LevelConstructSet : BaseScriptable
    {
        [SerializeField]
        private PopUpsController levelStartStoryPage;
        [SerializeField]
        private PopUpsController levelWinStoryPage;
        [SerializeField]
        public bool hardFlag;
        [SerializeField]
        public bool pathTELBR = true;

        [Space(16)]
        [SerializeField]
        private int vertSize = 8;
        [SerializeField]
        private int horSize = 8;
        [SerializeField]
        private float distX = 0.0f;
        [SerializeField]
        private float distY = 0.0f;
        [SerializeField]
        private float scale = 0.9f;
        [SerializeField]
        private int backGroundNumber = 0;

        [Space(16)]
       // [HideInInspector]
        [SerializeField]
        public List<CellData> spawnCells;
        [HideInInspector]
        [SerializeField]
        public List<Vector2> spawnOffsets;
        [SerializeField]
        public List<GCellObects> cells;
        public MissionConstruct levelMission;
        [HideInInspector]
        [SerializeField]
        public string boardJS;  // manually added fill pathes
        [Header ("Set min 3 matchObjects")]
        [SerializeField]
        public List<int> usedMatchObjects;

        private  const int minObjectsCount = 2;

        #region properties
        public PopUpsController LevelWinStoryPage { get { return levelWinStoryPage; } }

        public PopUpsController LevelStartStoryPage { get { return levelStartStoryPage; } }

        public int VertSize
        {
            get { return vertSize; }
            set
            {
                if (value < 1) value = 1;
                vertSize = value;
                SetAsDirty();
            }
        }

        public int HorSize
        {
            get { return horSize; }
            set
            {
                if (value < 1) value = 1;
                horSize = value;
                SetAsDirty();
            }
        }

        public float DistX
        {
            get { return distX; }
            set
            {
                distX = RoundToFloat(value, 0.05f);
                SetAsDirty();
            }
        }

        public float DistY
        {
            get { return distY; }
            set
            {
                distY = RoundToFloat(value, 0.05f);
                SetAsDirty();
            }
        }

        public float Scale
        {
            get { return scale; }
            set
            {
                if (value < 0) value = 0;
                scale = RoundToFloat(value, 0.05f);
                SetAsDirty();
            }
        }
        #endregion properties

        public int BackGround
        {
            get { return backGroundNumber; }
        }

        #region regular
        void OnEnable()
        {
            if (levelMission == null) levelMission = new MissionConstruct();
            levelMission.SaveEvent = SetAsDirty;
        }
        #endregion regular

        public void AddMatch(int id)
        {
            if (usedMatchObjects == null) usedMatchObjects = new List<int>();
            if (usedMatchObjects.Contains(id))
            {
                usedMatchObjects.Remove(id);
            }
            else
            {
                usedMatchObjects.Add(id);
            }
            SetAsDirty();
        }

        public bool ContainMatch(int id)
        {
            if (usedMatchObjects != null && usedMatchObjects.Contains(id))
            {
                return true;
            }
            return false;
        }

        #region spawners
        public void AddSpawnCell(CellData cd)
        {
            if (spawnCells == null) spawnCells = new List<CellData>();
            if (ContainCellData(spawnCells, cd))
            {
                RemoveCellData(spawnCells, cd);
            }
            else
            {
                spawnCells.Add(cd);
            }

            SetAsDirty();
        }

        public void SaveSpawnOfsets(MatchGrid mGrid)
        {
            spawnOffsets = new List<Vector2>();
            if (spawnCells != null)
            {
                foreach (var item in spawnCells)
                {
                    GridCell gC = mGrid.Rows[item.Row].cells[item.Column];
                    if (gC && gC.GCSpawner)
                    {
                        spawnOffsets.Add(gC.transform.InverseTransformPoint(gC.GCSpawner.transform.position));
                    }
                }
            }
            SetAsDirty();
        }

        private void CleanSpawners(GameObjectsSet gOS)
        {
            if (spawnCells != null)
            {
                spawnCells.RemoveAll((c) =>
                {
                    return ((c.Column >= horSize) || (c.Row >= vertSize) || CellsContainCellObject(c.Row, c.Column, gOS.Disabled.ID));
                });
            }
        }

        private bool CellsContainCellObject(int row, int col, int id)
        {
            Func<GCellObects, int, bool> contain = (gco, id) => {
                if (gco == null || gco.gridObjects == null) return false;
                foreach (var item in gco.gridObjects)
                {
                    if (item.id == id) return true;
                }
                return false; 
            };

            if (cells == null) return false;

            foreach (var c in cells)
            {
                if (c.row == row && c.column == col && contain(c, id))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion spawners

        /// <summary>
        /// Remove all non-existent cells data from board
        /// </summary>
        /// <param name="gOS"></param>
        public void Clean(GameObjectsSet gOS)
        {
            if (cells == null) cells = new List<GCellObects>();

            CleanSpawners(gOS);

            cells.RemoveAll((c)=> { return ((c.column >= horSize) || (c.row >= vertSize)); });
            if (gOS)
            {
                foreach (var item in cells)
                {
                    if (item.gridObjects != null)
                    {
                        item.gridObjects.RemoveAll((o)=> { return o == null || !gOS.ContainID(o.id); });
                    }
                }
            }
            if (usedMatchObjects != null) usedMatchObjects.RemoveAll((m) => { return !gOS.ContainMatchID(m); });
            SetAsDirty();
        }

        public void IncBackGround(int length)
        {
            backGroundNumber++;
            backGroundNumber = (int)Mathf.Repeat(backGroundNumber, length);
            Save();
        }

        public void DecBackGround(int length)
        {
            backGroundNumber--;
            backGroundNumber = (int)Mathf.Repeat(backGroundNumber, length);
            Save();
        }

        private float RoundToFloat(float val, float delta)
        {
            int vi = Mathf.RoundToInt(val / delta);
            return (float)vi * delta;
        }

        private void RemoveCellData(List<CellData> cdl, CellData cd)
        {
            if (cdl != null) cdl.RemoveAll((c) => { return ((cd.Column == c.Column) && (cd.Row == c.Row)); });
        }

        private bool ContainCellData(List<CellData> lcd, CellData cd)
        {
            if (lcd == null || cd == null) return false;
            foreach (var item in lcd)
            {
                if ((item.Row == cd.Row) && (item.Column == cd.Column)) return true;
            }
            return false;
        }

        private GCellObects GetCellObjects(int row, int column)
        {
            if (cells == null) return null;
            foreach (var item in cells)
            {
                if ((item.row == row) && (item.column == column)) return item;
            }
            return null;
        }

        private bool ContainEqualCellData(List<CellData> lcd, CellData cd)
        {
            if (lcd == null || cd == null) return false;
            foreach (var item in lcd)
            {
                if ((item.Row == cd.Row) && (item.Column == cd.Column) && (cd.ID == item.ID)) return true;
            }
            return false;
        }

        internal void SaveObjects(GridCell gC)
        {
            cells.RemoveAll((c)=> { return ((c.row == gC.Row) && (c.column == gC.Column)); });
            List<GridObjectState> gOSs = gC.GetGridObjectsStates();
            if (gOSs.Count > 0) cells.Add(new GCellObects(gC.Row, gC.Column, gOSs));

            SetAsDirty();
        }

        internal void SaveObjects( List<GridCell> gCs)
        {
            foreach (var gC in gCs)
            {
                if (gC)
                {
                    cells.RemoveAll((c) => { return ((c.row == gC.Row) && (c.column == gC.Column)); });
                    List<GridObjectState> gOSs = gC.GetGridObjectsStates();
                    if (gOSs.Count > 0) { cells.Add(new GCellObects(gC.Row, gC.Column, gOSs)); Debug.Log(gC + "; Underlay" + gC.Underlay); }
                }
            }

            SetAsDirty();
        }

        internal List<MatchObject> GetMatchObjects(GameObjectsSet goSet)
        {
          
            List<MatchObject> res = new List<MatchObject>();
            if (usedMatchObjects == null || usedMatchObjects.Count < minObjectsCount) return new List<MatchObject>(goSet.MatchObjects);
            foreach (var item in usedMatchObjects)
            {
                MatchObject m = goSet.GetMainObject(item);
                if (m && !res.Contains(m)) res.Add(m);
            }
            return (res.Count >= minObjectsCount) ? res : new List<MatchObject>(goSet.MatchObjects);

        }
    }

    [Serializable]
    public class GCellObects
    {
        public int row;
        public int column;
        public List<GridObjectState> gridObjects;

        public GCellObects(int row, int column, List<GridObjectState> gridObjects)
        {
            this.row = row;
            this.column = column;
            this.gridObjects = new List<GridObjectState>(gridObjects);
        }
    }
}



