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
    public string createdAt;
    public string updatedAt;
    public List<string> images;
}

public class Catelog
{
    public List<string> categories;
    public int status;
}
