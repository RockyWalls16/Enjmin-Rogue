using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
	private List<GameObject> enemies;
	public List<GameObject> Enemies
	{
		get { return enemies; }
	}

	private AudioSource audioSound;

	void Start()
	{
		enemies = new List<GameObject> ();
		audioSound = GetComponent<AudioSource> ();
	}

	void OnTriggerEnter(Collider other)
	{
		// Check collider is enemy
		if (other.CompareTag ("Enemy"))
		{
			enemies.Add (other.gameObject);
		}
	}

	void OnTriggerExit(Collider other)
	{
		// Check collider is enemy
		if (other.CompareTag ("Enemy"))
		{
			enemies.Remove (other.gameObject);
		}
	}

	public void Attack()
	{
		audioSound.pitch = Random.Range (0.9F, 1.1F);
		audioSound.Play ();

		foreach (GameObject obj in enemies)
		{
			if (obj.CompareTag("Enemy"))
			{
				obj.GetComponent<EnemyController> ().OnDeath ();
			}
		}
		enemies.Clear ();
	}
}
