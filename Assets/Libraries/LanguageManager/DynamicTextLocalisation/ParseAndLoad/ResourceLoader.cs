using nl.DTT.LocalizedLanguage.Managers;
using nl.DTT.Utils.Containers;
using nl.DTT.Utils.Enums;
using nl.DTT.Utils.Extensions.Enums;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

/// <summary>
/// COPYRIGHT: DTT (2018)
/// Made by: Frank van Hoof
/// </summary>
namespace nl.DTT.LocalizedLanguage.ParseAndLoad
{
    public static class ResourceLoader
    {
        #region Variables
        #region Constants
        /// <summary>
        /// SavedLanguage-Key for PlayerPrefs
        /// </summary>
        private static readonly string PlayerPrefKey_SavedLanguage = "DTT_RESOURCES_SAVED_LANGUAGE_KEY";
        /// <summary>
        /// SavedLanguageSleepyNight-Key for PlayerPrefs
        /// </summary>
        private static readonly string PlayerPrefKey_SavedLanguageSleepyNight = "DTT_RESOURCES_SAVED_LANGUAGE_SLEEPY_NIGHT_KEY";
        /// <summary>
        /// Path to Cache of LanguageMap (Excluding PersistentDataPath, / and FileName)
        /// </summary>
        private static readonly string PathToCachedLanguageMap = "/LanguageData";
        /// <summary>
        /// FileNumber (1 or 2) of current CacheFile (2 files are used so 1 can be loaded while the other is updated)
        /// </summary>
        private static readonly string PlayerPrefKey_LanguageFileNo = "DTT_RESOURCES_SAVED_LANGUAGE_FILENO";

        /// <summary>
        /// Client used to dowmload the translation document.
        /// </summary>
        private static WebClient client;

        /// <summary>
        /// Callback giving the state of the downloading.
        /// </summary>
        private static Action<CallBackResponse<string>> globalCallback;

        #endregion

        #region Language
        /// <summary>
        /// Whether the user has previously saved a chosen language
        /// </summary>
        public static bool UserHasSavedLanguage { get { return PlayerPrefs.HasKey(PlayerPrefKey_SavedLanguage); } }
        /// <summary>
        /// The Language Saved by the user
        /// </summary>
        public static Language UserSavedLanguage
        {
            get
            {
                if (!UserHasSavedLanguage)
                {
                    LoggingLevel.Debug.Log("ResourceLoader: User has no Saved Language. Returning Language.Unknown", LanguageManager.Instance.GetLoggingLevel());
                    return Language.Unknown;
                }
                return (Language)PlayerPrefs.GetInt(PlayerPrefKey_SavedLanguage);
            }
        }
        /// <summary>
        /// The Sleepy Night language saved by the user
        /// </summary>
        public static Language UserSavedLanguageSleepyNight
        {
            get
            {
                if (!PlayerPrefs.HasKey(PlayerPrefKey_SavedLanguageSleepyNight))
                {
                    LoggingLevel.Debug.Log("ResourceLoader: User has no Saved Language. Returning General Language", LanguageManager.Instance.GetLoggingLevel());
                    return UserSavedLanguage;
                }
                return (Language)PlayerPrefs.GetInt(PlayerPrefKey_SavedLanguageSleepyNight);
            }
        }
        #endregion

        #region File
        /// <summary>
        /// FileNo currently in use. (2 files are used so 1 can be loaded while the other is updated)
        /// </summary>
        public static int CurrentFile
        {
            get
            {
                if (PlayerPrefs.HasKey(PlayerPrefKey_LanguageFileNo))
                    return PlayerPrefs.GetInt(PlayerPrefKey_LanguageFileNo);
                LoggingLevel.Development.Log("ResourceLoader: No Cache-FileNo set. Setting to 1",
                    LanguageManager.Instance.GetLoggingLevel());
                PlayerPrefs.SetInt(PlayerPrefKey_LanguageFileNo, 1);
                return 1;
            }
        }
        /// <summary>
        /// Path to Cache of LanguageMap (Including PersistentDataPath)
        /// </summary>
        private static string CachePath { get { return string.Concat(Application.persistentDataPath, PathToCachedLanguageMap); } }
        /// <summary>
        /// Full path to current cache-file
        /// </summary>
        private static string CacheFilePath
        {
            get { return string.Concat(CachePath, "/Cache", CurrentFile); }
        }

        #endregion
        #endregion

