#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class ColaWeaponScenePreview
{
    private static GameObject pickupPreview;
    private static GameObject heldPreview;
    private static ColaWeaponController current;

    static ColaWeaponScenePreview()
    {
        EditorApplication.update += UpdatePreview;
        Selection.selectionChanged += UpdatePreview;
        AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
    }

    private static void UpdatePreview()
    {
        var selected = Selection.activeGameObject;
        var controller = selected != null ? selected.GetComponent<ColaWeaponController>() : null;
        if (controller == null && selected != null)
            controller = selected.GetComponentInParent<ColaWeaponController>();

        if (controller == null)
        {
            Cleanup();
            return;
        }

        current = controller;
        var so = new SerializedObject(controller);
        var cameraProp = so.FindProperty("playerCamera");
        Camera cam = cameraProp.objectReferenceValue as Camera;
        if (cam == null) cam = controller.GetComponentInChildren<Camera>();
        if (cam == null) return;

        Vector3 pickupWorldPosition = so.FindProperty("pickupWorldPosition").vector3Value;
        float height = so.FindProperty("pickupSpriteHeight").floatValue;
        float scale = so.FindProperty("pickupSpriteScale").floatValue;
        Vector3 pos = pickupWorldPosition;

        Texture2D pickupTexture = Resources.Load<Texture2D>("Weapons/Cola/yerde");
        if (pickupTexture != null)
        {
            EnsurePreview(ref pickupPreview, "Cola Pickup Scene Preview", pickupTexture);
            pickupPreview.transform.position = pos + Vector3.up * height;
            pickupPreview.transform.localScale = Vector3.one * scale;
            Vector3 toCamera = cam.transform.position - pos;
            toCamera.y = 0f;
            if (toCamera.sqrMagnitude > 0.001f)
                pickupPreview.transform.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
        }

        Vector3 heldPos = so.FindProperty("heldSpriteLocalPosition").vector3Value;
        float heldScale = so.FindProperty("heldSpriteScale").floatValue;
        Texture2D heldTexture = Resources.Load<Texture2D>("Weapons/Cola/dik");
        if (heldTexture != null)
        {
            EnsurePreview(ref heldPreview, "Held Cola Scene Preview", heldTexture);
            heldPreview.transform.SetParent(cam.transform, false);
            heldPreview.transform.localPosition = heldPos;
            heldPreview.transform.localRotation = Quaternion.identity;
            heldPreview.transform.localScale = Vector3.one * heldScale;
        }
        SceneView.RepaintAll();
    }

    private static void EnsurePreview(ref GameObject obj, string name, Texture2D texture)
    {
        if (obj == null)
        {
            obj = new GameObject(name);
            obj.hideFlags = HideFlags.HideAndDontSave;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 100;
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);
        }
    }

    private static void Cleanup()
    {
        if (pickupPreview != null) Object.DestroyImmediate(pickupPreview);
        if (heldPreview != null) Object.DestroyImmediate(heldPreview);
        pickupPreview = null;
        heldPreview = null;
        current = null;
    }
}
#endif
