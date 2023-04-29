using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
	public void LoadNextScene()
	{
		var operation = SceneManager.LoadSceneAsync("SecondScene", LoadSceneMode.Additive);
		operation.allowSceneActivation = false;
		StartCoroutine(WaitForLoading(operation));
	}

	IEnumerator WaitForLoading(AsyncOperation operation)
	{
		while (operation.progress < 0.9f)
		{
			yield return null;
		}

		Debug.Log("Loading complete");

		operation.allowSceneActivation = true;
	}
}
