using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Movement : MonoBehaviour
{
    [HideInInspector]
    public float lastHorizontalVector;
    [HideInInspector]
    public float lastVerticalVector;
    [HideInInspector]
    public Vector2 moveDir;
    [HideInInspector]
    public Vector2 lastMoveVector;
    
    Rigidbody2D rb;
    PlayerStats player;
    void Start()
    {
        player = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        lastMoveVector = new Vector2(1, 0f);
    }
    void Update()
    {
        InputManagement();
    }
    void FixedUpdate()
    {
        Move();
    }
    void InputManagement()
    {
        if (GameManager.instance.isGameOver)
        {
            return;
        }
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(moveX, moveY).normalized;

        if(moveDir.x != 0)
        {
            lastHorizontalVector = moveDir.x;
            lastMoveVector = new(lastHorizontalVector, 0f);
        }
        if (moveDir.y != 0)
        {
            lastVerticalVector = moveDir.y;
            lastMoveVector = new(0f, lastVerticalVector);
        }
        if (moveDir.x != 0 && moveDir.y != 0)
        {
            lastMoveVector = new Vector2(lastHorizontalVector, lastVerticalVector);

        }
    }
    void Move()
    {
        if (GameManager.instance.isGameOver)
        {
            return;
        }
        rb.linearVelocity = new Vector2 (moveDir.x * player.CurrentMoveSpeed, moveDir.y * player.CurrentMoveSpeed);
    }

}
