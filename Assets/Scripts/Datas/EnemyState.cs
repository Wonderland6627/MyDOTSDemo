using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;

public enum EnemyBehaviourState : int
{
    Idle = 0,
    Move = 1,
    Attack = 2,
}

public class EnemyStateTime
{
    public const float IdleTimeValue = 2f;
    public const float MoveTimeValue = 4f;
    public const float AttackDurationValue = 3f;//攻击间隔时间
}

[Serializable]
public struct EnemyState : IComponentData
{
    public float Duration;//持续时间

    public float moveWaitTime;//移动等待时间

    public float moveStartTime;//必须大于moveWaitTime

    public float aimTime;//瞄准时间 大于一定就攻击

    public float stateTime;//切换状态时间 初始化时随机赋值

    public EnemyBehaviourState BehaviourState;
}
