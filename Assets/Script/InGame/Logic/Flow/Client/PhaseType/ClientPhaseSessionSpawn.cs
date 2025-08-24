using System.Threading.Tasks;

using UnityEngine;
using Unit;
using System.Collections.Generic;

namespace InGame.Logic.Flow
{
    public class ClientPhaseSessionSpawn : ClientPhaseBase
    {
        //맵 스폰이나 주변 지형지물 이펙트 등 스폰 완료시점에 대한 페이즈로 사용 예정
        public override FlowPhase phaseType => FlowPhase.SessionSpawn;

    }
}