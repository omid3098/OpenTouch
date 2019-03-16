using UnityEngine;

namespace OpenTouch
{
    public class TouchHelper
    {
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
        static Camera touchCamera;
        public static void SetCamera(Camera camera)
        {
            TouchHelper.touchCamera = camera;
        }

        /// <summary>
        /// RaycastAll to check if passed collider is hit among them or not? use when you want to click or drag overlapped objects
        /// </summary>
        /// <param name="fingerID"></param>
        /// <param name="collider"></param>
        /// <param name="rayHit"></param>
        /// <returns></returns>
        public static bool DidHitAllCollider(ref Finger finger, Collider collider, out RaycastHit rayHit)
        {
            if (cachedAllFingerID != finger.fingerId)
            {
                cachedAllFingerID = finger.fingerId;
                Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                cachedAllHitInfos = Physics.RaycastAll(worldPos, Vector3.forward, float.PositiveInfinity);
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
        /// <param name="finger"> finger ID that you want to check if hit happend with</param>
        /// <param name="collider2D">pass your collider2d to check with finger ID</param>
        /// <param name="rayHit2D">we will out hitinfo back, so you can use it if you want</param>
        /// <returns>returns true if this collider is under finger</returns>
        public static bool DidHitAllCollider2D(ref Finger finger, Collider2D collider2D, out RaycastHit2D rayHit2D)
        {
            // if this fingerID is the last one, use the cached raycast hit infor
            if (cachedAllFingerID2D != finger.fingerId)
            {
                cachedAllFingerID2D = finger.fingerId;
                Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                cachedAllHitInfos2D = Physics2D.RaycastAll(worldPos, Vector3.forward, float.PositiveInfinity);
                // Debug.DrawRay(worldPos, new Vector3(worldPos.x, worldPos.y, 20), Color.green, 100.0f);
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

        public static bool DidHitCollider2D(ref Finger finger, Collider2D collider2D, out RaycastHit2D rayHit2D)
        {
            // if this fingerID is the last one, use the cached raycast hit infor
            if (cachedFingerID2D != finger.fingerId)
            {
                cachedFingerID2D = finger.fingerId;
                Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                cachedHitInfo2D = Physics2D.Raycast(worldPos, Vector3.forward, float.PositiveInfinity);
                // Debug.DrawRay(worldPos, new Vector3(worldPos.x, worldPos.y, 20), Color.green, 100.0f);
            }
            rayHit2D = cachedHitInfo2D;
            return (cachedHitInfo2D.collider == collider2D);
        }

        public static bool DidHitCollider(ref Finger finger, Collider collider, out RaycastHit rayHit)
        {
            // if this fingerID is the last one, use the cached raycast hit infor
            if (cachedFingerID != finger.fingerId)
            {
                cachedFingerID = finger.fingerId;
                Vector3 worldPos = touchCamera.ScreenToWorldPoint(finger.position);
                Physics.Raycast(worldPos, Vector3.forward, out cachedHitInfo, float.PositiveInfinity);
                // Debug.DrawRay(worldPos, new Vector3(worldPos.x, worldPos.y, 20), Color.green, 100.0f);
            }
            rayHit = cachedHitInfo;
            return (cachedHitInfo.collider == collider);
        }

        public static void ClearCache()
        {

        }
    }
}