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

        private bool playerInRange = false;

        #endregion
        #endregion
        #region Methods
        #region Unity

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
                if(hit.collider.CompareTag("Player"))
                {
                    playerInRange = true;
                }
            }

            spriteRenderer.enabled = playerInRange;
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
