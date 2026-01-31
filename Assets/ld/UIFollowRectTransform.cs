using UnityEngine;

public class UIFollowRectTransform : MonoBehaviour
{
    [SerializeField] private RectTransform _target; // 主头盔 helmImage 的 RectTransform
    [SerializeField] private RectTransform _self;   // fill 自己

    private void Reset()
    {
        _self = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (_self == null) _self = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (_target == null || _self == null) return;

        _self.anchoredPosition = _target.anchoredPosition;
        _self.localRotation = _target.localRotation;
        _self.localScale = _target.localScale;
    }
}