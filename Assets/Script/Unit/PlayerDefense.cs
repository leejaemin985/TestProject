using UnityEngine;

namespace Unit
{
    public class PlayerDefense : MonoBehaviour
    {
        private PlayerState playerState;

        public void Initialize(PlayerState playerState)
        {
            this.playerState = playerState;

            SetState();
        }

        public bool canDefense => !playerState.isMotion.state;

        private void SetState()
        {
            playerState.isDefense.AddStateOnListener(OnDefenseState);
            playerState.isDefense.AddStateOffListener(OffDefenseState);
        }

        private void OnDefenseState()
        {

        }

        private void OffDefenseState()
        {

        }

        public bool TryDefense()
        {
            return canDefense;
        }

        public void SetDefense(bool set)
        {
            playerState.isDefense.state = set;
        }

    }
}