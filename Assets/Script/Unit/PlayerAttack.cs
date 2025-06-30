using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Dictionary<int, float> attackMotionDuration = new()
    {
        { 0, 1.3f },
        { 1, 1.2f },
        { 2, 1.9f }
    };

    public bool canAttack { get; private set; }

    public int attackCount { get; private set; }
    private int attackMotionMaxCount => 3;

    private Action<string, float> attackMotionListener;
    private Action idleMotionListener;

    private IEnumerator attackMotionHandle = default;

    public void Initialized(Action<string, float> attackMotionEvent, Action idleMotionEvent)
    {
        canAttack = true;
        this.attackMotionListener = attackMotionEvent;
        this.idleMotionListener = idleMotionEvent;
    }


    private const string LIGHT_ATTACK_NAME_BASE = "_LightAttack";

    private const string HEAVY_ATTCK_NAME_BASE = "_HeavyAttack";

    private string GetAttackName(int attackNum)
    {
        return $"{LIGHT_ATTACK_NAME_BASE}_{attackNum}";
    }

    public int TryGetAttack()
    {
        int targetAttackNum = attackCount++;
        attackCount %= attackMotionMaxCount;
        return targetAttackNum;
    }

    public void SetAttackMotion(int attackMotionNum, float normalizedTime)
    {
        attackMotionListener?.Invoke(GetAttackName(attackMotionNum), normalizedTime);

        if (attackMotionHandle != null) StopCoroutine(attackMotionHandle);
        StartCoroutine(attackMotionHandle = AttackMotion(attackMotionDuration[attackMotionNum]));
    }

    private IEnumerator AttackMotion(float sec)
    {
        yield return new WaitForSeconds(sec);
        idleMotionListener?.Invoke();
        canAttack = true;
    }

    public void SetCanAttack(bool set)
    {
        canAttack = set;
    }
}