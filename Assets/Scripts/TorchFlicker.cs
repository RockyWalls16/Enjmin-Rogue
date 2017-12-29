using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
	private Light torchLight;

	[SerializeField]
	private float variation;

	private float baseIntensity;
	private float nextFlicker;

	void Start ()
	{
		torchLight = GetComponent<Light> ();
		baseIntensity = torchLight.intensity;
	}

	void Update ()
	{
		if (nextFlicker <= 0)
		{
			torchLight.intensity = baseIntensity + Random.Range (-variation, variation);
			nextFlicker = Random.Range (0.05F, 0.1F);
		}
		nextFlicker -= Time.deltaTime;
	}
}
