using System;
using OpenTouch;
using UnityEngine;

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
        TouchManager.OnFingerSet += OnFingerSet;
        TouchManager.OnFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        TouchManager.OnFingerDown -= OnFingerDown;
        TouchManager.OnFingerUp -= OnFingerUp;
        TouchManager.OnFingerSet -= OnFingerSet;
    }

    private void OnFingerSet(Finger finger)
    {
        // Debug.Log("OnFingerSet");
        if (myFinger != null) myFinger.position = finger.position;
        // else Debug.Log("my finger is null");
    }

    private void OnFingerDown(Finger finger)
    {
        RaycastHit hit;
        if (OpenTouch.TouchManager.DidHitCollider(finger.fingerId, _collider, out hit))
        {
            touched = true;
            myFinger = finger;
            // Debug.Log("Drag me: " + gameObject.name);
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
            // Debug.Log("touch is " + touched + " - fingerPos: " + myFinger.position + "fingerActivate: " + myFinger.active);
            Vector3 fingerPos = _camera.ScreenToWorldPoint(myFinger.position);
            fingerPos = new Vector3(fingerPos.x, fingerPos.y, transform.position.z);
            transform.position = fingerPos;
        }
    }
}
