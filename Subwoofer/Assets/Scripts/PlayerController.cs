using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private const float THRUST_SPEED = 7.5f;
	private const float ROTATION_SPEED = 2.5f;

	//Use of rigid body allows the physics engine to apply
	private Rigidbody2D _rigidBody;
	private SpriteRenderer _spaceshipSpriteRenderer;
	private float _rotation;
	
	/// <summary>
	/// Indicates if the thrusters are currently engaged.
	/// </summary>
	public bool ThrustersEngaged { get; private set; }

	/// <summary>
	/// Gets the amount of remaining fuel.
	/// </summary>
	public int RemainingFuel { get; private set; }

	/// <summary>
	/// Gets the maximum fuel capacity.
	/// </summary>
	public int MaximumFuel { get; private set; }

    //The start function runs once at the beginning of the game
    void Start ()
    {
		ThrustersEngaged = false;
	    RemainingFuel = MaximumFuel = 1000;

		// obtain a reference to the rigid body
		_rigidBody = GetComponent<Rigidbody2D>();
	    _spaceshipSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
	
	//The update function runs each frame
	void FixedUpdate ()
	{
		// obtain the movements
		// (if there is no fuel, disable thrusters)
        float inputX = Input.GetAxis("Horizontal");
        float inputY = RemainingFuel > 0 ? Input.GetAxis("Vertical") : 0;

		// burn fuel (burn more fuel when thrusters are engaged)
		ThrustersEngaged = (Math.Abs(inputY) > float.Epsilon);
		RemainingFuel -= ThrustersEngaged ? 5 : 1;

		// apply thrust and rotation
		
		var movement = new Vector2(inputY * (float)Math.Sin(_rotation * (Math.PI / 180.0f)), inputY * (float)Math.Cos(_rotation * (Math.PI / 180.0f)));
		_rigidBody.AddForce(movement * THRUST_SPEED);
		_rotation += inputX * ROTATION_SPEED;

		// rotate the sprite to match the internal rotation value
		_spaceshipSpriteRenderer.transform.localRotation = new Quaternion();
		_spaceshipSpriteRenderer.transform.Rotate(Vector3.forward, -_rotation);
    }
}
