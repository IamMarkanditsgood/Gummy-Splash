using UnityEngine;
using System;
using System.Collections.Generic;

/*
    02.12.2019 - first
    19.02.2020 - changes 
 */
namespace Mkey
{
    [CreateAssetMenu]
    public class GameObjectsSet : BaseScriptable, ISerializationCallbackReceiver
    {
        public Sprite[] backGrounds;

        public GameObject gridCellOdd;
        public GameObject gridCellEven;

        [SerializeField]
        private DisabledObject disabledObject; // flag for disabled gridcell

        [SerializeField]
        private List<BlockedObject> blockedObjects;

        [SerializeField]
        private List<BlockedBoxObject> blockedBoxObjects;

        [SerializeField]
        private List<MatchObject> matchObjects;

        [SerializeField]
        private List<OverlayObject> overlayObjects;

        [SerializeField]
        private List<UnderlayObject> underlayObjects;

        [SerializeField]
        private List<SubUnderlayMCObject> subUnderlayMCObjects;

        [Space(8)]
        [SerializeField]
        private DynamicClickBombObject dynamicClickBombObjectVertical;

        [SerializeField]
        private DynamicClickBombObject dynamicClickBombObjectHorizontal;

        [SerializeField]
        private DynamicClickBombObject dynamicClickBombObjectRadial;

        [SerializeField]
        private DynamicClickBombObject dynamicClickBombObjectRandom;

        [SerializeField]
        private DynamicClickBombObject dynamicClickBombObjectColor;

        [Space(8)]
        [SerializeField]
        private StaticMatchBombObject staticMatchBombObjectVertical;

        [SerializeField]
        private StaticMatchBombObject staticMatchBombObjectHorizontal;

        [SerializeField]
        private StaticMatchBombObject staticMatchBombObjectRadial;

        [SerializeField]
        private StaticMatchBombObject staticMatchBombObjectColor;

        [SerializeField]
        private List<Booster> boosterObjects;

        [SerializeField]
        private List<DynamicBlockerObject> dynamicBlockerObjects;

        [SerializeField]
        private List<HiddenObject> hiddenObjects;

        [SerializeField]
        private List<FallingObject> fallingObjects;

        [SerializeField]
        private List<TreasureObject> treasureObjects;

        private List<GridObject> targetObjects;

        #region properties
        public IList<MatchObject> MatchObjects
        {
            get { if (!enumerated) Enumerate(); return matchObjects.AsReadOnly(); }
        }

        public IList<BlockedObject> BlockedObjects
        {
            get { if (!enumerated) Enumerate(); return blockedObjects.AsReadOnly(); }
        }

        public IList<BlockedBoxObject> BlockedBoxObjects
        {
            get { if (!enumerated) Enumerate(); return blockedBoxObjects.AsReadOnly(); }
        }

        public IList<DynamicBlockerObject> DynamicBlockerObjects
        {
            get { if (!enumerated) Enumerate(); return dynamicBlockerObjects.AsReadOnly(); }
        }

        public DisabledObject Disabled // flag for disabled gridcell
        {
            get { if (!enumerated) Enumerate(); return disabledObject; }
        }

        public IList<OverlayObject> OverlayObjects
        {
            get { if (!enumerated) Enumerate(); return overlayObjects.AsReadOnly(); }
        }

        public IList<UnderlayObject> UnderlayObjects
        {
            get { if (!enumerated) Enumerate(); return underlayObjects.AsReadOnly(); }
        }

        public IList<SubUnderlayMCObject> SubUnderlayMCObjects
        {
            get { if (!enumerated) Enumerate(); return subUnderlayMCObjects.AsReadOnly(); }
        }

        public IList<Booster> BoosterObjects { get { if (!enumerated) Enumerate(); return boosterObjects.AsReadOnly(); } }

        public IList<GridObject> TargetObjects { get { if (!enumerated) Enumerate(); if(!targetsCreated) CreateTargets(); return targetObjects.AsReadOnly(); } }

        public IList<StaticMatchBombObject> StaticMatchBombObjects { get {
                if (!enumerated) Enumerate();
                List<StaticMatchBombObject> res = new List<StaticMatchBombObject>();
                res.Add(staticMatchBombObjectVertical);
                res.Add(staticMatchBombObjectHorizontal);
                res.Add(staticMatchBombObjectRadial);
                //foreach (var item in matchObjects)
                //{
                //    res.Add(item.staticMatchBombObjectColor);
                //}
                return res.AsReadOnly();
            } }

