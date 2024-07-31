using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GLTFast;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class ModelImporter : MonoBehaviour
{
    public FileDownloader Downloader;
    public string DataURL;
    public string ProductURL;
    private Catelog productTypes = new Catelog();
    public static LocalTableData LocalData;
    public GameObject SelectionUI;
    public GameObject CategoryPrefab;
    public GameObject ShelfGameObject;

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
            var option = Instantiate(CategoryPrefab,SelectionUI.transform);
            option.GetComponent<CategoryButton>().INIT(products.Key);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            //DownloadCategory(LocalData.DATA.ElementAt(Random.Range(0,LocalData.DATA.Count)).Key);
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

    // public void DownloadCategory(string category)
    // {
    //     if (!LocalData.IsCategoryDownloaded(category))
    //     {
    //         StartCoroutine(GetCategoryData(category));
    //     }
    //     else
    //     {
    //         Debug.Log($"Category {category} is already downloaded.");
    //     }
    // }

    public IEnumerator DownloadCategoryCoroutine(string category, System.Action onComplete)
    {
        if (!LocalData.IsCategoryDownloaded(category))
        {
            bool isDownloadComplete = false;
            StartCoroutine(GetCategoryData(category, () => isDownloadComplete = true));

            while (!isDownloadComplete)
            {
                yield return null;
            }
        }
        else
        {
            Debug.Log($"Category {category} is already downloaded.");
        }

        onComplete?.Invoke();
    }
    private IEnumerator GetCategoryData(string productType , Action OnComplete)
    {
        using (UnityWebRequest productRequest = UnityWebRequest.Get(ProductURL + $"/{productType}"))
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
        
        OnComplete?.Invoke();
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
    
    public static async Task<List<GameObject>> CreateGameObjectsFromCategoryAsync(string category)
    {
        List<GameObject> categoryObjects = new List<GameObject>();
        string categoryPath = Path.Combine(Application.dataPath, "Product Models", category);

        Debug.Log($"Category path: {categoryPath}");

        if (!Directory.Exists(categoryPath))
        {
            Debug.LogError($"Category directory does not exist: {categoryPath}");
            return categoryObjects;
        }

        string[] glbFiles = Directory.GetFiles(categoryPath, "*.glb");
        Debug.Log($"Found {glbFiles.Length} GLB files in {categoryPath}");

        foreach (string glbFileR in glbFiles)
        {
            string glbFile = Path.GetFullPath(glbFileR);
            Debug.Log($"Loading GLB file: {glbFile}");

            GameObject obj = await LoadGLBAsGameObjectAsync(glbFile);

            if (obj != null)
            {
                categoryObjects.Add(obj);
                Debug.Log($"Added object: {obj.name}");
            }
            else
            {
                Debug.LogError($"Failed to load GLB file: {glbFile}");
            }
        }

        return categoryObjects;
    }

    private static async Task<GameObject> LoadGLBAsGameObjectAsync(string path)
    {
        var gltf = new GLTFast.GltfImport();
        Debug.Log($"Starting to load: {path}");

        bool loadSuccess = await gltf.Load(path);

        if (loadSuccess)
        {
            GameObject newObject = new GameObject(Path.GetFileNameWithoutExtension(path));
            newObject.SetActive(false);
            await gltf.InstantiateMainSceneAsync(newObject.transform);
            StandardizeScale(newObject,1);
            // newObject.SetActive(true);
            Debug.Log($"Successfully loaded: {path}");
            return newObject;
        }
        else
        {
            Debug.LogError($"Failed to load GLB file: {path}");
            return null;
        }
    }
    
    public static void StandardizeScale(GameObject obj, float standardSize)
    {
        // Get all renderers in the object and its children
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        // Calculate the bounds of the entire object
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        // Calculate the maximum dimension of the object
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        // Calculate the scale factor
        float scaleFactor = standardSize / maxDimension;

        // Apply the scale
        obj.transform.localScale *= scaleFactor;

        Debug.Log($"Standardized scale for {obj.name}. Scale factor: {scaleFactor}");
    }
}
