using UnityEngine;
using Fusion;
using System;
using Unit;
using System.Collections.Generic;

public class EventHandler : NetworkBehaviour
{
    public static bool isServer { get; private set; }

    public static void SetServer(bool set) => isServer = set;

    public static bool isSpawned => instance != null;
    private static EventHandler instance;
    public static EventHandler Instance
    {
        get
        {
            if (!isSpawned) return null;
            return instance;
        }
    }

    private Dictionary<PlayerRef, Player> registedPlayer;


    public override void Spawned()
    {
        Initialize();
        instance = this;
    }

    private void Initialize()
    {
        registedPlayer = new();
    }

    public void RegisterPlayer(PlayerRef userRef, Player player)
    {
        if (registedPlayer.ContainsKey(userRef)) registedPlayer[userRef] = player;
        registedPlayer.Add(userRef, player);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TryForceHitMotion(PlayerRef userRef)
    {
        ForceHitMotion(userRef, Runner.Tick);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TryForceDefenseHitMotion(PlayerRef userRef)
    {
        ForceDefenseHitMotion(userRef, Runner.Tick);
    }

    public void TryPlayerHitRequestMotionSync(PlayerRef userRef)
    {
        RPC_TryForceHitMotion(userRef);
    }

    public void TryPlayerDefenseHitRequestMotionSync(PlayerRef userRef)
    {
        RPC_TryForceDefenseHitMotion(userRef);
    }

    private void ForceHitMotion(PlayerRef userRef,int tick)
    {
        foreach (var player in registedPlayer)
        {
            if (player.Key.Equals(userRef)) player.Value.ForceHitMotion(tick);
        }
    }

    private void ForceDefenseHitMotion(PlayerRef userRef, int tick)
    {
        foreach (var player in registedPlayer)
        {
            if (player.Key.Equals(userRef)) player.Value.ForceDefenseHitMotion(tick);
        }
    }

    public Player GetOtherUser(PlayerRef userRef)
    {
        foreach (var player in registedPlayer)
        {
            if (player.Key.Equals(userRef) == false) return player.Value;
        }
        return null;
    }


    private NetworkButtons prevInput;

    public override void FixedUpdateNetwork()
    {
        if (GetInput<PlayerInputData>(out var input) == false) return;

        if (input.buttons.WasPressed(prevInput, InputButton.LightAttack) == true)
        {

        }

        if (input.buttons.WasPressed(prevInput, InputButton.Defense) == true)
        {

        }

        prevInput = input.buttons;
    }

}