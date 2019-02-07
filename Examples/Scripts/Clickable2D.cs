using OpenTouch;
using UnityEngine;
public class Clickable2D : MonoBehaviour
{
    [SerializeField] Collider2D _collider2D;
    private void Awake()
    {
        if (_collider2D == null) _collider2D = GetComponent<Collider2D>();
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
        if (TouchManager.DidHitCollider2D(finger.fingerId, _collider2D, out hit))
        {
            Debug.Log("Clicked me: " + gameObject.name);
        }
    }
}
