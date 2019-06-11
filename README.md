# OpenTouch

### Setup: 
- Right click in Hierarchy, choose OpenTouch->TouchCamera. This will update your scene camera to use as main touch camera.
- Right click in Hierarchy, choose OpenTouch->TouchManager. This will add touch manager to your scene.

### How to use:
- Just subscribe to OpenTouch.TouchManager events (OnMouseDown, OnMouseSet, OnMouseUp, etc.)

```
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
            Debug.Log("Finger Down!");
        }
```

You can check out Finger methods and properties like position, deltaPosition, startPosition, etc.
