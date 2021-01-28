using nl.DTT.LocalizedLanguage.Managers;
using nl.DTT.LocalizedLanguage.ParseAndLoad;
using nl.DTT.Utils.Enums;
using nl.DTT.Utils.Extensions.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// COPYRIGHT: DTT (2018)
/// Made by: Frank van Hoof
/// </summary>
namespace nl.DTT.LocalizedLanguage.Texts
{
    public abstract class AbstractTranslatedText : MonoBehaviour
    {
        #region Variables
        #region enum 

        /// <summary>
        /// Any special settings for the text
        /// </summary>
        public enum SpecialTextSettings
        {
            None,
            OnlyFirstLetterCapital,
            AllLowerCase,
            AllUpperCase,
            SpaceToNewLine
        }

        #endregion
        #region Static

        /// <summary>
        /// List of all RegisterText objects. Used to refresh all at once.
        /// </summary>
        private static readonly List<AbstractTranslatedText> allRegisterTexts = new List<AbstractTranslatedText>();

        #endregion
        #region Public
        
        /// <summary>
        /// They Key-String used to get the translation for this RegisterText
        /// </summary>
        public string FieldName { get { return fieldName; } }
       
        /// <summary>
        /// Whether this object is currently registered
        /// </summary>
		public bool IsRegistered { get { return LanguageManager.Instance.IsRegistered(this); } }
       
        /// <summary>
        /// Whether to use a Custom Language for this Instance
        /// </summary>
        public bool UseCustomLanguage { get { return useCustomLanguage; }  set { useCustomLanguage = value; try { Refresh(); } catch (Exception) { } } }
        
        /// <summary>
        /// Custom Language for this Instance
        /// </summary>
        public Language CustomLanguage { get { return customLanguage; }  set { customLanguage = value; if (UseCustomLanguage) { Refresh(); } } }
        
        /// <summary>
        /// Whether this RegisterText should automatically register itself to the LanguageManager
        /// </summary>
        public bool AutoRegister { get { return autoRegister; } }

        /// <summary>
        /// The setting we have applied to this text
        /// </summary>
        public SpecialTextSettings TextSetting => specialTextSettings;

        /// <summary>
        /// Whether we should ignore linebreaks in the text
        /// </summary>
        public bool IgnoreLineBreak => ignoreLineBreaks;

        #endregion
        #region Editor

        /// <summary>
        /// The name of the field to register this Text-Object to (KEY)
        /// </summary>
        [SerializeField]
        [Tooltip("The name of the field to register this Text-Object to (KEY)")]
        protected string fieldName;

        /// <summary>
        /// Whether to use a Custom Language for this Instance
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to use a Custom Language for this Instance")]
        protected bool useCustomLanguage;

        /// <summary>
        /// Custom Language for this Instance
        /// </summary>
        [SerializeField]
        [Tooltip("Custom Language for this Instance")]
        protected Language customLanguage;

		/// <summary>
		/// Whether this RegisterText should automatically register itself to the LanguageManager
		/// </summary>
		[SerializeField]
        [Tooltip("Whether this RegisterText should automatically register itself to the LanguageManager")]
		protected bool autoRegister;

        #endregion   
        #region Logging

        /// <summary>
        /// Whether to print logs to the console (for debugging purposes)
        /// </summary>
        [SerializeField]
        protected bool logMethods;

        /// <summary>
        /// LoggingLevel of printed logs (for debugging purposes)
        /// </summary>
        [SerializeField]
        protected LoggingLevel logLevel;

        #endregion
        #region Protected

        /// <summary>
        /// Unformatted Text-String
        /// </summary>
        protected string text;
        /// <summary>
        /// Current Format-Values
        /// </summary>
        protected object[] currFormat;

        /// <summary>
        /// whether we remove/ignore line breaks.
        /// </summary>
        [SerializeField]
        protected bool ignoreLineBreaks;

