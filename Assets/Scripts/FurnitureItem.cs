using UnityEngine;

[System.Serializable]
public class FurnitureItem
{
    public string itemName;      // e.g. Wooden Bed
    public string roomType;      // Bedroom, LivingRoom, etc.
    public string style;         // Modern, Classic, Minimal
    public Vector2 size;         // Width & Length in meters
    public int price;            // Budget filtering
    public Sprite thumbnail;
    public GameObject prefab;    // Furniture prefab
}
