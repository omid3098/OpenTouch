using System;
using UnityEngine;
using UnityEngine.Events;

namespace OpenTouch.Examples
{
    public class Draggable2DEvent : MonoBehaviour
    {
        [SerializeField] Collider2D _collider;
        public FingerEvent onDragStart;
        public FingerEvent onDrag;
        public FingerEvent onDragEnd;
        bool dragging;
        private Finger draggingFinger;

        private void Awake()
        {
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (onDragStart == null) onDragStart = new FingerEvent();
            if (onDrag == null) onDrag = new FingerEvent();
            if (onDragEnd == null) onDragEnd = new FingerEvent();
            dragging = false;
            draggingFinger = null;
        }

        private void OnEnable()
        {
            TouchManager.OnFingerDown += OnFingerDown;
            TouchManager.OnFingerUp += OnFingerUp;
            dragging = false;
        }

        private void OnDisable()
        {
            TouchManager.OnFingerDown -= OnFingerDown;
            TouchManager.OnFingerUp -= OnFingerUp;
            dragging = false;
        }

        private void OnFingerDown(Finger finger)
        {
            RaycastHit2D hit;
            if (OpenTouch.TouchManager.DidHitCollider2D(finger.fingerId, _collider, out hit))
            {
                draggingFinger = finger;
                if (onDragStart != null) onDragStart.Invoke(finger);
                dragging = true;
            }
        }

        private void OnFingerUp(Finger finger)
        {
            if (dragging)
            {
                if (onDragEnd != null) onDragEnd.Invoke(finger);
                draggingFinger = null;
                dragging = false;
            }
        }
        private void Update()
        {
            if (dragging)
            {
                if (draggingFinger != null) if (onDrag != null) onDrag.Invoke(draggingFinger);
            }
        }
    }
}