using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Mkey
{
	public class UseBoosterAchievement : Achievement
	{
        [SerializeField]
        private Booster booster;
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

            GameEvents.ApplyBoosterEvent += UseBoosterEventHandler;
            RewardReceivedEvent +=(r)=> 
            {
                CoinsHolder.Add(r);
            };

            ChangeCurrentCountEvent += (cc, tc)=>{  };
        }

        private void OnDestroy()
        {
            GameEvents.ApplyBoosterEvent -= UseBoosterEventHandler;
        }
        #endregion regular

        public override string GetUniqueName()
        {
            return "usebooster_" + ((booster)? booster.ID.ToString() : name);
        }

        private void UseBoosterEventHandler(int id)
        {
            if (booster && id == booster.ID)
            {
                IncCurrentCount();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UseBoosterAchievement))]
    public class UseBoosterAchievementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UseBoosterAchievement t = (UseBoosterAchievement)target;
            t.DrawInspector();
        }
    }
#endif
}
