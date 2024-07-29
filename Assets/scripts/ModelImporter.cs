using System.Collections;
using System.Collections.Generic;
using System.IO;
using GLTFast;
using UnityEngine;

public class ModelImporter : MonoBehaviour
{
    public string GLBNAME;

    async void Start()
    {
        await ImportGLB(Path.Combine(Application.streamingAssetsPath,GLBNAME));
    }

    async System.Threading.Tasks.Task ImportGLB(string path)
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
}
