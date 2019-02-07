using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
        public static System.Action<Finger> OnFingerSwipe;
        public static System.Action<Finger> OnFingerGesture;
        static Camera touchCamera;

        // We store last finger id and last ray casted from camera so if we need to calculate hit data from the same 
        // finger, we wont recast a new ray
        private static int cachedFingerID2D = -1;
        private static int cachedFingerID = -1;
        private static RaycastHit2D[] cachedHitInfos2D;
        private static RaycastHit[] cachedHitInfos;

        private void Awake()
        {
            fingers = new List<Finger>();
            expiredIds = new List<int>();
            touchCamera = Camera.main;
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
                finger.position = Input.mousePosition;
                finger.deltaPosition = Vector2.zero;
                finger.tapCount = 1;
                finger.phase = TouchPhase.Began;
                finger.active = true;
                finger.duration = 0;
                if (OnFingerDown != null) OnFingerDown.Invoke(finger);
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
            }
            else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                fingers[0].phase = TouchPhase.Ended;
                if (OnFingerUp != null) OnFingerUp.Invoke(fingers[0]);
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
                            Debug.Log("New mobile touch");
                            int inactiveFinger = fingers.FindIndex(x => x.active == false);
                            if (inactiveFinger != -1)
                                finger = fingers[inactiveFinger];
                            else finger = new Finger();
                            finger.fingerId = touch.fingerId;
                            finger.position = touch.position;
                            finger.deltaPosition = touch.deltaPosition;
                            finger.phase = touch.phase;
                            finger.active = true;
                            if (inactiveFinger == -1) fingers.Add(finger);
                            if (OnFingerDown != null) OnFingerDown.Invoke(finger);
                            break;
                        case TouchPhase.Ended:
                            cachedFingerID = -1;
                            cachedFingerID2D = -1;
                            if (finger != null)
                            {
                                finger.phase = TouchPhase.Ended;
                                if (OnFingerUp != null) OnFingerUp.Invoke(fingers[0]);
                            }
                            break;
                        case TouchPhase.Moved:
                            if (finger != null)
                            {
                                finger.phase = TouchPhase.Moved;
                                if (OnFingerSet != null) OnFingerSet.Invoke(finger);
                            }
                            break;
                        case TouchPhase.Stationary:
                            if (finger != null)
                            {
                                finger.phase = TouchPhase.Stationary;
                                if (OnFingerSet != null) OnFingerSet.Invoke(finger);
                            }
                            break;
                    }
                }
            }
#endif
        }
        private static int GetNewID()
        {
            var id = Random.Range(1000, 9000);
            if (expiredIds.Contains(id)) return GetNewID();
            else expiredIds.Add(id);
            return id;
        }

        #region public methods
        public static bool DidHitCollider(int fingerID, Collider collider, out RaycastHit rayHit)
        {
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
                    cachedHitInfos = Physics.RaycastAll(worldPos, Vector3.forward, float.PositiveInfinity);
                }
            }
            foreach (var _rayHit in cachedHitInfos)
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
        /// Used for checking if collider is hit by finger 
        /// </summary>
        /// <param name="fingerID"> finger ID that you want to check if hit happend with</param>
        /// <param name="collider2D">pass your collider2d to check with finger ID</param>
        /// <param name="rayHit2D">we will out hitinfo back, so you can use it if you want</param>
        /// <returns>returns true if this collider is under finger</returns>
        public static bool DidHitCollider2D(int fingerID, Collider2D collider2D, out RaycastHit2D rayHit2D)
        {
            // if this fingerID is the last one, use the cached raycast hit infor
            // Debug.Log("1- checking hit collider 2D");
            if (cachedFingerID2D != fingerID)
            {
                // Debug.Log("2- looking for finger ID " + fingerID + " - cached: " + cachedFingerID2D);
                Finger finger = null;
                for (int i = 0; i < fingers.Count; i++)
                {
                    if (fingers[i].fingerId == fingerID)
                    {
                        finger = fingers[i];
                        // Debug.Log("3- finger found");
                        break;
                    }
                }
                if (finger != null)
                {
                    cachedFingerID2D = finger.fingerId;
                    Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                    // var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // go.transform.position = new Vector3(worldPos.x, worldPos.y, 0);

                    cachedHitInfos2D = Physics2D.RaycastAll(worldPos, Vector3.forward, float.PositiveInfinity);
                    // Debug.DrawRay(worldPos, new Vector3(worldPos.x, worldPos.y, 20), Color.green, 100.0f);
                    // Debug.Log("4- casting forward ray from " + worldPos + " with " + cachedHitInfos2D.Length + " hits.");
                }
            }
            foreach (var _rayHit2D in cachedHitInfos2D)
            {
                // Debug.Log("x- comparing with " + _rayHit2D.collider.name);
                if (_rayHit2D.collider == collider2D)
                {
                    rayHit2D = _rayHit2D;
                    return true;
                }
            }
            rayHit2D = new RaycastHit2D();
            return false;
        }
        #endregion
    }
}