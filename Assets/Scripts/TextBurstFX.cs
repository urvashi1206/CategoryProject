//using System.Collections;
//using UnityEngine;
//using TMPro;

//public class TextBurstFX : MonoBehaviour
//{
//    [Header("Refs")]
//    public TMP_Text tmp;                       // 3D TextMeshPro
//    public ParticleSystem sparkles;            // optional child; PlayOnAwake OFF
//    public ParticleSystem crashBurstPrefab;    // PS_CrashBurst prefab (Mesh shape)

//    [Header("Flow")]
//    public float displayTime = 1.0f;           // show text before crash
//    public float fadeOutTime = 0.15f;          // quick fade as it crashes
//    public float riseSpeed = 1.6f;             // gentle upward drift while visible
//    public bool billboardToCamera = true;      // keep readable
//    public bool yBillboardOnly = true;

//    [Header("Particles")]
//    public int burstCount = 100;               // fallback if prefab ignores
//    public bool inheritTextColor = true;
//    public Color overrideParticleColor = Color.white;

//    Color _baseColor;

//    void Reset()
//    {
//        if (!tmp) tmp = GetComponentInChildren<TMP_Text>();
//    }

//    void Awake()
//    {
//        if (!tmp) tmp = GetComponentInChildren<TMP_Text>();
//        if (tmp) _baseColor = tmp.color;
//    }

//    void LateUpdate()
//    {
//        if (!billboardToCamera) return;
//        var cam = Camera.main; if (!cam) return;

//        if (yBillboardOnly)
//        {
//            Vector3 toCam = cam.transform.position - transform.position;
//            toCam.y = 0f;
//            if (toCam.sqrMagnitude > 0.0001f)
//                transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
//        }
//        else
//        {
//            transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
//        }
//    }

//    public void Init(string text, Color color, float fontSize = 4f)
//    {
//        if (tmp)
//        {
//            tmp.text = text;
//            tmp.color = color;
//            tmp.fontSize = fontSize;
//            _baseColor = color;
//        }

//        // nice idle drift while visible
//        StartCoroutine(Co_Run());
//    }

//    IEnumerator Co_Run()
//    {
//        float t = 0f;

//        // sparkle on show
//        if (sparkles)
//        {
//            var main = sparkles.main;
//            main.simulationSpace = ParticleSystemSimulationSpace.World;
//            sparkles.Clear(true);
//            sparkles.Play(true);
//        }

//        // visible phase
//        while (t < displayTime)
//        {
//            float dt = Time.deltaTime;
//            t += dt;
//            transform.position += new Vector3(0f, riseSpeed * dt, 0f);
//            yield return null;
//        }

//        // CRASH: bake TMP mesh and feed to particle system
//        Mesh crashMesh = BakeTextMesh(); // may be null if no glyphs
//        SpawnCrashParticles(crashMesh);

//        // quick text fade-out
//        if (tmp && fadeOutTime > 0f)
//        {
//            float f = 0f;
//            while (f < fadeOutTime)
//            {
//                f += Time.deltaTime;
//                var c = _baseColor; c.a = 1f - Mathf.Clamp01(f / fadeOutTime);
//                tmp.color = c;
//                yield return null;
//            }
//        }

//        // remove text, let particles live their life
//        if (tmp) tmp.enabled = false;

//        // destroy after a short grace period
//        yield return new WaitForSeconds(1.5f);
//        Destroy(gameObject);
//    }

//    Mesh BakeTextMesh()
//    {
//        if (!tmp) return null;

//        // make sure geometry is up to date
//        tmp.ForceMeshUpdate(true, true);
//        var tInfo = tmp.textInfo;
//        if (tInfo == null || tInfo.characterCount == 0) return null;

//        // Combine all submeshes into one Mesh for the shape
//        Mesh combined = new Mesh { name = "TMP_BakedMesh" };

//        // Usually main text is meshInfo[0], but to be safe combine all
//        int totalVerts = 0, totalTris = 0;
//        foreach (var mi in tInfo.meshInfo)
//        {
//            if (mi.mesh == null) continue;
//            totalVerts += mi.mesh.vertexCount;
//            totalTris += mi.mesh.triangles.Length;
//        }
//        if (totalVerts == 0) return null;

//        var verts = new Vector3[totalVerts];
//        var tris = new int[totalTris];

//        int vOfs = 0, tOfs = 0;
//        foreach (var mi in tInfo.meshInfo)
//        {
//            var m = mi.mesh; if (m == null) continue;

