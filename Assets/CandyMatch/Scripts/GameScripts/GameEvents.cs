using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mkey
{
	public static class GameEvents 
	{
        #region comon events
        public static Action <int> ApplyBoosterEvent; // <id>
        public static Action <int> CollectGridObject;
        private static Dictionary<string, List <Action<string>>> CommonEventHandlersDict;
        #endregion comon events
		
		#region common
		public static void AddCommonEventHandler(string id , Action<string> CommonEventHandler)
        {
            if (CommonEventHandler == null) return;

            if (CommonEventHandlersDict == null) CommonEventHandlersDict = new Dictionary<string, List< Action<string>>>();

            if (CommonEventHandlersDict.ContainsKey(id))
            {
                if (CommonEventHandlersDict[id] == null) CommonEventHandlersDict[id] = new List<Action<string>>();
                CommonEventHandlersDict[id].Add(CommonEventHandler);
            }
            else
            {
                CommonEventHandlersDict.Add(id, new List<Action<string>>());
                CommonEventHandlersDict[id].Add(CommonEventHandler);
            }
        }

        public static void RemoveCommonEventHandler(string id, Action<string> CommonEventHandler)
        {
            if (CommonEventHandler == null) return;
            if (CommonEventHandlersDict == null) CommonEventHandlersDict = new Dictionary<string, List<Action<string>>>();
            if (CommonEventHandlersDict.ContainsKey(id))
            {
                if (CommonEventHandlersDict[id] != null && CommonEventHandlersDict[id].Contains(CommonEventHandler))
                {
                    CommonEventHandlersDict[id].Remove(CommonEventHandler);
                }
            }
        }

        public static void OnCommonEvent(string id, string jsonParam)
        {
            if (CommonEventHandlersDict == null) CommonEventHandlersDict = new Dictionary<string,List<Action<string>>>();
            if (CommonEventHandlersDict.ContainsKey(id))
            {
                if (CommonEventHandlersDict[id] != null)
                {
                    foreach (var item in CommonEventHandlersDict[id])
                    {
                        item?.Invoke(jsonParam);
                    }
                }
            }
        }
        #endregion common
    }
}