        #region Methods
        #region Language
        /// <summary>
        /// Saves Language to PlayerPrefs
        /// </summary>
        public static void SaveLanguage(Language language)
        {
            PlayerPrefs.SetInt(PlayerPrefKey_SavedLanguage, (int)language);
            LoggingLevel.Debug.Log(string.Format("ResourceLoader: Saved Language [{0}] to PlayerPrefs", language), LanguageManager.Instance.GetLoggingLevel());
        }
        /// <summary>
        /// Saves Sleepy Night Language to PlayerPrefs
        /// </summary>
        public static void SaveLanguageSleepyNight(Language language)
        {
            PlayerPrefs.SetInt(PlayerPrefKey_SavedLanguageSleepyNight, (int)language);
            LoggingLevel.Debug.Log(string.Format("ResourceLoader: Saved Language [{0}] to PlayerPrefs", language), LanguageManager.Instance.GetLoggingLevel());
        }
        /// <summary>
        /// Clears the currently saved Language
        /// </summary>
        public static void ClearSavedLanguage()
        {
            PlayerPrefs.DeleteKey(PlayerPrefKey_SavedLanguage);
            LoggingLevel.Debug.Log("ResourceLoader: Cleared Language from PlayerPrefs", LanguageManager.Instance.GetLoggingLevel());
        }
        #endregion

        #region File
        /// <summary>
        /// Loads a LanguageMap from a URL
        /// Should be called from a CoRoutine to prevent freezing/slowdown
        /// </summary>
        /// <param name="URL">URL to load LanguageMap from</param>
        /// <param name="callBack">CallBack for Download- and Parse-Updates</param>
        /// <returns>String Text (String.Empty if failed)</returns>
        public static void LoadRemoteMap(string URL, Action<CallBackResponse<string>> callBack)
        {
            client = new WebClient();
            globalCallback = callBack;
            client.DownloadDataCompleted += Client_DownloadFileCompleted;
            // Download file, then save 
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            LoggingLevel.Debug.Log(string.Format("ResourceLoader: Attempting to download LanguageMap from [{0}]", URL), log);

            client.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            client.Encoding = Encoding.UTF8;
            Uri myUri = new Uri(URL);
            string data = String.Empty;
            client.DownloadDataAsync(myUri);

            LoggingLevel.Debug.Log("ResourceLoader: [DL] Downloaded Data", log);
        }

        /// <summary>
        /// Callback for when the file is downloaded.
        /// </summary>
        /// <param name="sender">who sent the file</param>
        /// <param name="e">holds result and meta data.</param>
        private static void Client_DownloadFileCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            client.DownloadDataCompleted -= Client_DownloadFileCompleted;
            string map = string.Empty;
            if(e.Error == null)
                map = Encoding.UTF8.GetString(e.Result ?? new byte[] { });
            else
                LoggingLevel.Error.Log("Language Request Error: Invalid URL: " + e.Error.Message,log);

            //LoggingLevel.Info.Log(string.Format("ResourceLoader: [DL] Downloaded String: {0}", map), log);
            if (!string.IsNullOrEmpty(map))
            {
                try
                {
                    globalCallback?.Invoke(new CallBackResponse<string>(LoadingState.REQUEST_SUCCEED, map));
                    SaveCachedMap(map);
                    ClearUnusedCache();
                }
                catch (Exception exp)
                {
                    throw new Exception("LanguageManager: [DL] Unable to Save Cache", exp);
                }
            }
            else
            {
                globalCallback?.Invoke(new CallBackResponse<string>(LoadingState.REQUEST_FAILED));
                throw new Exception("LanguageManager: [DL] Downloaded file is Empty");
            }
        }

