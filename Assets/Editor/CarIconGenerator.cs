#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CarIconGenerator
{
    [MenuItem("Tools/Cars/Generate Icons • Three-Quarter")]
    public static void GenerateThreeQuarter()
    {
        var catalog = Selection.activeObject as CarCatalogSO;
        if (!catalog)
        {
            EditorUtility.DisplayDialog("Select CarCatalogSO",
                "Select your CarCatalogSO asset in Project, then run this.",
                "OK");
            return;
        }

        string folder = "Assets/CarIcons";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "CarIcons");

        foreach (var skin in catalog.cars)
        {
            if (!skin || !skin.prefab) continue;

            // 3/4 camera angles (tweak if you like)
            float yawDeg = -35f;     // rotate around Y (left/right)  (- is front-left)
            float pitchDeg = 30f;    // tilt downwards (camera above looking down)
            float fov = 25f;

            var sprite = RenderPrefab3Q(skin.prefab, 512, 512, folder, skin.displayName, yawDeg, pitchDeg, fov);
            skin.icon = sprite;
            EditorUtility.SetDirty(skin);
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", "Generated 3/4 icons and assigned them.", "Great");
    }

    static Sprite RenderPrefab3Q(GameObject prefab, int w, int h, string folder, string name,
                                 float yawDeg, float pitchDeg, float fov)
    {
        // Spawn the prefab in a tiny temporary scene
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        // Bounds of the model (so we size/position camera)
        var bounds = CalcBounds(go);
        float radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
        float height = bounds.extents.y;

        // Camera
        var camGO = new GameObject("__IconCam__");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0f, 0f, 0f, 0f); // fully transparent
        cam.orthographic = false;
        cam.fieldOfView = fov;

        // Position camera at yaw/pitch around the bounds center
        Vector3 dir = Quaternion.Euler(pitchDeg, yawDeg, 0f) * Vector3.forward; // forward is Z+
        float dist = (radius / Mathf.Tan(Mathf.Deg2Rad * (fov * 0.5f))) * 1.2f; // back off a bit
        cam.transform.position = bounds.center - dir * dist + Vector3.up * (height * 0.2f);
        cam.transform.LookAt(bounds.center, Vector3.up);

        // Simple light
        var lightGO = new GameObject("__IconLight__");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.25f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Render
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        cam.targetTexture = null;

        // Cleanup
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camGO);
        Object.DestroyImmediate(lightGO);
        Object.DestroyImmediate(go);

        // Save PNG + import as Sprite
        string safe = MakeSafe(name);
        string path = $"{folder}/{safe}_3q.png";
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Bounds CalcBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        bool started = false;
        Bounds b = new Bounds(root.transform.position, Vector3.one * 0.1f);
        foreach (var r in renderers)
        {
            if (!started) { b = r.bounds; started = true; }
            else b.Encapsulate(r.bounds);
        }
        return b;
    }

    static string MakeSafe(string s)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }
}
#endif
