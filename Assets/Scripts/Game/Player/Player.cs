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
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        #region Variables
        #region Editor


        [SerializeField]
        private Collider2D colliderObject;

        [SerializeField]
        private Collider2D attackCollider;

        [SerializeField]
        private FloatingJoystick joystick;

        #endregion
        #region Public
        public float speed;

        public List<Sprite> playerSprites;

        #endregion
        #region Private

        private List<Enemy> enemiesInRange;

        private bool enable = false;

        private Rigidbody2D rb;
        private Vector3 lastPosition;

        private bool goingLeft = false;

        private int currentSprite = 0;

        private int spriteTempo = 0;

        private SpriteRenderer playerSpriteRenderer;

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            lastPosition = transform.position;
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
            Physics2D.IgnoreCollision(colliderObject, attackCollider, true);
        }

        private void OnEnable()
        {
            enemiesInRange = new List<Enemy>();
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
            enemiesInRange = new List<Enemy>();
        }

        public void Disable()
        {
            enable = false;
            enemiesInRange = new List<Enemy>();
            rb.velocity = new Vector2Int(0, 0);
        }

        public void EnemyInRange(Enemy enemy, bool inRange)
        {
            if (inRange)
            {
                if (!enemiesInRange.Contains(enemy))
                {
                    enemiesInRange.Add(enemy);
                }
            }
            else
            {
                if (enemiesInRange.Contains(enemy))
                {
                    enemiesInRange.Remove(enemy);
                }
            }

            GameManager.Instance.EnableAttackCapacity(enemiesInRange.Count > 0);
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
                rb.velocity = direction * speed * Time.fixedDeltaTime;

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
