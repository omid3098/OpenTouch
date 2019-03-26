using UnityEngine;
using UnityEngine.Events;

namespace OpenTouch
{
    [System.Serializable] public class FingerEvent : UnityEvent<Finger> { }
    public enum SwipeDirection
    {
        Left,
        Right,
        Up,
        Down,
    }
    [System.Serializable]
    public class Finger
    {
        // Summary:
        //     The unique id for the touch.
        public string guid { get; set; }
        // Summary:
        //     The unique index for the touch.
        public int touchId { get; set; }
        // Summary:
        //     The starting position of the touch in pixel coordinates.
        public Vector2 startPosition { get; set; }
        // Summary:
        //     The current position of the touch in pixel coordinates.
        public Vector2 position { get; set; }
        // Summary:
        //     The position delta since last change.
        public Vector2 deltaPosition { get; set; }
        // Summary:
        //     Number of taps.
        public int tapCount { get; set; }
        // Summary:
        //     Describes the phase of the touch.
        public TouchPhase phase { get; set; }
        // Summary:
        //     Deactive touches to reuse them.
        public bool active { get; set; }
        // Summary:
        //     duration of this finger on screen
        public float duration { get; set; }
    }
}