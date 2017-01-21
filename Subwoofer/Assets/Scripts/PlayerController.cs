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
	private const int MAXIMUM_HEALTH = 1000;
	private const int MAXIMUM_FUEL = 50000;

	private const string HEALTH_REMAINING_TEXT_FORMAT = "HEALTH : {0}";
	private const string FUEL_REMAINING_TEXT_FORMAT = "FUEL   : {0}";

	//Use of rigid body allows the physics engine to apply
	private Rigidbody _rigidBody;
	private SpriteRenderer _spaceshipSpriteRenderer;
	private IEnumerable<SpriteRenderer> _spaceshipThrusterSpriteRenderers;
	private AudioSource _spaceshipThrusterAudioSource;

	public Text HealthRemainingText;
	public Text HealthRemainingBackendText;
    public Text FuelRemainingText;
	public Text FuelRemainingBackendText;

	/// <summary>
	/// Gets the ship's rotation.
	/// </summary>
	public float ShipRotation { get; private set; }

	/// <summary>
	/// Gets the amount of remaining health.
	/// </summary>
	public int RemainingHealth { get; private set; }

	/// <summary>
	/// Gets the maximum health capacity.
	/// </summary>
	public int MaximumHealth { get { return MAXIMUM_HEALTH; } }

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
	public int MaximumFuel { get { return MAXIMUM_FUEL; } }

    //The start function runs once at the beginning of the game
    void Start ()
    {
		ThrustersEngaged = false;
	    RemainingHealth = MaximumHealth;
	    RemainingFuel = MaximumFuel;
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

	void UpdateHealth()
	{
		const int SCALE = 2;
		int healthRemainingPercentage = GetPercentage(RemainingHealth, MaximumHealth);
		HealthRemainingText.text = string.Format(HEALTH_REMAINING_TEXT_FORMAT, new string('|', healthRemainingPercentage / SCALE));
		HealthRemainingBackendText.text = string.Format(HEALTH_REMAINING_TEXT_FORMAT, new string('|', 100 / SCALE));
	}

    /// <summary>
    /// Create a graphical representation of the fuel to the user.
    /// </summary>
    void UpdateFuel()
    {
		const int SCALE = 2;
		int fuelRemainingPercentage = GetPercentage(RemainingFuel, MaximumFuel);
		FuelRemainingText.text = string.Format(FUEL_REMAINING_TEXT_FORMAT, new string('|', fuelRemainingPercentage / SCALE));
		FuelRemainingBackendText.text = string.Format(FUEL_REMAINING_TEXT_FORMAT, new string('|', 100 / SCALE));

		if (fuelRemainingPercentage > 66)
			FuelRemainingText.color = Color.green;
        else if (fuelRemainingPercentage > 33)
			FuelRemainingText.color = Color.yellow;
        else
			FuelRemainingText.color = Color.red;
    }

	private static int GetPercentage(int currentValue, int maximumValue)
	{
		return (int)((currentValue * 100.0)/ maximumValue);
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