//            var mVerts = m.vertices;
//            var mTris = m.triangles;

//            for (int i = 0; i < mVerts.Length; i++)
//                verts[vOfs + i] = tmp.transform.TransformPoint(mVerts[i]); // world space

//            for (int i = 0; i < mTris.Length; i++)
//                tris[tOfs + i] = vOfs + mTris[i];

//            vOfs += mVerts.Length;
//            tOfs += mTris.Length;
//        }

//        combined.vertices = verts;
//        combined.triangles = tris;
//        combined.RecalculateBounds();
//        combined.RecalculateNormals();
//        return combined;
//    }

//    void SpawnCrashParticles(Mesh shapeMesh)
//    {
//        if (!crashBurstPrefab) return;

//        var ps = Instantiate(crashBurstPrefab, Vector3.zero, Quaternion.identity);
//        ps.transform.position = Vector3.zero; // we'll emit in world space from mesh vertices

//        var main = ps.main;
//        main.simulationSpace = ParticleSystemSimulationSpace.World;
//        if (inheritTextColor) main.startColor = _baseColor; else main.startColor = overrideParticleColor;

//        var emission = ps.emission;
//        // ensure we have a burst even if prefab has none
//        var burst = new ParticleSystem.Burst(0f, (short)burstCount);
//        emission.SetBursts(new[] { burst });

//        var shape = ps.shape;
//        shape.shapeType = ParticleSystemShapeType.Mesh;
//        shape.mesh = shapeMesh;

//        ps.Play();
//        Destroy(ps.gameObject, 2.5f);
//    }

//    // convenience spawner
//    public static TextBurstFX Spawn(TextBurstFX prefab, Vector3 pos, string text, Color color, float size = 4f)
//    {
//        var fx = Instantiate(prefab, pos, Quaternion.identity);
//        fx.Init(text, color, size);
//        return fx;
//    }
//}

using System.Collections;
using UnityEngine;
using TMPro;

