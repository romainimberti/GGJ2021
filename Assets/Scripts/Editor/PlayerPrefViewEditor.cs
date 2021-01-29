#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using com.romainimberti.ggj2021.utilities;
using static com.romainimberti.ggj2021.editor.PlayerPrefViewEditor.PlayerPrefReference;

namespace com.romainimberti.ggj2021.editor
{
    /// <summary>
    /// Editor window class for managing, viewing and debugging 
    /// of all player prefs in the project
    /// </summary>
    public class PlayerPrefViewEditor : EditorWindow
    {
        #region Inner Classes
        /// <summary>
        /// A class container for a single Player Pref
        /// </summary>
        [Serializable]
        public class PlayerPrefReference
        {
            #region Enum
            /// <summary>
            ///  All possible PlayerPref types
            /// </summary>
            public enum PlayerPrefTypes
            {
                AUTO,
                STRING,
                INT,
                BOOL
            };
            #endregion
            #region Variables
            /// <summary>
            /// The type of PlayerPrefType this PlayerPref is by itself
            /// </summary>
            [SerializeField]
            public PlayerPrefTypes type;
            /// <summary>
            /// The type to override PlayerPrefType with, if not set use type
            /// </summary>
            [SerializeField]
            public PlayerPrefTypes overrideType = PlayerPrefTypes.AUTO;
            /// <summary>
            /// The name of the constant value in code
            /// </summary>
            [SerializeField]
            public string constant;
            /// <summary>
            /// The name of the key (PlayerPref key)
            /// </summary>
            [SerializeField]
            public string key;
            /// <summary>
            /// Whether we are watching this key in the "Watch" list
            /// </summary>
            [SerializeField]
            public bool IsWatched = false;
            /// <summary>
            /// Whether we adding this key manually or if it was loaded
            /// </summary>
            [SerializeField]
            public bool IsManual = false;
            #endregion
            #region Methods
            /// <summary>
            /// The Constructor to create a PlayerPrefReference
            /// </summary>
            /// <param name="constant">The name of the constant value</param>
            /// <param name="key">The name of the key</param>
            public PlayerPrefReference(string constant, string key)
            {
                this.constant = constant;
                this.key = key;
            }
            /// <summary>
            /// Method to Set (Update) this PlayerPref
            /// </summary>
            /// <param name="value">The value to set the PlayerPref to</param>
            public void SetPlayerPref(string value)
            {
                switch (DefinePlayerPref())
                {
                    // Update String PlayerPref
                    case PlayerPrefTypes.STRING:
                        PlayerPrefs.SetString(key, value);
                        break;
                    // Update Int PlayerPref
                    case PlayerPrefTypes.INT:
                        PlayerPrefs.SetInt(key, int.Parse(value));
                        break;
                    // Update Bool PlayerPref
                    case PlayerPrefTypes.BOOL:
                        PlayerPrefsX.SetBool(key, bool.Parse(value));
                        break;

                    // No matching PlayerPref type found to update
                    default:
                        Debug.LogError("NO TYPE MATCHED");
                        break;
                }
            }

            /// <summary>
            /// Method to get the PlayerPref value
            /// </summary>
            /// <returns>The value of this PlayerPref always cast to string</returns>
            public string GetPlayerPrefValue()
            {
                switch (DefinePlayerPref())
                {
                    case PlayerPrefTypes.STRING:
                        return PlayerPrefs.GetString(key);
                    case PlayerPrefTypes.INT:
                        return PlayerPrefs.GetInt(key).ToString();
                    case PlayerPrefTypes.BOOL:
                        return PlayerPrefsX.GetBool(key).ToString();
                    default:
                        return string.Empty;

                }
            }

