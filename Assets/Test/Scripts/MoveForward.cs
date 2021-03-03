using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed = 20;

    private void Update()
    {
        transform.Translate(transform.forward * Time.deltaTime * speed);
    }
}
