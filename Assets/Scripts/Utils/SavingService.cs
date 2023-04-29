using LitJson;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public interface ISaveable
{
	string SaveID { get; }
	JsonData SavedData { get; }

	void LoadFromData(JsonData data);
}

public static class SavingService
{
	private const string ACTIVE_SCENE_KEY = "activeScene";
	private const string SCENES_KEY = "scenes";
	private const string OBJECTS_KEY = "objects";
	private const string SAVE_ID_KEY = "$saveID";

	static UnityAction<Scene, LoadSceneMode> LoadObjectsAfterSceneLoad;

	public static void SaveGame(string filename)
	{
		var result = new JsonData();

		// get all saveable objects
		var allSavebleObjects = Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

		// save all saveable objects    
		if (allSavebleObjects.Count() > 0)
		{
			var savedObjects = new JsonData();
			foreach (var saveableObject in allSavebleObjects)
			{
				var data = saveableObject.SavedData;
				if (data.IsObject)
				{
					data[SAVE_ID_KEY] = saveableObject.SaveID;
					savedObjects.Add(data);
				}
				else
				{
					var behaviour = saveableObject as MonoBehaviour;
					Debug.LogWarning($"Object {behaviour.name} of type {behaviour.GetType()} does not implement ISaveable");
				}
			}

			result[OBJECTS_KEY] = savedObjects;
		}
		else
		{
			Debug.LogWarning("No objects implement ISaveable");
		}

		// save all open scenes
		var openScenes = new JsonData();
		var sceneCount = SceneManager.sceneCount;

		for (int i = 0; i < sceneCount; i++)
		{
			var scene = SceneManager.GetSceneAt(i);
			if (scene.isLoaded)
			{
				openScenes.Add(scene.name);
			}
		}
		result[SCENES_KEY] = openScenes;

		// save active scene
		result[ACTIVE_SCENE_KEY] = SceneManager.GetActiveScene().name;

		// save to file
		var outputPath = Path.Combine(Application.persistentDataPath, filename);
		var writer = new JsonWriter();
		writer.PrettyPrint = true;
		result.ToJson(writer);
		File.WriteAllText(outputPath, writer.ToString());
		Debug.Log($"Saved game to {outputPath}");

		// Clean memory
		result = null;
		allSavebleObjects = null; System.GC.Collect();

	}

	public static bool LoadGame(string filename)
	{
		var dataPath = Path.Combine(Application.persistentDataPath, filename);

		if (!File.Exists(dataPath))
		{
			Debug.LogWarning($"File {dataPath} does not exist");
			return false;
		}

		// read file
		var text = File.ReadAllText(dataPath);
		var data = JsonMapper.ToObject(text);

		// check if data is valid
		if (data == null || !data.IsObject)
		{
			Debug.LogWarning($"File {dataPath} is not a valid JSON file");
			return false;
		}

		// load all scenes
		if (!data.ContainsKey(SCENES_KEY))
		{
			Debug.LogWarning($"File {dataPath} does not contain any scenes");
			return false;
		}

		var scenes = data[SCENES_KEY];
		if (scenes.Count == 0)
		{
			Debug.LogWarning($"File {dataPath} does not contain any scenes");
			return false;
		}

		for (int i = 0; i < scenes.Count; i++)
		{
			var sceneName = scenes[i].ToString();
			if (i == 0)
			{
				SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
			}
			else
			{
				SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
			}
		}

		// set active scene
		if (data.ContainsKey(ACTIVE_SCENE_KEY))
		{
			var activeSceneName = data[ACTIVE_SCENE_KEY].ToString();
			var activeScene = SceneManager.GetSceneByName(activeSceneName);
			if (!activeScene.isLoaded)
			{
				Debug.LogWarning($"Scene {activeSceneName} is not loaded");
				return false;
			}

			SceneManager.SetActiveScene(activeScene);
		}
		else
		{
			Debug.LogWarning($"File {dataPath} does not contain an active scene");
			return false;
		}

		// load all objects
		if (!data.ContainsKey(OBJECTS_KEY))
		{
			Debug.LogWarning($"File {dataPath} does not contain any objects");
			return false;
		}

		var objects = data[OBJECTS_KEY];
		LoadObjectsAfterSceneLoad = (scene, loadSceneMode) =>
		{
			var allLoadableObjects = Object.FindObjectsOfType<MonoBehaviour>()
			.OfType<ISaveable>()
			.ToDictionary(o => o.SaveID, o => o);
			var objectsCount = objects.Count;

			for (int i = 0; i < objectsCount; i++)
			{
				var obj = objects[i];
				if (obj.ContainsKey(SAVE_ID_KEY))
				{
					var saveID = obj[SAVE_ID_KEY].ToString();
					if (allLoadableObjects.ContainsKey(saveID))
					{
						allLoadableObjects[saveID].LoadFromData(obj);
					}
					else
					{
						Debug.LogWarning($"Object with ID {saveID} not found");
					}
				}
				else
				{
					Debug.LogWarning($"Object does not contain a save ID");
				}
			}

			SceneManager.sceneLoaded -= LoadObjectsAfterSceneLoad;
			LoadObjectsAfterSceneLoad = null;
			System.GC.Collect();
		};

		SceneManager.sceneLoaded += LoadObjectsAfterSceneLoad;

		return true;
	}
}
