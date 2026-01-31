using UnityEngine;
using UnityEngine.UI;

public class HelmetAttackIndicator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Canvas _canvas;          // 头盔所在Canvas
    [SerializeField] private RectTransform _root;     // 标记挂载点（HelmMotionRot）
    [SerializeField] private RectTransform _slitRect; // 缝隙Rect（可选：用于Clamp）
    [SerializeField] private Image _markerPrefab;     // UI Image prefab（比如一个小红点）

    [Header("Feel")]
    [SerializeField] private float _lifeTime = 0.5f;
    [SerializeField] private float _scale = 1.0f;
    [SerializeField] private bool _clampToSlit = true;

    private Camera _uiCam;

    private void Awake()
    {
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
        if (_root == null) _root = transform as RectTransform;
        _uiCam = GetUICamera();
    }

    public void ShowAtWorldPos(Vector3 worldPos, Camera worldCam)
    {
        if (worldCam == null) return;
        Vector3 sp3 = worldCam.WorldToScreenPoint(worldPos);
        if (sp3.z <= 0f) return;

        ShowAtScreenPos(new Vector2(sp3.x, sp3.y));
    }

    public void ShowAtScreenPos(Vector2 screenPos)
    {
        if (_markerPrefab == null || _root == null || _canvas == null) return;

        // 屏幕点 -> root 的本地坐标
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screenPos, _uiCam, out Vector2 local))
            return;

        // 可选：把标记限制在缝隙范围内（更像“从缝里看到的攻击”）
        if (_clampToSlit && _slitRect != null)
        {
            Rect slitLocalRect = GetLocalRectInRoot(_slitRect, _root, _uiCam);
            local.x = Mathf.Clamp(local.x, slitLocalRect.xMin, slitLocalRect.xMax);
            local.y = Mathf.Clamp(local.y, slitLocalRect.yMin, slitLocalRect.yMax);
        }

        Image marker = Instantiate(_markerPrefab, _root);
        RectTransform mrt = marker.rectTransform;
        mrt.anchoredPosition = local;
        mrt.localScale = Vector3.one * _scale;

        Destroy(marker.gameObject, _lifeTime);
    }

    private Camera GetUICamera()
    {
        if (_canvas == null) return null;
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
    }

    private static Rect GetLocalRectInRoot(RectTransform child, RectTransform root, Camera uiCam)
    {
        // 把 child 的屏幕 rect 转到 root 的 local rect（用于 clamp）
        Vector3[] corners = new Vector3[4];
        child.GetWorldCorners(corners);

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, sp, uiCam, out Vector2 lp);
            min = Vector2.Min(min, lp);
            max = Vector2.Max(max, lp);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
}
