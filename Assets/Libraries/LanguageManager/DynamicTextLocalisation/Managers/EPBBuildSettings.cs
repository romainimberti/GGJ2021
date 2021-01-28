using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
*	Author  - Edward Smart
*	Date	- 17.03.20
*/

namespace nl.DTT.Build
{
    /// <summary>
    /// list of APIS we can use
    /// </summary>
    public enum API
    {
        staging,
        live
    }


    ///<summary>
    ///Build settings other classes should be getting their values from.
    ///</summary>
    public class EPBBuildSettings : MonoBehaviour, ISerializationCallbackReceiver
    {
        #region Variables
        #region Enum
        /// <summary>
        /// types of builds available to us.
        /// </summary>
        public enum BUILD_TYPE
        {
            Development,
            Testing,
            Live
        }
        #endregion
        #region Editor
        /// <summary>
        /// used to set the static value _buildType
        /// </summary>
        [SerializeField]
        private BUILD_TYPE build_type;



        #endregion
        #region Public
        /// <summary>
        /// Get which API we should be using
        /// </summary>
        public static API CurrentAPI => BuildType == BUILD_TYPE.Development ? API.staging : API.live;

        /// <summary>
        /// If we should show a debug menu.
        /// </summary>
        public static bool ShowDebugMenu => BuildType == BUILD_TYPE.Live ? false : true;


        /// <summary>
        /// If analytics are enabled and affiliate is set to production for this build.
        /// </summary>
        public static bool AnalyticsAndAffiliateEnabled => BuildType == BUILD_TYPE.Live ? true : false;
        #endregion
        #region Private
        /// <summary>
        /// The current build type, NEVER get this, instead make a getter
        /// for exactly what you need, so other classes/methods use it in the
        /// same way.
        /// </summary>
        public static BUILD_TYPE BuildType { get; private set; }
        #endregion
        #endregion
        #region Methods
        #region Unity

        #endregion
        #region Public

        #endregion
        #region Protected

        #endregion
        #region Private

        #endregion
        #endregion
        /// <summary>
        /// move a serialised value to a static value
        /// </summary>
        public void OnAfterDeserialize()
        {
            BuildType = build_type; 
        }

        /// <summary>
        /// unused.
        /// </summary>
        public void OnBeforeSerialize()
        {
            
        }
    }
}
