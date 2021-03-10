using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameInfo
{
    [Header("敌人数量")]
    public int enemiesCount;

    [Header("范围边长")]
    public float rangeLength;
}

public class SystemBoot : MonoBehaviour
{
    public GameInfo info;

    private void Awake()
    {
        GameWorld.GetInstance().Init(info);
    }
}