        /// <summary>
        /// Loads a LanguageMap from Cache
        /// </summary>
        /// <returns>String Text</returns>
        public static string LoadCachedMap(Action<CallBackResponse<string>> callBack = null)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            try
            {
                LoggingLevel.Debug.Log("ResourceLoader: [LOAD] Loading Cached Map", log);
                FileStream fs = File.Open(CacheFilePath, FileMode.Open);
                fs.Close();
                callBack?.Invoke(new CallBackResponse<string>(LoadingState.INPUT_PARSING, string.Empty));
                string text = File.ReadAllText(CacheFilePath);
                LoggingLevel.Info.Log(string.Format("ResourceLoader: [LOAD] Loaded Cached String: {0}", text), log);
                callBack?.Invoke(new CallBackResponse<string>(LoadingState.INPUT_PARSING_SUCCEED, text));
                return text;
            }
            catch (Exception)
            {
                try
                {
                    LoggingLevel.Warning.Log("LanguageManager: [LOAD] Getting Cached Map failed. Trying Fallback-LanguageMap", log);
                    callBack?.Invoke(new CallBackResponse<string>(LoadingState.REQUEST_FAILED, string.Empty));
                    if (LanguageManager.Instance.LanguageMap != null)
                        LoggingLevel.Info.Log(string.Format("ResourceLoader: [LOAD] Loaded Fallback-Text: {0}", LanguageManager.Instance.LanguageMap.text), log, LanguageManager.Instance.LanguageMap);
                    callBack?.Invoke(new CallBackResponse<string>(LoadingState.INPUT_PARSING_SUCCEED, LanguageManager.Instance.LanguageMap.text));
                    return LanguageManager.Instance.LanguageMap.text; // Throws exception if null (to log error)
                }
                catch (Exception ex)
                {
                    if (ex is NullReferenceException || ex is UnassignedReferenceException)
                    {
                        LoggingLevel.Error.Log("LanguageManager: [LOAD] Please set a FallBack-LanguageMap to use", log);
                        callBack?.Invoke(new CallBackResponse<string>(LoadingState.INPUT_PARSING_FAILED));
                        return string.Empty;
                    }
                    else
                    {
                        LoggingLevel.Error.Log("LanguageParser: [INIT] An error occurred", log);
                        callBack?.Invoke(new CallBackResponse<string>(LoadingState.INPUT_PARSING_FAILED));
                        throw ex;
                    }

                throw ex;
                }
            }
        }
        /// <summary>
        /// Clears Unused Cache File
        /// </summary>
        /// <param name="fileNo">FileNo that is cleared. Set to 0 to clear non-Current Cache</param>
        public static void ClearUnusedCache(int fileNo = 0)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            // Clear Unused File
            if (fileNo.Equals(0))
                fileNo = CurrentFile.Equals(1) ? 2 : 1;
            string path = string.Format("{0}/Cache{1}", CachePath, fileNo);
            try
            {
                if (!Directory.Exists(CachePath))
                {
                    LoggingLevel.Debug.Log("ResourceLoader: No Unused Cache to Clear", log);
                    return; // Directory & Files don't exist
                }
                File.Delete(path);
                LoggingLevel.Info.Log(string.Format("ResourceLoader: Cleared Cache file [{0}]", path), log);
            }
            catch (FileNotFoundException)
            {
                LoggingLevel.Debug.Log("ResourceLoader: No Unused Cache to Clear", log);
                // Cache does not exist (yet). Do Nothing
            }
        }
        /// <summary>
        /// Saves a LanguageMap to Cache
        /// </summary>
        /// <param name="map">Map to Save</param>
        private static void SaveCachedMap(string map)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            // Save to proper file. Don't forget to clear other file to save disk-space
            int newFilePath = CurrentFile.Equals(1) ? 2 : 1;
            string fullPath = string.Format("{0}/Cache{1}", CachePath, newFilePath);
            try
            {
                LoggingLevel.Info.Log(string.Format("ResourceLoader: [SAVE] Saving Cache String to [{0}]", fullPath), log);
                if (!Directory.Exists(CachePath))
                {
                    LoggingLevel.Debug.Log(string.Format("ResourceLoader: [SAVE] Creating Directory [{0}]", CachePath), log);
                    Directory.CreateDirectory(CachePath);
                }
                FileStream fs;
                if (!File.Exists(fullPath))
                {
                    fs = File.Create(fullPath);
                    LoggingLevel.Debug.Log(string.Format("ResourceLoader: [SAVE] Created File at [{0}]", fullPath), log);
                }
                else
                {
                    fs = File.Open(fullPath, FileMode.OpenOrCreate);
                    LoggingLevel.Debug.Log(string.Format("ResourceLoader: [SAVE] Opened File at [{0}]", fullPath), log);
                }
                fs.Close();
                File.WriteAllText(fullPath, map);
                LoggingLevel.Info.Log(string.Format("ResourceLoader: [SAVE] Saved Cache to [{0}], Cache-Content: {1}", fullPath, map), log);
                UpdateFileNo();
            }
            catch (Exception e)
            {
                // TODO: Find relevant Exceptions
                LoggingLevel.Error.Log("ResourceLoader: [SAVE] Exception while saving Cache", log);
                throw e;
            }
        }
        /// <summary>
        /// Updates current file number. Called when File is finished downloading.
        /// </summary>
        private static void UpdateFileNo()
        {
            PlayerPrefs.SetInt(PlayerPrefKey_LanguageFileNo, CurrentFile.Equals(1) ? 2 : 1);
            LoggingLevel.Development.Log(string.Format("ResourceLoader: Updated File No. for Cache. New No [{0}]", CurrentFile), LanguageManager.Instance.GetLoggingLevel());
        }
        #endregion
        #endregion
    }
}