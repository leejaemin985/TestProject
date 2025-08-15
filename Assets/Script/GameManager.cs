using Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    protected override void OnAwake()
    {
        Application.targetFrameRate = 60;
    }
}