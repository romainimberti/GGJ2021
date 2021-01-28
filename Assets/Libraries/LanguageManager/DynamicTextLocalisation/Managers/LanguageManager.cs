using nl.DTT.Build;
using nl.DTT.LocalizedLanguage.ParseAndLoad;
using nl.DTT.LocalizedLanguage.Texts;
using nl.DTT.Utils.Behaviours;
using nl.DTT.Utils.Enums;
using nl.DTT.Utils.Extensions.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// COPYRIGHT: DTT (2018)
/// Made by: Frank van Hoof
/// </summary>
namespace nl.DTT.LocalizedLanguage.Managers
{
    public class LanguageManager : SingletonBehaviour<LanguageManager>
    {
        #region Variables
        #region Public
        #region Static
        /// <summary>
        /// The language that is currently set
        /// </summary>
        public static Language CurrentLanguage
        {
            get
            {
                if (!CheckExists())
                    return Language.Unknown;
                if (instance.currentLanguage.Equals(Language.Unknown))
                {
                    LoggingLevel.Debug.Log("LanguageManager: Current Language not set. Setting from SystemLanguage", instance.GetLoggingLevel(), Instance);
                    instance.currentLanguage = Application.systemLanguage.GetLanguage();
                }
                return instance.currentLanguage;
            }
        }

        /// <summary>
        /// Flag set active when a langauge is no longer supported
        /// </summary>
        private static bool flagLanguageNoLongerSupported;