public class TextBurstFX : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the 3D TextMeshPro component (NOT TMP UGUI).")]
    public TMP_Text tmp;

    [Tooltip("Particle System prefab used for the crash burst (Mesh shape).")]
    public ParticleSystem crashBurstPrefab;

    [Header("Defaults (used if Init is not called)")]
    public string defaultText = "cha-ching!";
    public Color defaultColor = Color.white;
    public float defaultFontSize = 4f;

    [Header("Display Phase")]
    [Tooltip("How long the text is visible before crashing.")]
    public float displayTime = 0.5f; // you asked for faster crash

    [Tooltip("Upward drift while visible.")]
    public float riseSpeed = 1.6f;

    [Header("Billboard")]
    public bool billboardToCamera = true;
    public bool yBillboardOnly = true;
    public int sortingOrder = 5000; // render on top

    [Header("Crash Particles")]
    [Tooltip("How many debris particles to spawn (if prefab has none).")]
    public int burstCount = 120;

    [Tooltip("Use text color for debris.")]
    public bool inheritTextColor = true;

    public Color overrideParticleColor = Color.white;

    // internal
    private Color _baseColor;
    private bool _inited;

    void Reset()
    {
        if (!tmp) tmp = GetComponentInChildren<TMP_Text>();
    }

    void Awake()
    {
        if (!tmp) tmp = GetComponentInChildren<TMP_Text>();

        // Make sure text renders in front if needed
        if (tmp)
        {
            var mr = tmp.GetComponent<Renderer>();
            if (mr) mr.sortingOrder = sortingOrder;
        }
    }

    void OnEnable()
    {
        // SAFETY: ensure no "SAMPLE" survives even if Init is not called.
        if (tmp)
        {
            if (string.IsNullOrEmpty(tmp.text) || tmp.text == "SAMPLE")
                tmp.text = defaultText;

            // If someone left alpha low, restore it
            var c = tmp.color;
            if (c.a < 0.99f) c.a = 1f;
            if (!_inited) tmp.color = defaultColor;
            if (tmp.fontSize <= 0.1f) tmp.fontSize = defaultFontSize;

            _baseColor = tmp.color;
        }

        StopAllCoroutines();
        StartCoroutine(Co_Run());
    }

    void LateUpdate()
    {
        if (!billboardToCamera) return;
        var cam = Camera.main; if (!cam) return;

        if (yBillboardOnly)
        {
            Vector3 toCam = cam.transform.position - transform.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);
        }
    }

    /// <summary>
    /// Call this right after Instantiate to set text, color, size.
    /// </summary>
    public void Init(string text, Color color, float fontSize = 4f)
    {
        if (tmp)
        {
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = fontSize;
            _baseColor = color;
        }
        _inited = true;
    }

    IEnumerator Co_Run()
    {
        float t = 0f;

        // visible phase
        while (t < displayTime)
        {
            float dt = Time.deltaTime;
            t += dt;
            transform.position += new Vector3(0f, riseSpeed * dt, 0f);
            yield return null;
        }

        // bake TMP mesh -> crash burst
        Mesh crashMesh = BakeTextMesh();
        SpawnCrashParticles(crashMesh);

        // hide text immediately after crash
        if (tmp) tmp.enabled = false;

        // give particles time to live, then clean up
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }

    Mesh BakeTextMesh()
    {
        if (!tmp) return null;
        tmp.ForceMeshUpdate(true, true);

        var tInfo = tmp.textInfo;
        if (tInfo == null || tInfo.characterCount == 0) return null;

        var combined = new Mesh { name = "TMP_BakedMesh" };
        int totalVerts = 0, totalTris = 0;

        for (int i = 0; i < tInfo.meshInfo.Length; i++)
        {
            var mi = tInfo.meshInfo[i];
            if (mi.mesh == null) continue;
            totalVerts += mi.mesh.vertexCount;
            totalTris += mi.mesh.triangles.Length;
        }
        if (totalVerts == 0) return null;

        var verts = new Vector3[totalVerts];
        var tris = new int[totalTris];

        int vOfs = 0, tOfs = 0;
        for (int i = 0; i < tInfo.meshInfo.Length; i++)
        {
            var m = tInfo.meshInfo[i].mesh; if (m == null) continue;

            var mVerts = m.vertices;
            var mTris = m.triangles;

            for (int v = 0; v < mVerts.Length; v++)
                verts[vOfs + v] = tmp.transform.TransformPoint(mVerts[v]); // world-space

            for (int ti = 0; ti < mTris.Length; ti++)
                tris[tOfs + ti] = vOfs + mTris[ti];

            vOfs += mVerts.Length;
            tOfs += mTris.Length;
        }

        combined.vertices = verts;
        combined.triangles = tris;
        combined.RecalculateBounds();
        combined.RecalculateNormals();
        return combined;
    }

    void SpawnCrashParticles(Mesh shapeMesh)
    {
        if (!crashBurstPrefab) return;

    var ps = Instantiate(crashBurstPrefab, Vector3.zero, Quaternion.identity);

    // --- main ---
    var main = ps.main;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.startColor = inheritTextColor ? _baseColor : overrideParticleColor;
    main.startSize  = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 3.4f);

    // --- emission ---
    var emission = ps.emission;
    emission.rateOverTime = 0f;
    emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

    // --- shape (this is the fix) ---
    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Mesh;
    shape.mesh = shapeMesh;
    shape.meshShapeType = ParticleSystemMeshShapeType.Vertex; //
    // Optional distribution mode when emitting from multiple meshes/positions:
    shape.meshSpawnMode = ParticleSystemShapeMultiModeValue.Random; //

    // --- rotation over lifetime ---
    var rot = ps.rotationOverLifetime; rot.enabled = true;
    rot.z = new ParticleSystem.MinMaxCurve(-540f, 540f);

    // --- size over lifetime ---
    var sizeLife = ps.sizeOverLifetime; sizeLife.enabled = true;
    var curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.6f));
    sizeLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

    // --- color over lifetime ---
    var colorLife = ps.colorOverLifetime; colorLife.enabled = true;
    var grad = new Gradient();
    grad.SetKeys(
        new[] { new GradientColorKey(_baseColor, 0f), new GradientColorKey(_baseColor, 1f) },
        new[] { new GradientAlphaKey(1f, 0f),        new GradientAlphaKey(0f, 1f) }
    );
    colorLife.color = grad;

    ps.Play();
    Destroy(ps.gameObject, 2.5f);
    }

    // convenience spawner
    public static TextBurstFX Spawn(TextBurstFX prefab, Vector3 pos, string text, Color color, float size = 4f)
    {
        var fx = Instantiate(prefab, pos, Quaternion.identity);
        fx.Init(text, color, size);
        return fx;
    }
}