using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await InitializeServicesAsync();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitializeServicesAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in to Unity Services. Player ID: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing Unity Services: {e.Message}");
        }
    }

    // ===================================
    // SAVE LAYOUT & METADATA & SCREENSHOT
    // ===================================
    public async Task<bool> SaveDesignAsync(string slotId, SaveData payload, byte[] screenshotBytes)
    {
        try
        {
            // 1. Save JSON Metadata
            var data = new Dictionary<string, object>
            {
                { slotId, payload }
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"Saved layout metadata to slot {slotId}");

            // 2. Save Screenshot File
            string filename = $"{slotId}_screenshot.png";
            await CloudSaveService.Instance.Files.Player.SaveAsync(filename, screenshotBytes);
            Debug.Log($"Saved screenshot file {filename}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving design: {e.Message}");
            return false;
        }
    }

    // ===================================
    // GET ALL SAVE SLOTS (METADATA)
    // ===================================
    public async Task<Dictionary<string, SaveData>> GetAllSavedDesignsAsync()
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            Dictionary<string, SaveData> designs = new Dictionary<string, SaveData>();

            foreach (var item in results)
            {
                if (item.Key.StartsWith("SaveSlot_"))
                {
                    SaveData data = item.Value.Value.GetAs<SaveData>();
                    designs.Add(item.Key, data);
                }
            }
            return designs;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading designs: {e.Message}");
            return new Dictionary<string, SaveData>();
        }
    }

    // ===================================
    // FETCH SCREENSHOT BYTES
    // ===================================
    public async Task<byte[]> GetScreenshotAsync(string slotId)
    {
        string filename = $"{slotId}_screenshot.png";
        try
        {
            byte[] fileData = await CloudSaveService.Instance.Files.Player.LoadBytesAsync(filename);
            return fileData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading screenshot {filename}: {e.Message}");
            return null;
        }
    }

    // ===================================
    // DELETE DESIGN
    // ===================================
    public async Task<bool> DeleteDesignAsync(string slotId)
    {
        try
        {
            // Delete metadata
            await CloudSaveService.Instance.Data.Player.DeleteAsync(slotId);
            
            // Delete screenshot file
            string filename = $"{slotId}_screenshot.png";
            await CloudSaveService.Instance.Files.Player.DeleteAsync(filename);
            
            Debug.Log($"Successfully deleted design and screenshot for {slotId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting design {slotId}: {e.Message}");
            return false;
        }
    }
}

// Data Structures for Serialization
[Serializable]
public class SaveData
{
    public string slotId; // 🛡️ Fingerprint for data integrity verification
    public float totalCost;
    public float savedBudget; // 💰 The budget set at the time of saving
    public string roomCategory; // 🏠 The category (Bedroom, Kitchen, etc.)
    public Dictionary<string, int> inventorySummary;
    public List<SavedFurnitureItem> furnitureItems;
}

[Serializable]
public class SavedFurnitureItem
{
    public string prefabName;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;
}
