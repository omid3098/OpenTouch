using OpenTouch;
using UnityEditor;
using UnityEngine;

public class TouchCameraEditor : Editor
{
    // private static bool installed = false;
    private const string suffixName = " (TouchSupport)";
    static Camera sceneCamera;
    static TouchCamera touchCamera;
    private static TouchManager touchManager;
    // private static string installedKey = "OpenTouchInstallKey";

    [MenuItem("GameObject/OpenTouch/TouchCamera", false, 10)]
    private static void CreateTouchCameraInScene()
    {
        sceneCamera = FindObjectOfType<Camera>();
        if (sceneCamera == null) sceneCamera = new GameObject("TouchCamera").AddComponent<Camera>();
        touchCamera = sceneCamera.GetComponent<TouchCamera>();
        if (touchCamera == null) touchCamera = sceneCamera.gameObject.AddComponent<TouchCamera>();
        touchCamera.touchCamera = sceneCamera;
        if (!sceneCamera.gameObject.name.Contains(suffixName)) sceneCamera.gameObject.name = sceneCamera.gameObject.name + suffixName;
        Selection.activeGameObject = touchCamera.gameObject;
    }

    [MenuItem("GameObject/OpenTouch/TouchManager", false, 11)]
    private static void CreateOpenTouchManager()
    {
        touchManager = FindObjectOfType<TouchManager>();
        if (touchManager == null) touchManager = new GameObject("TouchManager").AddComponent<TouchManager>();
        Selection.activeGameObject = touchManager.gameObject;
    }

    // [MenuItem("Window/OpenTouch/Setup", false, 1)]
    // private static void InstallOpenTouch()
    // {
    //     installed = EditorPrefs.GetBool(installedKey, false);
    //     if (installed) return;
    //     EditorPrefs.SetBool(installedKey, true);
    //     installed = true;
    //     Debug.Log("Updating Open Script execution orders");

    //     MonoScript monoScript = MonoScript.FromMonoBehaviour(touchCamera);
    //     int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
    //     MonoImporter.SetExecutionOrder(monoScript, -250);

    //     monoScript = MonoScript.FromMonoBehaviour(touchManager);
    //     currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
    //     MonoImporter.SetExecutionOrder(monoScript, -251);

    //     Debug.Log("Setup Complete");
    // }

    // [MenuItem("Window/OpenTouch/Remove", false, 2)]
    // private static void RemoveOpenTouch()
    // {
    //     EditorPrefs.DeleteAll();
    //     Debug.Log("Uninstall Complete");
    //     // installed = EditorPrefs.GetBool(installedKey, false);
    //     // if (installed) return;
    //     // EditorPrefs.SetBool(installedKey, true);
    //     // installed = true;
    //     // Debug.Log("Updating Open Script execution orders");

    //     // MonoScript monoScript = MonoScript.FromMonoBehaviour(touchCamera);
    //     // int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
    //     // MonoImporter.SetExecutionOrder(monoScript, -250);

    //     // monoScript = MonoScript.FromMonoBehaviour(touchManager);
    //     // currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
    //     // MonoImporter.SetExecutionOrder(monoScript, -251);
    // }
}
