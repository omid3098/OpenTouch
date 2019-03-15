using UnityEngine;
namespace OpenTouch.Examples
{
    public class Draggable3D : MonoBehaviour
    {
        [SerializeField] Collider _collider;
        [SerializeField] Camera _camera;
        private bool drag;
        private bool touched;
        private Finger myFinger;
        private Vector2 offset;

        private void Awake()
        {
            if (_collider == null) _collider = GetComponent<Collider>();
            if (_camera == null) _camera = Camera.main;
            touched = false;
            myFinger = null;
        }

        private void OnEnable()
        {
            TouchManager.OnFingerDown += OnFingerDown;
            TouchManager.OnFingerUp += OnFingerUp;
        }

        private void OnDisable()
        {
            TouchManager.OnFingerDown -= OnFingerDown;
            TouchManager.OnFingerUp -= OnFingerUp;
        }

        private void OnFingerDown(Finger finger)
        {
            RaycastHit hit;
            if (OpenTouch.TouchManager.DidHitCollider(finger.fingerId, _collider, out hit))
            {
                offset = Vector2.zero;
                touched = true;
                myFinger = finger;
                Debug.Log("Drag me: " + gameObject.name);
            }
        }

        private void OnFingerUp(Finger finger)
        {
            touched = false;
        }

        private void Update()
        {
            if (touched)
            {
                Vector3 fingerPos = _camera.ScreenToWorldPoint(myFinger.position);
                if (offset == Vector2.zero) offset = new Vector2(fingerPos.x - transform.position.x, fingerPos.y - transform.position.y);
                fingerPos = new Vector3(fingerPos.x - offset.x, fingerPos.y - offset.y, transform.position.z);
                transform.position = fingerPos;
            }
        }
    }
}