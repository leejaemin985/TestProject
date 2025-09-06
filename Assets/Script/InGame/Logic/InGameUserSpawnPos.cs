using UnityEngine;
using Fusion;

namespace InGame.Logic
{
    public class InGameUserSpawnPos : MonoBehaviour
    {
        [SerializeField] private Transform[] userSpawnPos;

        public (Vector3, Quaternion) GetUserSpawnPos(PlayerRef userRef)
        {
            Transform targetSpawnPos = userRef.AsIndex % 2 == 1 ? userSpawnPos[0] : userSpawnPos[1];
            return (targetSpawnPos.position, targetSpawnPos.rotation);
        }
    }
}
