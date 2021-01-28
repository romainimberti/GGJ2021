using nl.DTT.LocalizedLanguage.Texts;
using UnityEngine;
using UnityEditor;
using System.IO;
using nl.DTT.LocalizedLanguage.Managers;
using nl.DTT.Utils.Enums;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace nl.DTT.LocalizedLanguage.EditorScripts
{
    /// <summary>
    /// Base Editor for any LocalizedLanguage Translation display
    /// </summary>
    public class TextDisplayEditor : UnityEditor.Editor
    {
        /// <summary>
        /// All possible translation errors
        /// Using Flags to allow for multiple error handling 
        /// </summary>
        [Flags]
        protected enum TranslationErrors
        {
            NONE = 0,
            NO_TRANSLATION = 1 << 0,
            NOT_ENOUGH_PARAMS = NO_TRANSLATION << 1
        }

        /// <summary>
        /// The file that we loaded as .csv
        /// </summary>
        protected string file;
        /// <summary>
        /// The path at which we load the <see cref="file"/> in .csv format
        /// </summary>
        protected string path;
        /// <summary>
        /// The text of the <see cref="AbstractTranslatedText.FieldName"/> of our <see cref="textComponent"/>
        /// </summary>
        protected string displayText;
        /// <summary>
        /// The translated text component that we use for the display
        /// </summary>
        protected AbstractTranslatedText textComponent;
        /// <summary>
        /// The reused style of the display
        /// </summary>
        protected GUIStyle selectableLabelStyle;
        /// <summary>
        /// The reused helpbox error display style
        /// </summary>
        protected GUIStyle errorFieldStyle;
        /// <summary>
        /// Whether we are currently folded or not
        /// </summary>
        protected bool isFoldOut = true;
        /// <summary>
        /// Whether to use the foldout or not, inheritance can override this value to define for itself
        /// </summary>
        protected bool useFoldOut = true;
        /// <summary>
        /// Whether to show richtext or raw text
        /// </summary>
        protected int textType = 0;
        /// <summary>
        /// The default language if none are selected
        /// </summary>
        protected string textLanguage = "english";
        /// <summary>
        /// All languages found in translation documents
        /// </summary>
        protected string[] allLanguages;
        /// <summary>
        /// All languages that have no translations for current key
        /// </summary>
        protected TranslationErrors[] missingLanguages;
        /// <summary>
        /// Whether the the languages is selected or not
        /// </summary>
        protected bool[] languageStates;
        /// <summary>
        /// The currently selected languages
        /// </summary>
        protected int selectedLanguage = -1;
        /// <summary>
        /// Whether we have already initialised the languages
        /// </summary>
        private bool initialiseLanguage = false;
        /// <summary>
        /// The last selected language on previous inspector update
        /// </summary>
        private int lastSelectedLanguage = -1;
        /// <summary>
        /// The last selected text type on previous inspector update
        /// </summary>
        private int lastSelectedTextType = 0;
        /// <summary>
        /// The highest parameter count in selected key
        /// </summary>
        private int highestParamCount;
        /// <summary>
        /// Save the position of the language scrollbar position
        /// </summary>
        private Vector2 languageScrollBarPosition;
        /// <summary>
        /// Whether we want debug logs to fire for this editor
        /// </summary>
        private bool debugLogMode = false;

        /// <summary>
        /// In the OnEnable we load the translation document from the Resources
        /// In OnEnable means we only run it on the first inspector open, not on every update
        /// </summary>
        protected virtual void OnEnable()
        {
            textLanguage = ((Language)PlayerPrefs.GetInt(LanguageManager.KEY_LANGUAGE_GENERAL)).ToString();
            path = Path.Combine(Application.dataPath, "Resources");
            string[] files = Directory.GetFiles(path, "*.csv");
            if (files.Length > 0)
            {
                SendLog("FILE FOUND " + files[0]);
                file = files[0];
            }
        }

        /// <summary>
        /// Getter method to retrieve a <see cref="AbstractTranslatedText"/>
        /// Override if the inheritance class does not inherit <see cref="AbstractTranslatedText"/>
        /// </summary>
        protected virtual void GetAbstractTranslatedText()
        {
             textComponent = (AbstractTranslatedText)target;
        }

        /// <summary>
        /// Get the Rendered text to be displayed in the TextArea
        /// Override to add extra rules or logic towards the rendered display
        /// </summary>
        /// <returns>The text to be rendered in the box</returns>
        protected virtual string GetRenderText()
        {
            if (textType == 1)
                return displayText.Replace('<', '[').Replace('>',']');
            return displayText;
        }

        /// <summary>
        /// Default content of the foldout area
        /// </summary>
        private void FoldOutContent()
        {
            SendLog("TOTAL LANGUAGES " + allLanguages.Length);
            for (int i = 0; i < allLanguages.Length; i++)
            {
               
                SendLog(string.Format("LANGUAGE{0} | First {1}",
                    i, !string.IsNullOrWhiteSpace(allLanguages[i]) ? allLanguages[i].First().ToString().ToUpper() + allLanguages[i].Substring(1).ToLower() : "empty"));
                if (!string.IsNullOrWhiteSpace(allLanguages[i])) 
                    allLanguages[i] = allLanguages[i].First().ToString().ToUpper() + allLanguages[i].Substring(1).ToLower();
            }

            // Make a style for with and without translation
            GUIStyle translationStyle = new GUIStyle(EditorStyles.toolbarButton);
            GUIStyle noTranslationStyle = new GUIStyle(EditorStyles.toolbarButton);
            noTranslationStyle.active = translationStyle.normal;

            // Create a custom toolbar for the language buttons
            languageScrollBarPosition = EditorGUILayout.BeginScrollView(languageScrollBarPosition);
            EditorGUILayout.BeginHorizontal();
            Color baseColor = GUI.backgroundColor;
            bool hasError = false;

            for (int i = 0; i < allLanguages.Length - 1; i++)
            {
                hasError = missingLanguages[i].HasFlag(TranslationErrors.NOT_ENOUGH_PARAMS) || missingLanguages[i].HasFlag(TranslationErrors.NO_TRANSLATION);
                // Make the button red if we have no translation
                if(hasError)
                    GUI.backgroundColor = Color.red;

                // Make a toggle for each language
                if (GUILayout.Toggle(
                    languageStates[i], 
                    allLanguages[i], 
                   hasError ? noTranslationStyle : translationStyle,
                    GUILayout.Width(90)))
                {
                    selectedLanguage = i;
                    for (int l = 0; l < languageStates.Length; l++)
                        languageStates[l] = false;
                    languageStates[i] = true;
                }
                GUI.backgroundColor = baseColor;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            // Draw the textfield and textType toolbars
            EditorGUILayout.SelectableLabel(GetRenderText(), selectableLabelStyle, GUILayout.ExpandHeight(true), GUILayout.MaxHeight(80));
            EditorGUILayout.BeginHorizontal();
            textType = GUILayout.Toolbar(textType, new[] { "Richtext", "Raw" });
            EditorGUILayout.EndHorizontal();

            if (missingLanguages[selectedLanguage].HasFlag(TranslationErrors.NOT_ENOUGH_PARAMS))
                EditorGUILayout.LabelField($"<b>Key: <color=red>{textComponent.FieldName}</color> parameter count does not match, highest value: <color=#58D68D>{highestParamCount}</color></b>", errorFieldStyle);

            if (missingLanguages[selectedLanguage].HasFlag(TranslationErrors.NO_TRANSLATION))
                EditorGUILayout.LabelField($"<b>Key: <color=red>{textComponent.FieldName}</color> translation for this language is not correct</b>", errorFieldStyle);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Whether the text display has been updated since the last time
        /// </summary>
        /// <returns>True if the text has been updated</returns>
        protected virtual bool TextDisplayUpdated()
        {
            bool value = false;
            if (lastSelectedLanguage != selectedLanguage)
                value = true;
            else if (lastSelectedTextType != textType)
                value = true;

            lastSelectedTextType = textType;
            lastSelectedLanguage = selectedLanguage;
            return value;
        }

        /// <summary>
        /// Whether to ignore the error handling or not
        /// By default its error handling
        /// </summary>
        /// <returns>Whether to ignore error handling</returns>
        protected virtual bool IgnoreErrorCondition()
        {
            return false;
        }

        /// <summary>
        /// Override to add additional content inside the Foldable area
        /// </summary>
        protected virtual void DrawInFoldOut()
        {
        }

        /// <summary>
        /// The default inspector for the DrawText Area
        /// Can be overriden to extend with additional functionality
        /// </summary>
        protected virtual void DrawTextDisplayInspector()
        {
            errorFieldStyle = new GUIStyle(EditorStyles.helpBox);
            errorFieldStyle.richText = true;
            errorFieldStyle.wordWrap = true;

            highestParamCount = 0;
            // Whether we should stop focussing the text because it was updated
            if (TextDisplayUpdated())
                GUI.FocusControl("");

            // Define a style with text rules
            selectableLabelStyle = new GUIStyle(EditorStyles.textField);
            selectableLabelStyle.wordWrap = false;
            selectableLabelStyle.stretchWidth = true;
            selectableLabelStyle.stretchHeight = true;
            selectableLabelStyle.richText = true;

            // Get the textComponent reference
            GetAbstractTranslatedText();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // If we have no translation key set throw an error field
            if (string.IsNullOrWhiteSpace(textComponent.FieldName) && !IgnoreErrorCondition())
            {
                EditorGUILayout.LabelField("No <color=red><b>Translation Key</b></color> set", errorFieldStyle);
                return;
            }
            // If no file was found throw an error field
            if (string.IsNullOrWhiteSpace(file) && !IgnoreErrorCondition())
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("No Translation Doc (.csv) found at:");
                EditorGUILayout.SelectableLabel($"{path}", selectableLabelStyle, GUILayout.MaxHeight(16));
                EditorGUILayout.EndHorizontal();
                return;
            }

            displayText = string.Empty;

            // Loop through the loaded .csv file
            using (StreamReader reader = new StreamReader(file))
            {
                int languageCollum = 1;
                string languageLine = reader.ReadLine();
                string[] languageRow = languageLine.Split(',');
                int languageCount = languageRow.Where(x => !string.IsNullOrWhiteSpace(x)).Count();

                // Create all tracking data we need
                allLanguages = new string[languageCount];
                languageStates = new bool[languageCount];
                missingLanguages = new TranslationErrors[languageCount];
                int[] paramTracker = new int[languageCount];

                for (int i = 0; i < languageCount; i++)
                {
                    if (i > 0)
                        allLanguages[i - 1] = languageRow[i].ToLower();

                    if (languageRow[i].ToLower() == textLanguage.ToLower())
                        languageCollum = i;
                }

                if (!initialiseLanguage)
                {
                    selectedLanguage = Array.IndexOf(allLanguages, textLanguage.ToLower());
                    initialiseLanguage = true;
                }

                // Set the state of the language based on what we have active
                languageStates[selectedLanguage == -1 ? 0 : selectedLanguage] = true;

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    // Split on the ',' character
                    string[] row = line.Split(',');
 
                    // If the Key collum (collum 0) matches our textComponent.Fieldname
                    if (row[0] == textComponent.FieldName)
                    {
                        // Check if the translation exists, if it doenst we set it to missing
                        for (int i = 1; i < row.Length; i++)
                        {
                            if (string.IsNullOrEmpty(row[i]))
                                continue;

                            int paramCount = GetParamFieldCount(row[i]);
                            paramTracker[i - 1] = paramCount;
                            if (highestParamCount < paramCount)
                                highestParamCount = paramCount;

                            if (string.IsNullOrWhiteSpace(row[i]) || row[i].ToLower().EndsWith("needed"))
                                missingLanguages[i - 1] |= TranslationErrors.NO_TRANSLATION;
                        }

                        for (int i = 0; i < paramTracker.Length; i++)
                            if (paramTracker[i] < highestParamCount)
                                missingLanguages[i] |= TranslationErrors.NOT_ENOUGH_PARAMS;

                        // Save the Translation for the key
                        displayText = row[selectedLanguage == -1 ? languageCollum : selectedLanguage + 1];
                        break;
                    }
                }
            }

            // If no translation for the key was found throw an error message with information
            if (string.IsNullOrEmpty(displayText) && !IgnoreErrorCondition())
            {
                EditorGUILayout.LabelField($"<b>Key: <color=red>{textComponent.FieldName}</color> was not found in the Translation Doc</b>", errorFieldStyle);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Translation Doc:");
                EditorGUILayout.SelectableLabel($"{file}", selectableLabelStyle, GUILayout.MaxHeight(16));
                EditorGUILayout.EndHorizontal();
                return;
            }

            selectableLabelStyle.wordWrap = true;

            // Get the rendered text for our key
            string renderText = GetRenderText();
            if (useFoldOut)
            {
                isFoldOut = EditorGUILayout.Foldout(isFoldOut, "Translation Content Example");
                if (isFoldOut)
                {
                    FoldOutContent();
                    DrawInFoldOut();
                }
            }
            else
            {
                FoldOutContent();
                DrawInFoldOut();
            }
        }

        /// <summary>
        /// Method that gets the amount of parameters within the English translation 
        /// of the <see cref="AbstractTranslatedText.FieldName"/>
        /// </summary>
        /// <param name="text">The string that we check for parameter args</param>
        /// <returns>The amount of unique parameter args in string</returns>
        protected int GetParamFieldCount(string text)
        {
            if (text.Contains("{"))
            {
                // Use regex to filter out the string for parameter args, do not count duplicates and add +1 for array indexing
                return Regex.Matches(Regex.Replace(text,
                @"(\{{2}|\}{2})", ""), // removes escaped curly brackets
                @"\{(\d+)(?:\:?[^}]*)\}").OfType<Match>()
                .SelectMany(match => match.Groups.OfType<Group>().Skip(1))
                .Select(index => Int32.Parse(index.Value))
                .Max() + 1;
            }
            else
                 return 0;
        }

        /// <summary>
        /// Whether to fire debugging logs for the editor script
        /// </summary>
        /// <param name="text">The text to fire</param>
        private void SendLog(string text)
        {
            if(debugLogMode)
                Debug.LogError("[TextDisplayEditor] " + text);
        }
    }
}
