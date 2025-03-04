using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class SpawnController : MonoBehaviour
    {
        public static SpawnController Instance;

        private void Awake()
        {
            if (Instance) Destroy(gameObject);
            else
            {
                Instance = this;
            }
        }

        /// <summary>
        /// spawn new MO or return previous spawned
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GridObject Get(GridCell gridCell, LevelConstructSet lCSet, GameObjectsSet gOSet, GameBoard mBoard, Vector3 position)
        {
            if (!gridCell) return null;
            MatchObject prefab = GetMainRandomObjectPrefab(lCSet, gOSet);
            if (!prefab) return null;

            GridObject match = prefab.Create(gridCell);
            if (match)
            {
                match.transform.position = position;
            }
            return match;
        }

        public MatchObject GetMainRandomObjectPrefab(LevelConstructSet lCSet, GameObjectsSet gOSet)
        {
            List<MatchObject> gridObjects = lCSet.GetMatchObjects(gOSet);
            gridObjects.Shuffle();
            return gridObjects.Count > 0 ? gridObjects[0] : null;
        }

        public MatchObject GetMainObjectPrefab(GameObjectsSet gOSet, int id)
        {
           return  gOSet.GetMainObject(id);
        }

        public List<MatchObject> GetMainRandomObjectPrefabs( int count, LevelConstructSet lCSet, GameObjectsSet gOSet)
        {
            List<MatchObject> gridObjects = lCSet.GetMatchObjects(gOSet);
            gridObjects.Shuffle();

            return gridObjects.Count >= count ? gridObjects.GetRange(0, count) : gridObjects;
        }

        public MatchObject GetMainRandomObjectPrefab(LevelConstructSet lCSet, GameObjectsSet gOSet, List<GridObject> exclude)
        {
            List<MatchObject> gridObjects = lCSet.GetMatchObjects(gOSet);
            gridObjects.Shuffle();
            if (exclude != null)
                gridObjects.RemoveAll((gO) => { return exclude.Contains(gO); });

            return (gridObjects.Count > 0) ? gridObjects[0] : null;
        }
    }
}