        /// <summary>
        /// allows special capatilisation settings
        /// </summary>
        [SerializeField]
        protected SpecialTextSettings specialTextSettings = SpecialTextSettings.None;

        #endregion
        #endregion
        #region Methods
        #region Deprecated

        /// <summary>
        /// Updates currently set text with new values (using string.Format)
        /// </summary>
        /// <param name="formatValues">Values to use in string.Format (if any)</param>
        [Obsolete("Deprecated. Use FormatText instead")]
        public void RefreshText(params object[] formatValues)
        {
            // Re-Set using previously saved string
            try
            {
                string formattedText = string.Format(text, formatValues);
                SetTextToComponent(formattedText);
            }
            catch (ArgumentNullException)
            {
                if (formatValues == null || formatValues.Length == 0) // No format values
                    SetTextToComponent(text);
                else
                    LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Unformatted Text is null", this);
            }
            catch (FormatException e)
            {
                Debug.LogException(e);
                if (formatValues == null || formatValues.Length == 0) // No format values
                    SetTextToComponent(text);
                else
                    LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Text [{text}] failed to format", this);
            }
        }

        /// <summary>
        /// Swaps out TranslationKey (fieldname) and Refreshes Text-Object
        /// </summary>
        /// <param name="newKey">New TranslationKey to set</param>
        [Obsolete("Deprecated. Use Refresh(newkey) instead")]
        public void SwapTranslationKey(string newKey)
        {
            if (string.IsNullOrEmpty(newKey))
                Debug.LogException(new ArgumentException($"TranslatedText[{gameObject.name}]: New TranslationKey is Invalid", "newKey"));
            else
            {
                LoggingLevel.Info.Log($"TranslatedText[{name}]: Changing Key & Refreshing Text", logLevel, this, logMethods);
                Refresh(newKey);
            }
        }
        #endregion

        #region Unity
        /// <summary>
        /// Registers this TranslatedText (if enabled)
        /// </summary>
        protected virtual void Awake()
        {
            if (AutoRegister)
            {
                LoggingLevel.Info.Log($"TranslatedText[{name}]: AutoRegistering", logLevel, this, logMethods);
                if (LanguageManager.Exists && LanguageParser.IsLoaded)
                {
                    Register();
                    Refresh();
                }
                else
                {
                    LoggingLevel.Debug.Log($"TranslatedText[{name}]: LanguageManager not yet loaded. Starting DelayedRegister", logLevel, this, logMethods);
                    StartCoroutine(DelayedRegister());
                }
            }
            if (!allRegisterTexts.Contains(this))
            {
                allRegisterTexts.Add(this);
                LoggingLevel.Debug.Log($"TranslatedText[{name}]: Added to AllTexts", logLevel, this, logMethods);
            }
        }

        /// <summary>
        /// Removes this behaviour from static list of texts
        /// </summary>
        protected void OnDestroy()
        {
            LoggingLevel.Info.Log($"TranslatedText[{name}]: Destroying", logLevel, this, logMethods);
            if (LanguageManager.Exists)
                Unregister();
            allRegisterTexts.Remove(this);
            LoggingLevel.Debug.Log($"TranslatedText[{name}]: Unregistered and removed from AllTexts;", logLevel, this, logMethods);
        }

        #endregion
        #region Static

        /// <summary>
        /// Refreshes all Text objects. Useful after changing languages
        /// </summary>
        public static void RefreshAll()
        {
            foreach (AbstractTranslatedText text in allRegisterTexts)
                text.Refresh();
        }

        #endregion
        #region Public
        #region Text Set/Get

        /// <summary>
        /// Text as currently displayed
        /// </summary>
        /// <returns>Text as currently displayed by TranslatedText (includes Formatting)</returns>
        public abstract string GetText();

        /// <summary>
        /// Sets text to UI-Component
        /// </summary>
        /// <param name="text">Formatted String</param>
        protected abstract void SetTextToComponent(string text);

