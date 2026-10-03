using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FPSSceneSetup
{
    [MenuItem("Bacroomstein/Setup FPS Player In Open Scene")]
    private static void SetupFPSPlayer()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Bacroomstein", "Open your level scene first.", "OK");
            return;
        }

        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Camera mainCamera = Camera.main;
        if (mainCamera == null && cameras.Length > 0)
            mainCamera = cameras[0];

        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(player, "Create FPS Player");
        }

        Vector3 cameraWorldPosition = mainCamera.transform.position;
        player.transform.SetParent(null);
        player.transform.position = new Vector3(cameraWorldPosition.x, 0f, cameraWorldPosition.z);
        player.transform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
        player.transform.localScale = Vector3.one;

        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController == null)
            characterController = Undo.AddComponent<CharacterController>(player);
        characterController.height = 2f;
        characterController.radius = 0.35f;
        characterController.center = new Vector3(0f, 1f, 0f);
        characterController.stepOffset = 0.3f;
        characterController.skinWidth = 0.08f;

        FPSController fpsController = player.GetComponent<FPSController>();
        if (fpsController == null)
            fpsController = Undo.AddComponent<FPSController>(player);

        Undo.RecordObject(mainCamera.transform, "Configure FPS Camera");
        mainCamera.transform.SetParent(player.transform, false);
        mainCamera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        mainCamera.transform.localRotation = Quaternion.identity;
        mainCamera.enabled = true;
        mainCamera.tag = "MainCamera";

        SerializedObject serializedFPS = new SerializedObject(fpsController);
        SerializedProperty cameraProperty = serializedFPS.FindProperty("playerCamera");
        if (cameraProperty != null)
        {
            cameraProperty.objectReferenceValue = mainCamera.transform;
            serializedFPS.ApplyModifiedProperties();
        }

        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (camera != mainCamera && camera.gameObject.activeInHierarchy)
                camera.enabled = false;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = player;
        EditorUtility.DisplayDialog(
            "Bacroomstein",
            "FPS Player and camera are configured in the open scene. Save the scene (Ctrl+S) to keep the changes.",
            "OK");
    }
}