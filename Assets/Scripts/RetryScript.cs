using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RetryScript : MonoBehaviour
{
	private bool launching;

	void Start()
	{
		DontDestroyOnLoad (gameObject);
	}

	void Update ()
	{
		if (Input.GetButtonDown ("Jump") && !launching )
		{
			launching = true;
			GetComponent<AudioSource> ().Play ();
			StartCoroutine (LaunchScene());

		}
	}

	IEnumerator LaunchScene()
	{
		yield return new WaitForSeconds (1.0F);
		SceneManager.LoadScene ("Main");

		float alpha = 1.0F;
		Image panel = GetComponentInChildren<Image> ();
		Text[] texts = GetComponentsInChildren<Text> ();

		while (alpha > 0)
		{
			panel.color = new Color (0.0F, 0.0F, 0.0F, alpha);

			foreach (Text text in texts)
			{
				text.transform.localScale = Vector3.one * alpha;
			}

			alpha -= Time.deltaTime;
			yield return null;
		}

		Destroy (gameObject);
	}
}
