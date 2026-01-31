using UnityEngine;

public class EnemyInRangeVFX : MonoBehaviour
{
    [Header("Auto Found Refs")]
    private Camera _worldCam;
    private Canvas _uiCanvas;
    private RectTransform _slitRect;
    private SpriteRenderer _sr;

    [Header("Range (same as attack)")]
    [SerializeField] private float _attackZMin = 0.0f;
    [SerializeField] private float _attackZMax = 3.5f;

    [Header("VFX")]
    [SerializeField] private Color _inRangeColor = Color.yellow;
    [SerializeField] private float _flashSpeed = 0f; // 0=不闪
    [SerializeField] private float _lerpSpeed = 18f;

    private Color _baseColor;
    private bool _inRange;
    private float _t;

    private void Awake()
    {
        // 1️⃣ World Camera
        _worldCam = Camera.main;

        // 2️⃣ SpriteRenderer
        _sr = GetComponentInChildren<SpriteRenderer>();

        // 3️⃣ UI Canvas（向上找）
        _uiCanvas = FindObjectOfType<Canvas>();

        // 4️⃣ Slit Rect（用 Tag）
        var slitGO = GameObject.FindWithTag("HelmetSlit");
        if (slitGO != null)
            _slitRect = slitGO.GetComponent<RectTransform>();

        if (_sr != null)
            _baseColor = _sr.color;
    }

    private void Update()
    {
        if (_worldCam == null || _uiCanvas == null || _slitRect == null || _sr == null)
            return;

        bool now = IsInAttackableArea();

        if (now != _inRange)
            _inRange = now;

        ApplyVFX();
    }

    private bool IsInAttackableArea()
    {
        // Z 距离（沿相机 forward）
        float zDist = Vector3.Dot(
            _sr.bounds.center - _worldCam.transform.position,
            _worldCam.transform.forward
        );
        if (zDist < _attackZMin || zDist > _attackZMax)
            return false;

        // Sprite 与缝隙重叠
        Rect slitScreen = GetRectTransformScreenRect(_slitRect, GetUICamera());
        if (!TryGetSpriteScreenRect(_sr, _worldCam, out Rect enemyScreen))
            return false;

        return enemyScreen.Overlaps(slitScreen);
    }

    private void ApplyVFX()
    {
        Color target = _inRange ? _inRangeColor : _baseColor;

        if (_inRange && _flashSpeed > 0f)
        {
            _t += Time.deltaTime * _flashSpeed;
            float k = 0.5f + 0.5f * Mathf.Sin(_t * Mathf.PI * 2f);
            target = Color.Lerp(_baseColor, _inRangeColor, k);
        }

        float s = 1f - Mathf.Exp(-_lerpSpeed * Time.deltaTime);
        _sr.color = Color.Lerp(_sr.color, target, s);
    }

    // ---------- helpers ----------

    private Camera GetUICamera()
    {
        if (_uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;
        return _uiCanvas.worldCamera != null ? _uiCanvas.worldCamera : Camera.main;
    }

    private static Rect GetRectTransformScreenRect(RectTransform rt, Camera uiCam)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
        Vector2 max = min;

        for (int i = 1; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);
            min = Vector2.Min(min, sp);
            max = Vector2.Max(max, sp);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static bool TryGetSpriteScreenRect(SpriteRenderer sr, Camera cam, out Rect rect)
    {
        rect = default;

        Vector3 c = sr.bounds.center;
        if (cam.WorldToViewportPoint(c).z <= 0f)
            return false;

        Bounds b = sr.bounds;

        Vector3[] pts =
        {
            new Vector3(b.min.x, b.min.y, b.min.z),
            new Vector3(b.min.x, b.min.y, b.max.z),
            new Vector3(b.min.x, b.max.y, b.min.z),
            new Vector3(b.min.x, b.max.y, b.max.z),
            new Vector3(b.max.x, b.min.y, b.min.z),
            new Vector3(b.max.x, b.min.y, b.max.z),
            new Vector3(b.max.x, b.max.y, b.min.z),
            new Vector3(b.max.x, b.max.y, b.max.z),
        };

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        int valid = 0;
        for (int i = 0; i < pts.Length; i++)
        {
            Vector3 sp3 = cam.WorldToScreenPoint(pts[i]);
            if (sp3.z <= 0f) continue;

            valid++;
            Vector2 sp = new Vector2(sp3.x, sp3.y);
            min = Vector2.Min(min, sp);
            max = Vector2.Max(max, sp);
        }

        if (valid == 0) return false;

        rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }

    private void OnDisable()
    {
        if (_sr != null)
            _sr.color = _baseColor;
    }
}
