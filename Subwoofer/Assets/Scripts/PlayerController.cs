using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
	private enum AutomaticRotation
	{
		None,
		Left,
		Right
	}

	private const float THRUST_SPEED = 30.5f;
	private const float ROTATION_SPEED = 5f;
	private const int MAXIMUM_HEALTH = 1000;
	private const int MAXIMUM_FUEL = 2000;

	private const char UI_CHARACTER = '|';
	private const int UI_SCALE = 2;
	private const string HEALTH_REMAINING_TEXT_FORMAT = "HEALTH : {0}";
	private const string FUEL_REMAINING_TEXT_FORMAT = "FUEL   : {0}";

	//Use of rigid body allows the physics engine to apply
	private readonly System.Random _rng = new System.Random();
	private Rigidbody _rigidBody;
	private SpriteRenderer _spaceshipSpriteRenderer;
	private IEnumerable<SpriteRenderer> _spaceshipThrusterSpriteRenderers;
	private AudioSource _spaceshipThrusterAudioSource;
	private IEnumerable<AudioSource> _spaceshipWallCollisionAudioSourceCollection;
	private AudioSource _spaceshipExplosionAudioSource;
	private AudioSource _healthPickupAudioSource;
	private AudioSource _fuelPickupAudioSource;
	private AutomaticRotation _deathRotation = AutomaticRotation.None;

    //UI Text
	public Text HealthRemainingText;
	public Text HealthRemainingBackendText;
    public Text FuelRemainingText;
	public Text FuelRemainingBackendText;
    public Text GameOverText;

	/// <summary>
	/// Gets the ship's rotation.
	/// </summary>
	public float ShipRotation
	{
		get
		{
			var normalizedRotation = _shipRotation % 360;
			if (normalizedRotation > 180)
				normalizedRotation -= 360;
			else if (normalizedRotation < -180)
				normalizedRotation += 360;

			return normalizedRotation;
		}
		private set { _shipRotation = value; }
	}

	private int _remainingHealth;
	private int _remainingFuel;
	private float _shipRotation;

	/// <summary>
	/// Gets the amount of remaining health.
	/// </summary>
	public int RemainingHealth { get { return _remainingHealth > 0 ? _remainingHealth : 0; } private set { _remainingHealth = value; } }

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
	public int RemainingFuel { get { return _remainingFuel > 0 ? _remainingFuel : 0; } private set { _remainingFuel = value; } }

	/// <summary>
	/// Gets the maximum fuel capacity.
	/// </summary>
	public int MaximumFuel { get { return MAXIMUM_FUEL; } }

    //The start function runs once at the beginning of the game
    void Start ()
    {
		ThrustersEngaged = false;
	    RemainingHealth = MaximumHealth / 2;
	    RemainingFuel = MaximumFuel;
	    UpdateUI();

		// obtain a reference to the rigid body
		_rigidBody = GetComponent<Rigidbody>();
	    _spaceshipSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_spaceshipThrusterSpriteRenderers = FindObjectsOfType<SpriteRenderer>().Where(x => x.name.ToLower().Contains("thruster"));
	    
		var allAudioSources = GetComponentsInChildren<AudioSource>();
	    _spaceshipThrusterAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "spaceshipThrusterLoop") == 0);
	    _spaceshipWallCollisionAudioSourceCollection = allAudioSources.Where(x => x.clip.name.ToLower().Contains("hitwall"));
		_spaceshipExplosionAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "spaceshipExplosion") == 0);
		_healthPickupAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "collectHealth") == 0);
		_fuelPickupAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "collectFuel") == 0);
    }
	
	//The update function runs each frame
	void FixedUpdate ()
	{
		UpdateUI();

		// do not move player (and hide them) if they have no health
		if (RemainingHealth <= 0)
		{
			_rigidBody.velocity = Vector3.zero;
			_spaceshipSpriteRenderer.color = _spaceshipSpriteRenderer.color.ToNotVisible();
			foreach (var spaceshipTrusterSpriteRenderer in _spaceshipThrusterSpriteRenderers)
				spaceshipTrusterSpriteRenderer.color = spaceshipTrusterSpriteRenderer.color.ToNotVisible();

			_spaceshipThrusterAudioSource.mute = true;
			return;
		}

		// obtain the movements
		// (if there is no fuel, disable thrusters)
		float inputX = RemainingFuel > 0 ? Input.GetAxis("Horizontal") : 0;
		float inputY = RemainingFuel > 0 ? Input.GetAxis("Vertical") : 0;

		// burn fuel (burn more fuel when thrusters are engaged)
		ThrustersEngaged = (Math.Abs(inputY) > float.Epsilon);
		bool hadFuel = RemainingFuel > 0;
		RemainingFuel -= ThrustersEngaged ? 5 : 1;
		
		if (hadFuel && RemainingFuel <= 0)
		{
			if (ShipRotation > 0 && ShipRotation < 180)
				_deathRotation = AutomaticRotation.Right;
			else if (ShipRotation > -180 && ShipRotation < 0)
				_deathRotation = AutomaticRotation.Left;
			else
				_deathRotation = _rng.Next(0, 100) > 50 ? AutomaticRotation.Right : AutomaticRotation.Left;
		}

		if (RemainingFuel <= 0)
		{
			ThrustersEngaged = false;
			switch (_deathRotation)
			{
				case AutomaticRotation.None:
					inputX = 0;
				break;
				case AutomaticRotation.Left:
					inputX = -1;
					break;
				case AutomaticRotation.Right:
					inputX = 1;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		// if ship is pointing straight up and not moving, then we are likely sitting on the ground...
		// do not allow rotation
		if (Math.Abs(ShipRotation) < float.Epsilon && ((int)_rigidBody.velocity.magnitude * 2) == 0)
			inputX = 0;

		// apply thrust and rotation
		var movement = new Vector3(inputY*(float)Math.Sin(ShipRotation*(Math.PI/180.0f)), inputY*(float)Math.Cos(ShipRotation*(Math.PI/180.0f)), 0.0f);
		_rigidBody.AddForce(movement*THRUST_SPEED);
		_rigidBody.AddForce(Physics.gravity);
		ShipRotation += inputX*ROTATION_SPEED;

		// rotate the sprite to match the internal rotation value
		_spaceshipSpriteRenderer.transform.localRotation = new Quaternion();
		_spaceshipSpriteRenderer.transform.Rotate(Vector3.forward, -ShipRotation);

		// show or hide thruster sprites based on input
		var thrustersThatAreOn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (RemainingHealth > 0 && RemainingFuel > 0)
		{
			if (inputY > 0)
				thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("mainthruster")).Select(x => x.name));
			if (inputY < 0)
				thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("backthruster")).Select(x => x.name));
			if (inputX > 0)
				thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("rotatecw")).Select(x => x.name));
			if (inputX < 0)
				thrustersThatAreOn.AddRange(_spaceshipThrusterSpriteRenderers.Where(x => x.name.ToLower().Contains("rotateccw")).Select(x => x.name));
		}

		var hidden = new Color(1, 1, 1, 0);
		var shown = new Color(1, 1, 1, 1);
		foreach (var spriteRenderer in _spaceshipThrusterSpriteRenderers)
			spriteRenderer.color = thrustersThatAreOn.Contains(spriteRenderer.name) ? shown : hidden;

		if (RemainingHealth > 0 && RemainingFuel > 0)
			_spaceshipThrusterAudioSource.mute = !ThrustersEngaged && Math.Abs(inputX) < float.Epsilon;
		else
			_spaceshipThrusterAudioSource.mute = true;
	}

    /// <summary>
    /// Handle collisions with wall
    /// </summary>
    void OnCollisionEnter(Collision other)
    {
        //If the ship is out of fuel, end the game
        if (RemainingFuel <= 0)
        {
	        RemainingHealth = 0;
            _spaceshipExplosionAudioSource.Play();
            GameOverText.text = "GAME OVER";
            return;
        }

		// play random collision sound effect
	    _spaceshipWallCollisionAudioSourceCollection.ElementAt(_rng.Next(0, _spaceshipWallCollisionAudioSourceCollection.Count())).Play();

		// compute the incident vector and its angle
	    var allContactNormals = other.contacts.Select(x => x.normal).ToArray();
	    var allXForContactNormals = allContactNormals.Sum(v => v.x);
	    var allYForContactNormals = allContactNormals.Sum(v => v.y);
	    var allZForContactNormals = allContactNormals.Sum(v => v.z);
	    var incidentVector = new Vector3(allXForContactNormals, allYForContactNormals, allZForContactNormals).normalized;
	    var incidentVectorAngle = Vector3.Angle(Vector3.up, incidentVector);

		// bounce off the wall
		// (no bounce if player is oriented straight up, is landing on a flat surface, and is at low speed)
		const float VERTICAL_LIMIT = 12.5f;
	    if (incidentVectorAngle > 30 || (ShipRotation < -VERTICAL_LIMIT || ShipRotation > VERTICAL_LIMIT) || other.relativeVelocity.magnitude > 3.5f)
	    {
		    float magnitude = 50*Math.Max(other.relativeVelocity.magnitude, 3);
			_rigidBody.AddForce(incidentVector * magnitude);
		    RemainingHealth -= (int)magnitude;
		    if (RemainingHealth <= 0)
				_spaceshipExplosionAudioSource.Play();
	    }
	    else
	    {
			ShipRotation = 0;
		}
    }

    /// <summary>
    /// Handle collisions with pickups
    /// </summary>
    void OnTriggerEnter(Collider pickup)
    {
        //Fuel
        if (pickup.tag == "Fuel")
	        _fuelPickupAudioSource.PlayOneShot(_fuelPickupAudioSource.clip, 1);
			RemainingFuel = MaximumFuel;
        else
            RemainingHealth = (RemainingHealth > MaximumHealth * 2 / 3 ? MaximumHealth : RemainingHealth + MaximumHealth / 3);

        //Health
        pickup.gameObject.SetActive(false);
    }


    void UpdateUI()
	{
		int healthRemainingPercentage = GetPercentage(RemainingHealth, MaximumHealth);
		HealthRemainingText.text = string.Format(HEALTH_REMAINING_TEXT_FORMAT, new string(UI_CHARACTER, healthRemainingPercentage/UI_SCALE));
		HealthRemainingBackendText.text = string.Format(HEALTH_REMAINING_TEXT_FORMAT, new string(UI_CHARACTER, 100/UI_SCALE));
		HealthRemainingText.color = GetPercentageColor(healthRemainingPercentage);

		int fuelRemainingPercentage = GetPercentage(RemainingFuel, MaximumFuel);
		FuelRemainingText.text = string.Format(FUEL_REMAINING_TEXT_FORMAT, new string(UI_CHARACTER, fuelRemainingPercentage/UI_SCALE));
		FuelRemainingBackendText.text = string.Format(FUEL_REMAINING_TEXT_FORMAT, new string(UI_CHARACTER, 100/UI_SCALE));
		FuelRemainingText.color = GetPercentageColor(fuelRemainingPercentage);
	}

	private static int GetPercentage(int currentValue, int maximumValue)
	{
		var value = (int)((currentValue*100.0)/maximumValue);
		return value > 0 ? value : 0;
	}

	private static Color GetPercentageColor(float percentage)
	{
		if (percentage > 66.6f)
			return Color.green;

		return percentage > 33.3f ? Color.yellow : Color.red;
	}
}

public static class ExtensionMethods
{
	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> range)
	{
		foreach (var item in range)
			collection.Add(item);
	}

	public static Color ToNotVisible(this Color color)
	{
		return new Color(color.r, color.g, color.b, 0);
	}
}
