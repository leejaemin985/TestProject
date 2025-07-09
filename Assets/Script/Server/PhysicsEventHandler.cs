using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Unit;

public class PhysicsEventHandler : MasterSingleton<PhysicsEventHandler>, IMasterSingleton
{
    public bool initialized => HasInstance;
}
