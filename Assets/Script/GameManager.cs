using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    protected override void OnAwake()
    {
        Application.targetFrameRate = 60;
    }
}