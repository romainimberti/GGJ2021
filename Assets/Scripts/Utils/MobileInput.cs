using UnityEngine;

namespace com.romainimberti.ggj2021.utilities
{
    /// <summary>
    /// Class that handles several mobile inputs
    /// </summary>
    public class MobileInput : SingletonBehaviour<MobileInput>
    {
        /// <summary>
        /// Deadzone for a swipe detection
        /// </summary>
        private const float DEADZONE = 75;

        /// <summary>
        /// Local variables
        /// </summary>
        private Vector2 swipeDelta, startTouch;

        /// <summary>
        /// Delegate for the tap event
        /// </summary>
        /// <returns>A delagate</returns>
        public delegate void OnTap();
        /// <summary>
        /// Tap event
        /// </summary>
        public event OnTap TapEvent;

        /// <summary>
        /// Delegate for the swipe event
        /// </summary>
        /// <param name="direction">The swipe direction</param>
        /// <returns>A delegate</returns>
        public delegate void OnSwipe(Enums.Direction direction);
        /// <summary>
        /// Swipe event
        /// </summary>
        public event OnSwipe SwipeEvent;

        /// <summary>
        /// Gets the inputs
        /// </summary>
        private void Update()
        {
            bool tap = false;
            #region Standalone Inputs
            if(Input.GetMouseButtonDown(0))
            {
                tap = true;
                startTouch = Input.mousePosition;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                startTouch = swipeDelta = Vector2.zero;
                return;
            }
            if(Input.GetKeyUp(KeyCode.LeftArrow))
            {
                SwipeEvent?.Invoke(Enums.Direction.Left);
            }else if(Input.GetKeyUp(KeyCode.RightArrow))
            {
                SwipeEvent?.Invoke(Enums.Direction.Right);
                return;
            }
            else if(Input.GetKeyUp(KeyCode.UpArrow))
            {
                SwipeEvent?.Invoke(Enums.Direction.Up);
                return;
            }
            else if(Input.GetKeyUp(KeyCode.DownArrow))
            {
                SwipeEvent?.Invoke(Enums.Direction.Down);
                return;
            }
            else
            {
                if(tap)
                    TapEvent?.Invoke();
            }
            #endregion

            #region Mobile Inputs
            tap = false;
            if(Input.touches.Length != 0)
            {
                if(Input.touches[0].phase == TouchPhase.Began)
                {
                    tap = true;
                    startTouch = Input.mousePosition;
                }
                else if(Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
                {
                    startTouch = swipeDelta = Vector2.zero;
                }
            }
            #endregion

            //Calculate distance
            swipeDelta = Vector2.zero;
            if(startTouch != Vector2.zero)
            {
                //Check with mobile
                if(Input.touches.Length != 0)
                {
                    swipeDelta = Input.touches[0].position - startTouch;
                }
                //Check with standalone
                else if(Input.GetMouseButton(0))
                {
                    swipeDelta = (Vector2)(Input.mousePosition) - startTouch;
                }
            }

            //Check if we're beyong the deadzone
            if(swipeDelta.magnitude > DEADZONE)
            {
                //This is a confirmed swipe
                float x = swipeDelta.x;
                float y = swipeDelta.y;

                if(Mathf.Abs(x) > Mathf.Abs(y))
                {
                    //Left or Right
                    if(x < 0)
                    {//Left
                        SwipeEvent?.Invoke(Enums.Direction.Left);
                    }
                    else
                    {//Right
                        SwipeEvent?.Invoke(Enums.Direction.Right);
                    }
                }
                else
                {
                    //Up or Down
                    if(y < 0)
                    {//Down
                        SwipeEvent?.Invoke(Enums.Direction.Down);
                    }
                    else
                    {//Up
                        SwipeEvent?.Invoke(Enums.Direction.Up);
                    }
                }

                startTouch = swipeDelta = Vector2.zero;
            }
            else
            {
                if(tap)
                    TapEvent?.Invoke();
            }
        }

    }
}