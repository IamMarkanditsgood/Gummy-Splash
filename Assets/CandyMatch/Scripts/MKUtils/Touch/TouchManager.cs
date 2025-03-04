using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mkey
{
    public class TouchManager : MonoBehaviour
    {
        public static TouchManager Instance;
        public bool dlog = false;

        [SerializeField]
        private bool showDrag = true;

        #region properties
        public GridCell Target
        {
            get; private set;
        }

        public GridCell Source
        {
            get; private set;
        }

        public GridObject Draggable
        {
            get; private set;
        }
        #endregion properties

        #region temp vars
        private Vector3 dragPos;
        private Vector3 pointerDownPos;
        private Vector3 draggableStartPos;
        private TouchPadEventArgs tPEA;
        private bool followStarted = false;
        private GridCell dirTarget;
        private Vector3 dragDirection;
        private Vector3 dragDirectionPrior;
        private float dragMagnitude;
        private Action <Action> ResetDragEvent;
        #endregion temp vars

        #region regular
        private IEnumerator Start()
        {
            if (Instance != null) Destroy(gameObject);
            else
            {
                Instance = this;
            }

            while (!TouchPad.Instance) yield return new WaitForEndOfFrame();
            TouchPad.Instance.ScreenDragEvent += Drag;
            TouchPad.Instance.ScreenPointerDownEvent +=PointerDownEventHandler;
        }
        #endregion regular

        /// <summary>
        /// Return true if touchpad is touched with mouse or finger
        /// </summary>
        public static bool IsTouched
        {
            get
            {
                return TouchPad.Instance.IsTouched;
            }
        }

        /// <summary>
        /// Return true touch pad run on mobile device
        /// </summary>
        public static bool IsMobileDevice()
        {
            //check if our current system info equals a desktop
            if (SystemInfo.deviceType == DeviceType.Desktop)
            {
                //we are on a desktop device, so don't use touch
                return false;
            }
            //if it isn't a desktop, lets see if our device is a handheld device aka a mobile device
            else if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                //we are on a mobile device, so lets use touch input
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enable or disable touch pad callbacks handling.
        /// </summary>
        internal static void SetTouchActivity(bool activity)
        {
            TouchPad.Instance.SetTouchActivity(activity);
        }

        public void Drag(TouchPadEventArgs tpea)
        {
            tPEA = tpea;
            dragPos = tpea.WorldPos;
            dragDirection = dragPos - pointerDownPos;
            dragMagnitude = dragDirection.magnitude;
            bool criticalDrag = dragMagnitude > GameBoard.MaxDragDistance;

            #if UNITY_EDITOR
            if (dlog) Debug.Log("drag: " + gameObject.name + " ; Draggable: " + Draggable + " ; distance:" + dragMagnitude);
            #endif
            if (Draggable)
            {
                if(!followStarted && !criticalDrag) StartCoroutine(SlowFollowC());

                else if (criticalDrag && !Target) // critical distance
                {
                    if (!dirTarget) dirTarget = GetDirTarget(GetPriorityDirType(GetPriorityDir(dragDirection)));
                    if (dirTarget)
                    {
                        #if UNITY_EDITOR
                            if (dlog) Debug.Log("manualy drag enter");
                        #endif
                        dirTarget.DragEnterEvent(tpea);// manualy raise dragenter event
                    }
                    else ResetDrag(null);
                }
            }
        }

        private void PointerDownEventHandler(TouchPadEventArgs tpea)
        {
            pointerDownPos = tpea.WorldPos;
        }

        private IEnumerator SlowFollowC() // slow motion
        {
            followStarted = true;

            if (dlog) Debug.Log("start follow cor, source " + Source);
            dirTarget = null;
            float dTime = 0;

            while (dragMagnitude < GameBoard.MaxDragDistance * 0.5f && dTime < 0.1f)
            {
                yield return new WaitForEndOfFrame();
                dTime += Time.deltaTime;
            }

            dragDirectionPrior = GetPriorityDir(dragDirection);

            dirTarget = GetDirTarget(GetPriorityDirType(dragDirectionPrior));

            if (dlog) Debug.Log("follow dirTarget: " + dirTarget);

            while (!Target && Draggable && dirTarget && dragDirectionPrior.magnitude < GameBoard.MaxDragDistance)
            {
                if (showDrag) Draggable.transform.position = draggableStartPos + dragDirectionPrior;  // show drag
                if (dlog) Debug.Log("dragMagnitude: " + dragMagnitude + " / " + GameBoard.MaxDragDistance + " ; axe : " + GetPriorityDir(dragDirection) + " ; startPosW:" + pointerDownPos);
                
                if (dTime >= 0.4 && !Target && dirTarget)
                {
                    if (dlog) Debug.Log("manualy drag enter");
                    dirTarget.DragEnterEvent(tPEA);
                }
                yield return new WaitForEndOfFrame();
                dTime += Time.deltaTime;
            }
            followStarted = false;
            if (dlog) Debug.Log("end follow cor");
        }

        public void SetDraggable(GridCell source, Action<Action> resetDrag)
        {
            Source = source;
            if (source)
            {
                Draggable = source.DynamicObject.GetComponent<GridObject>();
                draggableStartPos = source.transform.position;
            }
            else
            {
                Draggable = null;
            }
            ResetDragEvent = resetDrag;
        }

        public void SetTarget(GridCell target)
        {
            Target = target;
        }

        public void ResetDrag(Action completeCallBack)
        {
            if(dlog)Debug.Log("Reset drag");
            ResetDragEvent?.Invoke(completeCallBack);
            Draggable = null;
            Source = null;
            Target = null;
            ResetDragEvent = null;
        }

        private Vector2 GetPriorityOneDir(Vector2 sourceDir)
        {

            if (Mathf.Abs(sourceDir.x) > Mathf.Abs(sourceDir.y))
            {
                float x = (sourceDir.x > 0) ? 1 : -1;
                return new Vector2(x, 0f);
            }
            else
            {
                float y = (sourceDir.y > 0) ? 1 : -1;
                return new Vector2(0f, y);
            }
        }

        private Vector2 GetPriorityDir(Vector2 sourceDir)
        {

            if (Mathf.Abs(sourceDir.x) > Mathf.Abs(sourceDir.y))
            {
                return new Vector2(sourceDir.x, 0f);
            }
            else
            {
                return new Vector2(0f, sourceDir.y);
            }
        }

        private DirectionType GetPriorityDirType(Vector2 sourceDir)
        {
            Vector3 priorDir = GetPriorityDir(sourceDir);
            if (priorDir.x > 0)
            {
                return DirectionType.Right;
            }
            else if (priorDir.x < 0)
            {
                return DirectionType.Left;
            }
            else if (priorDir.y < 0)
            {
                return DirectionType.Bottom;
            }
            else if (priorDir.y >0)
            {
                return DirectionType.Top;
            }
            return DirectionType.None;
        }

        private GridCell GetDirTarget(DirectionType dT)
        {
            if (Source)
                switch (dT)
                {
                    case DirectionType.Right:
                        return Source.RightDraggable();
                    case DirectionType.Left:
                        return Source.LeftDraggable();
                    case DirectionType.Top:
                        return Source.TopDraggable();
                    case DirectionType.Bottom:
                        return Source.BottomDraggable();
                }
            return null;
        }
    }

    public enum DirectionType { Right, Left, Top, Bottom, None }
}