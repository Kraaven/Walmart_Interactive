using System;
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
    public FileDownloader Downloader;
    public string DataURL;
    private Catelog productTypes = new Catelog();
    private LocalTableData LocalData;

    private void Awake()
    {
        Downloader = GetComponent<FileDownloader>();
    }

    private IEnumerator Start()
    { 
        LoadLocalData();
        yield return StartCoroutine(FetchCategories());
        foreach (var products in LocalData.DATA)
        {
            print(products.Key);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            DownloadCategory("food");
        }
    }

    private void LoadLocalData()
    {
        if (!Directory.Exists(Path.Combine(Application.dataPath, "Product Models")))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Product Models"));
        }
        string dataPath = Path.Combine(Application.dataPath, "Product Models", "data.json");
        if (File.Exists(dataPath))
        {
            string json = File.ReadAllText(dataPath);
            LocalData = JsonConvert.DeserializeObject<LocalTableData>(json);
            print("Loaded Data file");
        }
        else
        {
            LocalData = new LocalTableData();
            print("Creating new Data Object");
        }
    }

    private IEnumerator FetchCategories()
    {
        using (UnityWebRequest getCatalog = UnityWebRequest.Get(DataURL))
        {
            yield return getCatalog.SendWebRequest();
            
            if (getCatalog.result == UnityWebRequest.Result.Success)
            {
                string types = getCatalog.downloadHandler.text;
                productTypes = JsonConvert.DeserializeObject<Catelog>(types);
                if (productTypes.categories != null)
                {
                    foreach (var productType in productTypes.categories)
                    {
                        LocalData.AddCategory(productType);
                    }
                    SaveLocalData();
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve Catalog: " + getCatalog.error);
            }
        }
    }

    public void DownloadCategory(string category)
    {
        if (!LocalData.IsCategoryDownloaded(category))
        {
            StartCoroutine(GetCategoryData(category));
        }
        else
        {
            Debug.Log($"Category {category} is already downloaded.");
        }
    }

    private IEnumerator GetCategoryData(string productType)
    {
        using (UnityWebRequest productRequest = UnityWebRequest.Get(DataURL + $"/{productType}"))
        {
            yield return productRequest.SendWebRequest();
                            
            if (productRequest.result == UnityWebRequest.Result.Success)
            {
                string cateJson = productRequest.downloadHandler.text;
                var productList = JsonConvert.DeserializeObject<ProductList>(cateJson);
                if (productList.products != null)
                {
                    foreach (var product in productList.products)
                    {
                        yield return StartCoroutine(Downloader.DownloadFile(product.meshURL, $"Model_{product._id}.glb", productType));
                        if (File.Exists(Path.Combine(Application.dataPath, "Product Models", productType, $"Model_{product._id}.glb")))
                        {
                            LocalData.AddProduct(product);
                        }
                        Debug.Log($"Downloaded Model {product._id}!");
                    }
        
                    Debug.Log($"All Models of type {productType} Retrieved");
                    SaveLocalData();
                }
            }
            else
            {
                Debug.LogError($"Failed To retrieve Catalog for type: {productType}. Error: {productRequest.error}");
            }
        }
    }

    private void SaveLocalData()
    {
        string json = JsonConvert.SerializeObject(LocalData, Formatting.Indented);
        File.WriteAllText(Path.Combine(Application.dataPath, "Product Models", "data.json"), json);
        print("Saved Data locally");
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
}
