using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.romainimberti.ggj2021.game.camera
{

	///<summary>
	///Class that handles the camera shake behaviour
	///</summary>
	public class CameraShake : MonoBehaviour
	{
        #region Variables
        #region Enums

        public enum ShakeMagnitude
        {
            Weak,
            Strong
        }

        #endregion
        #region Editor

        /// <summary>
        /// The damping speed
        /// </summary>
        [SerializeField]
        private float dampingSpeed = 1.0f;

        #endregion
        #region Private

        private bool isShaking = false;

        private Dictionary<ShakeMagnitude, float> shakeAmount = new Dictionary<ShakeMagnitude, float>()
        {
            { ShakeMagnitude.Weak, 0.015f},
            { ShakeMagnitude.Strong, 0.07f}
        };

        /// <summary>
        /// The initial position
        /// </summary>
        private Vector3 initialPosition;

        /// <summary>
        /// The remaining shake duration
        /// </summary>
        private float shakeDuration = 0f;

        /// <summary>
        /// The shake magnitude
        /// </summary>
        private float shakeMagnitude;

        #endregion
        #endregion
        #region Methods
        #region Unity

        private void Update()
        {
            if(!isShaking || Time.deltaTime == 0)
                return;

            if(shakeDuration > 0f)
            {
                transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
                shakeDuration -= Time.deltaTime * dampingSpeed;
            }
            else
            {
                shakeDuration = 0f;
                transform.localPosition = initialPosition;
            }
        }

        #endregion
        #region Public

        /// <summary>
        /// Starts shaking
        /// </summary>
        public void Shake(float duration, ShakeMagnitude magnitude = ShakeMagnitude.Weak)
        {
            if(isShaking)
                Stop();

            isShaking = true;
            shakeDuration = duration;
            shakeMagnitude = shakeAmount[magnitude];
            initialPosition = transform.position;
        }

        /// <summary>
        /// Stops shaking
        /// </summary>
        public void Stop()
        {
            if(!isShaking)
                return;

            isShaking = false;
            transform.position = initialPosition;
        }

        #endregion
        #endregion
    }
}
