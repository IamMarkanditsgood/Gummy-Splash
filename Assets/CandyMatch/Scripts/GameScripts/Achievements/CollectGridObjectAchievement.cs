using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
	public class CollectGridObjectAchievement : Achievement
	{
        [SerializeField]
        private GridObject gridObject;

        #region events

        #endregion events

        #region temp vars
        private bool dLog = true;
        #endregion temp vars

        #region properties

        #endregion properties

        #region regular
        public override void Load()
        {
            LoadRewardReceived();
            LoadCurrentCount();

            GameEvents.CollectGridObject += CollectGridObjectEventHandler;
            RewardReceivedEvent +=(r)=> 
            {
                CoinsHolder.Add(r);
            };
            ChangeCurrentCountEvent += (cc, tc)=>{  };
        }

        private void OnDestroy()
        {
            GameEvents.CollectGridObject -= CollectGridObjectEventHandler;
        }
        #endregion regular

        public override string GetUniqueName()
        {
            return "gridobjectcollect_" + ((gridObject)? gridObject.ID.ToString() : "");
        }

        private void CollectGridObjectEventHandler(int id)
        {
            if (gridObject && id == gridObject.ID)
            {
                IncCurrentCount();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CollectGridObjectAchievement))]
    public class CollectGridObjectAchievementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            CollectGridObjectAchievement t = (CollectGridObjectAchievement)target;
            t.DrawInspector();
        }
    }
#endif
}
