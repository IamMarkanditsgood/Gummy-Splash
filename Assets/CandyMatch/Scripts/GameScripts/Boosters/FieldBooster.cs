using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
	using UnityEditor;
#endif
/*
 18.10.2023
 */
namespace Mkey
{
	[CreateAssetMenu]
	public class FieldBooster : ScriptableObject
	{
		public GridObject gridObjectPrefab;
		public GuiFieldBoosterHelper guiHelperBrefab;

		#region properties
		public int Count { get { if (!IsLoaded()) LoadCount(); return _count; } }
		public bool Use { get; private set; }
		#endregion properties

		#region temp vars
		private string SaveKey => name; // gridObjectPrefab)? "fieldbooster_id_" + gridObjectPrefab.ID.ToString() : name;
		private int _count = -1; // load flag, "-1" - not loaded
		#endregion temp vars

		#region events
		public Action<int> ChangeCountEvent; //count
		public Action<int> LoadEvent; // count
		public Action<FieldBooster> ActivateEvent;
		public Action DeActivateEvent;
		public Action<FieldBooster> ChangeUseEvent;
		#endregion events

		#region count
		public void AddCount(int count)
		{
			SetCount(count + Count);
		}

		public void SetCount(int count)
		{
			count = Mathf.Max(0, count);
			bool changed = (count != Count);
			_count = count;

			if (changed)
			{
				SaveCount();
				ChangeCountEvent?.Invoke(count);
			}
		}

		internal void LoadCount()
		{
			_count = PlayerPrefs.GetInt(SaveKey, 0);
			LoadEvent?.Invoke(Count);
		}

		private void SaveCount()
		{
			PlayerPrefs.SetInt(SaveKey, Count);
		}

		private bool IsLoaded()
        {
			return _count > -1;
		}
		#endregion count

		internal GridObject CreateSceneObject(int sortingOrder, Transform parent)
		{
			GridObject sceneObject = Instantiate(gridObjectPrefab);
			Vector3 wPos = Vector3.zero;
			if (sceneObject)
			{
				//if (footerGUIObject != null)
				//{
				//	wPos = footerGUIObject.transform.position; //Coordinats.CanvasToWorld(guiObject.gameObject);
				//}

				sceneObject.transform.position = wPos;
				sceneObject.transform.parent = parent;
				SpriteRenderer sr = sceneObject.GetOrAddComponent<SpriteRenderer>();
				sr.sortingOrder = sortingOrder;
			}
			return sceneObject;
		}

		internal GridObject CreateSceneObject(int sortingOrder, Vector3 position, Transform parent)
		{
			GridObject sceneObject = Instantiate(gridObjectPrefab, position, Quaternion.identity, parent);
			if (sceneObject)
			{
				SpriteRenderer sr = sceneObject.GetOrAddComponent<SpriteRenderer>();
				sr.sortingOrder = sortingOrder;
			}
			return sceneObject;
		}

		protected void SetActive(GameObject gO, bool active, float delay)
		{
			Debug.Log("set active: " + active);
			if (gO)
			{
				if (delay > 0)
				{
					TweenExt.DelayAction(gO, delay, () => { if (gO) gO.SetActive(active); });
				}
			}
		}

		internal GuiFieldBoosterHelper CreateGuiHelper(RectTransform parent)
		{
			if (guiHelperBrefab == null) return null;
			return guiHelperBrefab.Create(this, parent);
		}

		public void AnimateObject(int sortingOrder, Vector3 startPosition, Vector3 targetPosition, Transform parent, Action completeCallback)
        {
			float baseTime = 0.35f;
			GridObject gridObject = CreateSceneObject(sortingOrder, startPosition, parent);
			GameObject gO = gridObject.gameObject;
			ScaleTween(gO, 1, 1.7f, baseTime, null);
			SimpleTween.Move(gO,  startPosition, targetPosition, baseTime).SetDelay(baseTime);
			ScaleTween(gO, 1.7f, 1.2f, baseTime, null).SetDelay(baseTime);
			ScaleAnim(gO, (baseTime + baseTime), 1.2f, 1.35f, 1, baseTime/2f, baseTime/2f, () => { if (gO) Destroy(gO); completeCallback?.Invoke(); });
			
		}

		private void ScaleAnim(GameObject g, float delay, float v0, float v1, float v2, float time1, float time2, Action completeCallback )
        {
			ScaleTween(g, v0, v1, time1, null).SetDelay(delay);
			ScaleTween(g, v1, v2, time2, completeCallback).SetDelay(time1 + delay);
		}

		private void Scale(Transform sTransform, float scale)
		{
			if (sTransform) sTransform.localScale = new Vector3(scale, scale, scale);
		}

		private void Scale(GameObject g, float scale)
		{
			if (g) Scale(g.transform, scale);
		}

		private SimpleTween.SimpleTweenObject ScaleTween(GameObject g, float scale0, float scale1, float time, Action completeCallback)
        {
			return SimpleTween.Value(g, scale0, scale1, time).SetOnUpdate((float f) => { Scale(g, f); }).AddCompleteCallBack(completeCallback);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(FieldBooster))]
	public class FieldBoosterEditor : Editor
	{
		private bool test = true;
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			FieldBooster cH = (FieldBooster)target;
			EditorGUILayout.LabelField("!- Use unique object name to save count -!");
			EditorGUILayout.LabelField("Object name: " + cH.name);
			EditorGUILayout.LabelField("Count: " + cH.Count);
			if (test = EditorGUILayout.Foldout(test, "Test"))
			{
				EditorGUILayout.BeginHorizontal("box");
				if (GUILayout.Button("Add 5 boosters"))
				{
					cH.AddCount(5);
				}
				
				if (GUILayout.Button("Clear boosters"))
				{
					cH.SetCount(0);
				}
				EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Log booster count"))
                {
                    Debug.Log(cH.name + ":" + cH.Count);

                }
                if (GUILayout.Button("Load saved count"))
                {
                    cH.LoadCount();
                }
                EditorGUILayout.EndHorizontal();

            }
		}
	}
#endif
}
