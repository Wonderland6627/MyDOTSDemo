using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EntityConvertTest : MonoBehaviour, IConvertGameObjectToEntity
{
    public float moveSpeed = 2;
    public float rotateSpeed = 10;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        TestMoveSpeed move = new TestMoveSpeed { Value = moveSpeed };
        TestRotateSpeed rotate = new TestRotateSpeed { Value = rotateSpeed };

        dstManager.AddComponentData(entity, move);
        dstManager.AddComponentData(entity, rotate);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        Destroy(collision.gameObject);
    }
}
