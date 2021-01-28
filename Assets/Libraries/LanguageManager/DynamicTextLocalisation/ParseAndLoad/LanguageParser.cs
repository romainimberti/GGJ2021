using nl.DTT.LocalizedLanguage.Managers;
using nl.DTT.Utils.Containers;
using nl.DTT.Utils.Enums;
using nl.DTT.Utils.Extensions.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// COPYRIGHT: DTT (2018)
/// Made by: Frank van Hoof
/// </summary>
namespace nl.DTT.LocalizedLanguage.ParseAndLoad
{
    public class LanguageParser
    {
        #region Variables
        #region Internal
        /// <summary>
        /// Whether the LanguageMap has been loaded
        /// </summary>
        internal static bool IsLoaded { get { return language_map != null && !language_map.Count.Equals(0); } }
        #endregion

        #region Private
        /// <summary>
        /// The language map is a dictionary representation of the LanguageMap,
        /// containing [string_id, [Language, string_value]]
        /// </summary>
        internal static Dictionary<string, Dictionary<Language, string>> language_map;
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// (Re-)Initializes the LanguageParser
        /// </summary>
        /// <param name="callback">Callback for Status</param>
        /// <param name="useRemoteLanguageFile">Whether to Download a LanguageFile</param>
        internal static IEnumerator Initialize(Action<CallBackResponse<bool>> callback, bool useRemoteLanguageFile)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            yield return new WaitUntil(() => LanguageManager.Exists);
            LoggingLevel.Info.Log("LanguageParser: Initializing", log);
            callback?.Invoke(new CallBackResponse<bool>(LoadingState.INPUT_PARSING));
            LoggingLevel.Info.Log("LanguageParser: [INIT] Downloading Remote Map", log);

            Action<CallBackResponse<string>> onLanguageLoaded = success =>
            {
                LoggingLevel.Debug.Log(success.ToString(), log);
                if (success.HasData && !success.HasFailed && !success.LoadingState.Equals(LoadingState.INPUT_PARSING))
                {

                    LoggingLevel.Info.Log("LanguageParser: [INIT] Getting LanguageMap succeeded", log);
                    callback?.Invoke(new CallBackResponse<bool>(LoadingState.REQUEST_SUCCEED, false));
                    LoadLanguageMap(success.Data, callback);

                }
                else if (success.LoadingState.Equals(LoadingState.REQUEST_FAILED))
                {
                    LoggingLevel.Info.Log("LanguageParser: [INIT] Getting LanguageMap failed", log);
                    callback?.Invoke(new CallBackResponse<bool>(LoadingState.REQUEST_FAILED, true));
                }
            };

            if (useRemoteLanguageFile)
                GetRemoteLanguageMap(onLanguageLoaded);
            else
                GetCachedLanguageMap(onLanguageLoaded);
            yield return null;
        }

        #region Get
        /// <summary>
        /// Gets the string associated with the string ID from the LanguageMap CSV file for the current language
        /// </summary>
        /// <param name="language">Language to retrieve string for</param>
        /// <param name="stringID">ID of the string to retrieve</param>
        /// <returns>String associated with ID for Language, or Fallback-Language if it does not exist</returns>
        internal static string GetString(Language language, string stringID)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            if (stringID == null || stringID.Equals(string.Empty))
                throw new ArgumentNullException("stringID", "LanguageParser: Invalid or Empty Key");
            if (language_map == null || language_map.Count.Equals(0))
                throw new InvalidOperationException("LanguageParser: LanguageMap has not been Initialized");
            if (!language_map.ContainsKey(stringID))
                throw new ArgumentException("stringID", string.Format("LanguageParser: Key does not exist in LanguageMap: {0}", stringID));
            string translation = string.Empty;
            LoggingLevel.Debug.Log(string.Format("LanguageParser: Getting translation for key [{0}] in Language [{1}]", stringID, language), log);
            try { translation = language_map[stringID][language]; }
            catch (Exception)
            {
                LoggingLevel.Warning.Log(string.Format("LanguageParser: Unable to get Translation [Key={0}] [Language={1}]", stringID, language), log);
                translation = string.Empty;
            }
            if (translation.Equals(string.Empty))
            {
                LoggingLevel.Warning.Log(string.Format("LanguageParser: Could not retrieve translation in [{0}], trying Fallback-Language {1}", language, LanguageManager.Instance.FallbackLanguage), log);
                return language_map[stringID][LanguageManager.Instance.FallbackLanguage];
            }
            LoggingLevel.Info.Log(string.Format("LanguageParser: Retrieved translation [{0}] for Key [{1}] in Language [{2}]", translation, stringID, language), log);
            
