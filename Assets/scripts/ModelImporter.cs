using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ModelImporter : MonoBehaviour
{
    //public List<string> GLBNAMES;
    public FileDownloader Downloader;
    public string DataURL;

    public IEnumerator Start()
    {
        Downloader = GetComponent<FileDownloader>();
        ProductList productList = new ProductList();
        Catelog productTypes = new Catelog();
        
        
        using (UnityWebRequest GetCatelog = UnityWebRequest.Get(DataURL))
        {
            yield return GetCatelog.SendWebRequest();
            
            if (GetCatelog.result == UnityWebRequest.Result.ConnectionError ||
                GetCatelog.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed To retrieve Catelog");
            }
            else
            {
                string CateJson = GetCatelog.downloadHandler.text;
                productList = JsonConvert.DeserializeObject<ProductList>(CateJson);
                if (productList.products == null)
                {
                    print("Failed to Get Data");
                    yield break;
                }
            }
        }

        foreach (var Product in productList.products)
        {
            yield return StartCoroutine(Downloader.DownloadFile(Product.meshURL,"Model_"+Product._id+".glb","test"));
            print($"Downloaded Model {Product._id}!");
        }
        
        print("All Models Retrieved");

        // for (int i = 0; i < GLBNAMES.Count; i++)
        // {
        //     yield return ImportSingleGLB(Path.Combine(Path.Combine(Application.dataPath,"test"),"Model_"+i+".glb"));
        // }

        foreach (var product in productList.products)
        {
            yield return ImportSingleGLB(Path.Combine(Path.Combine(Application.dataPath,"test"),"Model_"+product._id+".glb"));
        }
        
        yield return null;


        
        
    }

    async Task ImportGLB(string path)
    {
        var gltf = new GltfImport();
        print(path);
        var success = await gltf.Load(path);

        if (success)
        {
            await gltf.InstantiateMainSceneAsync(transform);
            print("Model Loaded Successfully");
        }
        else
        {
            Debug.LogError("Failed to load GLB file.");
        }
    }
    private IEnumerator ImportSingleGLB(string path)
    {
        Task importTask = ImportGLB(path);
        
        while (!importTask.IsCompleted)
        {
            yield return null;
        }

        if (importTask.IsFaulted)
        {
            Debug.LogError($"Error importing {path}: {importTask.Exception}");
        }
        else
        {
            Debug.Log($"Successfully imported {path}");
        }
    }

    // private IEnumerator GetModels(List<string> url_list)
    // {
    //     int i = 0;
    //     foreach (var url in url_list)
    //     {
    //         yield return StartCoroutine(Downloader.DownloadFile(url,"Model_"+i+".glb","test"));
    //         print("Downloaded Model!");
    //         i++;
    //     }
    // }
}