            /// <summary>
            /// Method that returns the type of PlayerPref this is
            /// </summary>
            /// <returns>The type of PlayerPref</returns>
            public PlayerPrefTypes DefinePlayerPref()
            {
                if (overrideType != PlayerPrefTypes.AUTO)
                    return overrideType;

                // Int Check
                else if (PlayerPrefs.GetInt(key, -999) != -999)
                    type = PlayerPrefTypes.INT;

                // String Check
                else if (!string.IsNullOrEmpty(PlayerPrefs.GetString(key, string.Empty)))
                    type = PlayerPrefTypes.STRING;

                return type;
            }
            #endregion
        }
        #endregion
        //=========================================================================================
        /// <summary>
        /// The script of which to load all the variables as keys for PlayerPrefs
        /// </summary>
        [SerializeField]
        private MonoScript playerPrefsScript;
        /// <summary>
        /// List of all the PlayerPrefReferences which are used to generate the visual view elements
        /// </summary>
        [SerializeField]
        private List<PlayerPrefReference> playerPrefs = new List<PlayerPrefReference>();
        /// <summary>
        /// A list of all manual keys we have added
        /// </summary>
        [SerializeField]
        private List<string> manualKeys = new List<string>();
        /// <summary>
        /// Save the scrollposition 
        /// </summary>
        [SerializeField]
        private Vector2 scrollView;
        /// <summary>
        /// Whether we should show the <see cref="PlayerPrefReference.constant"/>,
        /// which is the reference to the variable in code
        /// </summary>
        [SerializeField]
        private bool showConst = true;
        /// <summary>
        /// Whether we should show the <see cref="PlayerPrefReference.key"/>,
        /// which is the name of the key used for saving the PlayerPref
        /// </summary>
        [SerializeField]
        private bool showKeys = true;
        /// <summary>
        /// The key we are currently editing
        /// </summary>
        [SerializeField]
        private string editKey = string.Empty;
        /// <summary>
        /// The size of the label fields
        /// </summary>
        private const int labelHeight = 20;
        /// <summary>
        /// The field with the string of the manual key to add
        /// </summary>
        [SerializeField]
        private string addKeyField;
        /// <summary>
        /// The current toolbar that is selected (the tab of the view)
        /// </summary>
        [SerializeField]
        private int toolbarSelection;
        /// <summary>
        /// The input on which we filter the playerPrefs
        /// </summary>
        private string filterTextInput;
        /// <summary>
        /// The sorting type we have selected
        /// </summary>
        private int selectedSort = 0;
        /// <summary>
        /// All possibilities to sort by
        /// </summary>
        private enum SortingTypes { NONE, CONST_AZ, CONST_ZA, KEY_AZ, KEY_ZA };
        /// <summary>
        /// List of all names of the sorting types we have
        /// </summary>
        private string[] sortingTypes;
     
        /// <summary>
        /// Menu item creation method
        /// </summary>
        [MenuItem("Window/PlayerPrefsEditor")]
        private static void Init()
        {
            PlayerPrefViewEditor window = (PlayerPrefViewEditor)GetWindow(typeof(PlayerPrefViewEditor), false, "PlayerPrefs View");
            window.Show();
        }
        /// <summary>
        /// On awake we refresh the list of the PlayerPrefs with the provided script
        /// </summary>
        private void Awake()
        {
            if (playerPrefsScript != null)
                GetPlayerPrefsFromFile();

            Array array = Enum.GetValues(typeof(SortingTypes));
            string[] tabs = new string[array.Length];
            for (int i = 0; i < array.Length; i++)
                tabs[i] = ((SortingTypes)i).ToString();

            sortingTypes = tabs;
        }
        /// <summary>
        /// OnGUI method, draws the editor window field
        /// </summary>
        public void OnGUI()
        {
            // The Script input field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("PlayerPref Keys Script");
            playerPrefsScript = EditorGUILayout.ObjectField(playerPrefsScript, typeof(MonoScript), false) as MonoScript;
            EditorGUILayout.EndHorizontal(); 
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // The tab selection (view tabs)
            GUIContent[] content = new GUIContent[]
                {
                    new GUIContent("ALL"),
                    new GUIContent("WATCH")
                };
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, content);

            // The row names & options to enable/disable them
            EditorGUILayout.BeginHorizontal();
            showConst = EditorGUILayout.Toggle(showConst, GUILayout.Width(20));
            EditorGUILayout.LabelField("CONST", GUILayout.Height(labelHeight));
            showKeys = EditorGUILayout.Toggle(showKeys, GUILayout.Width(20));
            EditorGUILayout.LabelField("KEY", GUILayout.Height(labelHeight));          
            EditorGUILayout.LabelField("VALUE", GUILayout.Height(labelHeight));
            EditorGUILayout.EndHorizontal();

