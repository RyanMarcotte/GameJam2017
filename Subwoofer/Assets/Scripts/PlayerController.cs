using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	private const float THRUST_SPEED = 2.5f;
	private const float ROTATION_SPEED = 5f;

	//Use of rigid body allows the physics engine to apply
	private Rigidbody2D _rigidBody;
	private SpriteRenderer _spaceshipSpriteRenderer;
	private IEnumerable<SpriteRenderer> _spaceshipThrusterSpriteRenderers;
	private IEnumerable<SpriteRenderer> _spaceshipBackThrusterSpriteRenderers;

    public Text fuelRemainingText;

	/// <summary>
	/// Gets the ship's rotation.
	/// </summary>
	public float ShipRotation { get; private set; }

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
	    RemainingFuel = MaximumFuel = 50000;
        updateFuel();

        // obtain a reference to the rigid body
        _rigidBody = GetComponent<Rigidbody2D>();
	    _spaceshipSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_spaceshipThrusterSpriteRenderers = FindObjectsOfType<SpriteRenderer>().Where(x => x.name.ToLower().Contains("mainthruster"));
	    _spaceshipBackThrusterSpriteRenderers = FindObjectsOfType<SpriteRenderer>().Where(x => x.name.ToLower().Contains("backthruster"));
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
        updateFuel();

		// apply thrust and rotation
		
		var movement = new Vector2(inputY * (float)Math.Sin(ShipRotation * (Math.PI / 180.0f)), inputY * (float)Math.Cos(ShipRotation * (Math.PI / 180.0f)));
		_rigidBody.AddForce(movement * THRUST_SPEED);
		ShipRotation += inputX * ROTATION_SPEED;

		// rotate the sprite to match the internal rotation value
		_spaceshipSpriteRenderer.transform.localRotation = new Quaternion();
		_spaceshipSpriteRenderer.transform.Rotate(Vector3.forward, -ShipRotation);

		// show or hide thruster sprites based
		var hidden = new Color(1, 1, 1, 0);
		var shown = new Color(1, 1, 1, 1);
		foreach (var spriteRenderer in _spaceshipThrusterSpriteRenderers)
			spriteRenderer.color = inputY > 0 ? shown : hidden;
		foreach (var spriteRenderer in _spaceshipBackThrusterSpriteRenderers)
			spriteRenderer.color = inputY < 0 ? shown : hidden;
	}

    //Display the 
    void updateFuel()
    {
        fuelRemainingText.text = "Fuel Remaining: " + ((int)(RemainingFuel * 100.0 / MaximumFuel)).ToString() + "%";
    }
}
