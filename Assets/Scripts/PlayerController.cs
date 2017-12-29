using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
	public const int MAX_KEY = 5;
	public const int MAX_HEALTH = 100;

	private static PlayerController instance;
	public static PlayerController Instance
	{
		get { return instance; }
	}

	private Camera playerCamera;
	private CharacterController characterController;
	private AudioSource playerSource;

	[SerializeField]
	private float baseSpeed = 6.0F;

	private float speedModifier = 1.0F;
	public float SpeedModifier
	{
		get { return speedModifier; }
	}

	private Cell lastCell;

	// Current room
	private Room playerRoom;
	public Room PlayerRoom
	{
		get { return playerRoom; }
	}

	private int keys;
	public int Keys
	{
		get { return keys; }
		set
		{
			keys = value;
			keyText.text = keys + " / " + MAX_KEY;

			if (keys >= MAX_KEY)
			{
				LaunchScene ("Win");
			}
		}
	}

	private int health;
	public int Health
	{
		get { return health; }
		set
		{
			health = value;
			healthText.text = value + "%";

			if (health <= 0)
			{
				LaunchScene ("Lose");
			}
		}
	}

	// Fire delay
	private float fireTimer;

	[SerializeField]
	private PostProcessingProfile ppProfile;

	[SerializeField]
	private Animator swordAnimator;

	private IEnumerator lastVignetteAnimation;

	[SerializeField]
	private SwordAttack swordAttack;

	[SerializeField]
	private Text keyText;

	[SerializeField]
	private Text healthText;

	public PlayerController()
	{
		instance = this;
	}

	void Start ()
	{
		playerCamera = GetComponentInChildren<Camera> ();
		characterController = GetComponent<CharacterController> ();
		playerSource = GetComponent<AudioSource> ();
		ChangeVignette (Color.black);
		Keys = 0;
		Health = MAX_HEALTH;
	}

	void Update ()
	{
		// Update camera
		float mouseX = Input.GetAxis ("Mouse X") * Time.deltaTime * 150.0F;
		float mouseY = -Input.GetAxis ("Mouse Y") * Time.deltaTime * 150.0F;

		// Set Pitch
		if (true || Cursor.lockState == CursorLockMode.Locked)
		{
			Vector3 cameraRotation = playerCamera.transform.localRotation.eulerAngles;
			cameraRotation.x += mouseY;
			cameraRotation.x = cameraRotation.x < 180 ? Mathf.Min (cameraRotation.x, 90) : Mathf.Max (cameraRotation.x, 270);
			playerCamera.transform.localRotation = Quaternion.Euler (cameraRotation);

			// Set Yaw
			transform.rotation = Quaternion.Euler (new Vector3 (0.0F, mouseX + transform.rotation.eulerAngles.y, 0.0F));
		}

		// Sprinting ?
		speedModifier = Input.GetButton("Sprint") ? 1.6F : 1.0F;

		// Handle movement
		float forwardInput = Input.GetAxis("Vertical") * baseSpeed * speedModifier;
		float strafeInput = Input.GetAxis ("Horizontal") * baseSpeed * speedModifier;

		if (forwardInput != 0 || strafeInput != 0)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		Vector3 forward = transform.forward;
		Vector3 strafe = transform.right;

		characterController.Move((forward * forwardInput + strafe * strafeInput) * Time.deltaTime);

		// Update room in
		Cell cell = Cell.CellAt (transform.position);
		if (!cell.Equals (lastCell))
		{
			lastCell = cell;

			TileType tile = MapGenerator.Instance.SafeTileAt (cell);

			// Wander in corridor
			if (tile == TileType.ROOM)
			{
				playerRoom = MapGenerator.Instance.GetRoomAt (cell.x, cell.y);
			}
			else
			{
				playerRoom = null;
			}
		}

		fireTimer -= Time.deltaTime;
		if (Input.GetButton ("Fire1") && fireTimer <= 0.0F)
		{
			Fire ();
			fireTimer = 0.75F;
		}
	}

	void Fire()
	{
		// Launch animation
		swordAnimator.SetTrigger ("SwingTrigger");
	}

	public void Attacked(int i)
	{
		SetVignetteColor (Color.red);
		Health -= Random.Range (1, 3);
	}

	public void LaunchAttack()
	{
		// Add damage
		swordAttack.Attack ();
	}

	// Coroutine Animation (Blood fade effect)
	IEnumerator ResetVignette()
	{
		VignetteModel.Settings settings = ppProfile.vignette.settings;
		float time = 1.0F;
		while (time > 0)
		{
			time -= Time.deltaTime;
			ChangeVignette (Color.Lerp(new Color(0.0F, 0.0F, 0.0F, 0.0F), settings.color, time));
			yield return null;
		}

		// Reset the color to black
		ChangeVignette (Color.black);
	}

	// Set Post processing vignete effect color
	void ChangeVignette(Color color)
	{
		VignetteModel.Settings settings = ppProfile.vignette.settings;
		settings.color = color;
		ppProfile.vignette.settings = settings;
	}

	public void SetVignetteColor(Color color)
	{
		// Vignette blink animation
		if (lastVignetteAnimation != null)
		{
			StopCoroutine (lastVignetteAnimation);
		}

		// Set blood effect
		ChangeVignette (color);

		// Start coroutine
		lastVignetteAnimation = ResetVignette ();
		StartCoroutine (lastVignetteAnimation);
	}

	void LaunchScene(string sceneName)
	{
		SceneManager.LoadScene (sceneName);
	}

	public void PlayItemSound(AudioClip clip)
	{
		playerSource.clip = clip;
		playerSource.Play ();
	}
}
