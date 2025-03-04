using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Mkey
{
    public class FooterGUIController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform BoostersParent;
        [SerializeField]
        private GameObject PauseButton;
        [SerializeField]
        private List<BoosterParent> boostersParents;

        #region temp vars
        private GameBoard MBoard => GameBoard.Instance;
        private  GuiController MGui => GuiController.Instance; 
        private GameConstructSet GCSet => GameConstructSet.Instance; 
        private GameObjectsSet GOSet =>(GCSet) ? GCSet.GOSet : null;
        #endregion temp vars

        public static FooterGUIController Instance { get; private set; }

        #region regular
        void Awake()
        {
            if (Instance) Destroy(Instance.gameObject);
            Instance = this;
        }

        private IEnumerator Start()
        {
            while (MBoard == null) yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (GameBoard.GMode == GameMode.Edit)
            {
                gameObject.SetActive(false);
            }
            else
            {
                //set booster events
                foreach (var item in GOSet.BoosterObjects)
                {
                    item.ChangeUseEvent += ChangeBoosterUseEventHandler;
                }
                CreateBoostersPanel();
            }

            if (MBoard.WinContr != null && MBoard.WinContr.IsTimeLevel && PauseButton) PauseButton.SetActive(false);
        }

        private void OnDestroy()
        {
            // remove boostar events
            if (GOSet && GOSet.BoosterObjects != null)
                foreach (var item in GOSet.BoosterObjects)
                {
                    item.ChangeUseEvent -= ChangeBoosterUseEventHandler;
                }
        }
        #endregion regular

        private void CreateBoostersPanel()
        {
            foreach (var item in boostersParents)
            {
                if(item!=null && item.parent)
                {
                    GuiBoosterHelper guiBoosterHelper = item.parent.GetComponentInChildren<GuiBoosterHelper>();
                    if(guiBoosterHelper) DestroyImmediate(guiBoosterHelper.gameObject);
                    item.SetActiveLock(true);
                }
            }

            foreach (var item in GOSet.BoosterObjects)
            {
                BoosterParent boosterParent = GetFreeBoosterParent();
                if (boosterParent != null && boosterParent.parent) // item.Use && 
                {
                    item.CreateActivateHelper(boosterParent.parent);
                    boosterParent.SetActiveLock(false);
                }
            }
        }

        private void ChangeBoosterUseEventHandler(Booster booster)
        {
            BoosterParent boosterParent = GetFreeBoosterParent();
            if (booster.Use )
            {
                if (boosterParent != null && boosterParent.parent)
                {
                    booster.CreateActivateHelper(boosterParent.parent);
                    boosterParent.SetActiveLock(false);
                }
            }
            else
            {
                foreach (var item in boostersParents)
                {
                    if (item != null && item.parent)
                    {
                        GuiBoosterHelper guiBoosterHelper = item.parent.GetComponentInChildren<GuiBoosterHelper>();
                        if(guiBoosterHelper && guiBoosterHelper.booster == booster)
                        {
                            DestroyImmediate(guiBoosterHelper.gameObject);
                            item.SetActiveLock(true);
                        }
                    }
                }
            }
        }

        private BoosterParent GetFreeBoosterParent()
        {
            foreach ( var item in boostersParents)
            {
                if (item.parent.GetComponentInChildren<GuiBoosterHelper>() == null) return item;
            }
            return null;
        }

        public void SetControlActivity(bool activity)
        {
            Button[] buttons = GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = activity;
            }
        }

        public void Map_Click()
        {
            if (Time.timeScale == 0) return;
            if (GameBoard.GMode == GameMode.Play)
            {
                if (MGui) MGui.ShowPopUpByDescription("quit");
            }
        }

        public void Pause_Click()
        {
            if (MGui)
            {
                PopUpsController puPrefab = MGui.GetPopUpPrefabByDescription("pause");
                if(puPrefab) MGui.ShowPopUp(puPrefab, () => { if (MBoard) MBoard.Pause(); }, null);
            }
        }
    }

    [System.Serializable]
    public class BoosterParent
    {
        public RectTransform parent;
        public RectTransform boosterLock;


        public void SetActiveLock(bool active)
        {
            if(boosterLock) boosterLock.gameObject.SetActive(active);
        }
    }
}
