using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

	private const int THRUST_SPEED = 20;
	private const int ROTATION_SPEED = 500;

	//Use of rigid body allows the physics engine to apply
	private Rigidbody2D _rb;
    

    //The start function runs once at the beginning of the game
    void Start ()
    {
        //Obtain a reference to the rigid body
        _rb = GetComponent<Rigidbody2D>();
    }
	
	//The update function runs each frame
	void FixedUpdate ()
    {
        //Obtain the movements
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

		/*if (inputY > 0 || inputY < 0)
			_rb.velocity = new Vector2 (0.0f, 0.0f);*/

		if (inputX == 0)
			_rb.angularVelocity = 0.0f;
		
        //Apply the movements
		Vector2 movement = new Vector2(inputY * -(float)Math.Sin(_rb.rotation * (Math.PI / 180.0f)), inputY * (float)Math.Cos(_rb.rotation * (Math.PI / 180.0f)));

		_rb.AddForce(movement * THRUST_SPEED);
		_rb.AddTorque(inputX * ROTATION_SPEED);
    }
}
