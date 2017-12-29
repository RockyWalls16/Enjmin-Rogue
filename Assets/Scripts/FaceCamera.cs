using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
	{
		transform.LookAt(Camera.main.transform.position, Vector3.up);
		transform.rotation = Quaternion.Euler (0.0F, transform.rotation.eulerAngles.y + 180.0F, 0.0F);
	}
}
