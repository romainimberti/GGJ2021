using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021.game.camera
{
    [ExecuteAlways]
	public class CameraSize : MonoBehaviour
	{
        #region Variables

        #region Editor

        [SerializeField]
        private bool maintainWidth = true;

        [SerializeField]
        private float defaultWidth = 4.8f;

        #endregion
        #region Public

        public float Size => defaultWidth;

        #endregion
        #endregion
        #region Methods

        #region Unity

        private void Start()
        {
            //defaultWidth = Camera.main.orthographicSize * Camera.main.aspect;
        }

        private void Update()
        {
            if(maintainWidth)
            {
                Camera.main.orthographicSize = defaultWidth / Camera.main.aspect;
            }
        }

        #endregion

        #endregion
    }
}