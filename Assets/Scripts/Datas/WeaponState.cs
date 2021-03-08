using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct WeaponState : IComponentData
{
    public bool isAttacking;
}
