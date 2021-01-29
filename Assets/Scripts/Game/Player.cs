using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2020
{

    ///<summary>
    /// Class that handles the player
    ///</summary>
    public class Player : MonoBehaviour
    {
        #region Variables
        #region Editor

        #endregion
        #region Public
        public float speed;
        public FloatingJoystick joystick;

        #endregion
        #region Private

        #endregion
        #endregion
        #region Methods
        #region Unity

        #endregion
        #region Public

        private void Update()
        {
            Vector3 direction = Vector3.up * joystick.Vertical + Vector3.right * joystick.Horizontal;
            gameObject.GetComponent<Rigidbody2D>().velocity = direction * speed * Time.fixedDeltaTime;
        }
        #endregion
        #region Protected

        #endregion
        #region Private

        #endregion
        #endregion
    }
}
