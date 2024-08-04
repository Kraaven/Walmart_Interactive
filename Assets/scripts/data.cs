using GLTFast.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductList
{
    public List<ProductInfo> products;
    public int status;
}

public class ProductInfo
{
    public string _id;
    public string name;
    public string description;
    public float price;
    public string modelName;
    public string manufacturer;
    public string meshURL;
    public int stock;
    public string category;
    public string createdAt;
    public string updatedAt;
    public List<string> images;
    
}

public class Catelog
{
    public List<string> categories;
    public int status;
}

[Serializable]
public class LocalTableData
{
    public Dictionary<string, List<ProductDisplay>> DATA = new Dictionary<string, List<ProductDisplay>>();

    public void localTableData()
    {
        DATA = new Dictionary<string, List<ProductDisplay>>();
    }

    public void AddProduct(ProductInfo info)
    {
        ProductDisplay piece = new ProductDisplay();
        piece.ID = info._id;
        piece.Name = info.name;
        piece.Description = info.description;

        piece.Stock = info.stock;
        piece.Price = info.price;
        piece.Manufacturer = info.manufacturer;
        piece.Type = info.category;
        
        if (!DATA.ContainsKey(info.category))
        {
            DATA.Add(info.category,new List<ProductDisplay>());
        }
        
        
        DATA[info.category].Add(piece);

    }
    public ProductDisplay GetInformation(string id)
    {
        foreach (var category in DATA.Values)
        {
            foreach (var item in category)
            {
                if (id.Equals($"Model_{item.ID}"))
                {
                    Debug.Log("Information Exists");
                    return item;
                }
            }
        }
        return null;
    }


    public void AddCategory(string TYPE)
    {
        if (!DATA.ContainsKey(TYPE))
        {
            DATA.Add(TYPE, new List<ProductDisplay>());
        }
    }

    public bool IsCategoryDownloaded(string category)
    {
        return DATA.ContainsKey(category) && DATA[category].Count > 0;
    }
}

public class ProductDisplay
{
    public string ID;
    public string Name;
    public string Description;
    
    public int Stock;
    public float Price;
    public string Manufacturer;
    public string Type;
}