            if (toolbarSelection != 0)
                GUI.enabled = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter Keys By:", GUILayout.Width(90));
            filterTextInput = EditorGUILayout.TextField(filterTextInput);
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sort By:");
            selectedSort = GUILayout.Toolbar(selectedSort, sortingTypes, "Button");
            EditorGUILayout.EndHorizontal();

            // Create the elements 
            GUIStyle watchedButtonStyle = new GUIStyle(EditorStyles.miniButtonRight);
            scrollView = EditorGUILayout.BeginScrollView(scrollView, EditorStyles.helpBox, GUILayout.Height(position.height * 0.65f));
            List<PlayerPrefReference> currentPrefs = toolbarSelection == 0 ? playerPrefs : playerPrefs.Where(x => x.IsWatched).ToList();

            switch ((SortingTypes)selectedSort)
            {
                default:
                case SortingTypes.NONE:
                    break;
                case SortingTypes.CONST_AZ:
                    currentPrefs = currentPrefs.OrderBy(x => x.constant).ToList();
                    break;
                case SortingTypes.CONST_ZA:
                    currentPrefs = currentPrefs.OrderByDescending(x => x.constant).ToList();
                    break;
                case SortingTypes.KEY_AZ:
                    currentPrefs = currentPrefs.OrderBy(x => x.key).ToList();
                    break;
                case SortingTypes.KEY_ZA:
                    currentPrefs = currentPrefs.OrderByDescending(x => x.key).ToList();
                    break;
            }

            for (int i = 0; i < currentPrefs.Count; i++)
            {
                PlayerPrefReference pref = currentPrefs[i];
                if (!string.IsNullOrWhiteSpace(filterTextInput) && toolbarSelection == 0)
                {
                    if (!pref.key.ToLower().Contains(filterTextInput.ToLower())
                        && !pref.constant.ToLower().Contains(filterTextInput.ToLower()))
                        continue;
                }

                CreatePlayerPrefView(ref pref, watchedButtonStyle);
                currentPrefs[i] = pref;
            }
            EditorGUILayout.EndScrollView();

