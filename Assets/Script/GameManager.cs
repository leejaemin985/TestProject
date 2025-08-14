using Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    protected override void Initialize()
    {
        Application.targetFrameRate = 60;
    }
}