        public IList<DynamicMatchBombObject> DynamicMatchBombObjects { get {
                if (!enumerated) Enumerate();
                List<DynamicMatchBombObject> res = new List<DynamicMatchBombObject>();
                foreach (var item in matchObjects)
                {
                    res.Add(item.dynamicMatchBombObjectVertical);
                    res.Add(item.dynamicMatchBombObjectHorizontal);
                    res.Add(item.dynamicMatchBombObjectRadial);
                   // res.Add(item.dynamicMatchBombObjectColor);
                }
                return res.AsReadOnly();

            } }

        public IList<DynamicClickBombObject> DynamicClickBombObjects { get {
                if (!enumerated) Enumerate();
                List<DynamicClickBombObject> res = new List<DynamicClickBombObject>();
                res.Add(dynamicClickBombObjectVertical);
                res.Add(dynamicClickBombObjectHorizontal);
                res.Add(dynamicClickBombObjectRadial);
                res.Add(dynamicClickBombObjectRandom);
                res.Add(dynamicClickBombObjectColor);
                //foreach (var item in matchObjects)
                //{
                //    res.Add(item.dynamicClickBombObjectColor);
                //}
                return res.AsReadOnly();
            }
        }

        public IList <FallingObject> FallingObjects 
        {
            get { if (!enumerated) Enumerate(); return fallingObjects.AsReadOnly(); }
        }

        public IList<TreasureObject> TreasureObjects
        {
            get { if (!enumerated) Enumerate(); return treasureObjects.AsReadOnly(); }
        }

        public IList<HiddenObject> HiddenObjects
        {
            get { if (!enumerated) Enumerate(); return hiddenObjects.AsReadOnly(); }
        }

        public int RegularLength
        {
            get { return MatchObjects.Count; }
        }
        #endregion properties

        #region temp vars
        private bool enumerated = false;
        private bool targetsCreated = false;
        #endregion temp vars

        void OnEnable()
        {
            enumerated = false;
           targetsCreated = false;
        }

        #region serialization
        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            //   Debug.Log("deserialized ");
        }
        #endregion serialization

