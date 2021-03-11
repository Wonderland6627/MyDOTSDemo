using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[SerializeField]
public struct DangerousGoods : IComponentData
{
    public float power;
}
