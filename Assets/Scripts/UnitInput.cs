/*
 *	Date: 2020-08-27 17:31:08
 *	Description: 角色控制输入
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInput : MonoBehaviour
{
    public Transform mainCam;

    [SerializeField] private UnitCharacter player;
    [SerializeField] private Vector3 inputDirection;
    [SerializeField] private bool isJump;
    [SerializeField] private bool sprint;

    [SerializeField] private float attackDeltaTime;

    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isHeavyAttacking;

    [SerializeField] private Vector3 camForward;

    private void Start()
    {
        mainCam = Camera.main.transform;
        player = GetComponent<UnitCharacter>();
    }

    private void Update()
    { 
        if (!isJump)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isJump = true;
            }
        }

        isAttacking = Input.GetMouseButton(0);
        isHeavyAttacking = Input.GetMouseButtonDown(1);
        player.ComboAttack(isAttacking, isHeavyAttacking);
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        sprint = Input.GetKey(KeyCode.LeftShift);

        camForward = Vector3.Scale(mainCam.forward, new Vector3(1, 0, 1));
        inputDirection = v * camForward + h * mainCam.right;

        player.MoveCharacter(inputDirection, isJump, sprint);
        isJump = false;
    }
}