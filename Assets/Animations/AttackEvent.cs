using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEvent : MonoBehaviour
{
	public void OnAttackEvent()
	{
		PlayerController.Instance.LaunchAttack ();
	}
}