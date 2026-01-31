using UnityEngine;

public class HelmetFollowMouse : MonoBehaviour
{
    [SerializeField] private RectTransform _helmetRoot;     // HelmMotion
    [SerializeField] private Canvas _canvas;                // 用于正确换算坐标
    [SerializeField] private Vector2 _maxOffset = new Vector2(200f, 120f); // 最大偏移（UI单位）
    [SerializeField] private bool _pixelSnap = true;        // 防止亚像素抖动

    private void Reset()
    {
        _helmetRoot = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    private void Awake()
    {
        if (_helmetRoot == null) _helmetRoot = GetComponent<RectTransform>();
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        // Cursor.lockState = CursorLockMode.None;
        // Cursor.visible = true;
    }

    private void Update()
    {
        // 1) 鼠标相对屏幕中心偏移（屏幕像素）
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 v = (Vector2)Input.mousePosition - center;

        // 2) 归一化到 [-1..1]（用屏幕半宽/半高做比例）
        Vector2 n = new Vector2(
            v.x / (Screen.width * 0.5f),
            v.y / (Screen.height * 0.5f)
        );
        n = Vector2.Max(Vector2.one * -1f, Vector2.Min(Vector2.one, n)); // clamp

        // 3) 映射到 UI 偏移并限制最大范围
        Vector2 offset = new Vector2(n.x * _maxOffset.x, n.y * _maxOffset.y);

        // 4) 像素对齐（可选）
        if (_pixelSnap)
        {
            float sf = (_canvas != null) ? Mathf.Max(0.0001f, _canvas.scaleFactor) : 1f;
            offset = new Vector2(Mathf.Round(offset.x * sf) / sf, Mathf.Round(offset.y * sf) / sf);
        }

        _helmetRoot.anchoredPosition = offset;
    }
}