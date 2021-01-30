using com.romainimberti.ggj2021.game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2020
{

    ///<summary>
    /// Class that handles an enemy
    ///</summary>
    public class Enemy : MonoBehaviour
    {
        #region Variables
        #region Editor

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private float range = 12f;

        [SerializeField]
        private List<Sprite> enemySprites;

        #endregion
        #region Public

        #endregion
        #region Private
        [SerializeField]
        private float movementSpeed = 0;
        Vector3 movementDirection = new Vector3(1, 0, 0);
        Vector3 position = new Vector3(0, 0, 0);

        private bool playerInRange = true;
        private Vector3 playerLastPosition = new Vector3Int(0, 0, 0);

        private LTDescr animationFade = null;

        private int startingIndexForSprint;

        private bool goingRight = true;

        private int currentSprite = 0;

        private int spriteTempo = 0;

        private SpriteRenderer enemySpriteRenderer;

        private bool alive = true;


        private Vector3 lastPosition;

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

        private void Awake()
        {
            startingIndexForSprint = Random.Range(0, 100) < 50 ? 0 : 3;
            enemySpriteRenderer = GetComponent<SpriteRenderer>();
            enemySpriteRenderer.sprite = enemySprites[startingIndexForSprint];
            position = transform.position;
            lastPosition = position;
            CalculateNewDirection();

            Fade(false);
        }

        private void followPlayer(Vector3 playerPosition)
        {

            Debug.Log("KILL KILL KILL AT " + playerPosition);
            Vector3 fromPosition = transform.position;
            Vector3 toPosition = GameManager.Instance.Player.transform.position;
            Vector3 offset = toPosition - fromPosition;
            movementDirection = new Vector3(Mathf.Clamp(offset.x, -1, 1), Mathf.Clamp(offset.y, -1, 1), Mathf.Clamp(offset.z, -1, 1));
            //movementDirection = ()
        }
        private void FixedUpdate()
        {
            if (alive)
            {
                Fade();

                Vector3 offset = transform.position - position;
                if (offset.x != 0 || offset.y != 0 || playerInRange)
                {
                    position = new Vector3(transform.position.x, transform.position.y, transform.position.z);// code to execute when X is getting bigger
                }
                else
                {
                    CalculateNewDirection();
                }

                gameObject.GetComponent<Rigidbody2D>().velocity = movementDirection * movementSpeed * Time.fixedDeltaTime;

                if (playerInRange) followPlayer(playerLastPosition);

                if (!goingRight)
                {
                    if (movementDirection.x > 0)
                    {
                        goingRight = true;
                        transform.Rotate(0, 180, 0);
                    }
                }
                else
                {
                    if (movementDirection.x < 0)
                    {
                        goingRight = false;
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
                        if(currentSprite == 2 || currentSprite == 5)
                            currentSprite = startingIndexForSprint;
                        enemySpriteRenderer.sprite = enemySprites[currentSprite];
                        lastPosition = transform.position;
                    }
                }
            }
        }

        private void Fade(bool animate = true)
        {
            Vector3 fromPosition = transform.position;
            Vector3 toPosition = GameManager.Instance.Player.transform.position;
            Vector3 direction = toPosition - fromPosition;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range);

            //Debug.DrawRay(transform.position, direction * range, Color.red);

            playerInRange = false;
            if (hit.collider != null)
            {
                if (!hit.collider.CompareTag("Wall") && !hit.collider.CompareTag("Untagged") && !hit.collider.CompareTag("AttackCollider"))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerInRange = true;
                    }
                }
            }

            GameManager.Instance.Player.EnemyInRange(this, playerInRange);

            //spriteRenderer.enabled = playerInRange;

            playerLastPosition = toPosition;
        }

        private void CalculateNewDirection()
        {
            int randomDirection = Random.Range(0, 4);
            switch (randomDirection)
            {
                case 0:
                    if (movementDirection.y == 1.0f) CalculateNewDirection();
                    movementDirection = new Vector3(0, 1.0f, 0);
                    break;
                case 1:
                    if (movementDirection.x == 1.0f) CalculateNewDirection();
                    movementDirection = new Vector3(1.0f, 0, 0);
                    break;
                case 2:
                    if (movementDirection.y == -1.0f) CalculateNewDirection();
                    movementDirection = new Vector3(0, -1.0f, 0);
                    break;
                case 3:
                    if (movementDirection.x == -1.0f) CalculateNewDirection();
                    movementDirection = new Vector3(-1.0f, 0, 0);
                    break;
                default:
                    break;
            }

        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            CalculateNewDirection();

        }
        #endregion
        #region Public

        public void Die()
        {
            Destroy(GetComponent<BoxCollider2D>());
            alive = false;
            spriteRenderer.sprite = enemySprites[startingIndexForSprint + 2];
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        #endregion
        #endregion
    }
}
