using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 23.02.2021
 */
namespace Mkey
{
	[CreateAssetMenu]
	public class Booster : ScriptableObject
	{
		public BoosterFunc funcPrefab;
		public GuiBoosterHelper activateHelperBrefab;
		public List<GuiBoosterHelper> guiHelpers;

		#region properties
		public static Booster ActiveBooster { get; private set; }
		public int ID { get { return id; } private set { id = value; } }
		public int Count { get; private set; }
		public bool Use { get; private set; }
		public bool IsActive => (ActiveBooster && ID == ActiveBooster.ID);
		#endregion properties

		#region temp vars
		private string SaveKey => "booster_id_" + ID.ToString();
		[HideInInspector]
		[SerializeField]
		private int id = Int32.MinValue;

		private GuiBoosterHelper footerGUIObject;
		private BoosterFunc sceneObject;
		#endregion temp vars

		#region events
		public Action<int> ChangeCountEvent; //count
		public Action<int> LoadEvent; // count
		public Action<Booster> ActivateEvent;
		public Action DeActivateEvent;
		public Action<Booster> ChangeUseEvent;
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
			Count = count;

			if (changed)
			{
				SaveCount();
				ChangeCountEvent?.Invoke(count);
			}
		}

		private void LoadCount()
		{
			Count = PlayerPrefs.GetInt(SaveKey, 0);
			LoadEvent?.Invoke(Count);
		}

		private void SaveCount()
		{
			PlayerPrefs.SetInt(SaveKey, Count);
		}

		public void LogCount()
        {
			Debug.Log(name + "; " + Count);
        }
		#endregion count

		#region handlers
		public void BoosterActivateOrShop(PopUpsController boosterShop)
		{
			if (Count == 0 && boosterShop)
			{
				boosterShop.CreateWindow();
				if (ActiveBooster) ActiveBooster.DeActivateBooster();
				return;
			}

			if (!IsActive && Count > 0)     // activate booster
			{
				if (ActiveBooster) ActiveBooster.DeActivateBooster();
				ActivateBooster();
				return;
			}

			if (IsActive)              // deactivate booster
			{
				DeActivateBooster();
				return;
			}
		}

		public void ChangeUseOrShop(PopUpsController boosterShop)
		{
			if (Count == 0 && boosterShop) boosterShop.CreateWindow();
			else ChangeUse();
		}
		#endregion handlers

		#region apply
		/// <summary>
		/// Match3 Match2 functionality
		/// </summary>
		/// <param name="gCell"></param>
		/// <param name="completeCallBack"></param>
		public void ApplyToGridM(GridCell gCell, Action completeCallBack)
		{
			if (!funcPrefab.CanApply(gCell))
			{
				ActiveBooster.DeActivateBooster();
				completeCallBack?.Invoke();
				return;
			}

			sceneObject = CreateSceneObject(SortingOrder.Booster, null);
			if (sceneObject)
			{
				sceneObject.Apply(gCell, completeCallBack);
				if (ActiveBooster != null)
				{
					ActiveBooster = null;
				}
				AddCount(-1);
				GameEvents.ApplyBoosterEvent?.Invoke(ID);
			}
			else
			{
				completeCallBack?.Invoke();
			}
		}

		/// <summary>
		/// Bubble shooter functionality
		/// </summary>
		public static void ApplyToGridBS()
		{
			if (ActiveBooster)
			{
				ActiveBooster.AddCount(-1);
				GameEvents.ApplyBoosterEvent?.Invoke(ActiveBooster.ID);
				ActiveBooster.DeActivateBooster();
			}
		}
		#endregion apply

		internal void ActivateBooster()
		{
			ActiveBooster = this;
			if (funcPrefab.ActivateApply())
			{
				AddCount(-1);
				GameEvents.ApplyBoosterEvent?.Invoke(ID);
				DeActivateBooster();
				return;
			}
			ActivateEvent?.Invoke(this);
		}

		/// <summary>
		/// Set  ActiveBooster = null, raise DeActivateEvent
		/// </summary>
		internal void DeActivateBooster()
		{
			if (ActiveBooster != null) ActiveBooster = null;
			DeActivateEvent?.Invoke();
		}

		internal void Enumerate(int id)
		{
			this.id = id;
			//name = (ObjectImage == null) ? "null" + "-" + id : ObjectImage.name + "; ID : " + id;
			LoadCount();
			Use = false;
		}

		internal GuiBoosterHelper CreateActivateHelper(RectTransform parent)
		{
			if (!activateHelperBrefab) return null;
			footerGUIObject  = activateHelperBrefab.Create(this, parent);
			return footerGUIObject;
		}

		internal GuiBoosterHelper CreateGuiHelper(RectTransform parent, string description)
		{
			GuiBoosterHelper gBH = GetGuiHelperByDescription(description);
			if (gBH == null) return null;
			return gBH.Create(this, parent);
		}

		internal BoosterFunc CreateSceneObject(int sortingOrder, Transform parent)
		{
			BoosterFunc sceneObject = Instantiate(funcPrefab);
			Vector3 wPos = Vector3.zero;
			if (sceneObject)
			{
				if (footerGUIObject != null)
				{
					wPos = footerGUIObject.transform.position; //Coordinats.CanvasToWorld(guiObject.gameObject);
				}

				sceneObject.transform.position = wPos;
				sceneObject.transform.parent = parent;
				SpriteRenderer sr = sceneObject.GetOrAddComponent<SpriteRenderer>();
				sr.sortingOrder = sortingOrder;
			}
			return sceneObject;
		}

		internal BoosterFunc CreateSceneObject(int sortingOrder, Vector3 position, Transform parent)
		{
			BoosterFunc sceneObject = Instantiate(funcPrefab, position, Quaternion.identity, parent);
			if (sceneObject)
			{
				SpriteRenderer sr = sceneObject.GetOrAddComponent<SpriteRenderer>();
				sr.sortingOrder = sortingOrder;
			}
			return sceneObject;
		}

		internal void ChangeUse()
		{
			Use = !Use;
			ChangeUseEvent?.Invoke(this);
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

		public GuiBoosterHelper GetGuiHelperByDescription(string description)
        {
			if (string.IsNullOrEmpty(description)) return null;

            foreach (var item in guiHelpers)
            {
                if (item)
                {
					if (string.CompareOrdinal(description, item.description) == 0)
					{
						return item;
					}
				}
            }
			return null;
		}
	}
}
