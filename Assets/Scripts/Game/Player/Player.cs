using com.romainimberti.ggj2020.game.maze;
using com.romainimberti.ggj2021.game;
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

        [SerializeField]
        private Collider2D attackCollider;

        #endregion
        #region Public
        public float speed;
        public FloatingJoystick joystick;

        #endregion
        #region Private

        private bool enable = false;

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void Awake()
        {
            joystick = GameObject.Find("Floating Joystick").GetComponent<FloatingJoystick>();
        }
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col != null && col.gameObject != null && col.gameObject != gameObject && col.attachedRigidbody != null && col.attachedRigidbody.gameObject.CompareTag("End"))
            {
                GameManager.Instance.MazeFinished();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameManager.Instance.EnableCapacities((int)collision.transform.position.x, (int)collision.transform.position.y);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            GameManager.Instance.DisableCapacities();
        }

        #endregion
        #region Public

        public void Enable()
        {
            enable = true;
        }

        public void Disable()
        {
            enable = false;
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2Int(0, 0);
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        private void FixedUpdate()
        {
            if (enable)
            {
                Vector3 direction = Vector3.up * joystick.Vertical + Vector3.right * joystick.Horizontal;
                gameObject.GetComponent<Rigidbody2D>().velocity = direction * speed * Time.fixedDeltaTime;
            }
        }

        #endregion
        #endregion
    }
}
