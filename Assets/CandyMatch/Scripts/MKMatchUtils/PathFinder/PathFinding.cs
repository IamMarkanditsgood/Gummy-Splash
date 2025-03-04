using System.Collections.Generic;
using System.Threading;

namespace Mkey
{
    public class PathFinder
    {
        public bool TELBR { get; set; } // pathfinder option - how to get cell neighbors

        private List<PFCell> fullPath;
        public IList<PFCell> FullPath { get { return (fullPath == null) ? null: fullPath.AsReadOnly(); } }

        public PathFinder()
        {
            fullPath = new List<PFCell>();
        }

        public PathFinder (bool telbr) : this()
        {
            TELBR = telbr;
        }

        public List<GridCell> GCPath()
        {
            List<GridCell> res = new List<GridCell>();
            if (fullPath != null)
            {
                foreach (var item in fullPath)
                {
                    res.Add(item.gCell);
                }
            }
            return res;
        }

        /// <summary>
        /// Create all possible paths from this position
        /// </summary>
        /// <param name="A"></param>
        private void CreateGlobWayMap(Map WorkMap, PFCell A)
        {
           // UnityEngine.Debug.Log("create path to top ");
            WorkMap.ResetPath();
            List<PFCell> waveArray = new List<PFCell>();
            waveArray.Add(A);
            A.mather = A;

            bool work = true;
            while (work)
            {
                work = false;
                List<PFCell> waveArrayTemp = new List<PFCell>();
                foreach (PFCell mather in waveArray)
                {
                    if (mather.available || (A == mather && !mather.available))
                    {
                        List<PFCell> childrens = (TELBR) ? mather.Neighbors.GetNeighBorsPF_TELBR() : mather.Neighbors.GetNeighBorsPF_TLBR(); //  15.12.2023
                        foreach (PFCell child in childrens)
                        {
                            if (!child.HaveMather() && child.available  && child.IsPassabilityFrom(mather)) /// try
                            {
                                child.mather = mather;
                                waveArrayTemp.Add(child);
                                work = true;
                            }
                        }
                    }
                }
                waveArray = waveArrayTemp;
            }
        }

        /// <summary>
        /// Create all possible paths to destination point
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        private void CreateGlobWayMap(Map WorkMap, PFCell A, PFCell B)
        {
            // try to create direct vertical path

            WorkMap.ResetPath();
            List<PFCell> waveArray = new List<PFCell>();
            waveArray.Add(A);
            A.mather = A;
            bool work = true;
            List<PFCell> waveArrayTemp;
            List<PFCell> childrens;
            PFCell child;
            while (work)
            {
                work = false;
                waveArrayTemp = new List<PFCell>();
                
                foreach (PFCell mather in waveArray)
                {
                    if (mather.available)
                    {
                        childrens = TELBR ? mather.Neighbors.GetNeighBorsPF_TELBR() : mather.Neighbors.GetNeighBorsPF_TLBR(); // 15.12.2023 - old mather.Neighbors.GetNeighBorsPF_TLBR() - first cell to select Top Enbled Or Top
                                                                                                                              // if (mather.row == 9 && mather.col == 1 && A.row == 9 && A.col == 1) UnityEngine.Debug.Log("childrens: " + childrens.MakeString("; "));
                        for (int ci = 0; ci < childrens.Count; ci++)
                        {
                            child = childrens[ci];
                            if (!child.HaveMather())
                            {
                                child.mather = mather;
                                waveArrayTemp.Add(child);
                                work = true;
                                if (child == B) return;             // end of path found -> return
                                if (mather.row > child.row) break;  // try to get top child
                            }
                        }
                    }
                }
                waveArray = waveArrayTemp;
            }

            // search shortest path
            WorkMap.ResetPath();
            waveArray = new List<PFCell>();
            waveArray.Add(A);
            A.mather = A;
            work = true;

            while (work)
            {
                work = false;
                waveArrayTemp = new List<PFCell>();
                foreach (PFCell mather in waveArray)
                {
                    if (mather.available)
                    {
                        childrens = TELBR ? mather.Neighbors.GetNeighBorsPF_TELBR() : mather.Neighbors.GetNeighBorsPF_TLBR(); // 15.12.2023 - old mather.Neighbors.GetNeighBorsPF_TLBR() - first cell to select Top Enbled Or Top
                        for (int ci = 0; ci < childrens.Count; ci++)
                        {
                            child = childrens[ci];
                            if (!child.HaveMather())
                            {
                                child.mather = mather;
                                waveArrayTemp.Add(child);
                                work = true;
                                if (child == B) return; // end of path found -> return
                            }
                        }
                    }
                }
                waveArray = waveArrayTemp;
            }
        }

        /// <summary>
        /// Return true if FullPathA contain start point and end point
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private bool IsWayCreated(PFCell A, PFCell B)
        {
            return (fullPath!=null && PathLenght > 0 && fullPath[0] == A && fullPath[PathLenght - 1] == B);
        }

