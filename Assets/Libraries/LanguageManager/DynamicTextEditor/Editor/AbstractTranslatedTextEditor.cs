using nl.DTT.LocalizedLanguage.Managers;
using nl.DTT.LocalizedLanguage.Texts;
using nl.DTT.Utils.Enums;
using System;
using nl.DTT.Utils.Extensions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// COPYRIGHT: DTT (2018)
/// Made by: Frank van Hoof
/// </summary>
namespace nl.DTT.LocalizedLanguage.EditorScripts
{
    [CustomEditor(typeof(AbstractTranslatedText),true, isFallback = true)]
    public class AbstractTranslatedTextEditor : TextDisplayEditor
    {
        private const int SPACING_SIZE = 5;

        private bool debugFoldout;

        private GUIStyle boldFoldout;

        public override void OnInspectorGUI()
        {
            if (boldFoldout == null)
            {
                if (EditorStyles.foldout == null)
                    return; // Fix for NullRef on first frame when entering play-mode
                boldFoldout = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold
                };
            }

            // Grab target
            AbstractTranslatedText myTarget = (AbstractTranslatedText)target;
            // Space above first line
            GUILayout.Space(SPACING_SIZE);
            // Fields
            string newVal = EditorGUILayout.TextField("Translation Key", myTarget.FieldName);
            if (!string.Equals(newVal, myTarget.FieldName))
            {
                myTarget.SetSerializedField("fieldName", newVal);
                GUI.changed = true;
            }
            bool autoRegister = EditorGUILayout.Toggle("Auto-Register", myTarget.AutoRegister);
            if (autoRegister != myTarget.AutoRegister)
            {
                myTarget.SetSerializedField("autoRegister", autoRegister);
                GUI.changed = true;
            }
            bool useCustomLang = EditorGUILayout.ToggleLeft("Use Custom Language", myTarget.UseCustomLanguage);
            if (!useCustomLang.Equals(myTarget.UseCustomLanguage))
            {
                if (!LanguageManager.Exists) // Game is running. Use setter with Refresh()
                    myTarget.UseCustomLanguage = useCustomLang;
                else myTarget.SetSerializedField("useCustomLanguage", useCustomLang); // Game is NOT running. Use setter without Refresh()
                GUI.changed = true;
            }
            // Only show CustomLanguage is UseCustomLanguage is true
            if (myTarget.UseCustomLanguage)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                Language customLang = (Language)Enum.GetValues(typeof(Language)).GetValue(EditorGUILayout.Popup("Custom Language:", Array.IndexOf(Enum.GetNames(typeof(Language)), myTarget.CustomLanguage.ToString()), Enum.GetNames(typeof(Language)), EditorStyles.popup));
                if (!customLang.Equals(myTarget.CustomLanguage))
                {
                    if (LanguageManager.Exists)// Game is running. Use setter with Refresh()
                        myTarget.CustomLanguage = customLang;
                    else myTarget.SetSerializedField("customLanguage", customLang);// Game is NOT running. Use setter without Refresh()
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            AbstractTranslatedText.SpecialTextSettings textSetting = (AbstractTranslatedText.SpecialTextSettings)EditorGUILayout.EnumPopup("Special Text Setting", myTarget.TextSetting);
            if (textSetting != myTarget.TextSetting)
            {
                myTarget.SetSerializedField("specialTextSettings", textSetting);
                GUI.changed = true;
            }

            bool lineBreak = EditorGUILayout.Toggle("Ignore Line Breaks", myTarget.IgnoreLineBreak);
            if (lineBreak != myTarget.IgnoreLineBreak)
            {
                myTarget.SetSerializedField("ignoreLineBreaks", lineBreak);
                GUI.changed = true;
            }

            GUILayout.Space(SPACING_SIZE);
           
            bool changed = GUI.changed;
            EditorGUI.indentLevel++; 

            DrawTextDisplayInspector();
            debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug", boldFoldout);
            EditorGUI.indentLevel--;
            // Skip foldout for GUI.changed
            GUI.changed = changed;
            if (debugFoldout)
            {
                bool log = myTarget.GetSerializedField<bool>("logMethods");
                bool newLog = EditorGUILayout.Toggle("Log Methods", log);
                if (log != newLog)
                {
                    myTarget.SetSerializedField("logMethods", newLog);
                    GUI.changed = true;
                }
                if (newLog)
                {
                    LoggingLevel logging = myTarget.GetSerializedField<LoggingLevel>("logLevel");
                    LoggingLevel newLogging = (LoggingLevel)Enum.GetValues(typeof(LoggingLevel)).GetValue(EditorGUILayout.Popup("Logging-Level:", Array.IndexOf(Enum.GetNames(typeof(LoggingLevel)), logging.ToString()), Enum.GetNames(typeof(LoggingLevel)), EditorStyles.popup));
                    if (!logging.Equals(newLogging))
                    {
                        myTarget.SetSerializedField("logLevel", newLogging);
                        GUI.changed = true;
                    }
                }   
            }
            // GAME IS RUNNING
            if (LanguageManager.Exists)
            {
                GUILayout.Space(SPACING_SIZE);
                FontStyle currStyle = EditorStyles.label.fontStyle;
                int currSize = EditorStyles.label.fontSize;
                EditorStyles.label.fontStyle = FontStyle.Bold;
                EditorStyles.label.fontSize = 12;
                EditorGUILayout.LabelField("Runtime", GUILayout.Height(20));
                EditorStyles.label.fontStyle = currStyle;
                EditorStyles.label.fontSize = currSize;
                if (!myTarget.IsRegistered.Equals(EditorGUILayout.Toggle("Registered", myTarget.IsRegistered)))
                {
                    if (myTarget.IsRegistered)
                        LanguageManager.UnregisterTranslatedText(myTarget);
                    else
                        LanguageManager.RegisterTranslatedText(myTarget);
                }
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                    myTarget.Refresh();
                if (GUILayout.Button("Refresh All", EditorStyles.toolbarButton))
                    AbstractTranslatedText.RefreshAll();
            }
            if (GUI.changed && !Application.isPlaying)
            {
                EditorUtility.SetDirty((AbstractTranslatedText)target);
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }
    }
}
