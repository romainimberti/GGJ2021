using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.romainimberti.ggj2021.utilities
{

    ///<summary>
    /// Class to handle a sprite cycle
    ///</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteCycle : MonoBehaviour
    {
        #region Variables
        #region Editor
        /// <summary>
        /// The list of sprites in order of animation
        /// </summary>
        [SerializeField]
        [Tooltip("The list of sprites in order of animation")]
        private Sprite[] spritesToCycle;

        /// <summary>
        /// Inactive sprite
        /// </summary>
        [SerializeField]
        [Tooltip("Inactive sprite")]
        private Sprite inactiveSprite;

        /// <summary>
        /// Time to wait between each animation frame
        /// </summary>
        [SerializeField]
        [Tooltip("Time to wait between each animation frame")]
        private float secondsBetweenFrame = 0.2f;

        /// <summary>
        /// Whether we should start animating or not
        /// </summary>
        [SerializeField]
        [Tooltip("Whether to start animating or not")]
        private bool autoAnimate = true;

        /// <summary>
        /// Should we bounce through the sprites? (otherwise loop)
        /// </summary>
        [SerializeField]
        [Tooltip("Should we bounce through the sprites? (otherwise loop)")]
        private bool bounce = true;

        /// <summary>
        /// Should we cycle through the sprites?
        /// </summary>
        [SerializeField]
        [Tooltip("Should we cycle through the sprites?")]
        private bool cycle = true;

        #endregion
        #region Public
        /// <summary>
        /// Sets whether the sprite should be animating
        /// </summary>
        public bool Animating
        {
            set
            {
                StopAllCoroutines();
                if(currentAnimation != null)
                    StopCoroutine(currentAnimation);

                if(spriteRenderer == null)
                    spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                if(value)
                {
                    currentAnimation = AnimateSprites();
                    StartCoroutine(currentAnimation);
                }
                else if(cycle)
                    spriteRenderer.sprite = inactiveSprite;
            }
        }

        /// <summary>
        /// The time it takes for one cycle
        /// </summary>
        public float CycleTime => spritesToCycle.Length * secondsBetweenFrame;

        public int FrameCount => spritesToCycle.Length;

        /// <summary>
        /// Getter for the sprite renderer
        /// </summary>
        public SpriteRenderer SpriteRenderer
        {
            get
            {
                if (spriteRenderer == null)
                    spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                return spriteRenderer;
            }
        }

        #endregion
        #region Private
        /// <summary>
        /// Current frame we are on
        /// </summary>
        private int currentSprite;

        /// <summary>
        /// Reference to the SpriteRenderer component
        /// </summary>
        private SpriteRenderer spriteRenderer;

        /// <summary>
        /// The current coroutine animating the sprites
        /// </summary>
        private IEnumerator currentAnimation;

        /// <summary>
        /// Used when bouncing to hold if we are currently incrementing
        /// </summary>
        private int increment = 1;
        #endregion
        #endregion
        #region Methods
        #region Unity

        /// <summary>
        /// Start animating when enabled
        /// </summary>
        private void OnEnable() => Animating = autoAnimate;

        #endregion
        #region Private

        /// <summary>
        /// Iterate throught the sprites based on the animators properties.
        /// </summary>
        private IEnumerator AnimateSprites()
        {
            spriteRenderer.sprite = spritesToCycle[currentSprite];
            yield return new WaitForSeconds(secondsBetweenFrame);
            currentSprite = (currentSprite + increment) % spritesToCycle.Length;
            //Check if we are bouncing
            if((currentSprite == spritesToCycle.Length - 1 || currentSprite == 0))
            {
                if(!cycle)
                {
                    Animating = false;
                    spriteRenderer.sprite = spritesToCycle[spritesToCycle.Length - 1];
                    yield break;
                }

                if(bounce)
                    increment = -increment;
            }
            StartCoroutine(AnimateSprites());
        }

        #endregion
        #endregion
    }
}