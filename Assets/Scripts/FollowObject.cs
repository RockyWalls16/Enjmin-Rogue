using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
	[SerializeField]
	private GameObject target;

	private Vector3 positionOffset;

	void Start ()
	{
		positionOffset = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = target.transform.position + positionOffset;
	}
}
