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
        private Collider2D colliderObject;

        [SerializeField]
        private Collider2D attackCollider;

        #endregion
        #region Public
        public float speed;

        [HideInInspector]
        public FloatingJoystick joystick;

        public List<Sprite> playerSprites;

        #endregion
        #region Private

        private Vector3 lastPosition;

        private bool goingLeft = false;

        private bool enable = false;

        private int currentSprite = 0;

        private int spriteTempo = 0;

        private SpriteRenderer playerSpriteRenderer;

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void Awake()
        {
            lastPosition = transform.position;
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
            Physics2D.IgnoreCollision(colliderObject, attackCollider, true);
        }
        private void OnTriggerEnter2D(Collider2D col)
        {
            GameManager.Instance.MazeFinished();
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

                if (goingLeft)
                {
                    if(direction.x > 0)
                    {
                        goingLeft = false;
                        transform.Rotate(0, 180, 0);
                    }
                }
                else
                {
                    if (direction.x < 0)
                    {
                        goingLeft = true;
                        transform.Rotate(0, 180, 0);
                    }
                }

                if (lastPosition != transform.position)
                {
                    spriteTempo++;

                    if (spriteTempo > 10)
                    {
                        spriteTempo = 0;
                        currentSprite++;
                        if (currentSprite >= playerSprites.Count)
                            currentSprite = 0;
                        playerSpriteRenderer.sprite = playerSprites[currentSprite];
                        lastPosition = transform.position;
                    }
                }
            }
        }

        #endregion
        #endregion
    }
}
