using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AEColor : MonoBehaviour
{
    [SerializeField] private Image _image;

    public RectTransform _rect;
    public Color startColor,endColor;

    private void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();
    }

    private void Update()
    {
        float angleZ = Mathf.Abs(_rect.localEulerAngles.z);
        float t = Mathf.InverseLerp(0f, 45f, angleZ);
        _image.color = Color.Lerp(Color.green, Color.red, t);
    }
    
}
