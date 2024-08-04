using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UIElements;

public class CategoryButton : MonoBehaviour
{
    public static CategoryButton CurrentButton;
    
    public string CATEGORY;

    public bool Downloding;
    private bool ContentDownloaded;
    private List<GameObject> CatelogModels;
    public GameObject Collection;

    public GameObject Cloud;
    public GameObject Downloading;
    public GameObject Run;
    private ModelImporter Importer;

    // [Header("Settings")]
    // public bool DEBUG;
    //
    // public float shelfSpacing;
    // public float levelSpacing;
    // public float itemSpacing;
    // public float columnSpacing;
    

    public void INIT(string cat)
    {
        Importer = FindObjectOfType<ModelImporter>();
        CATEGORY = cat;
        transform.GetChild(0).GetComponent<TMP_Text>().text = CATEGORY.FirstToUpper();
        Downloding = false;
        CatelogModels = new List<GameObject>();

        ContentDownloaded = ModelImporter.LocalData.IsCategoryDownloaded(CATEGORY);

        if (ContentDownloaded)
        {
            Run.SetActive(true);
        }
        else
        {
            Cloud.SetActive(true);
        }

    }

    public void Download_OrAnd_Proceed()
    {
        if (Downloding)
        {
            print("Download in Progress");

        }
        else
        {
            if (ContentDownloaded)
            {
                print("Display");
                if (Collection != null)
                {
                    LoadObjects();
                }
                else
                {
                    LoadCategoryObjects(CATEGORY);
                }

            }
            else
            {
                Cloud.SetActive(false);
                Downloading.SetActive(true);
                StartCoroutine(DownloadAndProceedCoroutine(CATEGORY)); 
            }
        }
    }

    private void Update()
    {
        // if (DEBUG)
        // {
        //     DistributeObjectsOnShelves(CatelogModels,shelfSpacing,levelSpacing,itemSpacing,columnSpacing);
        // }
    }

    private IEnumerator DownloadAndProceedCoroutine(string category)
    {
        bool isComplete = false;
        Downloding = true;
        yield return StartCoroutine(FindObjectOfType<ModelImporter>().DownloadCategoryCoroutine(category, () => isComplete = true));
        
        while (!isComplete)
        {
            yield return null;
        }

        FindObjectOfType<ModelImporter>().SaveLocalData();

        Downloding = false;
        ContentDownloaded = true;
        Downloading.SetActive(false);
        Run.SetActive(true);
    }
    
    public async void LoadCategoryObjects(string category)
    {
        List<GameObject> categoryObjects = await ModelImporter.CreateGameObjectsFromCategoryAsync(category);

        if (categoryObjects.Count > 0)
        {
            Debug.Log($"Loaded {categoryObjects.Count} objects from category: {category}");

            Collection = new GameObject(CATEGORY);
            Collection.transform.position = Vector3.zero;
            foreach (GameObject obj in categoryObjects)
            {
                CatelogModels.Add(obj); 
                obj.transform.SetParent(Collection.transform);
                obj.AddComponent<Rigidbody>().isKinematic = true;
                ProductInteractable.AddSphereColliderToFirstMeshRenderer(obj);
               
                // obj.SetActive(true);
            }
            
            //DistributeObjectsOnShelves(CatelogModels,5,5,5);
            //DistributeObjectsOnShelves(CatelogModels,30,2,1,5);
            
            DistributeObjectsOnShelves(categoryObjects,Importer.ShelfGameObject,Collection.transform,20,3,2.5f,14);
            Collection.transform.position = new Vector3(-20,1.5f,14);
            foreach (var model in CatelogModels)
            {
                model.SetActive(true);
                model.AddComponent<ProductInteractable>().Initialize();
                
            }
        }
        else
        {
            Debug.Log($"No objects loaded from category: {category}");
        }

        Debug.Log("Category loading complete, proceeding with next steps");
        LoadObjects();
    }

    public void LoadObjects()
    {
        if (CurrentButton != null)
        {
            CurrentButton.Collection.SetActive(false);
        }
        CurrentButton = this;
        Collection.SetActive(true);
    }
    
    public void DistributeObjectsOnShelves(List<GameObject> objects, GameObject shelfPrefab, Transform parentTransform, float shelfSpacing = 1.0f, float levelSpacing = 1.0f, float itemSpacing = 0.2f, float columnSpacing = 2.0f)
    {
        int levelsPerShelf = 3;
        int itemsPerLevel = 5;
        int columns = 4;
        int itemsPerShelf = levelsPerShelf * itemsPerLevel;

        int lastShelfIndex = -1;

        for (int i = 0; i < objects.Count; i++)
        {
            int shelfIndex = i / itemsPerShelf;
            int columnIndex = shelfIndex % columns;
            int rowIndex = shelfIndex / columns;

            // Check if we're starting a new shelf
            if (shelfIndex != lastShelfIndex)
            {
                // Instantiate a new shelf
                Vector3 shelfPosition = new Vector3(columnIndex * columnSpacing, 0, rowIndex * shelfSpacing);
                GameObject newShelf = GameObject.Instantiate(shelfPrefab, shelfPosition, Quaternion.identity, parentTransform);
                newShelf.transform.Rotate(-90,0,0);
                newShelf.transform.position += new Vector3(0, 6, 0);
            
                // You might want to name the shelf for easy identification
                newShelf.name = $"Shelf_{shelfIndex}";

                lastShelfIndex = shelfIndex;
            }

            int itemIndexInShelf = i % itemsPerShelf;
            int levelIndex = itemIndexInShelf / itemsPerLevel;
            int itemIndexInLevel = itemIndexInShelf % itemsPerLevel;

            float x = columnIndex * columnSpacing;
            float y = levelIndex * levelSpacing;
            float z = rowIndex * shelfSpacing + itemIndexInLevel * itemSpacing;

            objects[i].transform.position = new Vector3(x, y, z);
        }
    }}
