using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EnemyBehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
    public float moveSpeed = 2.5f;
    public float hp = 10f;

    private bool isDead = false;

    private Animator enemyAnimator;
    private Rigidbody enemyRigidbody;


    private void Start()
    {
        enemyAnimator = GetComponent<Animator>();
        enemyRigidbody = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (isDead)
        {
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Weapon"))
        {
            hp -= 6f;
        }

        if (!isDead && hp <= 0)
        {
            isDead = true;
            Debug.Log("Death");
            Destroy(gameObject, 1f);
        }
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(EnemyTag));
    }
}
