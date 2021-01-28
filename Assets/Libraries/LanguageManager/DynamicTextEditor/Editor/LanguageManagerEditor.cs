using nl.DTT.LocalizedLanguage.Managers;
using nl.DTT.LocalizedLanguage.ParseAndLoad;
using nl.DTT.LocalizedLanguage.Texts;
using nl.DTT.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [CustomEditor(typeof(LanguageManager), true)]
    public class LanguageManagerEditor : UnityEditor.Editor
    {
        private const int INDENT_SIZE = 15;
        private const int SPACING_SIZE = 5;
        private bool csvSettingsFoldout;
        private bool debugFoldout;

        public override void OnInspectorGUI()
        {
            GUIStyle boldFoldout = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
            serializedObject.Update();
            // Grab target
            LanguageManager myTarget = (LanguageManager)target;
            // Space above first line
            GUILayout.Space(SPACING_SIZE);
            AddHeader("Auto-Loading");
            myTarget.SetSerializedField("loadOnAwake", EditorGUILayout.Toggle("Load On Awake", myTarget.LoadOnAwake));
            // Header
            AddHeader("Preferred Language");
            // Toggle for usage of Preferred Language
            myTarget.SetSerializedField("usePreferredLanguage", EditorGUILayout.ToggleLeft("Use a preferred Language", myTarget.UsePreferredLanguage));
            // Toogle for usage of saved language
            myTarget.SetSerializedField("useSavedLanguage", EditorGUILayout.ToggleLeft("Use saved language settings", myTarget.UseSavedLanguage));
            // Dropdown shown only if toggle = true
            if (myTarget.UsePreferredLanguage)
            {
                EditorGUI.indentLevel++;
                myTarget.SetSerializedField("preferredLanguage", (Language)Enum.GetValues(typeof(Language)).GetValue(EditorGUILayout.Popup("Language:", Array.IndexOf(Enum.GetNames(typeof(Language)), myTarget.PreferredLanguage.ToString()), Enum.GetNames(typeof(Language)), EditorStyles.popup)));
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * INDENT_SIZE);
                if (GUILayout.Button("Set as Current", EditorStyles.miniButtonRight))
                {
                    if (LanguageManager.Exists) // Game is Running
                        LanguageManager.SetLanguage(myTarget.PreferredLanguage);
                    else // Game is NOT running
                        myTarget.SetSerializedField("currentLanguage", myTarget.PreferredLanguage);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(SPACING_SIZE);
            // Header
            AddHeader("Translation-File  (CSV)");
            EditorGUI.indentLevel++;
            bool changed = GUI.changed;
            csvSettingsFoldout = EditorGUILayout.Foldout(csvSettingsFoldout, "CSV Settings");
            // Skip GUI.changed for Foldout
            GUI.changed = changed;
            if (csvSettingsFoldout)
            {
                EditorGUI.indentLevel++;
                Dictionary<string, string> newLines = new Dictionary<string, string>
                {
                    { "Environment_NewLine", Environment.NewLine },
                    { "\\n", "\n" },
                    { "\\r\\n", "\r\n" },
                    { ";", ";"  }
                };
                if (newLines.Values.Contains(myTarget.NewLineStr))
                    myTarget.SetSerializedField("newLineStr", newLines.Values.ToList()[EditorGUILayout.Popup("NewLine", newLines.Values.ToList().IndexOf(myTarget.NewLineStr), newLines.Keys.ToArray())]);
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("NewLine");
                    EditorGUILayout.LabelField(myTarget.NewLineStr);
                    EditorGUILayout.EndHorizontal();
                }
                char[] delimiter = {
                    ',',
                    '.',
                    ';',
                    ':',
                    '|',
                    '^',
                    '#',
                };
                List<string> delStrings = new List<string>();
                foreach (char c in delimiter)
                    delStrings.Add(c.ToString());
                if (delStrings.Contains(myTarget.Delimiter.ToString()))
                    myTarget.SetSerializedField("delimiter", delimiter.ToList()[EditorGUILayout.Popup("Delimiter", delimiter.ToList().IndexOf(myTarget.Delimiter), delStrings.ToArray())]);
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Delimiter");
                    EditorGUILayout.LabelField(myTarget.Delimiter.ToString());
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;

            // Toggle for usage of Remote (Downloaded & Cached) Language-File
            myTarget.SetSerializedField("useRemoteLanguageFile", EditorGUILayout.ToggleLeft("Use a remote Language-File", myTarget.UseRemoteLanguageFile));
            // TextField (for URL) shown only if toggle = true;
            if (myTarget.UseRemoteLanguageFile)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("URL for Language-File:");
                EditorGUILayout.LabelField("Staging:");
                myTarget.SetSerializedField("stagingLanguageURL", EditorGUILayout.TextField(myTarget.StagingLanguageUrl));
                EditorGUILayout.LabelField("Live:");
                myTarget.SetSerializedField("liveLanguageURL", EditorGUILayout.TextField(myTarget.LiveLanguageUrl));
                myTarget.SetSerializedField("appendApplicationVersion", EditorGUILayout.Toggle("Append Version Number", myTarget.AppendApplicationVersion));
                EditorGUILayout.EndVertical();
                myTarget.SetSerializedField("userName", EditorGUILayout.TextField("Custom Username: ", myTarget.UserName));
                if (myTarget.UserName != null && !myTarget.UserName.Equals(string.Empty))
                {
                    myTarget.SetSerializedField("password", EditorGUILayout.PasswordField("Custom Password: ", myTarget.Password));
                    myTarget.SetSerializedField("domain", EditorGUILayout.TextField("Custom Domain: ", myTarget.Domain));
                }
                else
                {
                    myTarget.SetSerializedField("password", null);
                    myTarget.SetSerializedField("domain", null);
                }
                myTarget.SetSerializedField("wifiDownloadOnly", EditorGUILayout.Toggle("Download on WIFI only", myTarget.WIFIDownloadOnly));
                myTarget.SetSerializedField("languageMap", (TextAsset)EditorGUILayout.ObjectField("FirstBoot-File:", myTarget.LanguageMap, typeof(TextAsset), false));
                EditorGUI.indentLevel--;
            }
            else // ObjectField (for TextAsset) shown if toggle = false
            {
                GUILayout.Space(SPACING_SIZE);
                myTarget.SetSerializedField("languageMap", (TextAsset)EditorGUILayout.ObjectField("Language-File:", myTarget.LanguageMap, typeof(TextAsset), false));
            }
            GUILayout.Space(SPACING_SIZE);
            // Header
            AddHeader("Language");
            EditorGUILayout.BeginHorizontal();
            // Current Language in Manager
            EditorGUILayout.LabelField("Current Language:");
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = -4; // Don't ask why
            EditorGUILayout.LabelField(myTarget.GetSerializedField<Language>("currentLanguage").ToString());
            EditorGUI.indentLevel = indent;
            EditorGUILayout.EndHorizontal();
            // FallbackLanguage
            myTarget.SetSerializedField("fallbackLanguage", (Language)Enum.GetValues(typeof(Language)).GetValue(EditorGUILayout.Popup("Fallback Language:", Array.IndexOf(Enum.GetNames(typeof(Language)), myTarget.FallbackLanguage.ToString()), Enum.GetNames(typeof(Language)), EditorStyles.popup)));
            GUILayout.Space(SPACING_SIZE);
            changed = GUI.changed;
            EditorGUI.indentLevel++;
            debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug", boldFoldout);
            EditorGUI.indentLevel--;
            // Skip foldout for GUI.changed
            GUI.changed = changed;
            if (debugFoldout)
            {
                bool LogMethods = myTarget.GetSerializedField<bool>("logMethods");
                LogMethods = EditorGUILayout.Toggle("Log Methods", LogMethods);
                myTarget.SetSerializedField("logMethods", LogMethods);
                if (LogMethods)
                    myTarget.SetSerializedField("logLevel", (LoggingLevel)Enum.GetValues(typeof(LoggingLevel)).GetValue(EditorGUILayout.Popup("Logging-Level:", Array.IndexOf(Enum.GetNames(typeof(LoggingLevel)), myTarget.GetSerializedField<LoggingLevel>("logLevel").ToString()), Enum.GetNames(typeof(LoggingLevel)), EditorStyles.popup)));
            }
            // GAME IS RUNNING
            if (LanguageManager.Exists)
            {
                GUILayout.Space(SPACING_SIZE);
                if (ResourceLoader.UserHasSavedLanguage)
                {
                    EditorGUILayout.BeginHorizontal();
                    // User-Saved Language
                    EditorGUILayout.LabelField("User-Saved Language:");
                    EditorGUILayout.LabelField(ResourceLoader.UserSavedLanguage.ToString());
                    EditorGUILayout.EndHorizontal();
                    // Clear-Button
                    if (GUILayout.Button("Clear Saved Language", EditorStyles.miniButtonRight))
                        ResourceLoader.ClearSavedLanguage();
                    GUILayout.Space(SPACING_SIZE);
                }
                // Header
                AddHeader("TranslatedTexts");
                // Button for updating Registered Texts
                if (GUILayout.Button("Update Registered TranslatedTexts", EditorStyles.toolbarButton))
                    LanguageManager.RefreshRegisteredTranslatedTexts();
                // Button for updating ALL Texts
                if (GUILayout.Button("Update All TranslatedTexts", EditorStyles.toolbarButton))
                    AbstractTranslatedText.RefreshAll();
            }
            if (GUI.changed && !Application.isPlaying)
            {
                EditorUtility.SetDirty((LanguageManager)target);
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// Creates a Header-Label
        /// </summary>
        /// <param name="text">Text for label</param>
        private void AddHeader(string text)
        {
            FontStyle currStyle = EditorStyles.label.fontStyle;
            int currSize = EditorStyles.label.fontSize;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorStyles.label.fontSize = 12;
            EditorGUILayout.LabelField(text, GUILayout.Height(20));
            EditorStyles.label.fontStyle = currStyle;
            EditorStyles.label.fontSize = currSize;
        }
    }
}
