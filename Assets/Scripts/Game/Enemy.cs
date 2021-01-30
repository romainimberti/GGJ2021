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
        Vector3Int movementDirection = new Vector3Int(0, 0, 0);
        Vector3 position = new Vector3(0, 0, 0);

        private bool playerInRange = false;

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

        }
        private void FixedUpdate()
        {

            Vector3 fromPosition = transform.position;
            Vector3 toPosition = GameManager.Instance.Player.transform.position;
            Vector3 direction = toPosition - fromPosition;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range);

            Debug.DrawRay(transform.position, direction * range, Color.red);

            playerInRange = false;
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    playerInRange = true;
                }
            }

            spriteRenderer.enabled = playerInRange;

            Vector3 offset = transform.position - position;
            if (offset.x != 0 || offset.y != 0)
            {
                position = new Vector3(transform.position.x, transform.position.y, transform.position.z);// code to execute when X is getting bigger
            }
            else
            {
                CalculateNewDirection();
            }

            Vector3 dir = movementDirection;
            gameObject.GetComponent<Rigidbody2D>().velocity = dir * movementSpeed * Time.fixedDeltaTime;
        }


        void CalculateNewDirection()
        {
            int randomDirection = Random.Range(0, 4);
            switch (randomDirection)
            {
                case 0:
                    if (movementDirection.y == 1) CalculateNewDirection();
                    movementDirection = new Vector3Int(0, 1, 0);
                    break;
                case 1:
                    if (movementDirection.x == 1) CalculateNewDirection();
                    movementDirection = new Vector3Int(1, 0, 0);
                    break;
                case 2:
                    if (movementDirection.y == -1) CalculateNewDirection();
                    movementDirection = new Vector3Int(0, -1, 0);
                    break;
                case 3:
                    if (movementDirection.x == -1) CalculateNewDirection();
                    movementDirection = new Vector3Int(-1, 0, 0);
                    break;
                default:
                    break;
            }

        }
        void OnCollisionEnter2D(Collision2D collision)
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
