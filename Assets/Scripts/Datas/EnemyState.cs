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
}

[Serializable]
public struct EnemyState : IComponentData
{
    public float Duration;//持续时间

    public float moveWaitTime;//移动等待时间
    public float stateTime;//切换状态时间 初始化时随机赋值

    public EnemyBehaviourState BehaviourState;
}
