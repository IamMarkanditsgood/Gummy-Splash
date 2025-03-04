using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.SceneManagement;

namespace Mkey
{
    [CustomEditor(typeof(GameObjectsSet))]
    public class GameObjectsSetEditor : Editor
    {
        private bool drawDefault;

        private bool drawBombs;
        private bool drawBoosters;

        private void OnEnable()
        {
        }

        private void OnDisable()
        {

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();

            //backgrounds
            ShowPropertiesBox(new string[] { "backGrounds" }, true);

            //cells
            ShowPropertiesBox(new string[] { "gridCellOdd", "gridCellEven" }, true);
            ShowPropertiesBox(new string[] { "disabledObject" }, false);

            //Blocked objects
            ShowPropertiesBox(new string[] { "blockedObjects", "dynamicBlockerObjects", "blockedBoxObjects" }, true);

            //Regular objects
            ShowPropertiesBox(new string[] { "matchObjects" }, true);

            //Protectors 
            ShowPropertiesBox(new string[] { "overlayObjects", "underlayObjects", "subUnderlayMCObjects" }, true );

            //Bombs
            ShowPropertiesBoxFoldOut("Bombs", new string[] {
                 "staticMatchBombObjectVertical", "staticMatchBombObjectHorizontal", "staticMatchBombObjectRadial",
                 "dynamicClickBombObjectVertical", "dynamicClickBombObjectHorizontal", "dynamicClickBombObjectRadial", "dynamicClickBombObjectRandom", "dynamicClickBombObjectColor"
            },
                ref drawBombs, true);

            //Boosters
            ShowPropertiesBox(new string[] { "boosterObjects" }, true);

            //Falling
            ShowPropertiesBox(new string[] { "fallingObjects" }, true);

            //Hidden
            ShowPropertiesBox(new string[] { "hiddenObjects" }, true);

            //Treasure
            ShowPropertiesBox(new string[] { "treasureObjects" }, true);

            ////Additional
            //ShowPropertiesBox(new string[] { "additionalObjects" }, true);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                if (!SceneManager.GetActiveScene().isDirty) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            drawDefault = EditorGUILayout.Foldout(drawDefault, "Draw Default");
            if (drawDefault) DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }

        #region showProperties
        private void ShowProperties(string[] properties, bool showHierarchy)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(properties[i]), showHierarchy);
            }
        }

        private void ShowPropertiesBox(string[] properties, bool showHierarchy)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            ShowProperties(properties, showHierarchy);
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        private void ShowPropertiesBoxFoldOut(string bName, string[] properties, ref bool fOut, bool showHierarchy)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                ShowProperties(properties, showHierarchy);
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        private void ShowReordListBoxFoldOut(string bName, ReorderableList rList, ref bool fOut)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                rList.DoLayoutList();
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }
        #endregion showProperties
    }


}
