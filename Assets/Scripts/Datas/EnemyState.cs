using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;

public enum EnemyBehaviourState
{
    Idle,
    Move,
    Attack,
}

[Serializable]
public struct EnemyState : IComponentData
{
    public float Duration;//当前状态持续时间 到时间切换状态

    public EnemyBehaviourState BehaviourState;
}
