using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobbing : MonoBehaviour
{
	[SerializeField]
	private float headBobbingSpeed = 0.18F;

	private Vector3 headMidPoint;
	private Vector3 headMidRotationPoint;
	private float headBobbingTimer = 0.0F;

	[SerializeField]
	private GameObject bobbingObject;

	[SerializeField]
	private Vector3 bobbingAmount;

	[SerializeField]
	private Vector3 bobbingRotationAmount;

	// Use this for initialization
	void Start () 
	{
		headMidPoint = bobbingObject.transform.localPosition;
		headMidRotationPoint = bobbingObject.transform.localRotation.eulerAngles;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Handle head bobbing
		float bobbingWave = 0.0F;
		Vector3 headPosition = bobbingObject.transform.localPosition;
		Vector3 headRotation = bobbingObject.transform.localRotation.eulerAngles;

		float verticalInput = Input.GetAxis ("Vertical");
		float horizontalInput = Input.GetAxis ("Horizontal");
		if (verticalInput == 0 && horizontalInput == 0)
		{
			headBobbingTimer = 0.0F;
		}
		else
		{
			bobbingWave = Mathf.Sin (headBobbingTimer);
			headBobbingTimer += headBobbingSpeed * PlayerController.Instance.SpeedModifier;

			if (headBobbingTimer > Mathf.PI * 2)
			{
				headBobbingTimer -= Mathf.PI * 2;
			}
		}

		if (bobbingWave != 0.0F)
		{
			float totalAxis = Mathf.Clamp(Mathf.Abs (horizontalInput) + Mathf.Abs (verticalInput), 0.0F, 1.0F);
			float bobbingEffect = bobbingWave * totalAxis;
			headPosition = headMidPoint + bobbingAmount * bobbingEffect;
			headRotation = headMidRotationPoint + bobbingRotationAmount * bobbingEffect;
		}
		else
		{
			headPosition = headMidPoint;
			headRotation = headMidRotationPoint;
		}
		bobbingObject.transform.localPosition = headPosition;

		if (bobbingRotationAmount != Vector3.zero)
		{
			bobbingObject.transform.localRotation = Quaternion.Euler (headRotation);
		}
	}
}
