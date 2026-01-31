using UnityEngine;

namespace Scene
{
    public sealed class SpringBoard : MonoBehaviour
    {
        [SerializeField] private float jumpVel = 20;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IPhyMotion ipm))
            {
                ipm.AddImpulse(jumpVel * transform.up);
            }
        }
    }
}