        #region get object
        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public MatchObject GetMainObject(int id)
        {
            foreach (var item in MatchObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns random objects array.
        /// </summary>
        /// <returns>Reference to char array</returns>
        public List<MatchObject> GetMainRandomObjects(int count)
        {
            List<MatchObject> r = new List<MatchObject>(count);
            IList<MatchObject> source = MatchObjects;

            for (int i = 0; i < count; i++)
            {
                int rndNumber = UnityEngine.Random.Range(0, source.Count);
                r.Add(source[rndNumber]);
            }
            return r;
        }

        /// <summary>
        /// Returns random MainObjectData array without "notInclude" list featured objects .
        /// </summary>
        public List<MatchObject> GetMainRandomObjects(int count, List<GridObject> notInclude)
        {
            List<MatchObject> r = new List<MatchObject>(count);
            List<MatchObject> source = new List<MatchObject>(MatchObjects);

            if (notInclude != null)
                for (int i = 0; i < notInclude.Count; i++)
                {
                    source.RemoveAll((mOD) => { return mOD.ID == notInclude[i].ID; });
                }

            for (int i = 0; i < count; i++)
            {
                int rndNumber = UnityEngine.Random.Range(0, source.Count);
                r.Add(source[rndNumber]);
            }
            return r;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public OverlayObject GetOverlayObject(int id)
        {
            foreach (var item in OverlayObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public BlockedObject GetBlockedObject(int id)
        {
            foreach (var item in BlockedObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public UnderlayObject GetUnderlayObject(int id)
        {
            foreach (var item in UnderlayObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public SubUnderlayMCObject GetSubUnderlayMCObject(int id)
        {
            foreach (var item in SubUnderlayMCObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public StaticMatchBombObject GetStaticMatchBombObject(int id)
        {
            foreach (var item in StaticMatchBombObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public StaticMatchBombObject GetStaticMatchBombObject(BombDir bombDir, int matchID)
        {
            //if (bombDir == BombDir.Color)
            //{
            //    foreach (var item in StaticMatchBombObjects)
            //    {
            //        if (item && bombDir == item.bombDirection &&  item.matchID == matchID) return item;
            //    }
            //    return null;
            //}

            foreach (var item in StaticMatchBombObjects)
            {
                if (item &&  bombDir == item.bombDirection) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public DynamicMatchBombObject GetDynamicMatchBombObject(int id)
        {
            foreach (var item in DynamicMatchBombObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public DynamicMatchBombObject GetDynamicMatchBombObject(BombDir bType, int matchID)
        {
            foreach (var item in DynamicMatchBombObjects)
            {
                if (item && bType == item.bombDirection && item.matchID == matchID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public DynamicClickBombObject GetDynamicClickBombObject(int id)
        {
            foreach (var item in DynamicClickBombObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public DynamicClickBombObject GetDynamicClickBombObject(BombDir bombDir, int matchID)
        {
            //if (bombDir == BombDir.Color)
            //{
            //    foreach (var item in DynamicClickBombObjects)
            //    {
            //        if (item && bombDir == item.bombDirection && item.matchID == matchID) return item;
            //    }
            //    return null;
            //}
            foreach (var item in DynamicClickBombObjects)
            {
                if (item && bombDir == item.bombDirection) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public DynamicClickBombObject GetDynamicClickBombObject(BombDir bombDir)
        {
            //if (bombDir == BombDir.Color) return null;
            foreach (var item in DynamicClickBombObjects)
            {
                if (item && bombDir == item.bombDirection) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public FallingObject GetFallingObject(int id)
        {
            foreach (var item in FallingObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public HiddenObject GetHiddenObject(int id)
        {
            foreach (var item in HiddenObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public TreasureObject GetTreasureObject(int id)
        {
            foreach (var item in TreasureObjects)
            {
                if (item && id == item.ID) return item;
            }
            return null;
        }

        /// <summary>
        /// Returns reference  object from set.
        /// </summary>
        /// <returns>Reference letter</returns>
        public GridObject GetObject(int id)
        {
            if (id == 1) return Disabled;

            foreach (var item in MatchObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in OverlayObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in UnderlayObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in SubUnderlayMCObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in BlockedObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in BlockedBoxObjects)
            {
                if (item && id == item.ID) return item;
            }
            foreach (var item in StaticMatchBombObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in DynamicMatchBombObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in DynamicClickBombObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in FallingObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in HiddenObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in TreasureObjects)
            {
                if (item && id == item.ID) return item;
            }

            foreach (var item in DynamicBlockerObjects)
            {
                if (item && id == item.ID) return item;
            }

            return null;
        }

        public GridObject GetTargetObject(int targetGroupID)
        {
            foreach (var item in TargetObjects)
            {
                if (item.TargetGroupID == targetGroupID) return item;
            }
            return null;
        }
        #endregion get object 

        #region contain
        public bool ContainID(int id)
        {
            return
                (
                   ContainMatchID(id)
                || ContainOverlayID(id)
                || ContainUnderlayID(id)
                || ContainSubUnderlayMCID(id)
                || ContainBoosterID(id)
                || ContainDynamicClickBombID(id)
                || ContainDynamicMatchBombID(id)
                || ContainStaticMatchBombID(id)
                || ContainBlockedID(id)
                || ContainBoxBlockedID(id)
                || IsDisabledObject(id)
                || ContainFallingObjectID(id)
                || ContainDynamicBlockerID(id)
                || ContainHiddenObjectID(id)
                || ContainTreasureObjectID(id)
                );
        }

        public bool ContainMatchID(int id)
        {
            return ContainID(MatchObjects, id);
        }

        public bool ContainBlockedID(int id)
        {
            return ContainID(BlockedObjects, id);
        }

        public bool ContainBoxBlockedID(int id)
        {
            return ContainID(BlockedBoxObjects, id);
        }

        public bool ContainBoosterID(int id)
        {
            if (BoosterObjects == null || BoosterObjects.Count == 0) return false;
            foreach (var item in BoosterObjects)
            {
                if (item.ID == id) return true;
            }
            return false;
        }

        public bool ContainOverlayID(int id)
        {
            return ContainID(OverlayObjects, id);
        }

        public bool ContainUnderlayID(int id)
        {
            return ContainID(UnderlayObjects, id);
        }

        public bool ContainSubUnderlayMCID(int id)
        {
            return ContainID(SubUnderlayMCObjects, id);
        }

        public bool ContainStaticMatchBombID(int id)
        {
            return ContainID(StaticMatchBombObjects, id);
        }

        public bool ContainDynamicMatchBombID(int id)
        {
            return ContainID(DynamicMatchBombObjects, id);
        }

        public bool ContainDynamicClickBombID(int id)
        {
            return ContainID(DynamicClickBombObjects, id);
        }

        public bool ContainDynamicBlockerID(int id)
        {
            return ContainID(DynamicBlockerObjects, id);
        }

        public bool ContainTargetForID(int id)
        {
            return ContainID(TargetObjects, id);
        }

        public bool ContainFallingObjectID(int id)
        {
            return ContainID(FallingObjects, id);
        }

        public bool ContainUnderlayObjectID(int id)
        {
            return ContainID(UnderlayObjects, id);
        }

        public bool ContainHiddenObjectID(int id)
        {
            return ContainID(HiddenObjects, id);
        }

        public bool ContainTreasureObjectID(int id)
        {
            return ContainID(TreasureObjects, id);
        }
        #endregion contain

        public Sprite GetBackGround(int index)
        {
            index = (int)Mathf.Repeat(index, backGrounds.Length);
            return backGrounds[index];
        }

        public int BackGroundsCount
        {
            get { return backGrounds.Length; }
        }

        public Sprite DisabledCellSprite
        {
            get { return Disabled.ObjectImage; }
        }

        private void CreateTargets()
        {
            if (targetsCreated) return;
            targetObjects = new List<GridObject>();

            if (overlayObjects != null)
                foreach (var item in overlayObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (underlayObjects != null)
                foreach (var item in underlayObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (subUnderlayMCObjects != null)
                foreach (var item in subUnderlayMCObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (hiddenObjects != null)
                foreach (var item in hiddenObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (treasureObjects != null)
                foreach (var item in treasureObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (matchObjects != null)
                foreach (var item in matchObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (fallingObjects != null)
                foreach (var item in fallingObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (dynamicBlockerObjects != null)
                foreach (var item in dynamicBlockerObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (blockedObjects != null)
                foreach (var item in blockedObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            if (blockedBoxObjects != null)
                foreach (var item in blockedBoxObjects)
                {
                    if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
                }

            foreach (var item in StaticMatchBombObjects)
            {
                if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
            }

            foreach (var item in DynamicMatchBombObjects)
            {
                if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
            }

            foreach (var item in DynamicClickBombObjects)
            {
                if (item && item.canUseAsTarget && !ContainTargetGroupID(targetObjects, item.TargetGroupID)) targetObjects.Add(item);
            }

            // Debug.Log("create targets: "+ targetObjects.MakeString("; "));
            targetsCreated = true;
        }

        private bool ContainTargetGroupID(List<GridObject> gridObjects, int targetGroupID)
        {
            foreach (var item in gridObjects)
            {
                if (item.TargetGroupID == targetGroupID) return true;
            }
            return false;
        }

        public bool ContainTargetGroupID( int targetGroupID)
        {
            foreach (var item in TargetObjects)
            {
                if (item.TargetGroupID == targetGroupID) return true;
            }
            return false;
        }

        public static bool IsDisabledObject(int id)
        {
            return id == 1;
        }

        public static bool IsBlockedObject(int id)
        {
            return (id >=100) && (id < 1000);
        }

        public static bool IsFallingObject(int id)
        {
            return (id >= 500000) && (id < 600000);
        }

        private void Enumerate()
        {
            if (enumerated) return;
            // set ids for game objects
            if (disabledObject) disabledObject.Enumerate(1);
            EnumerateArray(blockedObjects, 100);
            EnumerateArray(blockedBoxObjects, 200);
            EnumerateArray(matchObjects, 1000);
            EnumerateArray(overlayObjects, 100000);
            EnumerateArray(underlayObjects, 200000);
            EnumerateArray(subUnderlayMCObjects, 210000);

            int startIndex = 300000;
            foreach (var item in boosterObjects)
            {
                item.Enumerate(startIndex++);
            }

            if (dynamicClickBombObjectVertical) dynamicClickBombObjectVertical.bombDirection = BombDir.Vertical;
            if (dynamicClickBombObjectHorizontal) dynamicClickBombObjectHorizontal.bombDirection = BombDir.Horizontal;
            if (dynamicClickBombObjectRadial) dynamicClickBombObjectRadial.bombDirection = BombDir.Radial;
            if (dynamicClickBombObjectRandom) dynamicClickBombObjectRandom.bombDirection = BombDir.Random;
            if (dynamicClickBombObjectColor) dynamicClickBombObjectColor.bombDirection = BombDir.Color;

            if (dynamicClickBombObjectVertical) dynamicClickBombObjectVertical.Enumerate(400000);
            if (dynamicClickBombObjectHorizontal) dynamicClickBombObjectHorizontal.Enumerate(400001);
            if (dynamicClickBombObjectRadial) dynamicClickBombObjectRadial.Enumerate(400002);
            if (dynamicClickBombObjectRandom) dynamicClickBombObjectRandom.Enumerate(400003);
            if (dynamicClickBombObjectColor) dynamicClickBombObjectColor.Enumerate(400004);

            if (staticMatchBombObjectVertical) staticMatchBombObjectVertical.bombDirection = BombDir.Vertical;
            if (staticMatchBombObjectHorizontal) staticMatchBombObjectHorizontal.bombDirection = BombDir.Horizontal;
            if (staticMatchBombObjectRadial) staticMatchBombObjectRadial.bombDirection = BombDir.Radial;
            if (staticMatchBombObjectVertical) staticMatchBombObjectVertical.Enumerate(400010);
            if (staticMatchBombObjectHorizontal) staticMatchBombObjectHorizontal.Enumerate(400011);
            if (staticMatchBombObjectRadial) staticMatchBombObjectRadial.Enumerate(400012);

            int startID = 400100;
            foreach (var item in matchObjects)
            {
                if (item && item.dynamicMatchBombObjectVertical)
                {
                    item.dynamicMatchBombObjectVertical.bombDirection = BombDir.Vertical;
                    item.dynamicMatchBombObjectVertical.Enumerate(startID++);
                    item.dynamicMatchBombObjectVertical.matchID = item.ID;
                }

                if (item && item.dynamicMatchBombObjectHorizontal)
                {
                    item.dynamicMatchBombObjectHorizontal.bombDirection = BombDir.Horizontal;
                    item.dynamicMatchBombObjectHorizontal.Enumerate(startID++);
                    item.dynamicMatchBombObjectHorizontal.matchID = item.ID;
                }

                if (item && item.dynamicMatchBombObjectRadial)
                {
                    item.dynamicMatchBombObjectRadial.bombDirection = BombDir.Radial;
                    item.dynamicMatchBombObjectRadial.Enumerate(startID++);
                    item.dynamicMatchBombObjectRadial.matchID = item.ID;
                }

                //if (item && item.dynamicMatchBombObjectColor)
                //{
                //    item.dynamicMatchBombObjectColor.bombDirection = BombDir.Color;
                //    item.dynamicMatchBombObjectColor.Enumerate(startID++);
                //    item.dynamicMatchBombObjectColor.matchID = item.ID;
                //}

                //if (item && item.staticMatchBombObjectColor)
                //{
                //    item.staticMatchBombObjectColor.bombDirection = BombDir.Color;
                //    item.staticMatchBombObjectColor.Enumerate(startID++);
                //    item.staticMatchBombObjectColor.matchID = item.ID;
                //}

                //if (item && item.dynamicClickBombObjectColor)
                //{
                //    item.dynamicClickBombObjectColor.bombDirection = BombDir.Color;
                //    item.dynamicClickBombObjectColor.Enumerate(startID++);
                //    item.dynamicClickBombObjectColor.matchID = item.ID;
                //}
            }

            EnumerateArray(fallingObjects, 500000);

            EnumerateArray(dynamicBlockerObjects, 600000);

            EnumerateArray(hiddenObjects, 800000);

            EnumerateArray(treasureObjects, 900000);

            Debug.Log("Enumerate");
            enumerated = true;
    }

        public int GetPrefabIdForBoosterObject(Booster booster)
        {
            foreach (var item in BoosterObjects)
            {
                if (item == booster) return item.ID;
            }
            return -1;
        }

        #region utils
        private void EnumerateArray<T>(ICollection<T> a, int startIndex) where T : GridObject
        {
            if (a != null && a.Count > 0)
            {
                foreach (var item in a)
                {
                    if(item)  item.Enumerate(startIndex++);
                }
            }
        }

        private bool ContainID<T>(ICollection<T> a, int id) where T : GridObject
        {
            if (a == null || a.Count == 0) return false;
            foreach (var item in a)
            {
                if (item!=null && item.ID == id) return true;
            }
            return false;
        }
        #endregion utils
    }

    [Serializable]
    public class CellData
    {
        [SerializeField]
        private int id;
        [SerializeField]
        private int row;
        [SerializeField]
        private int column;

        public int ID { get { return id; } }
        public int Row { get { return row; } }
        public int Column { get { return column; } }

        public CellData(int id, int row, int column)
        {
            this.row = row;
            this.column = column;
            this.id = id;
        }
    }

    /// <summary>
    /// Helper serializable class object with the equal ID
    /// </summary>
    [Serializable]
    public class ObjectsSetData
    {
        public Action <int> ChangeEvent;

        [SerializeField]
        private int id;
        [SerializeField]
        private int count;

        public int ID { get { return id; } }
        public int Count { get { return count; } }

        public ObjectsSetData(int id, int count)
        {
            this.id = id;
            this.count = count;
        }

        public Sprite GetImage(GameObjectsSet mSet)
        {
            return mSet.GetMainObject(id).ObjectImage;
        }

        public void IncCount()
        {
            SetCount(count + 1);
        }

        public void DecCount()
        {
            SetCount(count - 1);
        }

        public void SetCount(int newCount)
        {
            newCount = Mathf.Max(0, newCount);
            bool changed = (Count != newCount);
            count = newCount;
            if(changed)  ChangeEvent?.Invoke(count);
        }
    }

    /// <summary>
    /// Helper class that contains list of object set data 
    /// </summary>
    [Serializable]
    public class ObjectSetCollection
    {
        [SerializeField]
        private List<ObjectsSetData> list;

        public IList<ObjectsSetData> ObjectsList { get { return list.AsReadOnly(); } }

        public ObjectSetCollection()
        {
            list = new List<ObjectsSetData>();
        }

        public ObjectSetCollection(ObjectSetCollection oSCollection)
        {
            list = new List<ObjectsSetData>();
            Add(oSCollection);
        }

        public ObjectSetCollection(List<ObjectsSetData> oSCollection)
        {
            list = new List<ObjectsSetData>();
            Add(oSCollection);
        }

        public uint Count
        {
            get { return (list == null) ? 0 : (uint)list.Count; }
        }

        public void AddById(int id, int count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id)
                {
                    list[i].SetCount(list[i].Count + count);
                    return;
                }
            }
            list.Add(new ObjectsSetData(id, count));
        }

        public void RemoveById(int id, int count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id)
                {
                    list[i].SetCount(list[i].Count - count);
                  //  if (list[i].Count == 0) list.RemoveAt(i);
                    return;
                }
            }
        }

        public void SetCountById(int id, int count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id)
                {
                    list[i].SetCount(count);
                    return;
                }
            }
            list.Add(new ObjectsSetData(id, count));
        }

        public void CleanById(int id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id)
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        public int CountByID(int id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id)
                    return list[i].Count;
            }
            return 0;
        }

        public bool ContainObjectID(int id)
        {
            return CountByID(id) > 0;
        }

        public void Add(ObjectSetCollection oSCollection)
        {
            if (oSCollection != null)
            {
                foreach (var item in oSCollection.ObjectsList)
                {
                    AddById(item.ID, item.Count);
                }
            }
        }

        public void Add(List<ObjectsSetData> oSCollection)
        {
            if (oSCollection != null)
            {
                foreach (var item in oSCollection)
                {
                    AddById(item.ID, item.Count);
                }
            }
        }

        public void RemoveAll(Func<ObjectsSetData, bool> func)
        {
            list.RemoveAll((item) => { return func(item); });
        }
    }
}