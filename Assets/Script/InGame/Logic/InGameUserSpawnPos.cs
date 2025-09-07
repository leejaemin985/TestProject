using UnityEngine;
using Fusion;

namespace InGame.Logic
{
    public class InGameUserSpawnPos : MonoBehaviour
    {
        [SerializeField] private Transform[] userSpawnPos;

        public (Vector3, Quaternion) GetUserSpawnPos(bool isMasterClient)
        {
            Transform targetSpawnPos = isMasterClient ? userSpawnPos[0] : userSpawnPos[1];
            return (targetSpawnPos.position, targetSpawnPos.rotation);
        }
    }
}
