using UnityEngine;
using UnityEngine.Events;

namespace OpenTouch.Examples
{
    public class Clickable2D : MonoBehaviour
    {
        [SerializeField] Collider2D _collider2D;
        [SerializeField] UnityEvent myEvent;
        private void Awake()
        {
            if (_collider2D == null) _collider2D = GetComponent<Collider2D>();
            if (myEvent == null) myEvent = new UnityEvent();
        }
        private void OnEnable()
        {
            TouchManager.OnFingerDown += OnFingerDown;
        }

        private void OnDisable()
        {
            TouchManager.OnFingerDown -= OnFingerDown;
        }

        private void OnFingerDown(Finger finger)
        {
            RaycastHit2D hit;
            if (TouchHelper.DidHitAllCollider2D(ref finger, _collider2D, out hit))
            {
                if (myEvent != null) myEvent.Invoke();
            }
        }
    }
}