            // THe menu for addign a new Manual Key
            EditorGUILayout.HelpBox(new GUIContent("Enter the 'key' name here to track it manually, this is case sensitive"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Track key", GUILayout.Width(75));
            addKeyField = EditorGUILayout.TextField(addKeyField);
            if (GUILayout.Button("Add", GUILayout.Width(75)) && !string.IsNullOrWhiteSpace(addKeyField))
            {
                manualKeys.Add(addKeyField);
                AddPlayerPrefKey(addKeyField);
                addKeyField = string.Empty;
            }
            EditorGUILayout.EndHorizontal();

            // The button to refresh the current PlayerPref list
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
                GetPlayerPrefsFromFile();

            // The button to delete all added Manual Keys
            if (GUILayout.Button("Delete Manual Keys"))
            {
                foreach (string key in manualKeys)
                {
                    PlayerPrefReference pref = playerPrefs.FirstOrDefault(x => x.key == key && x.IsManual);
                    if (pref == null)
                        continue;

                    playerPrefs.Remove(pref);
                }
                manualKeys.Clear();
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                Repaint();
        }
        /// <summary>
        /// Create a PlayerPrefView in the scrollview
        /// </summary>
        /// <param name="pref">The PlayerPrefReference to create the view for</param>
        /// <param name="watchedButtonStyle">The style to use for the watch style</param>
        private void CreatePlayerPrefView(ref PlayerPrefReference pref, GUIStyle watchedButtonStyle)
        {
            EditorGUILayout.BeginHorizontal();

            // Create the watch button based on IsWatched state
            Color current = GUI.color;
            if (pref.IsWatched)
                GUI.color = Color.red;
            else
                GUI.color = Color.green;
            if (GUILayout.Button(pref.IsWatched ? "+" : "-", watchedButtonStyle, GUILayout.Width(20)))
                pref.IsWatched = !pref.IsWatched;
            GUI.color = current;

            // Create visual elements for Key & Constant value
            EditorGUILayout.SelectableLabel(showConst ? pref.constant : string.Empty, GUILayout.Height(labelHeight));
            EditorGUILayout.SelectableLabel(showKeys ? pref.key : string.Empty, GUILayout.Height(labelHeight));

            // Create the field for the PlayerPrefReference.value
            pref.GetPlayerPrefValue();
            GetLayoutField(pref);

            // Allow the changing of the type of variable the playerpref is
            PlayerPrefTypes selectedType = pref.overrideType != PlayerPrefTypes.AUTO ? pref.overrideType : pref.type;
            selectedType = (PlayerPrefReference.PlayerPrefTypes)EditorGUILayout.EnumPopup(selectedType, GUILayout.Width(55));
            if ((pref.type != selectedType && pref.overrideType == PlayerPrefTypes.AUTO) 
                || (pref.overrideType != selectedType && pref.overrideType != PlayerPrefTypes.AUTO))
                pref.overrideType = selectedType;

            // If the key is manually added we add the option to remove it from the view
            if (pref.IsManual)
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    playerPrefs.Remove(pref);
                    return;
                }

            // Add the option to edit the key, making it an interactable field
            if (GUILayout.Button("Edit", GUILayout.Width(35)))
            {
                GUI.FocusControl("");
                if (editKey == pref.key)
                    editKey = string.Empty;
                else
                    editKey = pref.key;
            }

            EditorGUILayout.EndHorizontal();
        }
        /// <summary>
        /// Get the layotu field associated with the provided PlayerPrefReference
        /// </summary>
        /// <param name="reference">The PlayerPrefReference to create a field for</param>
        private void GetLayoutField(PlayerPrefReference reference)
        {
            // If this is the field we have selected create an editable field
            if (editKey == reference.key)
            {
                string value = reference.GetPlayerPrefValue();
                switch (reference.DefinePlayerPref())
                {
                    case PlayerPrefTypes.STRING:
                        reference.SetPlayerPref(EditorGUILayout.TextField(value, GUILayout.Height(labelHeight)));
                        break;
                    case PlayerPrefTypes.INT:
                        reference.SetPlayerPref(EditorGUILayout.IntField(int.Parse(value), GUILayout.Height(labelHeight)).ToString());
                        break;
                    case PlayerPrefTypes.BOOL:
                        reference.SetPlayerPref((string)EditorGUILayout.Toggle(bool.Parse(value), GUILayout.Height(labelHeight)).ToString());
                        break;
                    default:
                        reference.SetPlayerPref(EditorGUILayout.TextField(value, GUILayout.Height(labelHeight)));
                        break;
                }
            }
            else
                EditorGUILayout.SelectableLabel(reference.GetPlayerPrefValue(), GUILayout.Height(labelHeight));
        }

        /// <summary>
        /// Add a new PlayerPrefKey manually
        /// </summary>
        /// <param name="keyName">The name of the key to add</param>
        private void AddPlayerPrefKey(string keyName)
        {
            PlayerPrefReference newPlayerPref = new PlayerPrefReference("<-MANUAL->", keyName);
            newPlayerPref.IsManual = true;

            if (toolbarSelection == 1)
                newPlayerPref.IsWatched = true;

            playerPrefs.Add(newPlayerPref);

        }
        /// <summary>
        /// Load the PlayerPrefs from provided script using Reflection
        /// </summary>
        private void GetPlayerPrefsFromFile()
        {
            if (playerPrefsScript == null)
                return;

            // The flags of variables we want to load
            BindingFlags bindingFlags =
                               BindingFlags.Public |
                               BindingFlags.NonPublic |
                               BindingFlags.Static;

            Type script = playerPrefsScript.GetClass();
            playerPrefs.Clear();
            foreach (FieldInfo field in script.GetFields(bindingFlags))
            {
                if (field.FieldType != typeof(string))
                    continue;

                playerPrefs.Add(new PlayerPrefReference(field.Name, (string)field.GetRawConstantValue()));
            }

            // Additionaly to the loaded variables also add the PlayerPrefs from the manual list
            foreach (string key in manualKeys)
                AddPlayerPrefKey(key);
        }
    }   
}
#endif