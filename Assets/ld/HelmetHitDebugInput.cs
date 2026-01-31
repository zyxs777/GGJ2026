using UnityEngine;

public class HelmetHitDebugKeys : MonoBehaviour
{
    [SerializeField] private HelmetHitTiltPersistentShake _hit;
    [SerializeField] private float _strength = 1f;
    [SerializeField] private float _edgeInset = 80f;

    private void Reset()
    {
        _hit = GetComponent<HelmetHitTiltPersistentShake>();
    }

    private void Update()
    {
        if (_hit == null) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            _hit.ApplyHitScreenPos(new Vector2(_edgeInset, Screen.height * 0.5f), _strength);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            _hit.ApplyHitScreenPos(new Vector2(Screen.width - _edgeInset, Screen.height * 0.5f), _strength);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            _hit.ApplyHitScreenPos(new Vector2(Screen.width * 0.5f, Screen.height - _edgeInset), _strength);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            _hit.ApplyHitScreenPos(new Vector2(Screen.width * 0.5f, _edgeInset), _strength);
        }
        
    }
}