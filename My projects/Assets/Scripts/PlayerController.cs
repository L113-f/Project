using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public Rigidbody rb;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.right * moveY + transform.forward * moveX;

        rb.velocity = new Vector3(moveSpeed * moveDir.x , rb.velocity.y, moveSpeed * moveDir.z );

        
        

        
        
    }
}
