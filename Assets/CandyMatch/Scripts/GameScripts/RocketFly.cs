using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mkey
{
    public class RocketFly : MonoBehaviour
    {
        [SerializeField]
        private EaseAnim easeAnim = EaseAnim.EaseInCubic;
        [SerializeField]
        private float speed = 8f;
        public bool enableOrientation;
        public Transform spriteTransform;
        public GridCell startCell;
        public DynamicClickBombRandRocket randRocketBomb;
        public Action EndOFlyAction;
        public Action<GridCell>SelectTargetAction;
        public GameObject TargetSelectorPrefab;

        [SerializeField]
        private float minDeltaSqr = 0.01f;
        private const float flyHight = 4f;

        #region temp vars
        private SceneCurve flyCurve;
        private Vector3 lastPosition;
        private Vector3 currPosition;
        private Vector3 moveDir;
        private GameObject flyCurveObject;
        private GridCell target;
        private GameObject tsGO;
        private Vector3 targetPos;
        #endregion temp vars

        #region regular
        IEnumerator Start()
        {
            lastPosition = transform.position;

            // set up
            TweenExt.RotateZTween(gameObject, spriteTransform, -45.4f, 1, EaseAnim.EaseOutBounce, null);
            yield return new WaitForSeconds(1f);

            CreateFlyCurve();

            // orientation to curve
            if (enableOrientation)
            {
                Vector3 _p = flyCurve.transform.TransformPoint(flyCurve.Curve.GetPosition(0.1f));
                moveDir = _p - lastPosition;
                SimpleTween.Value(gameObject, Vector3.up, moveDir, 0.3f).SetOnUpdate((Vector3 dir) => { transform.up = dir; });
                yield return new WaitForSeconds(0.3f);
            }
            yield return null;

            float startSpeed = speed;
            float endSpeed = speed * 2f;
            float length = flyCurve.Length;
            WaitForEndOfFrame wfef = new WaitForEndOfFrame();
            float t = length / (endSpeed - startSpeed);
            float a = (endSpeed - startSpeed) / t;
            float dt = 0;
            float d = 0;

            // fly
            while (length > d)
            {
                dt += Time.deltaTime;
                d = startSpeed * dt + a * dt * dt * 0.5f;
                d = (d > length) ? length : d;
                transform.position = flyCurve.transform.TransformPoint(flyCurve.Curve.GetPosition(d));
                if (d + 0.1f < length)
                {
                    targetPos = flyCurve.transform.TransformPoint(flyCurve.Curve.GetPosition(d + 0.1f)); // +0.1f test
                    moveDir = targetPos - transform.position;
                    transform.up = moveDir;   // transform.up = Vector3.Lerp(transform.up, moveDir, 0.5f); // not work on mobile
                }
                yield return wfef;
            }

            if (tsGO) Destroy(tsGO, 0f);
            if (flyCurveObject) Destroy(flyCurveObject, 1f);
            EndOFlyAction?.Invoke();

        }
        #endregion regular

        private void CreateFlyCurve()
        {
            flyCurveObject = new GameObject();
            flyCurveObject.transform.position = startCell.transform.position;
            flyCurve = flyCurveObject.transform.GetOrAddComponent<SceneCurve>();

            CellsGroup cellsGroup = randRocketBomb.GetArea(startCell);
            List<int> targetIDs = randRocketBomb.GetTargetIds();

            if (cellsGroup.Length > 0)
            {
               // Debug.Log("Target search, cellsGroup.Length: " + cellsGroup.Length + "; target ids: " + targetIDs.MakeString(", ")) ;
                cellsGroup.Cells.RemoveAll((c) => { return !c.CanSetBombForTargets(targetIDs); });
                if (cellsGroup.Length > 0) target = cellsGroup.Cells.GetRandomPos();
            }

            if (!target) 
            {
               // Debug.Log("Target not found, search random match");
                List<GridCell> cells = GameBoard.Instance.MainGrid.GetRandomMatch(GameBoard.Instance.MainGrid.Cells.Count);
                foreach (var item in cells)
                {
                    if (item.GetMyBombCount() == 0) target = item;
                }
            }

            if (!target) { 
               // Debug.Log("Target not found, search random pos");
                target = GameBoard.Instance.MainGrid.Cells.GetRandomPos();
            }

            if (target) { target.SetMyBomb(randRocketBomb); tsGO = Instantiate(TargetSelectorPrefab, target.transform.position, Quaternion.identity);  }

            SelectTargetAction?.Invoke(target);

            List<Vector3> attackPosLs = DynamicClickBombRandRocket.GetAttackPositions(startCell, target, flyHight);
            int cLength = flyCurve.Curve.HandlesCount;
            for (int i = 0; i < attackPosLs.Count; i++)
            {
                if (i < cLength)
                {
                    flyCurve.ChangePoint(i, attackPosLs[i]);
                }
                else
                {
                    flyCurve.AddPoint(attackPosLs[i]);
                }
            }
        }
    }
}