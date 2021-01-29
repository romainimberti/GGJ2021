using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace com.romainimberti.ggj2021.utilities
{
    /// <summary>
    /// Simple class used for starting coroutines such as Wait(time, action) or WaitUntil(predictor, action).
    /// Can be used instead of writing IEnumarator methods. Also useful for inactive scripts.
    /// </summary>
    public class CoroutineManager : SingletonBehaviour<CoroutineManager>
    {
        #region Methods
        #region Public
        /// <summary>
        /// Waits for a certain time before invoking the callback
        /// </summary>
        /// <param name="time">Time to wait</param>
        /// <param name="callback">Callback to be invoked</param>
        public Coroutine Wait(float time, UnityAction callback) => StartCoroutine(WaitCoroutine(time, callback));
        /// <summary>
        /// Waits for a certain time in real time before invoking the callback
        /// </summary>
        /// <param name="time">Time to wait in real time</param>
        /// <param name="callback">Callback to be invoked</param>
        public Coroutine WaitRealtime(float time, UnityAction callback) => StartCoroutine(WaitRealtimeCoroutine(time, callback));
        /// <summary>
        /// Waits for a certain predictor to be true before invoking the callback
        /// </summary>
        /// <param name="predictor">Predictor that needs to be true</param>
        /// <param name="callback">Callback to be invoked</param>
        public void WaitUntil(Func<bool> predictor, UnityAction callback) => StartCoroutine(WaitUntilCoroutine(predictor, callback));
        /// <summary>
        /// Waits for a scene to be loaded before invoking the callback
        /// </summary>
        /// <param name="scene">The scene you wait for</param>
        /// <param name="callback">Callback to be invoked</param>
        public void WaitUntilSceneIsLoaded(string scene, UnityAction callback) => StartCoroutine(WaitUntilSceneIsLoadedCoroutine(scene,callback));
        /// <summary>
        /// Waits for a scene to be unloaded before invoking the callback
        /// </summary>
        /// <param name="scene">The scene you wait for</param>
        /// <param name="callback">Callback to be invoked</param>
        public void WaitUntilSceneIsUnloaded(string scene, UnityAction callback) => StartCoroutine(WaitUntilSceneIsUnloadedCoroutine(scene, callback));
        /// <summary>
        /// Wait a frame before invoking the callback
        /// </summary>
        /// <param name="callback">Callback to be invoked</param>
        public void WaitForEndOfFrame(UnityAction callback) => StartCoroutine(WaitForEndOfFrameCoroutine(callback));
        #endregion
        #region Private
        /// <summary>
        /// The coroutine for Wait()
        /// </summary>
        /// <param name="time">Time to wait</param>
        /// <param name="callback">Callback to be invoked</param>
        private IEnumerator WaitCoroutine(float time, UnityAction callback)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();
        }
        /// <summary>
        /// The coroutine for WaitRealtime()
        /// </summary>
        /// <param name="time">Time to wait in real time</param>
        /// <param name="callback">Callback to be invoked</param>
        private IEnumerator WaitRealtimeCoroutine(float time, UnityAction callback)
        {
            yield return new WaitForSecondsRealtime(time);
            callback?.Invoke();
        }
        /// <summary>
        /// The coroutine for WaitUntil()
        /// </summary>
        /// <param name="predictor">Predictor that needs to be true</param>
        /// <param name="callback">Callback to be invoked</param>
        private IEnumerator WaitUntilCoroutine(Func<bool> predictor, UnityAction callback)
        {
            yield return new WaitUntil(predictor);
            callback?.Invoke();
        }
        /// <summary>
        /// The coroutine for WaitUntilSceneIsLoaded()
        /// </summary>
        /// <param name="scene">The scene you wait for</param>
        /// <param name="callback">What should happen when the scene is loaded</param>
        private IEnumerator WaitUntilSceneIsLoadedCoroutine(string scene, UnityAction callback)
        {
            yield return new WaitUntil(() => SceneManager.GetSceneByName(scene).isLoaded);
            callback?.Invoke();
        }
        /// <summary>
        /// The coroutine for WaitUntilSceneIsLoaded()
        /// </summary>
        /// <param name="scene">The scene you wait for</param>
        /// <param name="callback">What should happen when the scene is loaded</param>
        private IEnumerator WaitUntilSceneIsUnloadedCoroutine(string scene, UnityAction callback)
        {
            yield return new WaitUntil(() => !SceneManager.GetSceneByName(scene).isLoaded);
            callback?.Invoke();
        }
        /// <summary>
        /// The Coroutine for WaitForEndOfFrame()
        /// </summary>
        /// <param name="callback">Callback to be invoked</param>
        private IEnumerator WaitForEndOfFrameCoroutine(UnityAction callback)
        {
            yield return new WaitForEndOfFrame();
            callback?.Invoke();
        }
        #endregion
        #endregion
    }
}
