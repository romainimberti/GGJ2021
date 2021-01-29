#if UNITY_EDITOR
using com.romainimberti.ggj2021.utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.romainimberti.ggj2021.editor
{
	public class Tools : EditorWindow
	{

        [MenuItem("Tools/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[DEBUG] Cleared PlayerPrefs.");
        }

	}
}
#endif