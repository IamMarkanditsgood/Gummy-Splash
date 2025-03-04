using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mkey
{
    public class GridObject : MonoBehaviour
    {
        [Tooltip("Picture that is used in GUI")]
        [SerializeField]
        private Sprite GuiObjectImage;
        [Tooltip("Picture that is used as constructor brush")]
        [SerializeField]
        private Sprite ConstructorObjectImage;
        [SerializeField]
        private string groupID;

        #region temp vars
        [HideInInspector]
        private int id = Int32.MinValue;
        #endregion temp vars

        #region construct object properties
        public bool canUseAsTarget;
        #endregion construct object properties

        #region properties
        public int ID { get { return id; } private set { id = value; } }
        public int TargetGroupID { get { return string.IsNullOrEmpty(groupID) ? ID : groupID.GetHashCode(); }}
        public string Name { get { return name; } }
        protected SpriteRenderer SRenderer { get; set; }
        protected SoundMaster MSound { get { return SoundMaster.Instance; } }
        protected GameBoard MBoard { get { return GameBoard.Instance; } }
        protected MatchGrid MGrid { get { return MBoard.MainGrid; } }
        public Sprite ObjectImage { get { SpriteRenderer sr = GetComponent<SpriteRenderer>(); return (sr) ? sr.sprite : null; } }
        public Sprite GuiImage { get { return (GuiObjectImage) ? GuiObjectImage : ObjectImage; } }
        public Sprite ConsructorImage { get { return (ConstructorObjectImage) ? ConstructorObjectImage : GuiImage; } }
        public Sprite ConsructorImageHover { get { return ConsructorImage; } }
        public Sprite GuiImageHover { get { return GuiImage; } }
        public int Hits { get; set; }
        #endregion properties

        #region events
        public static Action<int> CollectEvent;
        #endregion events

        #region protected temp vars
        protected Action<GameObject, float, Action> delayAction = (g, del, callBack) => { SimpleTween.Value(g, 0, 1, del).AddCompleteCallBack(callBack); };
        protected TweenSeq collectSequence;
        protected TweenSeq hitDestroySeq;
        private static Canvas parentCanvas;

        protected GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        protected LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        protected GameObjectsSet GOSet { get { return GCSet.GOSet; } }
        #endregion protected temp vars

        #region regular
        void OnDestroy()
        {
            CancellTweensAndSequences();
        }
        #endregion regular

        #region common
        /// <summary>
        /// Return true if is the same object (the same reference)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal bool ReferenceEquals(GridObject other)
        {
            return System.Object.ReferenceEquals(this, other);//Determines whether the specified Object instances are the same instance.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        internal void SetLocalScale(float scale)
        {
            transform.localScale = (transform.parent) ? transform.parent.localScale * scale : new Vector3(scale, scale,scale);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        internal void SetLocalScaleX(float scale)
        {
            Vector3 parLS = (transform.parent) ? transform.parent.localScale : Vector3.one;
            float ns = parLS.x * scale ;
            transform.localScale = new Vector3(ns, parLS.y, parLS.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        internal void SetLocalScaleY(float scale)
        {
            Vector3 parLS = (transform.parent) ? transform.parent.localScale : Vector3.one;
            float ns = parLS.y * scale;
            transform.localScale = new Vector3(parLS.x, ns, parLS.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha"></param>
        internal void SetAlpha(float alpha)
        {
            if (!SRenderer) GetComponent<SpriteRenderer>();
            if (SRenderer)
            {
                Color c = SRenderer.color;
                Color newColor = new Color(c.r, c.g, c.b, alpha);
                SRenderer.color = newColor;
            }
        }

        internal void InstantiateScoreFlyer(GUIFlyer scoreFlyerPrefab, int score)
        {
            if (!scoreFlyerPrefab) return;
            if (!parentCanvas)
            {
                GameObject gC = GameObject.Find("CanvasMain");
                if (gC) parentCanvas = gC.GetComponent<Canvas>();
                if (!parentCanvas) parentCanvas = FindObjectOfType<Canvas>();
            }

            GUIFlyer flyer = scoreFlyerPrefab.CreateFlyer(parentCanvas, score.ToString());
            if (flyer)
            {
                flyer.transform.localScale = transform.lossyScale;
                flyer.transform.position = transform.position;
            }
        }

        internal void InstantiateGuiTargetFlyer(GUIFlyer prefab)
        {
            InstantiateGuiTargetFlyer(prefab, transform.position);
        }

        internal void InstantiateGuiTargetFlyer(GUIFlyer prefab, Vector3 posW)
        {
            if (!prefab) return;
            if (!parentCanvas)
            {
                GameObject gC = GameObject.Find("CanvasMain");
                if (gC) parentCanvas = gC.GetComponent<Canvas>();
                if (!parentCanvas) parentCanvas = FindObjectOfType<Canvas>();
            }

            GUIFlyer flyer = prefab.CreateFlyer(parentCanvas, null, MBoard.GetGuiTargetPosW(TargetGroupID));
            if (flyer)
            {
                flyer.transform.localScale = transform.lossyScale;
                flyer.transform.position = posW;
            }
        }

        public void SetSprite(Sprite nSprite)
        {
            if (SRenderer) SRenderer.sprite = nSprite;
        }

        public GridCell GetParentCell()
        {
            return GetComponentInParent<GridCell>();
        }


        public void DestroyHierCompetitor(GridCell gCell)
        {
            DestroyHierCompetitor(gCell, true);
        }

        public void DestroyHierCompetitor(GridCell gCell, bool andProxy)
        {
            if (!gCell) return;
            if (GetSize() == Vector2.one)   // simple object
            {
                GridObject gO = gCell.GetHierarchyObject(GetHierarchy(), andProxy);
                if (gO) gCell = gO.GetParentCell();
                if (gO && gCell)
                {
                    gCell.RemoveObject(gO.ID);
                }
            }
            else                            // multicells object
            {
                List<GridCell> gridCells = GetOccupiedCells(gCell);
                gridCells.ApplyAction((gC) => {
                    GridObject gOH = gC.GetHierarchyObject(GetHierarchy(), andProxy);
                    if (gOH)
                    {
                        GridCell cell = gOH.GetParentCell();
                        if ( cell)
                        {
                            cell.RemoveObject(gOH.ID);
                        }
                    }
                });
            }

            //GridObject gO = gCell.GetHierarchyObject(GetHierarchy());
            //if(gO) gCell.RemoveObject(gO.ID);
        }

        #endregion common

        #region virtual
        /// <summary>
        /// Hit object from collected, bomb or booster
        /// </summary>
        /// <param name="completeCallBack"></param>
        public virtual void Hit(GridCell gCell,  Action completeCallBack)
        {
            completeCallBack?.Invoke();
        }

        /// <summary>
        /// Hit this object from the side match
        /// </summary>
        /// <param name="completeCallBack"></param>
        public virtual void SideMatchHit(GridCell gCell,int matchID, Action completeCallBack)
        {
            completeCallBack?.Invoke();
        }

        /// <summary>
        /// Cancel all tweens and sequences
        /// </summary>
        public virtual void CancellTweensAndSequences()
        {
            collectSequence?.Break();
            hitDestroySeq?.Break();
            SimpleTween.Cancel(gameObject, false);
        }

        /// <summary>
        /// Changing the rendering order of an object
        /// </summary>
        /// <param name="set"></param>
        public virtual void SetToFront(bool set)
        {

        }

        public virtual GridObject Create(GridCell parent)
        {
            return parent? Instantiate(this, parent.transform) : Instantiate(this);
        }

        public virtual GridObject Create(GridCell parent, int hitsCount)
        {
            return parent ? Instantiate(this, parent.transform) : Instantiate(this);
        }

        public virtual Sprite[] GetProtectionStateImages()
        {
            return null;
        }

        /// <summary>
        /// We check whether this object can be placed in a cell of the game board according to its size and the boundaries of the game board
        /// </summary>
        /// <param name="gCell"></param>
        /// <returns></returns>
        public virtual bool CanSetBySize(GridCell gCell)
        {
            return true;
        }

        /// <summary>
        /// Raise an event when collecting an object from the game board
        /// </summary>
        public virtual void CollectEventRaise()
        {
            CollectEvent?.Invoke(TargetGroupID);
        }

        /// <summary>
        /// All objects have their own hierarchy. A grid cell of the game board can have only one object of a given hierarchy.
        /// 0 - MainHierarchy, 10 - OverHierarchy, -10 - UnderHierarchy, -20 - SubUnderHierarchy
        /// </summary>
        /// <returns></returns>
        public virtual int GetHierarchy()
        {
            return 0;
        }

        /// <summary>
        /// Get occupied size (rows, columns)
        /// </summary>
        /// <returns></returns>
        public virtual Vector2Int GetSize()
        {
            return Vector2Int.one;
        }

        /// <summary>
        /// Returns an list of cells that are occupied or can be occupied by a given object according to its size
        /// </summary>
        /// <param name="gCell"></param>
        /// <returns></returns>
        public virtual List<GridCell> GetOccupiedCells(GridCell gCell)
        {
            List<GridCell> res = new List<GridCell>();
            res.Add(gCell);
            return res;
        }

        /// <summary>
        /// Returns an list of cells that are occupied by a given object according to its size
        /// </summary>
        /// <returns></returns>
        public virtual List<GridCell> GetOccupiedCells()
        {
            return GetOccupiedCells(GetParentCell());
        }

        /// <summary>
        /// The object can independently move along the path of filling into a free cell
        /// </summary>
        /// <returns></returns>
        public virtual bool CanSelfMove()
        {
            return false;
        }

        /// <summary>
        /// An object can swap with another object
        /// </summary>
        /// <returns></returns>
        public virtual bool CanSwap()
        {
            return false;
        }

        /// <summary>
        /// the object participates in mixing with other objects
        /// </summary>
        /// <returns></returns>
        public virtual bool CanMix()
        {
            return false;
        }

        public virtual Sprite GetTargetImage()
        {
            return GuiImage;
        }

        public virtual int RemainingHits()
        {
            return (GetProtectionStateImages() != null) ? 1 - Hits + GetProtectionStateImages().Length : 1 - Hits;
        }
        #endregion virtual

        public void Enumerate(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// notify the remaining objects on the game board
        /// </summary>
        public virtual void TargetCollectEventHandler(TargetData targetData)
        {
           
        }

        /// <summary>
        /// notify the remaining objects on the game board
        /// </summary>
        public virtual void TargetReachedEventHandler(TargetData targetData)
        {

        }


    }

    [Serializable]
    public class GridObjectState
    {
        public int id;
        public int hits;

        public GridObjectState(int id, int hits)
        {
            this.id = id;
            this.hits = hits;
        }
    }
}

