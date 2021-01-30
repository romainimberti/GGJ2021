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
        private float range = 5f;

        #endregion
        #region Public

        #endregion
        #region Private
        [SerializeField]
        private float movementSpeed = 0;
        Vector3 movementDirection = new Vector3(0, 0, 0);
        Vector3 position = new Vector3(0, 0, 0);

        private bool playerInRange = true;
        private Vector3 playerLastPosition = new Vector3Int(0, 0, 0);

        private LTDescr animationFade = null;

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
            position = transform.position;
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
        }

        private void Fade(bool animate = true)
        {
            Vector3 fromPosition = transform.position;
            Vector3 toPosition = GameManager.Instance.Player.transform.position;
            Vector3 direction = toPosition - fromPosition;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range);

            Debug.DrawRay(transform.position, direction * range, Color.red);

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
            /*
            Color c = spriteRenderer.color;
            float target = playerInRange ? 1 : 0;
            if (playerInRange != previousInRange)
            {
                if (animationFade != null)
                {
                    LeanTween.cancel(animationFade.id);
                }
                animationFade = LeanTween.value(c.a, target, animate ? 0.15f : 0f).setEaseInOutQuad().setOnUpdate((float f) =>
                {
                    c.a = f;
                    spriteRenderer.color = c;
                }).setOnComplete(() =>
                {
                    animationFade = null;
                });
            }*/
            spriteRenderer.enabled = playerInRange;

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

        #endregion
        #region Protected

        #endregion
        #region Private

        #endregion
        #endregion
    }
}
