using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntBehavior : MonoBehaviour
{
    Rigidbody rigidBody;
    Camera viewCamera;
    Vector3 velocity;

    float moveSpeed;
    float turnSpeed;

    public bool mouseControl;

    void Start()
    {
        mouseControl = false;
        viewCamera = Camera.main;

        //Fetch the Rigidbody component you attach from your GameObject
        rigidBody = GetComponent<Rigidbody>();
        //Set the speed of the GameObject
        moveSpeed = 10.0f;
        turnSpeed = 25.0f;
    }

    private void Update()
    {
        if (mouseControl)
        {
            Vector3 mousePosition = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));

            // Look at mouse position. Mouse height is translated to be leveled with objects position.
            transform.LookAt(mousePosition + Vector3.up * transform.position.y);
            velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * moveSpeed;
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!mouseControl)
        {
            //MoveForward();
        }
        else
        {
            rigidBody.MovePosition(rigidBody.position + velocity * Time.fixedDeltaTime);
        }
    }

    private void MoveForward()
    {
        rigidBody.velocity = transform.forward * moveSpeed;
    }

    private void RotateRight()
    {
        transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * turnSpeed, Space.World);
    }

    private void RotateLeft()
    {
        transform.Rotate(new Vector3(0, -1, 0) * Time.deltaTime * turnSpeed, Space.World);
    }
}
