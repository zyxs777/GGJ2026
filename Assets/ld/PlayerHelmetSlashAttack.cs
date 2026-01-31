using UnityEngine;

public class PlayerHelmetSlashAttack : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera _worldCam;          // 世界相机（渲染敌人）
    [SerializeField] private Canvas _uiCanvas;          // 头盔UI所在Canvas
    [SerializeField] private RectTransform _slitRect;   // 缝隙Image的RectTransform

    [Header("Attack")]
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _attackCooldown = 0.25f;

    [Header("Z Distance Gate (along camera forward)")]
    [SerializeField] private float _attackZMin = 0.0f;  // 0=不限制过近
    [SerializeField] private float _attackZMax = 3.5f;  // 太远砍不到

    [Header("Filter (optional)")]
    [Tooltip("建议给敌人Tag=Enemy，然后勾上这个过滤。为空则不过滤。")]
    [SerializeField] private string _enemyTag = "Enemy";

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

        // 找场上所有 SpriteRenderer（GameJam阶段够用；建议用Tag过滤避免误伤）
        SpriteRenderer[] srs = FindObjectsOfType<SpriteRenderer>();
        Debug.Log($"[SLASH] click, SpriteRenderer in scene = {srs.Length}");

        int hitCount = 0;

        foreach (var sr in srs)
        {
            if (sr == null || !sr.enabled) continue;

            if (!string.IsNullOrEmpty(_enemyTag) && !sr.CompareTag(_enemyTag))
                continue;

            // 计算敌人在屏幕上的矩形
            if (!TryGetSpriteScreenRect(sr, _worldCam, out Rect enemyScreenRect))
                continue;

            // ✅ Z 距离判定：沿相机 forward 的距离（你的“远近”语义）
            float zDist = Vector3.Dot(sr.bounds.center - _worldCam.transform.position, _worldCam.transform.forward);
            if (zDist < _attackZMin || zDist > _attackZMax)
            {
                if (_logMiss) Debug.Log($"[SLASH] OUT Z {sr.name} zDist={zDist:F2}");
                continue;
            }

            // ✅ 缝隙重叠判定
            bool overlap = enemyScreenRect.Overlaps(slitScreenRect);
            if (!overlap)
            {
                if (_logMiss) Debug.Log($"[SLASH] OUT SLIT {sr.name} enemyRect={enemyScreenRect} slitRect={slitScreenRect}");
                continue;
            }

            // 命中：对敌人伤害
            var dmg = sr.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(_damage);
                Debug.Log($"<color=green>[HIT]</color> {sr.GetComponentInParent<EnemyInfo>()?.EnemyName ?? sr.name} z={zDist:F2}");
                hitCount++;
            }
            else
            {
                Debug.LogWarning($"[SLASH] Overlap but NO IDamageable on {sr.name}");
            }
        }

        if (hitCount == 0)
            Debug.Log("[SLASH] no enemy hit");
    }

    // ---------- Screen rect helpers ----------

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

        // 后方剔除：bounds中心在相机后方则不算
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