        protected string ApplyTextRules(string textToSet)
        {
            switch (specialTextSettings)
            {
                case SpecialTextSettings.None:
                    break;
                case SpecialTextSettings.OnlyFirstLetterCapital:
                    //remove whitespace.
                    string trimmed = textToSet.TrimStart();
                    int whiteSpaceToAdd = textToSet.Length - trimmed.Length;
                    string allLower = trimmed.ToLowerInvariant();
                    textToSet = $"{new string(' ', whiteSpaceToAdd)}{textToSet[0].ToString().ToUpperInvariant()}{allLower.Substring(1)}";
                    break;
                case SpecialTextSettings.AllUpperCase:
                    textToSet = textToSet.ToUpperInvariant();
                    break;
                case SpecialTextSettings.AllLowerCase:
                    textToSet = textToSet.ToLowerInvariant();
                    break;
                case SpecialTextSettings.SpaceToNewLine:
                    textToSet = textToSet.Replace(' ', '\n');
                    break;
            }
            if (ignoreLineBreaks)
                textToSet = textToSet.Replace('\n', ' ');
            return textToSet;
        }

        /// <summary>
        /// Unformatted Text currently set to TranslatedText
        /// </summary>
        /// <returns>Text as currently set to TranslatedText (does NOT include Formatting)</returns>
        public string GetUnformattedText()
            => text;

        /// <summary>
        /// Text setter for a TranslatedText
        /// </summary>
        /// <param name="textToSet">Text to set to TranslatedText</param>
        /// <param name="formatValues">Values to use in string.Format (if any)</param>
        public void SetText(string textToSet, params object[] formatValues)
        {
            text = textToSet.Replace("\\n", "\n");
            if (formatValues != null && formatValues.Length > 0)
                FormatText(formatValues);
            else
                FormatText(currFormat);
        }

        /// <summary>
        /// Updates currently set text with new values (using string.Format)
        /// </summary>
        /// <param name="formatValues">Values to use in string.Format (if any)</param>
        public void FormatText(params object[] formatValues)
        {
            // Re-Set using previously saved string
            try
            {
                int formatParams = 0;
                if (text.Contains("{") && formatValues != null)
                {
                    formatParams = Regex.Matches(Regex.Replace(text,
                            @"(\{{2}|\}{2})", ""), // removes escaped curly brackets
                            @"\{(\d+)(?:\:?[^}]*)\}").OfType<Match>()
                            .SelectMany(match => match.Groups.OfType<Group>().Skip(1))
                            .Select(index => Int32.Parse(index.Value))
                            .Max() + 1;

                    object[] newArray = new object[formatParams];
                    for (int i = 0; i < newArray.Length; i++)
                    {
                        if (formatValues != null && formatValues.Length > i)
                            newArray[i] = formatValues[i];
                        else if (currFormat != null && currFormat.Length > i)
                            newArray[i] = currFormat[i];
                    }

                    // Replace all empty format args with the placeholder of {0} to show developer missing values
                    for (int i = 0; i < formatParams; i++)
                        if (text.Contains($"{{{i}}}"))
                            if (newArray[i] == null || string.IsNullOrWhiteSpace(newArray[i].ToString()))
                                newArray[i] = string.Empty;//string.Format("{{{0}}}", i);

                    formatValues = newArray; // Set new Format-Values as current
                }
                string formattedText = string.Format(text, formatValues);
                currFormat = formatValues;
                SetTextToComponent(formattedText);
            }
            catch (ArgumentNullException)
            {
                if (formatValues == null || formatValues.Length == 0) // No format values
                    SetTextToComponent(text);
                else
                    LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Unformatted Text is null", this);
            }
            catch (FormatException e)
            {
                Debug.LogError("FORMAT EXCEPTION ERROR");
                if (formatValues == null || formatValues.Length == 0) // No format values
                {
                    currFormat = null;
                    SetTextToComponent(text);
                }
                else
                    LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Text [{text}] failed to format", this);
            }
        }

