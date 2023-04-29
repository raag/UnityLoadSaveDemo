using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	private const string SAVE_FILE_NAME = "save.json";
	private string SAVE_FILE_PATH
	{
		get
		{
			var folderToStoreFilesIn = Application.persistentDataPath;
			var path = Path.Combine(folderToStoreFilesIn, SAVE_FILE_NAME);
			return path;
		}
	}

	public void Save()
	{
		SavingService.SaveGame(SAVE_FILE_PATH);
	}

	public void Load()
	{
		SavingService.LoadGame(SAVE_FILE_PATH);
	}
}
