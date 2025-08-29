using Physics;

namespace Unit
{
    public class PlayerCollisionInfo : CollisionInfoData
    {
        public HitInfo hitInfo { get; private set; }

        public PlayerCollisionInfo(CollisionInfoData data, HitInfo hitInfo)
        {
            hitObject = data.hitObject;
            hitPoint = data.hitPoint;
            sweepProgress = data.sweepProgress;

            this.hitInfo = hitInfo;
        }
    }
}