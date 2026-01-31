using UnityEngine;

public class PlayerHelmetSlashAttack : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera _worldCam;
    [SerializeField] private Canvas _uiCanvas;
    [SerializeField] private RectTransform _slitRect;

    [Header("Attack")]
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _attackCooldown = 0.25f;

    [Header("Z Distance Gate (along camera forward)")]
    [SerializeField] private float _attackZMin = 0.0f;
    [SerializeField] private float _attackZMax = 3.5f;

    [Header("Enemy Filter")]
    [SerializeField] private string _enemyTag = "Enemy";

    [Header("Alpha Hit Test")]
    [Tooltip("alpha 大于该值才算命中（0~1）。0.1~0.3 通常不错")]
    [Range(0f, 1f)]
    [SerializeField] private float _alphaThreshold = 0.15f;

    [Tooltip("对重叠区域采样点数量：1=中心点；5=中心+四点（更准）")]
    [SerializeField] private int _sampleCount = 1;

    [Header("Debug")]
    [SerializeField] private bool _logMiss = false;

    private float _nextAttackTime;

    private void Awake()
    {
        if (_worldCam == null) _worldCam = Camera.main;
        if (_uiCanvas == null) _uiCanvas = FindObjectOfType<Canvas>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + _attackCooldown;
            SlashOnce();
        }
    }

    private void SlashOnce()
    {
        if (_worldCam == null || _uiCanvas == null || _slitRect == null)
        {
            Debug.LogWarning("[SLASH] Missing refs: worldCam/uiCanvas/slitRect");
            return;
        }

        Rect slitScreenRect = GetRectTransformScreenRect(_slitRect, GetUICamera());

        SpriteRenderer[] srs = FindObjectsOfType<SpriteRenderer>();
        Debug.Log($"[SLASH] click, SpriteRenderer in scene = {srs.Length}");

        int hitCount = 0;

        foreach (var sr in srs)
        {
            if (sr == null || !sr.enabled) continue;

            if (!string.IsNullOrEmpty(_enemyTag) && !sr.CompareTag(_enemyTag))
                continue;

            // Z 距离 gate：沿相机 forward
            float zDist = Vector3.Dot(sr.bounds.center - _worldCam.transform.position, _worldCam.transform.forward);
            if (zDist < _attackZMin || zDist > _attackZMax)
                continue;

            // 先算 sprite 屏幕矩形（粗筛）
            if (!TryGetSpriteScreenRect(sr, _worldCam, out Rect enemyScreenRect))
                continue;

            // 矩形必须重叠（粗筛）
            if (!enemyScreenRect.Overlaps(slitScreenRect))
                continue;

            // 计算重叠区域（采样点来自这里）
            Rect overlapRect = Intersect(enemyScreenRect, slitScreenRect);
            if (overlapRect.width <= 1f || overlapRect.height <= 1f)
                continue;

            // 精筛：像素 alpha 命中
            bool pixelHit = PixelHitTest(sr, overlapRect);
            if (!pixelHit)
            {
                if (_logMiss) Debug.Log($"[SLASH] PIXEL MISS {sr.name}");
                continue;
            }

            // 命中：伤害 + log
            var dmg = sr.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(_damage);
                Debug.Log($"<color=green>[HIT]</color> {sr.GetComponentInParent<EnemyInfo>()?.EnemyName ?? sr.name} z={zDist:F2}");
                hitCount++;
            }
            else
            {
                Debug.LogWarning($"[SLASH] Overlap+PixelHit but NO IDamageable on {sr.name}");
            }
        }

        if (hitCount == 0)
            Debug.Log("[SLASH] no enemy hit");
    }

    private bool PixelHitTest(SpriteRenderer sr, Rect overlapRect)
    {
        // 采样点：中心点或中心+四点
        Vector2 center = overlapRect.center;

        if (_sampleCount <= 1)
            return IsOpaqueAtScreenPoint(sr, center, _alphaThreshold);

        // 5 点：中心 + 左右上下（在重叠区域内偏移 25%）
        Vector2 dx = new Vector2(overlapRect.width * 0.25f, 0f);
        Vector2 dy = new Vector2(0f, overlapRect.height * 0.25f);

        return
            IsOpaqueAtScreenPoint(sr, center, _alphaThreshold) ||
            IsOpaqueAtScreenPoint(sr, center + dx, _alphaThreshold) ||
            IsOpaqueAtScreenPoint(sr, center - dx, _alphaThreshold) ||
            IsOpaqueAtScreenPoint(sr, center + dy, _alphaThreshold) ||
            IsOpaqueAtScreenPoint(sr, center - dy, _alphaThreshold);
    }

    /// <summary>
    /// 把屏幕点打到 Sprite 平面上，换算到 Sprite 像素坐标取 alpha。
    /// 要求：Texture Read/Write Enabled
    /// </summary>
    private bool IsOpaqueAtScreenPoint(SpriteRenderer sr, Vector2 screenPoint, float alphaThreshold)
    {
        if (sr.sprite == null) return false;

        // 1) 屏幕点 -> 射线
        Ray ray = _worldCam.ScreenPointToRay(screenPoint);

        // 2) 射线与 Sprite 所在平面求交
        // SpriteRenderer 的“面”一般是 transform.forward 的法线
        Plane plane = new Plane(sr.transform.forward, sr.transform.position);
        if (!plane.Raycast(ray, out float enter))
            return false;

        Vector3 hitWorld = ray.GetPoint(enter);

        // 3) 命中点转到 sprite 本地坐标
        Vector3 local = sr.transform.InverseTransformPoint(hitWorld);

        // 处理 flip
        if (sr.flipX) local.x = -local.x;
        if (sr.flipY) local.y = -local.y;

        Sprite sp = sr.sprite;
        float ppu = sp.pixelsPerUnit;
        Vector2 pivot = sp.pivot;              // 像素单位
        Rect texRect = sp.textureRect;         // 在 texture/atlas 中的位置（像素）

        // 4) local(单位) -> 像素坐标（相对 sprite rect）
        // local.xy 是以 SpriteRenderer 原点为基准的单位坐标
        Vector2 pixel = pivot + new Vector2(local.x * ppu, local.y * ppu);

        // 在 sprite rect 范围内才有效
        if (pixel.x < 0 || pixel.y < 0 || pixel.x >= texRect.width || pixel.y >= texRect.height)
            return false;

        // 5) 取纹理 alpha（考虑 atlas 偏移）
        Texture2D tex = sp.texture;
        if (tex == null) return false;

        // ⚠️ 纹理必须 Read/Write Enabled
        int tx = Mathf.FloorToInt(texRect.x + pixel.x);
        int ty = Mathf.FloorToInt(texRect.y + pixel.y);

        Color c = tex.GetPixel(tx, ty);
        return c.a >= alphaThreshold;
    }

    // ---------- UI helpers ----------

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

    private static Rect Intersect(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    // ---------- Sprite screen rect helper ----------

    private static bool TryGetSpriteScreenRect(SpriteRenderer sr, Camera cam, out Rect rect)
    {
        rect = default;

        Vector3 c = sr.bounds.center;
        Vector3 vpC = cam.WorldToViewportPoint(c);
        if (vpC.z <= 0f) return false;

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
}
