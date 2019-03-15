using UnityEngine;

namespace OpenTouch.Examples
{
    public class Draggable2D : MonoBehaviour
    {
        [SerializeField] Collider2D _collider;
        [SerializeField] Camera _camera;
        [Tooltip("keep finger offset from center? or move object to the middle of finger position?")]
        private bool touched;
        private Finger myFinger;
        private Vector2 offset;

        private void Awake()
        {
            if (_collider == null) _collider = GetComponent<Collider2D>();
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
            RaycastHit2D hit;
            if (OpenTouch.TouchManager.DidHitCollider2D(finger.fingerId, _collider, out hit))
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