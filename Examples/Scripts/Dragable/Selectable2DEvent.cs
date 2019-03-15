using UnityEngine;

namespace OpenTouch.Examples
{
    public class Selectable2DEvent : MonoBehaviour
    {
        [SerializeField] Collider2D _collider;
        [SerializeField] bool deSelectOnRelease = true;
        public FingerEvent onSelect;
        public FingerEvent onDeSelect;
        public bool selected { get; private set; }

        private void Awake()
        {
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (onSelect == null) onSelect = new FingerEvent();
            if (onDeSelect == null) onDeSelect = new FingerEvent();
            selected = false;
        }

        private void OnEnable()
        {
            TouchManager.OnFingerDown += OnFingerDown;
            TouchManager.OnFingerUp += OnFingerUp;
            selected = false;
        }

        private void OnDisable()
        {
            TouchManager.OnFingerDown -= OnFingerDown;
            TouchManager.OnFingerUp -= OnFingerUp;
            selected = false;
        }

        private void OnFingerDown(Finger finger)
        {
            RaycastHit2D hit;
            if (OpenTouch.TouchManager.DidHitCollider2D(finger.fingerId, _collider, out hit))
            {
                if (deSelectOnRelease) selected = !selected;
                else selected = true;
                if (selected) { if (onSelect != null) onSelect.Invoke(finger); }
                else { if (onDeSelect != null) onDeSelect.Invoke(finger); }
            }
        }

        private void OnFingerUp(Finger finger)
        {
            if (deSelectOnRelease)
            {
                if (selected)
                {
                    if (onDeSelect != null) onDeSelect.Invoke(finger);
                    selected = false;
                }
            }
        }
    }
}