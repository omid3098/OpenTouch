using UnityEngine;
using UnityEngine.Events;

namespace OpenTouch.Examples
{
    public class Clickable2D : MonoBehaviour
    {
        [SerializeField] Collider2D _collider2D;
        [SerializeField] public UnityEvent onClickEvent;
        protected virtual void Awake()
        {
            if (_collider2D == null) _collider2D = GetComponent<Collider2D>();
            if (onClickEvent == null) onClickEvent = new UnityEvent();
        }
        protected virtual void OnEnable()
        {
            TouchManager.OnFingerDown += OnFingerDown;
        }

        protected virtual void OnDisable()
        {
            TouchManager.OnFingerDown -= OnFingerDown;
        }

        protected virtual void OnFingerDown(Finger finger)
        {
            RaycastHit2D hit;
            if (TouchManager.DidHitAllCollider2D(finger.guid, _collider2D, out hit))
            {
                if (onClickEvent != null) onClickEvent.Invoke();
            }
        }
    }
}
