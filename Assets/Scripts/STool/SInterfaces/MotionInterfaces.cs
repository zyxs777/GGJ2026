using UnityEngine;

namespace STool.Interfaces
{
    public delegate void RefVec3Delegate(ref Vector3 v);
    
    public interface IPhysicsInteract
    {
        //施加单次冲击
        public void IImpulse(Vector3 imp);
        //施加持续力
        public virtual void IRegVelocity(RefVec3Delegate reg){}
        public virtual void IUnRegVelocity(RefVec3Delegate unReg){}
        public virtual void IRegAcceleration(RefVec3Delegate reg) { }
        public virtual void IUnRegAcceleration(RefVec3Delegate unReg) { }
    }
}
