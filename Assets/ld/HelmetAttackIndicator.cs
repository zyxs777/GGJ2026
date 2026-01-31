using UnityEngine;
using UnityEngine.UI;

public class HelmetAttackIndicator : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _root;        // 标记挂载的UI根（建议 HelmMotionRot）
    [SerializeField] private RectTransform _helmetCenter; // 用来取“头盔中心”的参考（不填就用 _root）
    [SerializeField] private RectTransform _slitRect;    // 可选：限制显示区域
    [SerializeField] private Image _markerPrefab;

    [Header("Feel")]
    [SerializeField] private float _lifeTime = 0.5f;
    [SerializeField] private float _scale = 1.0f;
    [SerializeField] private bool _clampToSlit = true;

    [Header("Rotation")]
    [Tooltip("marker 美术默认朝向的补偿角度。比如箭头贴图默认朝上，就填 90；默认朝右就填 0")]
    [SerializeField] private float _rotationOffsetDeg = 0f;
    [Tooltip("如果你希望箭头指向'从中心指向受击点'，保持 true；想反过来就关掉")]
    [SerializeField] private bool _pointFromCenterToHit = true;

    private Camera _uiCam;

    private void Awake()
    {
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
        if (_root == null) _root = transform as RectTransform;
        if (_helmetCenter == null) _helmetCenter = _root;

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

        // 1) screen -> root local
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screenPos, _uiCam, out Vector2 hitLocal))
            return;

        // 2) 计算“头盔中心”在 root local 里的坐标
        Vector2 centerLocal = GetCenterLocalInRoot();

        // 3) 可选：Clamp 到缝隙区域（防止标记跑出去）
        if (_clampToSlit && _slitRect != null)
        {
            Rect slitLocalRect = GetLocalRectInRoot(_slitRect, _root, _uiCam);
            hitLocal.x = Mathf.Clamp(hitLocal.x, slitLocalRect.xMin, slitLocalRect.xMax);
            hitLocal.y = Mathf.Clamp(hitLocal.y, slitLocalRect.yMin, slitLocalRect.yMax);
        }

        // 4) 计算方向角（中心 -> 受击点）
        Vector2 dir = hitLocal - centerLocal;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right; // 避免 atan2(0,0)

        if (!_pointFromCenterToHit)
            dir = -dir;

        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + _rotationOffsetDeg;

        // 5) 生成标记并旋转
        Image marker = Instantiate(_markerPrefab, _root);
        RectTransform mrt = marker.rectTransform;
        mrt.anchoredPosition = hitLocal;
        mrt.localScale = Vector3.one * _scale;
        mrt.localRotation = Quaternion.Euler(0f, 0f, angleDeg);

        Destroy(marker.gameObject, _lifeTime);
    }

    private Vector2 GetCenterLocalInRoot()
    {
        // 用 _helmetCenter 的中心点作为“头盔中心”
        // 把它的世界坐标转成 root local
        Vector3 worldCenter = _helmetCenter.TransformPoint(_helmetCenter.rect.center);
        Vector2 screenCenter = RectTransformUtility.WorldToScreenPoint(_uiCam, worldCenter);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screenCenter, _uiCam, out Vector2 centerLocal);
        return centerLocal;
    }

    private Camera GetUICamera()
    {
        if (_canvas == null) return null;
        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
    }

    private static Rect GetLocalRectInRoot(RectTransform child, RectTransform root, Camera uiCam)
    {
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
