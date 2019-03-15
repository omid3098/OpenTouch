using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace OpenTouch
{

    /// <summary>
    /// I think the fastest way to support multiplatform touch between editor/standalone and mobile is to convert 
    /// editor or standalone clicks into touches and use the same functionalities between them.
    /// For performance reasons and reducing calculations we wont consider right and middle click at 
    /// the same time as separate touches. so if you press right click and left click, OpenTouch will 
    /// register only one touch.
    /// </summary>

    public class TouchManager : MonoBehaviour
    {
        // we define dedicated IDs for mouse Buttons so we can easily add or remove them using unity API
        private static List<Finger> fingers;
        private static List<int> expiredIds;
        public static System.Action<Finger> OnFingerDown;
        public static System.Action<Finger> OnFingerUp;
        public static System.Action<Finger> OnFingerSet;
        public static System.Action<Finger> OnFingerTap;
        public static System.Action<Finger, SwipeDirection> OnFingerSwipe;
        public static System.Action<Finger> OnFingerGesture;
        static Camera touchCamera;

        // We store last finger id and last ray casted from camera so if we need to calculate hit data from the same 
        // finger, we wont recast a new ray
        private static int cachedAllFingerID2D = -1;
        private static int cachedAllFingerID = -1;
        private static int cachedFingerID2D = -1;
        private static int cachedFingerID = -1;

        // used for RaycastAll methods
        private static RaycastHit2D[] cachedAllHitInfos2D;
        // used for RaycastAll methods
        private static RaycastHit[] cachedAllHitInfos;
        // used for Raycast method
        private static RaycastHit2D cachedHitInfo2D;
        // used for Raycast method
        private static RaycastHit cachedHitInfo;
        private static TouchManager instance;
        private static bool initialized;
        private float startTapTime;
        private float tapThreashold = 0.2f;
        private const float swipeDuration = 1.4f;
        private const float swipeThreashold = 65f;

        public static void Initialize()
        {
            if (!initialized) instance = new GameObject("OpenTouch").AddComponent<TouchManager>();
        }

        private void Awake()
        {
            initialized = true;
            if (instance == null) instance = this;
            else Destroy(this.gameObject);
            DontDestroyOnLoad(gameObject);
            fingers = new List<Finger>();
            expiredIds = new List<int>();
            touchCamera = Camera.main;
            startTapTime = -1;
        }

        public static void SetCamera(Camera camera)
        {
            touchCamera = camera;
        }

        private void Update()
        {
            // mark all fingers that marked as touchphase.Ended in previous frame as inactive.
            for (int i = 0; i < fingers.Count; i++)
            {
                Finger finger = fingers[i];
                if (finger.phase == TouchPhase.Ended)
                    finger.active = false;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            // Register mouse buttons
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                Finger finger = null;
                if (fingers.Count != 0 && fingers[0].active == false)
                {
                    // activate the old finger and use new values
                    finger = fingers[0];
                }
                else
                {
                    // create a new finger and set its values with mouse posision
                    finger = new Finger();
                    fingers.Add(finger);
                }
                finger.fingerId = GetNewID();
                finger.startPosition = Input.mousePosition;
                finger.position = Input.mousePosition;
                finger.deltaPosition = Vector2.zero;
                finger.tapCount = 1;
                finger.phase = TouchPhase.Began;
                finger.active = true;
                finger.duration = 0;
                if (OnFingerDown != null) OnFingerDown.Invoke(finger);
                startTapTime = Time.realtimeSinceStartup;
            }
            else if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                Assert.IsTrue(fingers.Count != 0, "No click registered yet.");
                var finger = fingers[0];
                finger.deltaPosition = new Vector2(Input.mousePosition.x - finger.position.x, Input.mousePosition.y - finger.position.y);
                finger.position = Input.mousePosition;
                finger.phase = (finger.deltaPosition == Vector2.zero) ? TouchPhase.Stationary : TouchPhase.Moved;
                finger.duration += Time.unscaledDeltaTime;
                if (OnFingerSet != null) OnFingerSet.Invoke(finger);
                if (startTapTime != -1)
                    if (Time.realtimeSinceStartup - startTapTime >= tapThreashold)
                        startTapTime = -1;
            }
            else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                fingers[0].phase = TouchPhase.Ended;
                CheckSwipe(fingers[0]);
                if (OnFingerUp != null) OnFingerUp.Invoke(fingers[0]);

                if (startTapTime != -1)
                    if (Time.realtimeSinceStartup - startTapTime < tapThreashold)
                        if (OnFingerTap != null) OnFingerTap.Invoke(fingers[0]);

                startTapTime = -1;
            }

#elif UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0)
            {
                // Debug.Log("TouchCount > 0");
                foreach (var touch in Input.touches)
                {
                    Finger finger = fingers.Find(x => x.fingerId == touch.fingerId && x.active == true);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            // Debug.Log("New mobile touch");
                            int inactiveFinger = fingers.FindIndex(x => x.active == false);
                            if (inactiveFinger != -1)
                                finger = fingers[inactiveFinger];
                            else finger = new Finger();
                            finger.fingerId = touch.fingerId;
                            finger.position = touch.position;
                            finger.startPosition = touch.position;
                            finger.deltaPosition = touch.deltaPosition;
                            finger.phase = touch.phase;
                            finger.active = true;
                            if (inactiveFinger == -1) fingers.Add(finger);
                            if (OnFingerDown != null) OnFingerDown.Invoke(finger);
                            startTapTime = Time.realtimeSinceStartup;
                            break;
                        case TouchPhase.Ended:
                            cachedFingerID = -1;
                            cachedFingerID2D = -1;
                            if (finger != null)
                            {
                                finger.deltaPosition = touch.position - finger.position;
                                finger.position = touch.position;
                                finger.duration += Time.unscaledDeltaTime;
                                finger.phase = TouchPhase.Ended;
                                if (startTapTime != -1) if (Time.realtimeSinceStartup - startTapTime < tapThreashold) if (OnFingerTap != null) OnFingerTap.Invoke(fingers[0]);
                                CheckSwipe(finger);
                                if (OnFingerUp != null) OnFingerUp.Invoke(finger);
                            }
                            break;
                        case TouchPhase.Moved:
                            if (finger != null)
                            {
                                finger.deltaPosition = touch.position - finger.position;
                                finger.position = touch.position;
                                finger.duration += Time.unscaledDeltaTime;
                                finger.phase = TouchPhase.Moved;
                                if (OnFingerSet != null) OnFingerSet.Invoke(finger);
                                if (startTapTime != -1) if (Time.realtimeSinceStartup - startTapTime >= tapThreashold) startTapTime = -1;
                            }
                            break;
                        case TouchPhase.Stationary:
                            if (finger != null)
                            {
                                finger.deltaPosition = touch.position - finger.position;
                                finger.position = touch.position;
                                finger.duration += Time.unscaledDeltaTime;
                                finger.phase = TouchPhase.Stationary;
                                if (OnFingerSet != null) OnFingerSet.Invoke(finger);
                                if (startTapTime != -1) if (Time.realtimeSinceStartup - startTapTime >= tapThreashold) startTapTime = -1;
                            }
                            break;
                    }
                }
            }
