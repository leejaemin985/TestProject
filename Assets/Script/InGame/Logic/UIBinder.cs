using InGame.UI;
using UnityEngine;
using Unit;
using System.Threading.Tasks;

namespace InGame.Logic
{
    public class UIBinder : MonoBehaviour
    {
        [SerializeField] private InGameBattleUI battleUI;

        //private async void Start()
        //{
        //    //TestCode
        //    while (!PlayerRegistry.isInitialized || PlayerRegistry.Instance.RegistedUsers.Count < 2)
        //    {
        //        await Task.Delay(100);
        //    }

        //    Player user=null;
        //    Player opponent=null;
        //    foreach (var kv in PlayerRegistry.Instance.RegistedUsers)
        //    {
        //        if (kv.Key == GameNetworkManager.Instance.runner.LocalPlayer)
        //            user=kv.Value;
        //        else
        //            opponent=kv.Value;
        //    }

        //    if (user != null && opponent != null) BindBattleUI(user, opponent);
        //}

        //public void BindBattleUI(Player user, Player opponent)
        //{
        //    battleUI.Initialize(user, opponent);
        //}

    }
}