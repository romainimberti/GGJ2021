using com.romainimberti.ggj2020.game.maze;
using com.romainimberti.ggj2021.game;
using com.romainimberti.ggj2021.utilities;
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
        [Range(0, 20f)]
        private float attackRange = 3f;

        [SerializeField]
        private float minStepTime = 0.3f, maxStepTime = 0.55f;

        #endregion
        #region Public
        public float speed;

        [HideInInspector]
        public FloatingJoystick joystick;

        public List<Sprite> playerSprites;

        #endregion
        #region Private

        public List<Enemy> enemiesInRange;

        private bool enable = false;

        private Rigidbody2D rb;
        private Vector3 lastPosition;

        private bool goingLeft = false;

        public int currentSprite = 0;

        private int spriteTempo = 0;

        private SpriteRenderer playerSpriteRenderer;

        private float lastStepTime = 0f;
        private float timeToWaitForStep = 0f;

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void Awake()
        {
            currentSprite = GameManager.Instance.level > 2 ? 2 : 0;
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

            if (collision.gameObject.tag == "Start")
            {
                Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
                Debug.Log("Ignoring");
            }
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

            float distance = GetDistance(enemy.transform, transform);
            if (distance > attackRange)
            {
                return;
            }

            if (inRange)
            {
                if (!enemiesInRange.Contains(enemy))
                {
                    enemiesInRange.Add(enemy);

                    int clip = Random.Range(0, 3);
                    switch (clip)
                    {
                        case 0:
                            AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SpiderAttack1);
                            break;
                        case 1:
                            AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SpiderAttack2);
                            break;
                        case 2:
                            AudioManager.Instance.PlayAudioClip(AudioManager.SFX.SpiderAttack3);
                            break;
                    }
                }
            }
            else
            {
                if (enemiesInRange.Contains(enemy))
                {
                    enemiesInRange.Remove(enemy);
                }
            }

            AudioManager.Instance.Mute(enemiesInRange.Count <= 0, AudioManager.CHANNEL.SECOND_MUSIC);

            //GameManager.Instance.EnableAttackCapacity(enemiesInRange.Count > 0);
        }

        #endregion
        #region Protected

        #endregion
        #region Private

        private static float GetDistance(Transform lhs, Transform rhs)
        {
            return Mathf.Abs(rhs.position.x - lhs.position.x) + Mathf.Abs(rhs.position.y - lhs.position.y);
        }

        private void FixedUpdate()
        {
            if (enable)
            {
                Vector3 direction = Vector3.up * joystick.Vertical + Vector3.right * joystick.Horizontal;
                rb.velocity = direction * speed * Time.fixedDeltaTime;

                if (rb.velocity.x != 0 || rb.velocity.y != 0)
                {
                    if (Time.time >= lastStepTime + timeToWaitForStep)
                    {
                        int sound = Random.Range(0, 5);
                        lastStepTime = Time.time;
                        timeToWaitForStep = Random.Range(minStepTime, maxStepTime);
                        switch (sound)
                        {
                            case 0:
                                AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Step1);
                                break;
                            case 1:
                                AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Step2);
                                break;
                            case 2:
                                AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Step3);
                                break;
                            case 3:
                                AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Step4);
                                break;
                            case 4:
                                AudioManager.Instance.PlayAudioClip(AudioManager.SFX.Step5);
                                break;
                        }
                    }
                }

                gameObject.GetComponent<Rigidbody2D>().velocity = direction * speed * Time.fixedDeltaTime;


                if (goingLeft)
                {
                    if (direction.x > 0)
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
                        if (currentSprite % 2 == 0)
                            currentSprite = GameManager.Instance.level > 2 ? 2 : 0;
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
