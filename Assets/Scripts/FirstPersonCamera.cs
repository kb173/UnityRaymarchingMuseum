﻿using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float turnSpeed = 1.0f;
    public float moveSpeed = 5.0f;

    public float height = 2.5f;

    private float xRotation;
    private float yRotation;

    void MouseAiming()
    {
        // When right click is pressed, rotate the camera
        if (Input.GetMouseButton(1))
        {
            xRotation -= Input.GetAxis("Mouse Y") * turnSpeed;
            yRotation += Input.GetAxis("Mouse X") * turnSpeed;

            transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);
        }
    }

    void KeyboardMovement()
    {
        var hMove = Input.GetAxis("Horizontal");
        var vMove = Input.GetAxis("Vertical");

        var movementVector = new Vector3(hMove, 0f, vMove);

        transform.Translate(movementVector * Time.deltaTime * moveSpeed);

        // Lock movement on y axis
        transform.position = new Vector3(transform.position.x, height, transform.position.z);
    }

    void Update()
    {
        MouseAiming();
        KeyboardMovement();
    }
}