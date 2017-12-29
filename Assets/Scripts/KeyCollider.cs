using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCollider : MonoBehaviour 
{
	[SerializeField]
	private AudioClip sound;

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Player"))
		{
			PlayerController.Instance.PlayItemSound (sound);
			PlayerController.Instance.Keys++;
			PlayerController.Instance.SetVignetteColor (Color.yellow);
			Destroy (gameObject);
		}
	}
}
