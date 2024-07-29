using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

public class FileDownloader : MonoBehaviour
{

    public IEnumerator DownloadFile(string FileURL, string FileName, string type)
    {
        string filePath = Path.Combine(Path.Combine(Application.dataPath, type), FileName);

        // Create directory if it doesn't exist
        Directory.CreateDirectory(Path.Combine(Application.dataPath, type));

        if (File.Exists(filePath))
        {
            print("File already Exists!");
            yield return null;
        }
        else
        {
            UnityWebRequest www = UnityWebRequest.Get(FileURL);
            www.SendWebRequest();

            while (!www.isDone)
            {
                float progress = www.downloadProgress;
                Debug.Log($"Download Progress: {progress:P}");

                yield return null;
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(filePath, www.downloadHandler.data);
                Debug.Log($"File downloaded successfully: {filePath}");
            }
            else
            {
                Debug.LogError($"Error downloading file: {www.error}");
            }
        }
    }
}