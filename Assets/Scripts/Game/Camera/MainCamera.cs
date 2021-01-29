using UnityEngine;

namespace com.romainimberti.ggj2021.game.camera
{

	///<summary>
	///Class that handles the main camera
	///</summary>
    [RequireComponent(typeof(Camera))]
	public class MainCamera : MonoBehaviour
	{
        #region Variables
        #region Editor

        /// <summary>
        /// The camera shake component
        /// </summary>
        [SerializeField]
        private CameraShake cameraShake;

        [SerializeField]
        private CameraSize cameraSize;

        [SerializeField]
        private CameraFollow cameraFollow;

        #endregion
        #region Public

        /// <summary>
        /// The MainCamera instance
        /// </summary>
        public static MainCamera Instance { get; private set; }

        /// <summary>
        /// The main camera
        /// </summary>
        public Camera Camera { get; private set; }

        public CameraSize CameraSize => cameraSize;
        public CameraShake CameraShake => cameraShake;
        public CameraFollow CameraFollow=> cameraFollow;

        #endregion
        #endregion
        #region Methods
        #region Unity

        protected void Awake()
        {
            Instance = this;

            Camera = GetComponent<Camera>();
        }

        #endregion
        #endregion
    }
}
