using com.romainimberti.ggj2021.ui.popups;
using com.romainimberti.ggj2021.utilities;
using nl.DTT.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace com.romainimberti.ggj2021.editor
{
    [CustomEditor(typeof(PopupManager))]
    class PopupManagerEditor : UnityEditor.Editor
    {
        #region varibles
        /// <summary>
        /// the manager component
        /// </summary>
        private PopupManager Manager;
        #endregion
        #region methodes
        #region unity
        /// <summary>
        /// gets the serialized object and saves it
        /// </summary>
        private void OnEnable()
        {
            Manager = (PopupManager)serializedObject.targetObject;
        }
        #endregion
        #region public
        /// <summary>
        /// creates the ui
        /// </summary>
        public override void OnInspectorGUI()
        {
            //check if there was anny change
            bool change = false;
            serializedObject.Update();
            //disables propertys we dont want to show
            DrawPropertiesExcluding(serializedObject, "Popups", "m_Script", "PopupsEditor");
            List<PopupManager.Data> popups = Manager.GetSerializedField<List<PopupManager.Data>>("PopupsEditor");
            
            //the add button
            if (GUILayout.Button(new GUIContent("add")))
            {
                popups.Add(new PopupManager.Data());
                change = true;
            }
            //goes over all elements and displays them
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                PopupManager.Data d = popups[i];
                Rect buttonPosition = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight/*, Styles.selectionRect*/);

                //creates the label
                //buttonPosition.width -= 15;
                EditorGUI.PrefixLabel(buttonPosition, new GUIContent(d.type != null ? d.type.Name: ""));

                //creates teh field selector
                //buttonPosition.x += buttonPosition.width;
                d.prefab = (GameObject)EditorGUI.ObjectField(buttonPosition, popups[i].prefab, typeof(GameObject));
                if (d.prefab != null)
                    if (d.prefab.GetComponent<PopupElement>() == null)
                    {
                        d.prefab = null;
                        d.type = null;
                    }
                    else
                    {
                        d.type = new SerializableSystemType(d.prefab.GetComponent<PopupElement>().GetType());
                        if (popups[i].type == null)
                            change = true;
                        else if (d.type != popups[i].type)
                            change = true;
                    }

                popups[i] = d;

                //buttonPosition.x += buttonPosition.width;
                buttonPosition.width = 30;
                //creates the delete button
                if (GUI.Button(buttonPosition, new GUIContent("X")))
                {
                    popups.RemoveAt(i);
                    change = true;
                }
            }
            
            //converts it back to a distinct list so we dont have duplicates
            popups = popups.Distinct().OrderByDescending(x => x.type == null ? "": x.type.Name ).ToList();

            Manager.SetSerializedField("PopupsEditor", popups);
            serializedObject.ApplyModifiedProperties();
            if (change)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        #endregion
        #endregion
    }
}
