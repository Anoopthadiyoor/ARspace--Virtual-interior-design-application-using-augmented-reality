using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class FurnitureManager : MonoBehaviour
{
    public static FurnitureManager Instance;

    [Header("Selected Furniture")]
    public GameObject selectedFurniturePrefab;

    private List<GameObject> placedObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // =========================
    // SET SELECTED FURNITURE
    // =========================
    public void SetFurniture(GameObject prefab)
    {
        selectedFurniturePrefab = prefab;

        Debug.Log("Selected Furniture: " + prefab.name);

        // Cost is now added only when the object is actually placed (in ARFurniturePlacer)
    }

    public GameObject GetSelectedFurniture()
    {
        return selectedFurniturePrefab;
    }

    // =========================
    // CLEAR SELECTION
    // =========================
    public void ClearSelection()
    {
        selectedFurniturePrefab = null;
        Debug.Log("Furniture selection cleared 🔄");
    }

    // =========================
    // REGISTER PLACED OBJECT
    // =========================
    public void RegisterPlacedObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("❌ Placed object is NULL");
            return;
        }

        placedObjects.Add(obj);

        Debug.Log("✅ Object registered: " + obj.name);

        // Selection is cleared by ARFurniturePlacer after placement is fully complete
    }
    
    // DELETE OBJECT
    // =========================
    public void DeletePlacedObject(GameObject obj)
    {
        if (!placedObjects.Contains(obj))
            return;

        placedObjects.Remove(obj);
        Destroy(obj);

        Debug.Log("🗑️ Object deleted: " + obj.name);
    }

    // CLEAR ALL OBJECTS
    // =========================
    public void ClearAllFurniture()
    {
        foreach (GameObject obj in placedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        placedObjects.Clear();

        Debug.Log("🧹 All furniture cleared");
    }

    public List<GameObject> GetPlacedObjects()
    {
        return placedObjects;
    }
}