using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollider : MonoBehaviour 
{
	[SerializeField]
	private AudioClip sound;

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Player"))
		{
			PlayerController.Instance.PlayItemSound (sound);
			PlayerController.Instance.Health = Mathf.Min (PlayerController.MAX_HEALTH, PlayerController.Instance.Health + 20);
			PlayerController.Instance.SetVignetteColor (new Color(1.0F, 0.55F, 0.75F));
			Destroy (gameObject);
		}
	}
}