        public void CreatePath(Map WorkMap, PFCell A, PFCell B)
        {
            /*
            // debug path
            if (A.row == 8 && A.col == 8 && B.row == 0 && B.col == 7)
            {
                UnityEngine.Debug.Log("create path between:" + A.gCell + " : " + B.gCell);
            }
            */

            fullPath = null;
            if (WorkMap == null || A == null || B == null  || !A.available|| !B.available) return;
        //    if (!IsWayCreated(A, B))
            {
                CreateGlobWayMap(WorkMap, A, B);
                if (IsWayExistTo(B))
                {
                    fullPath = new List<PFCell>();
                    fullPath.Add(B);
                    PFCell mather = B.mather;
                    while (mather != A.mather)
                    {
                        fullPath.Add(mather);
                        mather = mather.mather;
                    }
                    fullPath.Reverse();
                    /*
                    // debug path
                    if (A.row == 8 && A.col == 8 && B.row == 0 && B.col == 7)
                    {
                        UnityEngine.Debug.Log("way exist : " + fullPath.MakeString(";"));
                    }
                    */
                }
                //else
                //{
                //    fullPath.Add(A);
                //}
            }
        }

        /// <summary>
        /// Create the shortest path if exist, else fullPath set to null
        /// </summary>
        /// <param name="WorkMap"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        public void CreatePath(Map WorkMap, PFCell A, List<PFCell> B)
        {
            fullPath = null;
            if (WorkMap == null || A ==null || B == null || B.Count == 0 || !A.available) return;

            List<PFCell> tempPath;
            CreateGlobWayMap(WorkMap, A);
            foreach (var item in B)
            {
                if (item.available)
                {
                    if (IsWayExistTo(item))
                    {
                        tempPath = new List<PFCell>();
                        tempPath.Add(item);

                        PFCell mather = item.mather;
                        while (mather != A.mather)
                        {
                            tempPath.Add(mather);
                            mather = mather.mather;
                        }
                        tempPath.Reverse();
                        if (fullPath == null || fullPath.Count > tempPath.Count) fullPath = tempPath;
                    }
                }
            }
        }

        /// <summary>
        /// Create the shortest path if exist, else fullPath set to null
        /// </summary>
        /// <param name="WorkMap"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        public void CreatePathToTop(Map WorkMap, PFCell A)
        {
            fullPath = null;
            if (WorkMap == null || A == null) return;

            List<PFCell> tempPath;
            CreateGlobWayMap(WorkMap, A);
            PFCell mather;
            List<PFCell> topAvailable = new List<PFCell>();
            int minRow = int.MaxValue;

            // get top available cells
            foreach (var item in WorkMap.PFCells)
            {
                if (IsWayExistTo(item))
                {
                    if (minRow >= item.row)
                    {
                        minRow = item.row;
                        topAvailable.Add(item);
                    }
                    else
                    {
                        break;
                    }
                }
            }
           // UnityEngine.Debug.Log("min row :" + minRow);

            // create shortest path to top available cells
            foreach (var item in topAvailable)
            {
                if (item.row == minRow)
                {
                    tempPath = new List<PFCell>(topAvailable.Count);
                    tempPath.Add(item);

                    mather = item.mather;
                    while (mather != A.mather)
                    {
                        tempPath.Add(mather);
                        mather = mather.mather;
                    }
                    tempPath.Reverse();
                    if (fullPath == null || fullPath.Count > tempPath.Count) fullPath = tempPath;
                }
            }
          //  UnityEngine.Debug.Log("Path to top created " + DebugPath());
        }

        private void CreatePathThread(Map WorkMap, PFCell A, PFCell B)
        {
            ThreadPool.QueueUserWorkItem(m => CreatePath(WorkMap, A, B));
        }

        private bool IsWayExistTo(PFCell B)
        {
            return (B.HaveMather() && B.available); 
        }

        public int PathLenght { get { return (fullPath == null)? int.MaxValue : fullPath.Count; } }

        public int GetHorPathLength(PFCell a)
        {
            if (fullPath == null || a == null || fullPath.Count == 0) return int.MaxValue;
            int horLength = 0;
            if (fullPath[0].col != a.col)  horLength = 1;

            if (fullPath.Count > 1)
            {
                for (int i = 1; i < fullPath.Count; i++)
                {
                    if (fullPath[i].col != fullPath[i - 1].col) horLength++;
                }
            }
            return horLength;
        }
        public List<PFCell> GetAvailablePFPositionAround(Map WorkMap, PFCell A, int distance)
        {
            List<PFCell> lPos = new List<PFCell>();
            CreateGlobWayMap(WorkMap, A);
            foreach (var item in WorkMap.PFCells)
            {
                if(IsWayExistTo(item) && item.GetDistanceTo(A) == distance)
                {
                    lPos.Add(item);
                }
            } 
            return lPos;
        }

        public string DebugPath()
        {
            string debugString = "";
            if (fullPath != null)
            {
                foreach (var item in fullPath)
                {
                    if (item != null)
                    {
                        debugString += item.ToString();
                    }
                    else
                    {
                        debugString += "null";
                    }
                }
            }
            return debugString;
        }
    }
}
