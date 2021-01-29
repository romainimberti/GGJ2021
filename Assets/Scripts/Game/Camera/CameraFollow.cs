using com.romainimberti.ggj2021.utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021.game.camera
{

	///<summary>
	///Class that handles the camera follow
	///</summary>
	public class CameraFollow : MonoBehaviour
	{
        #region Variables
        #region Editor

        [SerializeField]
        private float offsetY = 3f;

        #endregion
        #region Public

        #endregion
        #endregion
        #region Methods
        #region Unity

        #endregion
        #region Public

        public void Focus(Transform target)
        {
            transform.position = transform.position.With(x: target.position.x, y: target.position.y + offsetY);
        }

        #endregion
        #endregion
    }
}
