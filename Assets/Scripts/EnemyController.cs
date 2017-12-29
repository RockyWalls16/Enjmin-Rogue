using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
	private static string[] ENEMY_TEXTURES = { "Bat_", "Dark_Mage_", "Ghost_", "Lizard_", "Ogre_", "Orc_", "Skeleton_", "Slime_", "Wizard_" };

	private CharacterController controller;
	private Renderer enemyRenderer;
	private Direction lastDirection;
	private Cell lastCell;

	private TileType lastTile;
	private Vector3 motion;
	private Vector3 nextMotion;
	private float timer;
	private Cell targetDoor;
	private Room enemyRoom;
	private Texture[] enemyTextures;
	private int animationSequence;
	private float animationTimer;

	private Vector3 pos;

	private AudioSource hitSound;

	[SerializeField]
	private AudioClip deathSound;

	void Start()
	{
		controller = GetComponent<CharacterController> ();
		enemyRenderer = GetComponent<Renderer> ();
		hitSound = GetComponent<AudioSource> ();

		// Find random texture
		string tex = ENEMY_TEXTURES [Random.Range (0, ENEMY_TEXTURES.Length)];
		enemyTextures = new Texture[]{ (Texture) Resources.Load(tex + "1"), (Texture) Resources.Load(tex + "2") };
		enemyRenderer.material.mainTexture = enemyTextures[0];
		animationSequence = Random.Range (1, 20);

		// Apply a random scale
		float randomScale = Random.Range (1.8F, 2.1F);
		transform.localScale = new Vector3 (randomScale, randomScale, randomScale);
	}

	void TargetPlayer()
	{
		lastCell = null;
		Vector3 playerPos = PlayerController.Instance.transform.position;
		Vector3 playerAttackMotion = Vector3.Normalize(playerPos - transform.position);
		controller.Move (playerAttackMotion * 7.0F * Time.deltaTime);
	}

	void Wander()
	{
		Cell cell = Cell.CellAt (transform.position);
		if (!cell.Equals(lastCell))
		{
			lastTile = MapGenerator.Instance.SafeTileAt (cell);

			// Wander in corridor
			if (lastTile == TileType.CORRIDOR)
			{
				timer = targetDoor != null ? 0.0F : 0.25F;
				targetDoor = null;
				enemyRoom = null;
				nextMotion = GetCorridorDirection (cell);

			}
			// Find next corridor
			else if (lastTile == TileType.ROOM)
			{
				if (targetDoor == null)
				{
					Room room = MapGenerator.Instance.GetRoomAt (cell.x, cell.y);
					enemyRoom = room;
					if (room.doors.Count == 1)
					{
						targetDoor = room.doors [0];
					}
					else
					{
						room.doors = room.doors.OrderBy (c => Vector3.Magnitude(transform.position - c.AsVector3())).ToList();
						targetDoor = room.doors [Random.Range (1, room.doors.Count)];
					}
				}

				pos = targetDoor.AsVector3();
				nextMotion = Vector3.Normalize(pos - transform.position);
				timer = 0.15F;
			}
		}

		if (timer < 0.0F)
		{
			motion = nextMotion;
		}

		controller.Move (motion * 7.0F * Time.deltaTime);
		lastCell = cell;
		timer -= Time.deltaTime;
	}

	void Update ()
	{
		//Player is in room -> Attack
		float distance = Vector3.SqrMagnitude (PlayerController.Instance.transform.position - transform.position);
		if ((enemyRoom != null && enemyRoom == PlayerController.Instance.PlayerRoom) || distance < 300)
		{
			if (distance > 4)
			{
				TargetPlayer ();
			}
			else
			{
				if(timer <= 0.0F)
				{
					hitSound.pitch = Random.Range (0.9F, 1.1F);
					hitSound.Play ();
					PlayerController.Instance.Attacked (1);
					timer = 1.0F;
				}
				timer -= Time.deltaTime;
			}
		}
		else
		{
			Wander ();
		}

		// Animation handling
		animationTimer += Time.deltaTime;
		if(animationTimer >= 0.6F)
		{
			animationTimer = 0.0F;
			animationSequence = (animationSequence + 1) % 2;
			enemyRenderer.material.mainTexture = enemyTextures[animationSequence];
		}
	}

	Vector3 GetCorridorDirection(Cell cell)
	{
		Direction[] directions = DirectionUtils.DIRECTIONS;
		Direction[] possibleDirections = new Direction[4];

		// Fetch all directions possible
		int writeIndex = 0;
		foreach (Direction dir in directions)
		{
			Cell offset = cell.Offset (dir, 1);
			if ((targetDoor != null || lastDirection != dir.GetOpposite()) && MapGenerator.Instance.Map[offset.x, offset.y] != TileType.WALL)
			{
				possibleDirections [writeIndex++] = dir;
			}
		}

		// Pick a random direction
		Direction targetDirection = possibleDirections[Random.Range(0, writeIndex)];
		lastDirection = targetDirection;

		return targetDirection.GetDirectionVector ();
	}

	public void OnDeath()
	{
		// Spawn a new enemy elsewhere
		PlayerController.Instance.PlayItemSound(deathSound);
		MapGenerator.Instance.SpawnEnemy ();
		Destroy (gameObject);
	}
}
