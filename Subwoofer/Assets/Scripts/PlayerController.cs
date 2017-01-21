﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private const int THRUST_SPEED = 15;
	private const int ROTATION_SPEED = 1;

	//Use of rigid body allows the physics engine to apply
	private Rigidbody2D _rb;
	private float _rotation;

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
		
        //Apply the movements
		var movement = new Vector2(inputY * (float)Math.Sin(_rotation * (Math.PI / 180.0f)), inputY * (float)Math.Cos(_rotation * (Math.PI / 180.0f)));

		_rb.AddForce(movement * THRUST_SPEED);
		_rotation += inputX * ROTATION_SPEED;
    }
}