        /// <summary>
        /// Updates currently set text with new values (using string.Format)
        /// </summary>
        /// <param name="formatValues">Value to set in specified index of format</param>
        /// <param name="paramIndex">The index at which we want to set this string</param>
        public void FormatText(object formatValues, int paramIndex)
        {
            try
            {
                int formatParams = 0;
                if (text != null && text.Contains("{"))
                {
                    // Get the amount of parameters within the string
                    formatParams = Regex.Matches(Regex.Replace(text,
                        @"(\{{2}|\}{2})", ""), // removes escaped curly brackets
                        @"\{(\d+)(?:\:?[^}]*)\}").OfType<Match>()
                        .SelectMany(match => match.Groups.OfType<Group>().Skip(1))
                        .Select(index => Int32.Parse(index.Value))
                        .Max() + 1;
                }

                // Create a new curr format array if we have none
                if (currFormat == null)
                    currFormat = new object[formatParams];
                // Update existing curr format array to fit the extra parameter
                else if (paramIndex > currFormat.Length)
                {
                    object[] newFormat = new object[formatParams];
                    currFormat.CopyTo(newFormat, paramIndex);
                    currFormat = newFormat;
                }

                // Replace all empty format args with the placeholder of {0} to show developer missing values
                for (int i = 0; i < formatParams; i++)
                    if (text.Contains($"{{{i}}}"))
                        if (currFormat[i] == null || string.IsNullOrWhiteSpace(currFormat[i].ToString()))
                            currFormat[i] = $"{{{i}}}";

                // Update the curr format reference with the latest version
                currFormat[paramIndex] = formatValues;
                string formattedText = string.Format(text, currFormat);
                SetTextToComponent(formattedText);
            }
            catch (ArgumentNullException)
            {
                if (formatValues == null || currFormat.Length == 0) // No format values
                    SetTextToComponent(text);
                else
                    LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Unformatted Text is null", this);
            }
            catch (IndexOutOfRangeException)
            {
                SetTextToComponent(text);
                LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Key [{FieldName}] does not have enough format args to fit given parameters", this);
            }
            catch (InvalidOperationException)
            {
                SetTextToComponent(text);
                LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Key [{FieldName}] does not contain any format args", this);
            }
            catch (FormatException e)
            {
                Debug.LogError("FORMAT EXCEPTION ERROR");
                if (formatValues == null || currFormat.Length == 0) // No format values
                {
                    currFormat = null;
                    SetTextToComponent(text);
                }
                else
                    LoggingLevel.Error.LogAlways($"Setting text to {name} failed. Text [{text}] failed to format", this);
            }
        }

        /// <summary>
        /// Refreshes text in this object
        /// </summary>
        public void Refresh(string newkey = "", params object[] formatValues)
        {
            if (!LanguageManager.Exists || !LanguageParser.IsLoaded)
            {
                LoggingLevel.Warning.Log($"LanguageManager has not been Initialized. Cancelling Refresh for [{gameObject.name}]", logLevel, this, logMethods);
                return;
            }
            if (this == null)
            {
                Debug.LogError("This gameObject has been destroyed, return out of Refresh() since we are no longer needed");
                return;
            }
            if (!string.IsNullOrEmpty(newkey) && !newkey.Equals(fieldName))
            {
                bool wasRegistered = LanguageManager.UnregisterTranslatedText(this);
                if (wasRegistered)
                    LoggingLevel.Debug.Log($"TranslatedText[{name}]: Text was Registered. Unregistering", logLevel, this, logMethods);
                fieldName = newkey;
                LoggingLevel.Info.Log($"TranslatedText[{name}]: Changing Key", logLevel, this, logMethods);
                if (wasRegistered)
                {
                    LanguageManager.RegisterTranslatedText(this, false);
                    LoggingLevel.Debug.Log($"TranslatedText[{name}]: Re-Registering", logLevel, this, logMethods);
                }
            }
            if (!string.IsNullOrEmpty(fieldName))
            {
                LoggingLevel.Info.Log($"TranslatedText[{name}]: Refreshing Text", logLevel, this, logMethods);
                if (formatValues == null || formatValues.Length.Equals(0))
                {
                    LoggingLevel.Info.Log($"TranslatedText[{name}]: Refresh: Using Current Format-Values", logLevel, this, logMethods);
                    formatValues = currFormat; // Use current format-Values
                }
                if (UseCustomLanguage)
                {
                    LoggingLevel.Debug.Log($"TranslatedText[{name}]: Refresh: Using Custom Language [{customLanguage}]", logLevel, this, logMethods);
                    SetText(LanguageManager.GetTranslation(fieldName, customLanguage), formatValues);
                }
                else
                    SetText(LanguageManager.GetTranslation(fieldName), formatValues);
            }
        }
        #endregion

