using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class Spawner : MonoBehaviour
    {
        public GridObject lastSpawned;
        public GridCell gridCell;

        #region temp vars
        private SoundMaster MSound { get { return SoundMaster.Instance; } }
        private GameBoard MBoard { get { return GameBoard.Instance; } }
        private GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        private LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        private GameObjectsSet GOSet { get { return GCSet.GOSet; } }
        private SpawnController spawnController;
        private SpawnController SController { get { if (!spawnController) spawnController = GetComponentInParent<SpawnController>(); return spawnController; }}
        #endregion temp vars


        /// <summary>
        /// spawn new MO or return previous spawned
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GridObject Get()
        {
            return SController.Get(gridCell, MBoard.MainGrid.LcSet,  GOSet, MBoard, transform.position);
        }

        public void Show(bool show)
        {
            SpriteRenderer sR = GetComponent<SpriteRenderer>();
            if (sR) sR.enabled = show;
        }
    }
}