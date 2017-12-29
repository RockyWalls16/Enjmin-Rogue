using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowText : MonoBehaviour
{
	[SerializeField]
	private float delay;

	[SerializeField]
	private float timer;

	private Text theText;
	private AudioSource audioSound;
	private string text;

	// Use this for initialization
	void Start ()
	{
		audioSound = GetComponent<AudioSource> ();
		theText = GetComponent<Text> ();
		text = theText.text;
		theText.text = " ";

		StartCoroutine (SpawnText());
	}

	IEnumerator SpawnText()
	{
		yield return new WaitForSeconds (delay);

		for (int i = 0; i < text.Length; i++)
		{
			audioSound.pitch = Random.Range (1.8F, 2.0F);
			audioSound.Play ();
			theText.text = text.Substring(0, theText.text.Length) + "_";
			yield return new WaitForSeconds (timer);
		}
		theText.text = text.Substring(0, theText.text.Length - 1);
	}
}