        /// <summary>
        /// On getting this flag, it will be set to false, to reduce
        /// multiple calls by the api/multiple rewards.
        /// </summary>
        public static bool FlagLanguageNoLongerSupported
        {
            get
            {
                if (flagLanguageNoLongerSupported)
                {
                    flagLanguageNoLongerSupported = false;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// True if the current set language is non latin
        /// </summary>
        public static bool nonLatinLanguage { get; set; }

        /// <summary>
        /// The language ISO codes that are supported for General language setting
        /// </summary>
        public static readonly List<string> GENERAL_SUPPORTED_LANGUAGES = new List<string>(
            new string[] {
            ISOCode639.EN.ToString(),
            ISOCode639.FR.ToString(),
            ISOCode639.ES.ToString(),
            ISOCode639.DE.ToString(),
            ISOCode639.NL.ToString(),
            ISOCode639.IT.ToString(),
            ISOCode639.PT.ToString()
        });

        /// <summary>
        /// Lookup dictionary for ISO language codes as strings.
        /// </summary>
        public static Dictionary<Language, string> languageToIsoCode = new Dictionary<Language, string>
        {
            { Language.English, ISOCode639.EN.ToString() },
            { Language.French, ISOCode639.FR.ToString() },
            { Language.Spanish, ISOCode639.ES.ToString() },
            { Language.German, ISOCode639.DE.ToString() },
            { Language.Dutch, ISOCode639.NL.ToString() },
            { Language.Italian, ISOCode639.IT.ToString() },
            { Language.Portuguese, ISOCode639.PT.ToString() },
        };
        #endregion

        #region Get
        /// <summary>
        /// Preferred Language to use for Translations
        /// </summary>
        public Language PreferredLanguage
        {
            get { return preferredLanguage; }
        }
        /// <summary>
        /// Language to fallback to if current language does not have a translation for a given key
        /// </summary>
        public Language FallbackLanguage
        {
            get { return fallbackLanguage; }
        }
        /// <summary>
        /// Whether to load the translations on Awake
        /// </summary>
        public bool LoadOnAwake
        {
            get { return loadOnAwake; }
        }
        /// <summary>
        /// Whether to use Preferred Language (or DeviceLanguage)
        /// </summary>
        public bool UsePreferredLanguage
        {
            get { return usePreferredLanguage; }
        }
        /// <summary>
        /// Whether to use Preferred Language (or DeviceLanguage)
        /// </summary>
        public bool UseSavedLanguage
        {
            get { return useSavedLanguage; }
        }
        /// <summary>
        /// Whether to attempt to use a remote file
        /// </summary>
        public bool UseRemoteLanguageFile
        {
            get { return useRemoteLanguageFile; }
        }

        /// <summary>
        /// whether to append the application version to the remote language file.
        /// </summary>
        public bool AppendApplicationVersion
        {
            get { return appendApplicationVersion; }
        }
        /// <summary>
        /// Whether to only download remote file if on WIFI
        /// </summary>
        public bool WIFIDownloadOnly
        {
            get { return wifiDownloadOnly; }
        }

        public string StagingLanguageUrl
        {
            get
            {
                return stagingLanguageURL;
            }
        }

        public string LiveLanguageUrl
        {
            get
            {
                return liveLanguageURL;
            }
        }

        /// <summary>
        /// URL used to download TranslationFile
        /// </summary>
        public string languageURL
        {
            get { return EPBBuildSettings.CurrentAPI == API.live ? LiveLanguageUrl : StagingLanguageUrl ; }
        }
        /// <summary>
        /// TextAsset used for loading translations
        /// </summary>
        public TextAsset LanguageMap
        {
            get { return languageMap; }
        }
        /// <summary>
        /// String used for NEWLINE
        /// </summary>
        public string NewLineStr
        {
            get { return newLineStr; }
        }
        /// <summary>
        /// Char used as delimiter
        /// </summary>
        public char Delimiter
        {
            get { return delimiter; }
        }
        /// <summary>
        /// Username used for Downloads
        /// </summary>
        public string UserName
        {
            get { return userName; }
        }
        /// <summary>
        /// Password used for Downloads
        /// </summary>
        public string Password
        {
            get { return password; }
        }
        /// <summary>
        /// Domain-Name used for Downloads
        /// </summary>
        public string Domain
        {
            get { return domain; }
        }
        #endregion
        #endregion

        #region Editor
        /// <summary>
        /// Preferred Language to use for Translations
        /// </summary>
        [SerializeField]
        [Tooltip("Preferred Language to use for Translations")]
        private Language preferredLanguage = Language.Unknown;
        /// <summary>
        /// Language currently being used
        /// </summary>
        [SerializeField]
        [Tooltip("Language currently being used")]
        private Language currentLanguage = Language.Unknown;
        /// <summary>
        /// Language to fallback to if current language does not have a translation for a given key
        /// </summary>
        [SerializeField]
        [Tooltip("Language to fallback to if current language does not have a translation for a given key")]
        private Language fallbackLanguage = Language.English;
        /// <summary>
        /// Whether to load the translations on Awake
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to load the translations on Awake")]
        private bool loadOnAwake = true;
        /// <summary>
        /// Whether to use Preferred Language (or DeviceLanguage)
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to use Preferred Language (or DeviceLanguage)")]
        private bool usePreferredLanguage;
        /// <summary>
        /// Whether to use the currently saved language
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to use the currently saved language")]
        private bool useSavedLanguage;
        /// <summary>
        /// Whether to attempt to use a remote file
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to attempt to use a remote file")]
        private bool useRemoteLanguageFile;

        /// <summary>
        /// whether to append the application version to the url.
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to append the application version to the remote file")]
        private bool appendApplicationVersion;
        /// <summary>
        /// Whether to only download remote file if on WIFI
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to only download remote file if on WIFI")]
        private bool wifiDownloadOnly;
        /// <summary>
        /// URL used to download TranslationFile
        /// </summary>
        [SerializeField]
        [Tooltip("URL used to download TranslationFile")]
        private string liveLanguageURL;
        /// <summary>
        /// URL used to download TranslationFile
        /// </summary>
        [SerializeField]
        [Tooltip("URL used to download TranslationFile")]
        private string stagingLanguageURL;
        /// <summary>
        /// TextAsset used for loading translations
        /// </summary>
        [SerializeField]
        [Tooltip("TextAsset used for loading translations")]
        private TextAsset languageMap;
        /// <summary>
        /// String used for NEWLINE
        /// </summary>
        [SerializeField]
        [Tooltip("String used for NEWLINE")]
        private string newLineStr = Environment.NewLine;
        /// <summary>
        /// Char used as delimiter
        /// </summary>
        [SerializeField]
        [Tooltip("Char used as delimiter")]
        private char delimiter = ';';
        /// <summary>
        /// Username used for Downloads
        /// </summary>
        [SerializeField]
        [Tooltip("Username used for Downloads")]
        private string userName;
        /// <summary>
        /// Password used for Downloads
        /// </summary>
        [SerializeField]
        [Tooltip("Password used for Downloads")]
        private string password;
        /// <summary>
        /// Domain-Name used for Downloads
        /// </summary>
        [SerializeField]
        [Tooltip("Domain-Name used for Downloads")]
        private string domain;
        #endregion

        #region Logging
        /// <summary>
        /// Whether to print logs to the console (for debugging purposes)
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to print logs to the console (for debugging purposes)")]
        private bool logMethods = false;
        /// <summary>
        /// LoggingLevel of printed logs (for debugging purposes)
        /// </summary>
        [SerializeField]
        [Tooltip("LoggingLevel of printed logs (for debugging purposes)")]
        private LoggingLevel logLevel = LoggingLevel.Warning;
        #endregion

        #region Private
        /// <summary>
        /// All currently registered RegisterTexts arranged by key-string
        /// </summary>
        private readonly Dictionary<string, List<AbstractTranslatedText>> registerTexts = new Dictionary<string, List<AbstractTranslatedText>>();
        /// <summary>
        /// Used to check if loading from a remote language doc has failed once
        /// </summary>
        private bool failedOnce;

        /// <summary>
        /// The last set language of General
        /// Value is used to reset to the users set General language on restart
        /// </summary>
        public const string KEY_LANGUAGE_GENERAL = "languageGeneral";


        /// <summary>
        /// Last app version we had a translation document for.
        /// </summary>
        private const string KEY_CACHED_VERSION_NUMBER = "languageCachedVersionNumber";


        private const string KEY_LAST_VERSION_NOT_SUPPORTED_LANGUAGE = "lastVersionNotSupportedLanguage";
        #endregion
        #endregion

        #region Methods
        #region Unity
        /// <summary>
        /// Init
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (appendApplicationVersion)
            {
                stagingLanguageURL += Application.version + ".csv";
                liveLanguageURL += Application.version + ".csv";
            }



            if (!PlayerPrefs.GetString(KEY_CACHED_VERSION_NUMBER, "").Equals(Application.version))
            {
                ClearAllCache();
                PlayerPrefs.SetString(KEY_CACHED_VERSION_NUMBER, Application.version);
            }

            if (PlayerPrefs.HasKey(KEY_LANGUAGE_GENERAL))
            {
                Language lastLanguage = (Language)PlayerPrefs.GetInt(KEY_LANGUAGE_GENERAL);
                if (lastLanguage != Language.Unknown && !IsSupportedGeneral(lastLanguage))
                {
                    if(PlayerPrefs.GetString(KEY_LAST_VERSION_NOT_SUPPORTED_LANGUAGE,"0") != Application.version)
                    {
                        PlayerPrefs.SetString(KEY_LAST_VERSION_NOT_SUPPORTED_LANGUAGE, Application.version);
                        flagLanguageNoLongerSupported = true;
                    }
                }
            }





                LoggingLevel log = GetLoggingLevel();
            LoggingLevel.Info.Log("LanguageManager: Initializing", log, this);
            LoggingLevel.Info.Log(string.Format("LanguageManager: [INIT] LoadOnAwake set to {0}", loadOnAwake), log, this);
            if (loadOnAwake)
                LoadTranslations(false);
            //Set which language we wish to use.
            if (usePreferredLanguage)
            {
                LoggingLevel.Info.Log(string.Format("LanguageManager: [INIT] Using Preferred Language [{0}]", preferredLanguage), log, this);
                SetLanguage(preferredLanguage);
            }
            else if (useSavedLanguage && ResourceLoader.UserHasSavedLanguage)
            {
                LoggingLevel.Info.Log(string.Format("LanguageManager: [INIT] Using User-Saved Language [{0}]", ResourceLoader.UserSavedLanguage), log, this);
                SetLanguage(ResourceLoader.UserSavedLanguage);
            }
            else
            {
                LoggingLevel.Info.Log(string.Format("LanguageManager: [INIT] Using SystemLanguage [{0}]", Application.systemLanguage), log, this);

                if (PlayerPrefs.HasKey(KEY_LANGUAGE_GENERAL))
                    SetLanguage((Language)PlayerPrefs.GetInt(KEY_LANGUAGE_GENERAL));
                else
                    SetLanguage(Application.systemLanguage);
            }
        }

        /// <summary>
        /// On scene load we refresh the current registered texts and remove the texts that have been cleaned up
        /// in the scene transition We do this by null checking both the script reference and the gameObject reference
        /// </summary>
        /// <param name="scene">The scene loaded</param>
        /// <param name="mode">The loading mode used</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (string key in registerTexts.Keys.ToList())
                for (int i = instance.registerTexts[key].Count - 1; i >= 0; i--)
                    if (!registerTexts[key][i].gameObject || registerTexts[key][i] == null)
                        registerTexts[key].RemoveAt(i);
        }
        #endregion

        #region Static
        #region GetFields
        /// <summary>
        /// Whether the LanguageManager (and -Map) is Loaded
        /// </summary>
        /// <returns></returns>
        public static bool IsLoaded()
        {
            return (Exists && GetLanguageMap() != null && GetLanguageMap().Count != 0);
        }
        /// <summary>
        /// Gets the currently set Preferred Language
        /// </summary>
        /// <returns>The currently set Preferred Language</returns>
        public static Language GetPreferredLanguage()
        {
            if (!CheckExists() || !CheckLoaded())
                return Language.Unknown;
            return instance.PreferredLanguage;
        }
        /// <summary>
        /// Gets the currently set Fallback Language
        /// </summary>
        /// <returns>The currently set Fallback Language</returns>
        public static Language GetFallbackLanguage()
        {
            if (!CheckExists() || !CheckLoaded())
                return Language.Unknown;
            return instance.fallbackLanguage;
        }
        /// <summary>
        /// Whether to use the currently set Preferred Language
        /// </summary>
        /// <returns>Whether the currently set Preferred Language is used</returns>
        public static bool GetUsePreferredLanguage()
        {
            return CheckExists() && instance.usePreferredLanguage;
        }
        /// <summary>
        /// Whether to use a remote LanguageMap file
        /// </summary>
        /// <returns>Whether the remote file is used</returns>
        public static bool GetUseRemoteLanguageMapFile()
        {
            return CheckExists() && instance.useRemoteLanguageFile;
        }
        /// <summary>
        /// Whether to only download when on WIFI
        /// </summary>
        /// <returns>Whether to only download when on WIFI</returns>
        public static bool GetUseWIFIDownloadOnly()
        {
            return CheckExists() && instance.wifiDownloadOnly;
        }
        /// <summary>
        /// Gets the currently set URL for the remote LanguageMap file
        /// </summary>
        /// <returns>The currently set URL for the remote LanguageMap file</returns>
        public static string GetLanguageMapURL()
        {
            return CheckExists() ? instance.languageURL : string.Empty;
        }
        /// <summary>
        /// Gets the string used as NewLine when parsing
        /// </summary>
        /// <returns>The string used as NewLine when parsing</returns>
        public static string GetNewLineString()
        {
            return CheckExists() ? instance.newLineStr : string.Empty;
        }
        /// <summary>
        /// Gets the char used as Delimiter when parsing
        /// </summary>
        /// <returns>The string used as Delimiter when parsing</returns>
        public static char GetDelimiter()
        {
            return CheckExists() ? instance.delimiter : '\0';
        }
        /// <summary>
        /// Gets a copy of the currently loaded LanguageMap
        /// </summary>
        /// <returns>READONLY Copy of the currently loaded LanguageMap</returns>
        public static Dictionary<string, Dictionary<Language, string>> GetLanguageMap()
        {
            return LanguageParser.language_map == null ? null : new Dictionary<string, Dictionary<Language, string>>(LanguageParser.language_map);
        }
        #endregion

        #region SetFields
        /// <summary>
        /// Sets a new Preferred Language
        /// </summary>
        /// <param name="preferred">Language to set</param>
        public static void SetPreferredLanguage(Language preferred)
        {
            if (!CheckExists())
                return;
            instance.preferredLanguage = preferred;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set Preferred Language to: [{0}]", preferred), instance.GetLoggingLevel(), instance);
        }
        /// <summary>
        /// Sets a new Fallback Language
        /// </summary>
        /// <param name="fallBack">Language to set</param>
        public static void SetFallBackLanguage(Language fallBack)
        {
            if (!CheckExists())
                return;
            instance.fallbackLanguage = fallBack;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set Fallback-Language to: [{0}]", fallBack), instance.GetLoggingLevel(), instance);
        }
        /// <summary>
        /// Sets whether to use preferred Language
        /// </summary>
        /// <param name="preferred">Whether to use preferred Language</param>
        public static void SetUsePreferred(bool preferred)
        {
            if (!CheckExists())
                return;
            instance.usePreferredLanguage = preferred;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set Use-Preferred-Language to: [{0}]", preferred), instance.GetLoggingLevel(), instance);
        }
        /// <summary>
        /// Sets whether to use a remote file
        /// </summary>
        /// <param name="useRemote">Whether to use a remote file</param>
        public static void SetUseRemote(bool useRemote)
        {
            if (!CheckExists())
                return;
            instance.useRemoteLanguageFile = useRemote;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set Use-Remote-File to: [{0}]", useRemote), instance.GetLoggingLevel(), instance);
        }
        /// <summary>
        /// Sets whether to only download remote file on WIFI
        /// </summary>
        /// <param name="wifiOnly">Whether to only download remote file on WIFI</param>
        public static void SetWIFIDownloadOnly(bool wifiOnly)
        {
            if (!CheckExists())
                return;
            instance.wifiDownloadOnly = wifiOnly;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set WIFI-Download to: [{0}]", wifiOnly), instance.GetLoggingLevel(), instance);
        }
        /// <summary>
        /// Sets string used for NewLine
        /// </summary>
        /// <param name="newLine">NewLine-String to set</param>
        public static void SetNewLineString(string newLine)
        {
            if (!CheckExists())
                return;
            instance.newLineStr = newLine;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set NewLine to: [{0}]", newLine), instance.GetLoggingLevel(), instance);
        }
        /// <summary>
        /// Sets char used as Delimiter
        /// </summary>
        /// <param name="delim">Delimiter-char to set</param>
        public static void SetDelimiter(char delim)
        {
            if (!CheckExists())
                return;
            instance.delimiter = delim;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set Delimiter to: [{0}]", delim), instance.GetLoggingLevel(), instance);
        }
        /// <summary>
        /// Sets Credentials used for downloading files
        /// </summary>
        /// <param name="userName">Username to set</param>
        /// <param name="password">Password to set</param>
        /// <param name="domain">Domain to set</param>
        public static void SetCredentials(string userName, string password, string domain = null)
        {
            if (!CheckExists())
                return;
            instance.userName = userName;
            instance.password = password;
            instance.domain = domain;
            LoggingLevel log = instance.GetLoggingLevel();
            LoggingLevel.Info.Log("LanguageManager: Set new UserCredentials", log, instance);
            if (log >= LoggingLevel.Development)
                LoggingLevel.Development.Log(string.Format("LanguageManager: Set UserName to [{0}], Password to [{1}] and Domain to [{2}]", userName, password, domain), log, instance);
            else // Don't log sensitive data
                LoggingLevel.Debug.Log(string.Format("LanguageManager: Set UserName to [{0}] and Domain to [{1}]", userName, domain), instance.GetLoggingLevel(), instance);
        }
        #endregion

        #region SetLanguage
        /// <summary>
        /// Sets (and stores) a language to this Manager
        /// Also refreshes all registered TranslatedText-objects
        /// </summary>
        /// <param name="language">Language to set</param>
        public static void SetLanguage(Language language)
        {
            if (!CheckExists())
                return;

            if (!IsSupportedGeneral(language))
                language = Language.English;

            nonLatinLanguage = CheckIfNonLatin(language);

            PlayerPrefs.SetInt(KEY_LANGUAGE_GENERAL, (int)language);
            instance.currentLanguage = language;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set Language to [{0}]", language), instance.GetLoggingLevel(), instance);
            ResourceLoader.SaveLanguage(language);
            if (IsLoaded())
                RefreshRegisteredTranslatedTexts();
        }

        /// <summary>
        /// Is the language we're trying to set as general supported by the application?
        /// </summary>
        /// <param name="language">Language to set</param>
        /// <returns>True if supported</returns>
        public static bool IsSupportedGeneral(Language language)
        {
            return GENERAL_SUPPORTED_LANGUAGES.Contains(language.GetISOCode().ToString());
        }

        /// <summary>
        /// Check if the current set langauge doesn't use a latin alphabet
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        private static bool CheckIfNonLatin(Language language)
        {
            switch (language)
            {
                case Language.Korean:
                    return true;
                case Language.Japanese:
                    return true;
                case Language.Chinese:
                    return true;
                case Language.Hebrew:
                    return true;
                case Language.Arabic:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Sets (and stores) a language to this Manager
        /// Also refreshes all RegisterText-objects
        /// </summary>
        /// <param name="ISO_Code">ISO-Code of Language to set</param>
        public static void SetLanguage(ISOCode639 ISO_Code)
        {
            LoggingLevel.Info.Log(string.Format("LanguageManager: Setting Language from ISO-Code [{0}]", ISO_Code), instance.GetLoggingLevel(), instance);
            SetLanguage(ISO_Code.GetLanguage());
        }
        /// <summary>
        /// Sets (and stores) a language to this Manager
        /// Also refreshes all RegisterText-objects
        /// </summary>
        /// <param name="deviceLanguage">SystemLanguage representing Language to set</param>
        public static void SetLanguage(SystemLanguage deviceLanguage)
        {
            LoggingLevel.Info.Log(string.Format("LanguageManager: Setting Language from Device-Language [{0}]", deviceLanguage), instance.GetLoggingLevel(), instance);
            SetLanguage(deviceLanguage.GetLanguage());
        }
        #endregion

        #region RegisterText
        #region Registration
        /// <summary>
        /// Registers a TextField based on FieldName
        /// </summary>
        /// <param name="text">The TranslatedText-Object to register</param>
        /// <param name="setValue">Whether to update the Text-Value after registration</param>
        public static void RegisterTranslatedText(AbstractTranslatedText text, bool setValue = true)
        {
            if (!CheckExists())
                return;
            if (string.IsNullOrEmpty(text.FieldName))
            {
                LoggingLevel.Error.LogAlways(string.Format("LanguageManager: Please set a FieldName before registering TranslatedText [{0}]", text.gameObject.name));
                return;
            }
            Dictionary<string, List<AbstractTranslatedText>> texts = instance.registerTexts;
            LoggingLevel log = instance.GetLoggingLevel();
            if (!texts.ContainsKey(text.FieldName))
            {
                texts.Add(text.FieldName, new List<AbstractTranslatedText> { text });
                LoggingLevel.Info.Log(string.Format("LanguageManager: Registered new key [{0}] from TranslatedText [{1}]", text.FieldName, text.name), log, text);
            }
            else if (!texts[text.FieldName].Contains(text))
            {
                texts[text.FieldName].Add(text);
                LoggingLevel.Info.Log(string.Format("LanguageManager: Registered new TranslatedText [{0}]", text.name), log, text);
            }
            if (setValue)
                SetTextValue(text);
            LoggingLevel.Debug.Log(string.Format("LanguageManager: Set IsRegistered for TranslatedText [{0}]", text.name), log, text);
        }
        /// <summary>
        /// Registers a list of RegisterTexts based on their respective FieldNames
        /// </summary>
        /// <param name="texts">List of Texts to Register</param>
        public static void RegisterTranslatedTexts(List<AbstractTranslatedText> texts, bool setValue = true)
        {
            if (!CheckExists())
                return;
            LoggingLevel.Debug.Log(string.Format("LanguageManager: Registering [{0}] TranslatedTexts", texts.Count), instance.GetLoggingLevel(), instance);
            foreach (AbstractTranslatedText text in texts)
                RegisterTranslatedText(text, setValue);
        }
        /// <summary>
        /// Remove a RegisterText from being registered
        /// </summary>
        /// <param name="text">RegisterText to UnRegister</param>
        public static bool UnregisterTranslatedText(AbstractTranslatedText text)
        {
            if (!CheckExists())
                return false;
            LoggingLevel log = instance.GetLoggingLevel();
            LoggingLevel.Debug.Log(string.Format("LanguageManager: IsRegistered set to False for [{0}]", text.name), log, text);
            Dictionary<string, List<AbstractTranslatedText>> texts = instance.registerTexts;
            if (texts.ContainsKey(text.FieldName))
            {
                texts[text.FieldName].Remove(text);
                LoggingLevel.Info.Log(string.Format("LanguageManager: Removed registered text [{0}]", text.name), log, text);
                if (texts[text.FieldName].Count.Equals(0))
                {
                    LoggingLevel.Debug.Log(string.Format("LanguageManager: Removed key [{0}] from registered Texts", text.FieldName), log, text);
                    texts.Remove(text.FieldName);
                }
                return true;
            }
            LoggingLevel.Debug.Log(string.Format("LanguageManager: [{0}] was not Registered", text.name), log, text);
            return false;
        }
        /// <summary>
        /// Remove a RegisterText from being registered
        /// </summary>
        /// <param name="text">RegisterText to UnRegister</param>
        public static void UnregisterTranslatedTexts(List<AbstractTranslatedText> texts)
        {
            if (!CheckExists())
                return;
            LoggingLevel.Debug.Log(string.Format("LanguageManager: Unregistering [{0}] TranslatedTexts", texts.Count), instance.GetLoggingLevel(), instance);
            foreach (AbstractTranslatedText text in texts)
                UnregisterTranslatedText(text);
        }
        #endregion

        #region Refresh
        /// <summary>
        /// Refreshes a RegisterText-Object.
        /// This is the same as calling Refresh() on the object
        /// </summary>
        public static void RefreshTranslatedText(AbstractTranslatedText text)
        {
            if (!CheckExists())
                return;
            text.Refresh();
            LoggingLevel.Info.Log(string.Format("LanguageManager: Refreshed TranslatedText {0}", text.name), instance.GetLoggingLevel(), text);
        }
        /// <summary>
        /// Refreshes all registered RegisterText-Objects.
        /// </summary>
        public static void RefreshRegisteredTranslatedTexts()
        {
            if (!CheckExists())
                return;
            LoggingLevel.Info.Log("LanguageManager: Refreshing all registered TranslatedTexts", Instance.GetLoggingLevel(), instance);
            foreach (List<AbstractTranslatedText> list in instance.registerTexts.Values.ToList())
                foreach (AbstractTranslatedText text in list)
                    if(text != null && text.gameObject != null)
                        text.Refresh();
        }
        /// <summary>
        /// Refreshes ALL RegisterText-Objects, including non-registered ones
        /// <para>
        /// Is the same as calling RegisterText.RefreshAll()
        /// </para>
        /// </summary>
        public static void RefreshALLTranslatedTexts()
        {
            LoggingLevel.Info.Log("TranslatedText: Refreshing ALL TranslatedTexts", instance.GetLoggingLevel(), instance);
            AbstractTranslatedText.RefreshAll();
        }
        #endregion

        #region Value
        /// <summary>
        /// Set value for text
        /// </summary>
        /// <param name="text">Texts to set</param>
        public static void SetTextValue(AbstractTranslatedText text)
        {
            if (!CheckLoaded())
                return;
            LoggingLevel log = instance.GetLoggingLevel();
            LoggingLevel.Debug.Log(string.Format("LanguageManager: Using Language [{0}] and Key [{1}] to set value for [{2}]", text.UseCustomLanguage ? text.CustomLanguage : instance.currentLanguage, text.FieldName, text.name), log, text);
            text.SetText(GetTranslation(text.FieldName, text.UseCustomLanguage ? text.CustomLanguage : instance.currentLanguage));
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set [{0}] to [{1}]", text.name, text.GetText()), log, text);
        }
        /// <summary>
        /// Set value for texts
        /// </summary>
        /// <param name="texts">Texts to set</param>
        public static void SetTextValue(List<AbstractTranslatedText> texts)
        {
            if (!CheckLoaded())
                return;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Setting value for [{0}] TranslatedTexts", texts.Count), instance.GetLoggingLevel(), instance);
            foreach (AbstractTranslatedText t in texts)
                SetTextValue(t);
        }
        /// <summary>
        /// Set value for text
        /// </summary>
        /// <param name="text">Texts to set</param>
        /// <param name="language">Language to use</param>
        public static void SetTextValue(AbstractTranslatedText text, Language language)
        {
            if (!CheckLoaded())
                return;
            LoggingLevel log = instance.GetLoggingLevel();
            LoggingLevel.Development.Log(string.Format("LanguageManager: Setting value for [{0}] using Language [{1}]", text.name, language), log, text);
            text.SetText(GetTranslation(text.FieldName, language));
            LoggingLevel.Info.Log(string.Format("LanguageManager: Set [{0}] to [{1}]", text.name, text.GetText()), log, text);
        }
        /// <summary>
        /// Set value for texts
        /// </summary>
        /// <param name="texts">Texts to set</param>
        /// <param name="language">Language to use</param>
        public static void SetTextValue(List<AbstractTranslatedText> texts, Language language)
        {
            if (!CheckLoaded())
                return;
            LoggingLevel.Info.Log(string.Format("LanguageManager: Setting value for [{0}] TranslatedTexts using Language [{1}]", texts.Count, language), instance.GetLoggingLevel(), instance);
            foreach (AbstractTranslatedText t in texts)
                SetTextValue(t, language);
        }
        #endregion
        #endregion

        #region Translation
        /// <summary>
        /// Gets a translation for a specific key for the currently selected language
        /// </summary>
        /// <param name="key">Key to get translation for</param>
        /// <returns>The translated string for this key</returns>
        public static string GetTranslation(string key)
        {
            return GetTranslation(key, instance.currentLanguage);
        }
        /// <summary>
        /// Gets a translation for a specific key for the a specific language
        /// </summary>
        /// <param name="key">Key to get translation for</param>
        /// <param name="language">Language to get translation for</param>
        /// <returns>The translated string for this key and language</returns>
        public static string GetTranslation(string key, Language language)
        {
            if (!CheckLoaded())
                return string.Empty;
            return LanguageParser.GetString(language, key);
        }
        /// <summary>
        /// Clears all Cache-Files
        /// </summary>#if UNITY_EDITOR
#if UNITY_EDITOR
        [UnityEditor.MenuItem("DebugTools/Language/ClearCache")]
#endif
        public static void ClearAllCache()
        {
            Debug.Log("Clearing cached files");
            LoggingLevel.Info.Log("LanguageManager: Clearing Cache Files", instance.GetLoggingLevel());
            ResourceLoader.ClearUnusedCache();
            ResourceLoader.ClearUnusedCache(ResourceLoader.CurrentFile);
        }
        #endregion
        #endregion

        #region Public

        /// <summary>
        /// Loads Translations for this LanguageManager
        /// Uses a CoRoutine for downloading & parsing
        /// </summary>
        /// <param name="loadRemote">Whether to use a remote file (download from URL)</param>
        /// <param name="callback">Return if translation document was correctly updated</param>
        public void LoadTranslations(bool loadRemote, Action<bool> callback = null)
        {
            LoggingLevel log = GetLoggingLevel();
            LoggingLevel.Info.Log(string.Format("LanguageManager: Loading Translations [LoadRemoteFile={0}]", loadRemote), log, this);
            StartCoroutine(LanguageParser.Initialize(returnVal =>
            {
                if (returnVal.HasFinished)
                {
                    LoggingLevel.Development.Log(string.Format("LanguageManager: {0}", returnVal), log, instance);
                    if (returnVal.HasSucceeded)
                    {

                        LoggingLevel.Info.Log("LanguageManager: Translations loaded successfully", log, instance);
                        RefreshRegisteredTranslatedTexts();
                        if (useRemoteLanguageFile)
                        {
                            LoggingLevel.Development.Log("LanguageManager: Clearing Cache File in 5 seconds", log, instance);
                            StartCoroutine(DelayedClearCache());
                        }
                        callback?.Invoke(true);
                    }
                    else
                    {
                        callback?.Invoke(false);
                    }

                }
            }, loadRemote));
        }
        /// <summary>
        /// Gets Logging-Level for this LanguageManager
        /// </summary>
        /// <returns>logLevel if logMethods = true. LoggingLevel.Error otherwise</returns>
        public LoggingLevel GetLoggingLevel()
        {
            return logMethods ? logLevel : LoggingLevel.Error;
        }
        #endregion

        #region Internal
        /// <summary>
        /// Whether a TranslatedText is Registered
        /// </summary>
        /// <param name="text">Text to Check</param>
        /// <returns>True if Text is Registered</returns>
        internal bool IsRegistered(AbstractTranslatedText text)
        {
            foreach (List<AbstractTranslatedText> abstractTranslatedTexts in registerTexts.Values)
                if (abstractTranslatedTexts.Contains(text))
                    return true;
            return false;
        }
        #endregion

        #region Private
        /// <summary>
        /// Check whether Instance Exists
        /// </summary>
        /// <returns></returns>
        private static bool CheckExists()
        {
            if (!Exists)
                LoggingLevel.Error.LogAlways("LanguageManager: Manager does not Exist");
            return Exists;
        }

        /// <summary>
        /// Check whether manager is loaded
        /// </summary>
        /// <returns></returns>
        private static bool CheckLoaded()
        {
            if (!IsLoaded())
                LoggingLevel.Error.LogAlways("LanguageManager: Manager is not Loaded");
            return true;
        }

        /// <summary>
        /// Wait 5 Seconds, then clear old cache file.
        /// </summary>
        private IEnumerator DelayedClearCache()
        {
            yield return new WaitForSeconds(5);
            LoggingLevel.Debug.Log("LanguageManager: Clearing unused cache file", instance.GetLoggingLevel(), instance);
            ResourceLoader.ClearUnusedCache();
        }
        #endregion
        #endregion
    }
}