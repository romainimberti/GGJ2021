using System.Collections;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace com.romainimberti.ggj2021.utilities
{
    /// <summary>
    /// Class to activate vibrations, due to limited vibration options on iOS, only presets of vibrations have been implemented to keep the platforms consistent
    /// </summary>
    public static class VibrationManager
    {
        #region Variables
        /// <summary>
        /// Boolean to keep track of whether haptic feedback is enabled in settings
        /// </summary>
        public static bool VibrationsEnabled;
#if UNITY_ANDROID
#if !UNITY_EDITOR
        /// <summary>
        /// The Unity activity to specify on Android
        /// </summary>
        public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        /// <summary>
        /// The activity currently running on Android
        /// </summary>
        public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        /// <summary>
        /// The vibrating system service
        /// </summary>
        public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
#endif
#elif UNITY_IOS
        /// <summary>
        /// Native boolean to check whether vibrations are supported on the running device
        /// </summary>
        [DllImport ("__Internal")]
        private static extern bool _SupportsVibrations();
        /// <summary>
        /// Native function for generating a short vibration
        /// </summary>
        [DllImport ("__Internal")]
        private static extern void _VibrateShort();
        /// <summary>
        /// Native function for generating a strong vibration
        /// </summary>
        [DllImport ("__Internal")]
        private static extern void _VibrateStrong();
#endif
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Static constructor used to initialize vibrations on playsession
        /// </summary>
        static VibrationManager()
        {
            VibrationsEnabled = VibrationSupported();
        }

        /// <summary>
        /// Default Unity vibration for testing purposes and necessary for auto generating an androidmanifest with vibrate permission
        /// </summary>
        private static void VibrateTest() => Handheld.Vibrate();

        /// <summary>
        /// Vibrate an ANDROID device for the given milliseconds
        /// </summary>
        /// <param name="milliseconds">duration of the vibrate</param>
        private static void Vibrate(long milliseconds = 15, long[] pattern = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(pattern != null)
                vibrator.Call("vibrate", pattern, -1);
            else
                vibrator.Call("vibrate", milliseconds);
#elif UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
        
        /// <summary>
        /// Since ios only supports 2 vibrations, simulate an error vibration in this coroutine
        /// </summary>
        private static IEnumerator VibrationMultipleTimesIOS(int vibrationTimes, float seconds)
        {
            for (int i = 0; i < vibrationTimes; i++)
            {
#if UNITY_IOS
                _VibrateShort();
#endif
                yield return new WaitForSeconds(seconds);
            }
        }
#endregion

#region Public
        public static bool VibrationSupported()
        {
#if UNITY_EDITOR
            return true;
#elif UNITY_ANDROID
            return vibrator.Call<bool>("hasVibrator");
#elif UNITY_IOS
            return _SupportsVibrations();
#endif
        }

        /// <summary>
        /// Initiate a short vibration
        /// </summary>
        public static void VibrateSelect()
        {
            if (!VibrationsEnabled)
                return;
#if !UNITY_EDITOR
#if UNITY_IOS
            _VibrateShort();
#elif UNITY_ANDROID
	        Vibrate(15);
#endif
#endif
        }

        /// <summary>
        /// Initiate a strong vibration
        /// </summary>
        public static void VibrateStrong()
        {
            if (!VibrationsEnabled)
                return;
#if !UNITY_EDITOR
#if UNITY_IOS
            _VibrateStrong();
#elif UNITY_ANDROID
	        Vibrate(25);
#endif
#endif
        }

        /// <summary>
        /// Initiate a 3 short vibrations to signal an error has been made
        /// </summary>
        public static void VibrateError()
        {
            if (!VibrationsEnabled)
                return;
#if !UNITY_EDITOR
#if UNITY_IOS
            CoroutineManager.Instance.StartCoroutine(VibrationMultipleTimesIOS(3,.13f));
#elif UNITY_ANDROID
		    Vibrate(pattern: new long[] { 0, 25, 25, 25, 25, 25 });
#endif
#endif
        }
        #endregion
        #endregion
    }
}