            //Replace all |'s with commas. We are unable to have commas in the google sheets doc so we will use |'s instead.
            translation = translation.Replace('|', ',');
            translation = translation.Replace("\r","");
            return translation;
        }
        #endregion

        #region Loading Logic
        /// <summary>
        /// Directly loads in a LanguageMap (from string)
        /// </summary>
        /// <param name="map">LanguageMap to load in</param>
        /// <param name="callBack">CallBackResponse used to check parse-status (if Data = true, execution has finished)</param>
        internal static void LoadLanguageMap(string map, Action<CallBackResponse<bool>> callBack = null,bool fallbackVersion = false)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            language_map = new Dictionary<string, Dictionary<Language, string>>();
            callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING));
            LoggingLevel.Info.Log(string.Format("LanguageParser: Parsing LanguageMap: {0}", map), log);
            string[] lines = map.Split(new [] { LanguageManager.Instance.NewLineStr }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length.Equals(0) && !fallbackVersion)
            {
                LoggingLevel.Error.Log("LanguageParser: Found 0 Lines. Newline-Error?", log);
                if (!fallbackVersion)
                    LoadLanguageMap(LanguageManager.Instance.LanguageMap.text, callBack, true);
                else
                    callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING_FAILED, true));
                return;
            }
            LoggingLevel.Debug.Log(string.Format("LanguageParser: [PARSE] Found [{0}] Lines", lines.LongLength), log);
            string[] column = lines[0].Split(new [] { LanguageManager.Instance.Delimiter }, StringSplitOptions.None);
            int columnAmount = column.Length; // ColumnSize
            LoggingLevel.Debug.Log(string.Format("LanguageParser: [PARSE] Parsing [{0}] Languages", column.Length -1), log);
            // Load Language from Line 1
            Dictionary<int, Language> columnNo = new Dictionary<int, Language>();
            for (int i = 1; i < column.Length; i++)
            {
                string text = column[i];
                LoggingLevel.Debug.Log(string.Format("LanguageParser: [PARSE] Parsing column [{0}], Value: [{1}]", i, text), log);
                try
                {
                    int test;
                    // Prevent parsing an int-value to a language
                    if (int.TryParse(text, out test))
                        throw new ArgumentException();
                    Language l = (Language)Enum.Parse(typeof(Language), text);                    
                    if (Enum.IsDefined(typeof(Language), l))
                    {
                        columnNo.Add(i, l);
                        continue;
                    }
                    ISOCode639 iso = (ISOCode639)Enum.Parse(typeof(Language), text);
                    if (Enum.IsDefined(typeof(ISOCode639), iso))
                    {
                        columnNo.Add(i, iso.GetLanguage());
                        continue;
                    }
                    SystemLanguage language = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), text);
                    if (Enum.IsDefined(typeof(SystemLanguage), language))
                    {
                        columnNo.Add(i, language.GetLanguage());
                        continue;
                    }
                    throw new ArgumentException();
                }
                catch (ArgumentException)
                {
                    LoggingLevel.Warning.Log(string.Format("LanguageParser: [PARSE] Invalid Language: [{0}] at Column: [{1}]", text, i), log);
                    callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING, false));
                }
                catch (Exception e)
                {
                    LoggingLevel.Error.Log(string.Format("LanguageParser: [PARSE] An error occurred , {0}", e), log);
                    callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING, true));
                    throw;
                }
            }
            if (columnNo.Count.Equals(0))
            {
                LoggingLevel.Error.Log("LanguageParser: [PARSE] No valid Languages found", log);
                callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING_FAILED, true));
                return;
            }
            LoggingLevel.Info.Log(string.Format("LanguageParser: [PARSE] Parsed [{0}] valid Languages", columnNo.Keys.Count), log);
            // Load Values
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    column = lines[i].Split(new char[] { LanguageManager.Instance.Delimiter }, StringSplitOptions.None);
                    string key = column[0];
                    if (!column.Length.Equals(columnAmount))
                    {
                        LoggingLevel.Error.Log("Failed line: " + lines[i],log);
                        LoggingLevel.Error.Log(string.Format("LanguageParser: [PARSE] Parsing Error on Line [{0}] - Invalid ColumnSize (Delimiter Error?). Skipping Line", i), log);
                        callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING, false));
                        continue;
                    }
                    if (string.IsNullOrEmpty(key))
                    {
                        LoggingLevel.Warning.Log(string.Format("LanguageParser: [PARSE] Parsing Error on Line [{0}] - Empty Key. Skipping Line", i), log);
                        callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING, false));
                        continue;
                    }
                    try
                    {
                        language_map.Add(key, new Dictionary<Language, string>());
                        LoggingLevel.Debug.Log(string.Format("LanguageParser: [PARSE] Parsing Line [{0}] - Added Key [{1}]", i, key), log);
                    }
                    catch (ArgumentException)
                    {
                        LoggingLevel.Warning.Log(string.Format("LanguageParser: [PARSE] Parsing Error on Line [{0}] - Duplicate Key Entry [{1}]. Skipping Line", i, key), log);
                        callBack?.Invoke(new CallBackResponse<bool>(LoadingState.OUTPUT_PARSING, false));
                        continue;
                    }
                    for (int j = 1; j < column.Length; j++)
                    {
                        string text = column[j];
                        text = text.Replace('|', ',');
                        text = text.Replace("\\n", "\n");
                        if (string.IsNullOrEmpty(text))
                        {
                            LoggingLevel.Debug.Log(string.Format("LanguageParser: [PARSE] Empty value for Line [{0}] Column [{1}]", i, j), log);
                            continue;
                        }
                        if (!columnNo.ContainsKey(j))
                        {
                            LoggingLevel.Debug.Log(string.Format("LanguageParser: [PARSE] No Language for Line [{0}] Column [{1}] for Value [{2}]", i, j, text), log);
                            continue;
                        }
                        language_map[key].Add(columnNo[j], text);
                        LoggingLevel.Development.Log(string.Format("LanguageParser: [PARSE] [L={0},C={1}] Parsed [{2}] to Key [{3}] in Language [{4}]", i, j, text, key, columnNo[j]), log);
                    }
                }
                catch (Exception)
                {
                    LoggingLevel.Error.Log(string.Format("LanguageParser: [PARSE] Error on Line : [{0}] - Stopping Parse.", i), log);
                    throw;
                }
            }
            LoggingLevel.Info.Log("LanguageParser: Parse Completed", log);
            callBack?.Invoke(new CallBackResponse<bool>(true));
        }
        #endregion

        #region DownloadingLogic
        /// <summary>
        /// Gets the cached download, or fallback if no cache exists.
        /// </summary>
        /// <param name="callback">callback to get status of download</param>
        private static void GetCachedLanguageMap(Action<CallBackResponse<string>> callback = null)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            LoggingLevel.Info.Log("LanguageManager: [DL] Loading Cached File", log);
            ResourceLoader.LoadCachedMap(callback);
        }

        /// <summary>
        /// (Attempts to) Download a remote LanguageMap
        /// Fallback to Cache-File or Fallback-File if remote call fails
        /// </summary>
        /// <param name="callBack">CallBack to get status of download</param>
        /// <param name="getCached">Whether to skip downloading and open the cached map</param>
        /// <returns>String Text</returns>
        private static void GetRemoteLanguageMap(Action<CallBackResponse<string>> callBack = null)
        {
            LoggingLevel log = LanguageManager.Instance.GetLoggingLevel();
            // use cache if only wifi and on Mobile Data, get cached is set, or no internet.
            if (Application.internetReachability == NetworkReachability.NotReachable)            
                GetCachedLanguageMap(callBack);
            else
            {
                try
                {
                    LoggingLevel.Info.Log("LanguageParser: [DL] Downloading Remote LanguageMap", log);
                    ResourceLoader.LoadRemoteMap(LanguageManager.Instance.languageURL, returnVal => 
                    {
                        Debug.Log("CAllback recieved in state:" + returnVal.LoadingState.ToString());
                        //If we sent a bad URL use  a cached version instead.
                        if (returnVal.LoadingState.Equals(LoadingState.REQUEST_FAILED))
                            ResourceLoader.LoadCachedMap(callBack);
                        else
                            callBack?.Invoke(returnVal);
                    });
                }
                catch (Exception)
                {
                    // LOG EXC
                    try
                    {
                        LoggingLevel.Warning.Log("LanguageParser: [DL] Loading Remote Map Failed. Trying Cached Map", log);
                        ResourceLoader.LoadCachedMap(callBack);
                    }
                    catch (Exception)
                    {
                        // Remote, Cache, & Fallback failed
                        LoggingLevel.Error.Log("LanguageParser: [DL] Failed to get a LanguageMap", log);
                        callBack?.Invoke(new CallBackResponse<string>(LoadingState.INPUT_PARSING_FAILED));
                        throw new Exception("LanguageManager: Failed to get a LanguageMap");
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}