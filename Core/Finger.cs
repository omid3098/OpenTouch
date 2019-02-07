using UnityEngine;

namespace OpenTouch
{
    public class Finger
    {
        // Summary:
        //     The unique index for the touch.
        public int fingerId { get; set; }
        // Summary:
        //     The position of the touch in pixel coordinates.
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