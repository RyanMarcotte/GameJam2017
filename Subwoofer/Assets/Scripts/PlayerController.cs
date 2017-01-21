using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Use of rigid body allows the physics engine to apply
    private Rigidbody2D rb;
    private const int THRUST_SPEED = 80;
	private const int ROTATION_SPEED = 500;

    //The start function runs once at the beginning of the game
    void Start ()
    {
        //Obtain a reference to the rigid body
        rb = GetComponent<Rigidbody2D>();
    }
	
	//The update function runs each frame
	void FixedUpdate ()
    {
        //Obtain the movements
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

		if (inputY > 0 || inputY < 0) {
			rb.velocity = new Vector2 (0.0f, 0.0f);
		}

		if (inputX == 0)
			rb.angularVelocity = 0.0f;
		
        //Apply the movements
		Vector2 movement = new Vector2(inputY * (float)Math.Sin(rb.rotation * (Math.PI / 180.0f)), inputY * (float)Math.Cos(rb.rotation * (Math.PI / 180.0f)));

		rb.AddForce(movement * THRUST_SPEED);
		rb.AddTorque(inputX * ROTATION_SPEED);
    }
}
