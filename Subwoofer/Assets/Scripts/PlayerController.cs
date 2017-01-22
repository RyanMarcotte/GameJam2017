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
	private const int MAXIMUM_HEALTH = 20000;
    private const int MAXIMUM_FUEL = 20000;
	private const int MAXIMUM_ENERGY = 250;
	private const int CONE_SHOT_ENERGY_REQUIREMENT = 20;
	private const int CONE_SHOT_BEAM_ANGLE_IN_DEGREES = 90;
	private const float CONE_SHOT_BEAM_LENGTH = 4.5f*3;
	private const int ALL_DIRECTION_SHOT_ENERGY_REQUIREMENT = CONE_SHOT_ENERGY_REQUIREMENT * 5;
	private const int ALL_DIRECTION_SHOT_BEAM_ANGLE_IN_DEGREES = 360;
	private const float ALL_DIRECTION_SHOT_BEAM_LENGTH = 2.25f*3;

	private const char UI_CHARACTER = '|';
	private const int UI_SCALE = 2;
	private const string HEALTH_REMAINING_TEXT_FORMAT = "HEALTH : {0}";
	private const string FUEL_REMAINING_TEXT_FORMAT = "FUEL   : {0}";
	private const string ENERGY_REMAINING_TEXT_FORMAT = "ENERGY : {0}";

	//Use of rigid body allows the physics engine to apply
	private readonly System.Random _rng = new System.Random();
	private Rigidbody _rigidBody;
	private SpriteRenderer _spaceshipSpriteRenderer;
	private IEnumerable<SpriteRenderer> _spaceshipThrusterSpriteRenderers;
	private AudioSource _spaceshipThrusterAudioSource;
	private IEnumerable<AudioSource> _spaceshipWallCollisionAudioSourceCollection;
	private IEnumerable<AudioSource> _spaceshipShotAudioSourceCollection;
	private AudioSource _spaceshipExplosionAudioSource;
	private AudioSource _healthPickupAudioSource;
	private AudioSource _fuelPickupAudioSource;
	private AudioSource _objectivePickupAudioSource;
	private AutomaticRotation _deathRotation = AutomaticRotation.None;
	private Sonar _spaceshipSonar;

    //UI Text
	public Text HealthRemainingText;
	public Text HealthRemainingBackendText;
    public Text FuelRemainingText;
	public Text FuelRemainingBackendText;
    public Text EnergyRemainingText;
    public Text EnergyRemainingBackendText;
    public Text LandingText;
    public Text GameOverText;
    public Text VictoryText;

    //UI Menus
    public GameObject Menu;

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
	private int _remainingEnergy;
	private float _shipRotation;

	/// <summary>
	/// Gets the amount of remaining health.
	/// </summary>
	public int RemainingHealth
	{
		get { return _remainingHealth > 0 ? _remainingHealth : 0; }
		private set { _remainingHealth = value; }
	}

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

	/// <summary>
	/// Gets the amount of remaining energy.
	/// </summary>
	public int RemainingEnergy { get { return _remainingEnergy > 0 ? _remainingEnergy : 0; } private set { _remainingEnergy = value; } }

	/// <summary>
	/// Gets the maximum energy capacity.
	/// </summary>
	public int MaximumEnergy { get { return MAXIMUM_ENERGY; } }

    //The start function runs once at the beginning of the game
    public void Start ()
    {
        //Initialize values
		ThrustersEngaged = false;
	    RemainingHealth = MaximumHealth;
	    RemainingFuel = MaximumFuel;
	    RemainingEnergy = MaximumEnergy;

        //Update the UI and hide menu
	    UpdateUI();
        Menu.gameObject.SetActive(false);

        //Get the rigid body
        _rigidBody = GetComponent<Rigidbody>();

        //Spaceship image renderers
	    _spaceshipSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_spaceshipThrusterSpriteRenderers = FindObjectsOfType<SpriteRenderer>().Where(x => x.name.ToLower().Contains("thruster"));
	    _spaceshipSonar = GetComponentInChildren<Sonar>();

        //Obtain audio sources
		var allAudioSources = GetComponentsInChildren<AudioSource>();
	    _spaceshipThrusterAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "spaceshipThrusterLoop") == 0);
	    _spaceshipWallCollisionAudioSourceCollection = allAudioSources.Where(x => x.clip.name.ToLower().Contains("hitwall"));
		_spaceshipShotAudioSourceCollection = allAudioSources.Where(x => x.clip.name.ToLower().Contains("waveshot"));
		_spaceshipExplosionAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "spaceshipExplosion") == 0);
		_healthPickupAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "collectHealth") == 0);
		_fuelPickupAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "collectFuel") == 0);
		_objectivePickupAudioSource = allAudioSources.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.clip.name, "collectObjective") == 0);
	}
	
	//The update function runs each frame
	void FixedUpdate()
	{
		UpdateUI();

		//Handle death
		if (RemainingHealth <= 0)
		{
			_spaceshipSpriteRenderer.color = _spaceshipSpriteRenderer.color.ToNotVisible();
			foreach (var spaceshipTrusterSpriteRenderer in _spaceshipThrusterSpriteRenderers)
				spaceshipTrusterSpriteRenderer.color = spaceshipTrusterSpriteRenderer.color.ToNotVisible();
			_spaceshipThrusterAudioSource.mute = true;
			_rigidBody.velocity = Vector3.zero;
			return;
		}

		//Handle victory
		if (VictoryText.text == "YOU WIN")
		{
			_rigidBody.velocity = Vector3.zero;
			_spaceshipThrusterAudioSource.mute = true;
			Menu.gameObject.SetActive(true);
			return;
		}

		if (RemainingEnergy < MaximumEnergy)
			_remainingEnergy++;

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
		if (Math.Abs(ShipRotation) < float.Epsilon && ((int)_rigidBody.velocity.magnitude*2) == 0)
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
		if (_spaceshipThrusterSpriteRenderers != null)
		{
			foreach (var spriteRenderer in _spaceshipThrusterSpriteRenderers)
				spriteRenderer.color = thrustersThatAreOn.Contains(spriteRenderer.name) ? shown : hidden;
		}

		if (RemainingHealth > 0 && RemainingFuel > 0)
			_spaceshipThrusterAudioSource.mute = !ThrustersEngaged && Math.Abs(inputX) < float.Epsilon;
		else
			_spaceshipThrusterAudioSource.mute = true;

		if (RemainingHealth > 0 && string.IsNullOrEmpty(VictoryText.text) && RemainingEnergy > CONE_SHOT_ENERGY_REQUIREMENT && Input.GetKeyDown("space"))
		{
			_spaceshipShotAudioSourceCollection.ElementAt(0).Play();
			RemainingEnergy -= CONE_SHOT_ENERGY_REQUIREMENT;
			_spaceshipSonar.CreateSonarMesh(CONE_SHOT_BEAM_ANGLE_IN_DEGREES, CONE_SHOT_BEAM_LENGTH);
		}
		else if (RemainingHealth > 0 && string.IsNullOrEmpty(VictoryText.text) && RemainingEnergy > ALL_DIRECTION_SHOT_ENERGY_REQUIREMENT && Input.GetKeyDown(KeyCode.LeftControl))
		{
			_spaceshipShotAudioSourceCollection.ElementAt(1).Play();
			RemainingEnergy -= ALL_DIRECTION_SHOT_ENERGY_REQUIREMENT;
			_spaceshipSonar.CreateSonarMesh(ALL_DIRECTION_SHOT_BEAM_ANGLE_IN_DEGREES, ALL_DIRECTION_SHOT_BEAM_LENGTH);
		}
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
	    if (incidentVectorAngle > 30 || !ShipRotationIsWithinVerticalLimit || other.relativeVelocity.magnitude > 3.5f)
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
	
	private const float VERTICAL_LIMIT = 12.5f;
	private bool ShipRotationIsWithinVerticalLimit { get { return ShipRotation > -VERTICAL_LIMIT && ShipRotation < VERTICAL_LIMIT;  } }

	/// <summary>
	/// Handle collisions with pickups
	/// </summary>
	void OnTriggerEnter(Collider pickup)
    {
        //Fuel
	    if (pickup.tag == "Fuel")
	    {
		    _fuelPickupAudioSource.PlayOneShot(_fuelPickupAudioSource.clip, 1);
		    RemainingFuel = MaximumFuel;
	    }
        //Health
	    else if(pickup.tag == "Health")
	    {
		    _healthPickupAudioSource.PlayOneShot(_healthPickupAudioSource.clip, 1);
			RemainingHealth = (RemainingHealth > MaximumHealth * 2 / 3 ? MaximumHealth : RemainingHealth + MaximumHealth / 3);
		}
		//Goal!
		else if (pickup.tag == "Goal")
		{
			_objectivePickupAudioSource.PlayOneShot(_objectivePickupAudioSource.clip, 1);
            VictoryText.text = "YOU WIN";
        }

        //Remove the pickup
	    Destroy(pickup.gameObject);
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
		FuelRemainingText.color = GetPercentageColor(fuelRemainingPercentage, new Color(1f, 0.5f, 0f), new Color(1f, 0.25f, 0), new Color(1f, 0f, 0f));

	    int energyRemainingPercentage = GetPercentage(RemainingEnergy, MaximumEnergy);
	    EnergyRemainingText.text = string.Format(ENERGY_REMAINING_TEXT_FORMAT, new string(UI_CHARACTER, energyRemainingPercentage/UI_SCALE));
	    EnergyRemainingBackendText.text = string.Format(ENERGY_REMAINING_TEXT_FORMAT, new string(UI_CHARACTER, 100/UI_SCALE));
		EnergyRemainingText.color = GetPercentageColor(energyRemainingPercentage, new Color(0f, 0.75f, 1f), new Color(0f, 0.5f, 1f), new Color(0f, 0f, 1f));

		RaycastHit hit;
	    if (RemainingHealth > 0 && string.IsNullOrEmpty(VictoryText.text) && Physics.Raycast(transform.position, Vector3.down, out hit, 4f))
	    {
		    if (ShipRotationIsWithinVerticalLimit && hit.normal == Vector3.up)
		    {
			    LandingText.text = "LAND";
			    LandingText.color = Color.green;
		    }
		    else
		    {
			    LandingText.text = "CANNOT LAND";
			    LandingText.color = Color.red;
		    }
	    }
	    else
	    {
		    LandingText.color = LandingText.color.ToNotVisible();
	    }

	    if (RemainingHealth <= 0)
			GameOverText.text = "GAME OVER";
	}

	private static int GetPercentage(int currentValue, int maximumValue)
	{
		var value = (int)((currentValue*100.0)/maximumValue);
		return value > 0 ? value : 0;
	}

	private static Color GetPercentageColor(float percentage)
	{
		return GetPercentageColor(percentage, Color.green, Color.yellow, Color.red);
	}

	private static Color GetPercentageColor(float percentage, Color full, Color medium, Color low)
	{
		if (percentage > 50)
			return full;

		return percentage > 25 ? medium : low;
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
