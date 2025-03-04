using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

/*
  29.11.2023 - first
*/

namespace Mkey
{
    public class PolarArrayHelper : MonoBehaviour
    {
        public float dist = 2.5f;
        public List<Transform> transforms;
        [Range(0,360)]
        public float startAngle = 90f;
        public bool rotateObjects = false;
        [Range(0, 360)]
        public float arc = 360f;

        public void SetPostion()
        {
            int length = transforms.Count;
            if (length == 0) return;

            float dAngleDeg = arc / length; // angle per object

            // set position
            for (int i = 0; i < length; i++)
            {
                if (transforms[i])
                {
                    transforms[i].parent = null;
                    float angleDeg = startAngle - (float)i * dAngleDeg;
                    float angleRad = angleDeg * Mathf.Deg2Rad;
                    transforms[i].position = transform.position + new Vector3(dist * Mathf.Cos(angleRad), dist * Mathf.Sin(angleRad), 0);
                    if(rotateObjects)  transforms[i].localEulerAngles = new Vector3(0, 0, angleDeg);
                    else transforms[i].localEulerAngles = new Vector3(0, 0, 0);
                    transforms[i].parent = transform;
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PolarArrayHelper))]
    public class PolarArrayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PolarArrayHelper setText = (PolarArrayHelper)target;
            serializedObject.Update();

            DrawDefaultInspector();
            if (GUILayout.Button("Arrange"))
            {
                setText.SetPostion();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
#endif
}
