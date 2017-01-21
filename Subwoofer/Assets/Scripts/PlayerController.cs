using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	private const float THRUST_SPEED = 30.5f;
	private const float ROTATION_SPEED = 5f;
	private const int MAXIMUM_FUEL = 50000;

	//Use of rigid body allows the physics engine to apply
	private Rigidbody _rigidBody;
	private SpriteRenderer _spaceshipSpriteRenderer;
	private IEnumerable<SpriteRenderer> _spaceshipThrusterSpriteRenderers;
	private AudioSource _spaceshipThrusterAudioSource;

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
	    RemainingFuel = MaximumFuel = (int)MAXIMUM_FUEL;
        UpdateFuel();

        // obtain a reference to the rigid body
        _rigidBody = GetComponent<Rigidbody>();
	    _spaceshipSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_spaceshipThrusterSpriteRenderers = FindObjectsOfType<SpriteRenderer>().Where(x => x.name.ToLower().Contains("thruster"));
	    _spaceshipThrusterAudioSource = GetComponentInChildren<AudioSource>();
    }
	
	//The update function runs each frame
	void FixedUpdate ()
	{
		// obtain the movements
		// (if there is no fuel, disable thrusters)
        float inputX = RemainingFuel > 0 ? Input.GetAxis("Horizontal") : 0;
        float inputY = RemainingFuel > 0 ? Input.GetAxis("Vertical") : 0;

		// burn fuel (burn more fuel when thrusters are engaged)
		ThrustersEngaged = (Math.Abs(inputY) > float.Epsilon);
		RemainingFuel -= ThrustersEngaged ? 5 : 1;
        UpdateFuel();

		// apply thrust and rotation
		var movement = new Vector3(inputY * (float)Math.Sin(ShipRotation * (Math.PI / 180.0f)), inputY * (float)Math.Cos(ShipRotation * (Math.PI / 180.0f)), 0.0f);
		_rigidBody.AddForce(movement * THRUST_SPEED);
		_rigidBody.AddForce(Physics.gravity);
		ShipRotation += inputX * ROTATION_SPEED;

		// rotate the sprite to match the internal rotation value
		_spaceshipSpriteRenderer.transform.localRotation = new Quaternion();
		_spaceshipSpriteRenderer.transform.Rotate(Vector3.forward, -ShipRotation);

		// show or hide thruster sprites based on input
		var thrustersThatAreOn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (inputY > 0)
			thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("mainthruster")).Select(x => x.name));
		if (inputY < 0)
			thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("backthruster")).Select(x => x.name));
		if (inputX > 0)
			thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("rotatecw")).Select(x => x.name));
		if (inputX < 0)
			thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("rotateccw")).Select(x => x.name));

		var hidden = new Color(1, 1, 1, 0);
		var shown = new Color(1, 1, 1, 1);
		foreach (var spriteRenderer in _spaceshipThrusterSpriteRenderers)
			spriteRenderer.color = thrustersThatAreOn.Contains(spriteRenderer.name) ? shown : hidden;

		_spaceshipThrusterAudioSource.mute = !ThrustersEngaged && Math.Abs(inputX) < float.Epsilon;
	}

    /// <summary>
    /// Create a graphical representation of the fuel to the user.
    /// </summary>
    void UpdateFuel()
    {
        //Obtain fuel percentage
        int fuelRemainingPercentage = (int)((RemainingFuel * 100.0) / MaximumFuel);

        //Set the fuel UI color
        if (fuelRemainingPercentage > 66)
            fuelRemainingText.color = Color.green;
        else if (fuelRemainingPercentage > 33)
            fuelRemainingText.color = Color.yellow;
        else
            fuelRemainingText.color = Color.red;

        //Fill the fuel bar
        fuelRemainingText.text = "Fuel Remaining: ";
        for (; fuelRemainingPercentage > 0; fuelRemainingPercentage -= 2)
        {
            fuelRemainingText.text += "|";
        }
    }
}

public static class HashSetExtensions
{
	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> range)
	{
		foreach (var item in range)
			collection.Add(item);
	}
}