        /// <summary>
        /// Registers this Text-Object to the LanguageManager
        /// </summary>
        public void Register()
        {
            if (string.IsNullOrEmpty(FieldName))
                Debug.LogException(new ArgumentException($"LanguageManager[{gameObject.name}]: No Key has been set for RegisterText (Is AutoRegister on?)", "FieldName"), this);
            else
            {
                LoggingLevel.Info.Log($"TranslatedText[{name}]: Registering Text", logLevel, this, logMethods);
                LanguageManager.RegisterTranslatedText(this, false);
            }
        }

        /// <summary>
        /// Unregisters this Text-Object from the LanguageManager
        /// </summary>
        /// <returns>True if successfull</returns>
        public bool Unregister()
            => LanguageManager.UnregisterTranslatedText(this);

        /// <summary>
        /// Clears TranslationKey for this object and Unregisters this RegisterText
        /// </summary>
        /// <param name="keepCurrentText">Whether to keep the current text in the object</param>
        public void ClearTranslationKey(bool keepCurrentText = true)
        {
            LanguageManager.UnregisterTranslatedText(this);
            fieldName = null;
            LoggingLevel.Info.Log($"TranslatedText[{name}]: Cleared Key", logLevel, this, logMethods);
            if (!keepCurrentText)
            {
                SetTextToComponent(string.Empty);
                text = null;
                currFormat = null;
                LoggingLevel.Info.Log($"TranslatedText[{name}]: Cleared Values", logLevel, this, logMethods);
            }
        }
        
        /// <summary>
        /// Sets a custom Language for this RegisterText.
        /// Set null to use default (LanguageManager-)Language
        /// </summary>
        /// <param name="customLanguage">Language to set for this RegisterText (null for default)</param>
        public void SetCustomLanguage(Language? customLanguage = null)
        {
            UseCustomLanguage = customLanguage.HasValue;
            if (useCustomLanguage)
            {
                LoggingLevel.Info.Log($"TranslatedText[{name}]: Setting Custom Language [{customLanguage}]", logLevel, this, logMethods);
                CustomLanguage = (Language)customLanguage;
            }
            else
            {
                LoggingLevel.Info.Log($"TranslatedText[{name}]: Clearing Custom Language", logLevel, this, logMethods);
                CustomLanguage = Language.Unknown;
            }
        }

        #endregion
        #region Private

        /// <summary>
        /// Delayed Registration in case the LanguageManager isn't loaded yet
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayedRegister()
        {
            LoggingLevel.Debug.Log($"TranslatedText[{name}]: Delayed Register", logLevel, this, logMethods);
            yield return new WaitUntil(() => LanguageManager.Exists && LanguageParser.IsLoaded);
            LoggingLevel.Debug.Log($"TranslatedText[{name}]: LanguageManager Loaded. Starting Register", logLevel, this, logMethods);
            Register();
        }

        #endregion
        #endregion
    }
}