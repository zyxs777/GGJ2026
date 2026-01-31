using Global;
using STool;
using UnityEngine;

namespace Scene
{
    public sealed class Billboard : MonoBehaviour
    {
        [SerializeReference] private Transform trans;
        private void Update()
        {
            var camPos = GlobalShare.MainCamera.transform.position.SetY(trans.position.y);
            trans.LookAt(camPos);
        }
    }
}
