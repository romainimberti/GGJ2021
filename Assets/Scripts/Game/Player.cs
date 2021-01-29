using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2020
{

    ///<summary>
    ///
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

        public void FixedUpdate()
        {
            Vector3 direction = Vector3.up * joystick.Vertical + Vector3.right * joystick.Horizontal;
            gameObject.GetComponent<Rigidbody2D>().AddForce(direction * speed * Time.fixedDeltaTime);
        }
        #endregion
        #region Protected

        #endregion
        #region Private

        #endregion
        #endregion
    }
}