#endif
        }

        private static void CheckSwipe(Finger swipeFinger)
        {
            var swipeDistance = swipeFinger.position - swipeFinger.startPosition;
            float absDistanceX = Mathf.Abs(swipeDistance.x);
            float absDistanceY = Mathf.Abs(swipeDistance.y);
            if (absDistanceX > swipeThreashold || absDistanceY > swipeThreashold)
            {
                if (swipeFinger.duration < swipeDuration)
                {
                    SwipeDirection swipeDir;
                    if (absDistanceX > absDistanceY)
                    {
                        if (swipeDistance.x > 0) swipeDir = SwipeDirection.Right;
                        else swipeDir = SwipeDirection.Left;
                    }
                    else
                    {
                        if (swipeDistance.y > 0) swipeDir = SwipeDirection.Up;
                        else swipeDir = SwipeDirection.Down;
                    }
                    if (OnFingerSwipe != null) OnFingerSwipe.Invoke(swipeFinger, swipeDir);
                    // Debug.Log("!!!_______Swipe_______!!! - " + swipeDistance + " - " + swipeDir);
                }
            }
        }

        private static int GetNewID()
        {
            var id = UnityEngine.Random.Range(1000, 9000);
            if (expiredIds.Contains(id)) return GetNewID();
            else expiredIds.Add(id);
            return id;
        }

        #region public methods

        /// <summary>
        /// RaycastAll to check if passed collider is hit among them or not? use when you want to click or drag overlapped objects
        /// </summary>
        /// <param name="fingerID"></param>
        /// <param name="collider"></param>
        /// <param name="rayHit"></param>
        /// <returns></returns>
        public static bool DidHitAllCollider(int fingerID, Collider collider, out RaycastHit rayHit)
        {
            if (cachedAllFingerID != fingerID)
            {
                Finger finger = null;
                for (int i = 0; i < fingers.Count; i++)
                {
                    if (fingers[i].fingerId == fingerID)
                    {
                        finger = fingers[i];
                        break;
                    }
                }
                if (finger != null)
                {
                    cachedAllFingerID = finger.fingerId;
                    Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                    cachedAllHitInfos = Physics.RaycastAll(worldPos, Vector3.forward, float.PositiveInfinity);
                }
            }
            foreach (var _rayHit in cachedAllHitInfos)
            {
                if (_rayHit.collider == collider)
                {
                    rayHit = _rayHit;
                    return true;
                }
            }
            rayHit = new RaycastHit();
            return false;
        }

        /// <summary>
        /// RaycastAll to check if passed 2D collider is hit among them or not? use when you want to click or drag overlapped objects
        /// </summary>
        /// <param name="fingerID"> finger ID that you want to check if hit happend with</param>
        /// <param name="collider2D">pass your collider2d to check with finger ID</param>
        /// <param name="rayHit2D">we will out hitinfo back, so you can use it if you want</param>
        /// <returns>returns true if this collider is under finger</returns>
        public static bool DidHitAllCollider2D(int fingerID, Collider2D collider2D, out RaycastHit2D rayHit2D)
        {
            // if this fingerID is the last one, use the cached raycast hit infor
            if (cachedAllFingerID2D != fingerID)
            {
                Finger finger = null;
                for (int i = 0; i < fingers.Count; i++)
                {
                    if (fingers[i].fingerId == fingerID)
                    {
                        finger = fingers[i];
                        break;
                    }
                }
                if (finger != null)
                {
                    cachedAllFingerID2D = finger.fingerId;
                    Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                    cachedAllHitInfos2D = Physics2D.RaycastAll(worldPos, Vector3.forward, float.PositiveInfinity);
                    // Debug.DrawRay(worldPos, new Vector3(worldPos.x, worldPos.y, 20), Color.green, 100.0f);
                }
            }
            foreach (var _rayHit2D in cachedAllHitInfos2D)
            {
                if (_rayHit2D.collider == collider2D)
                {
                    rayHit2D = _rayHit2D;
                    return true;
                }
            }
            rayHit2D = new RaycastHit2D();
            return false;
        }

        public static bool DidHitCollider2D(int fingerID, Collider2D collider2D, out RaycastHit2D rayHit2D)
        {
            // if this fingerID is the last one, use the cached raycast hit infor
            if (cachedFingerID2D != fingerID)
            {
                Finger finger = null;
                for (int i = 0; i < fingers.Count; i++)
                {
                    if (fingers[i].fingerId == fingerID)
                    {
                        finger = fingers[i];
                        break;
                    }
                }
                if (finger != null)
                {
                    cachedFingerID2D = finger.fingerId;
                    Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                    cachedHitInfo2D = Physics2D.Raycast(worldPos, Vector3.forward, float.PositiveInfinity);
                    // Debug.DrawRay(worldPos, new Vector3(worldPos.x, worldPos.y, 20), Color.green, 100.0f);
                }
            }
            rayHit2D = cachedHitInfo2D;
            return (cachedHitInfo2D.collider == collider2D);
        }

        public static bool DidHitCollider(int fingerID, Collider collider, out RaycastHit rayHit)
        {
            // if this fingerID is the last one, use the cached raycast hit infor
            if (cachedFingerID != fingerID)
            {
                Finger finger = null;
                for (int i = 0; i < fingers.Count; i++)
                {
                    if (fingers[i].fingerId == fingerID)
                    {
                        finger = fingers[i];
                        break;
                    }
                }
                if (finger != null)
                {
                    cachedFingerID = finger.fingerId;
                    Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                    Physics.Raycast(worldPos, Vector3.forward, out cachedHitInfo, float.PositiveInfinity);
                    // Debug.DrawRay(worldPos, new Vector3(worldPos.x, worldPos.y, 20), Color.green, 100.0f);
                }
            }
            rayHit = cachedHitInfo;
            return (cachedHitInfo.collider == collider);
        }
        #endregion
    }
}