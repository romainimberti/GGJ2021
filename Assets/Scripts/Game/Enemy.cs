using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2020
{

    ///<summary>
    ///
    ///</summary>
    public class Enemy : MonoBehaviour
    {
        #region Variables
        #region Editor

        #endregion
        #region Public

        #endregion
        #region Private
        [SerializeField]
        private float movementSpeed = 0;
        Vector3Int movementDirection = new Vector3Int(0, 0, 0);
        Vector3 position = new Vector3(0, 0, 0);

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
            calculateNewDirection();

        }
        private void FixedUpdate()
        {



            Vector3 offset = transform.position - position;
            if (offset.x != 0 || offset.y != 0)
            {
                position = new Vector3(transform.position.x, transform.position.y, transform.position.z);                                        // code to execute when X is getting bigger
            }
            else
            {
                Debug.Log("Blocked Calculating New Pos ");

                calculateNewDirection();
            }

        }
        private void Update()
        {
            Vector3 direction = movementDirection;
            //Debug.Log("Moving in Direction " + direction);
            gameObject.GetComponent<Rigidbody2D>().velocity = direction * movementSpeed * Time.fixedDeltaTime;



        }

        void calculateNewDirection()
        {
            Debug.Log("Calculating new direction");

            int randomDirection = Random.Range(0, 4);
            switch (randomDirection)
            {
                case 0:
                    if (movementDirection.y == 1) calculateNewDirection();
                    movementDirection = new Vector3Int(0, 1, 0);
                    break;
                case 1:
                    if (movementDirection.x == 1) calculateNewDirection();
                    movementDirection = new Vector3Int(1, 0, 0);
                    break;
                case 2:
                    if (movementDirection.y == -1) calculateNewDirection();
                    movementDirection = new Vector3Int(0, -1, 0);
                    break;
                case 3:
                    if (movementDirection.x == -1) calculateNewDirection();
                    movementDirection = new Vector3Int(-1, 0, 0);
                    break;
                default:
                    Debug.LogError("EUHHH WHAT random was?" + randomDirection);
                    break;
            }
            Debug.Log("New Direction" + randomDirection);

        }
        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collided");
            calculateNewDirection();
            /* foreach (ContactPoint2D contact in collision.contacts)
             {
                 contact.
                 Debug.DrawRay(contact.point, contact.normal, Color.white);

                 calculateNewDirection([0,1]);
             }*/
        }
        #endregion
        #endregion
    }
}
