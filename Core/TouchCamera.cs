using UnityEngine;

namespace OpenTouch
{
    public class TouchCamera : MonoBehaviour
    {
        [SerializeField] private Camera sceneCamera;
        public Camera touchCamera
        {
            get
            {
                if (sceneCamera == null) sceneCamera = GetComponent<Camera>();
                return sceneCamera;
            }
            set
            {
                sceneCamera = value;
            }
        }
        private void Awake()
        {
            TouchHelper.SetCamera(touchCamera);
        }
    }
}