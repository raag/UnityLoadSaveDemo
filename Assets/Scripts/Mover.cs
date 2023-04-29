using System.Collections;
using System.IO;
using UnityEngine;

public class Mover : MonoBehaviour
{
	public Vector3 direction = Vector3.up;

	[SerializeField]
	float speed = 0.1f;

	MeshRenderer meshRenderer;

	void Start()
	{
		MoverManager.Instance.ManageMovers();
		meshRenderer = GetComponent<MeshRenderer>();
		StartCoroutine(Countdown(10));
		StartCoroutine(LogAfterDelay());
	}

	// Update is called once per frame
	void Update()
	{

		ChangeColor();

		var movement = direction * speed;

		// Multiply by Time.deltaTime to make the movement framerate independent
		movement *= Time.deltaTime;

		transform.Translate(movement);
	}

	private void ChangeColor()
	{
		if (meshRenderer == null)
		{
			return;
		}
		var sineTime = Mathf.Sin(Time.time) + 1 / 2f;
		var color = new Color(sineTime, 0.5f, (1 - sineTime) / 2f);
		meshRenderer.material.color = color;
	}

	IEnumerator LogAfterDelay()
	{
		yield return new WaitForSeconds(10);
		Debug.Log("Hello");
	}

	IEnumerator Countdown(int seconds)
	{
		while (seconds > 0)
		{
			Debug.Log(seconds);
			yield return new WaitForSeconds(1);
			seconds--;
		}
		Debug.Log("Lift off!");
		var path = PathForFilename("test.txt");
		Debug.Log("PATH: " + path);
	}

	public string PathForFilename(string filename)
	{
		var folderToStoreFilesIn = Application.persistentDataPath;
		var path = Path.Combine(folderToStoreFilesIn, filename);
		return path;
	}
}
