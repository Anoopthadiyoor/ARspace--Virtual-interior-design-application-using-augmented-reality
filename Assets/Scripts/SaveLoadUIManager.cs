using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SaveLoadUIManager : MonoBehaviour
{
    public static SaveLoadUIManager Instance;

    [Header("Buttons")]
    public GameObject reloadDesignButton;
    public GameObject saveDesignButton;

    [Header("Menu & UI")]
    public GameObject saveSlotsMenuPanel;
    public GameObject[] uiElementsToHideDuringScreenshot;
    public Transform slotsContainer;
    public GameObject slotCardPrefab; // Prefab with Image (Thumbnail), Text (Stats), and Button (Load)

    private bool hasPassedSetup = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Intially hidden (except for reload button which is now visible on main menu)
        if (reloadDesignButton != null) reloadDesignButton.SetActive(true);
        if (saveDesignButton != null) saveDesignButton.SetActive(false);
        if (saveSlotsMenuPanel != null) saveSlotsMenuPanel.SetActive(false);
    }

    private void Update()
    {
        // Enforce save button visibility rules based on placed items
        if (hasPassedSetup)
        {
            // Save button only visible if items exist
            bool hasItems = CostManager.Instance != null && CostManager.Instance.GetPlacedItemsCount() > 0;
            if (hasItems && saveDesignButton != null && !saveDesignButton.activeSelf)
                saveDesignButton.SetActive(true);
            else if (!hasItems && saveDesignButton != null && saveDesignButton.activeSelf)
                saveDesignButton.SetActive(false);
        }
    }

    // Call this from RoomSetupManager when user clicks Confirm Budget
    public void OnSetupConfirmed()
    {
        hasPassedSetup = true;
    }
    public void OnSaveDesignClicked()
    {
        // 🔒 TRANSACTIONAL LOCK: Generate Unique GUID and Freeze Room Data IMMEDIATELY
        string timeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string uniqueGuid = System.Guid.NewGuid().ToString().Substring(0, 8);
        string slotId = $"SaveSlot_{timeStamp}_{uniqueGuid}";

        // 3. Compile layout data (FROZEN SCAN - Before screenshot starts!)
        SaveData dataToSave = new SaveData();
        dataToSave.slotId = slotId; // Embed fingerprint into the JSON
        dataToSave.inventorySummary = new Dictionary<string, int>();
        float calculatedTotal = 0;
        foreach (GameObject obj in FurnitureManager.Instance.GetPlacedObjects())
        {
            FurnitureData fData = obj.GetComponentInChildren<FurnitureData>(true);
            string fName = fData != null ? fData.furnitureName : obj.name.Replace("(Clone)", "").Trim();
            float fPrice = fData != null ? fData.price : 0;

            calculatedTotal += fPrice;

            if (dataToSave.inventorySummary.ContainsKey(fName))
                dataToSave.inventorySummary[fName]++;
            else
                dataToSave.inventorySummary[fName] = 1;
        }
        dataToSave.totalCost = calculatedTotal;
        // 💰 Capture Context: Budget & Category
        if (CostManager.Instance != null)
            dataToSave.savedBudget = CostManager.Instance.maxBudget;
        RoomSetupManager setup = FindObjectOfType<RoomSetupManager>();
        if (setup != null && setup.roomDropdown != null)
            dataToSave.roomCategory = setup.roomDropdown.options[setup.roomDropdown.value].text; 
        dataToSave.furnitureItems = new List<SavedFurnitureItem>();
        // Find reference point for spatial grouping
        Vector3 camPos = Camera.main.transform.position;
        float saveFloorY = camPos.y - 1.4f;
        ARRaycastManager raycastManager = FindObjectOfType<ARRaycastManager>();
        if (raycastManager != null)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(new Ray(camPos, Vector3.down), hits, TrackableType.PlaneWithinPolygon))
                saveFloorY = hits[0].pose.position.y;
        }
        Vector3 refPoint = new Vector3(camPos.x, saveFloorY, camPos.z);
        // 🛡️ User-Relative Anchor: Transform current scene into local space
        Transform camTransform = Camera.main.transform;
        // Generate a rotation that only cares about the camera's horizontal heading (Y-axis)
        Quaternion camHeading = Quaternion.Euler(0, camTransform.eulerAngles.y, 0);
        foreach (GameObject obj in FurnitureManager.Instance.GetPlacedObjects())
        {
            // Calculate Position relative to the camera
            Vector3 relativePos = camTransform.InverseTransformPoint(obj.transform.position);
            
            // Calculate Rotation relative to the camera's heading
            // (Furniture Rotation - Camera Heading)
            Quaternion relativeRot = Quaternion.Inverse(camHeading) * obj.transform.rotation;

            SavedFurnitureItem item = new SavedFurnitureItem
            {
                prefabName = obj.name.Replace("(Clone)", "").Trim(),
                localPosition = relativePos, 
                localRotation = relativeRot,
                localScale = obj.transform.localScale
            };
            dataToSave.furnitureItems.Add(item);
        }

        // 🖼️ 1. Setup Screenshot parameters
        ScreenshotManager.Instance.uiElementsToHide = uiElementsToHideDuringScreenshot;

        // 🖼️ 2. Capture and handle callback (Using our FROZEN data)
        ScreenshotManager.Instance.CaptureScreenshotAndSave(async (byte[] imageBytes) => 
        {
            // Upload to Cloud
            Debug.Log($"Uploading frozen design {slotId} to cloud...");
            bool success = await CloudSaveManager.Instance.SaveDesignAsync(slotId, dataToSave, imageBytes);
            
            if (success) Debug.Log("Design uploaded successfully with Fingerprint! ✅");
        });
    }

    private bool isReloading = false;

    public async void OnReloadMenuOpened()
    {
        if (isReloading) return;
        isReloading = true;

        saveSlotsMenuPanel.SetActive(true);

        try
        {
            // Auto-Hide Room Selection Panel to clear the view
            RoomSetupManager setup = FindObjectOfType<RoomSetupManager>();
            if (setup != null && setup.roomPanel != null)
                setup.roomPanel.SetActive(false);

            // 🧹 INSTANT CLEANUP: Detach children immediately to solve the "Offset by 1" ghosting bug
            List<Transform> childrenToClean = new List<Transform>();
            foreach (Transform child in slotsContainer) childrenToClean.Add(child);
            
            foreach (Transform child in childrenToClean)
            {
                child.SetParent(null); // Remove from layout instantly
                Destroy(child.gameObject);
            }

            // Fetch all slots from Cloud
            var designsDict = await CloudSaveManager.Instance.GetAllSavedDesignsAsync();
            
            // 🔥 SORTING: Convert to List and sort by Key (Timestamp) in Descending order (Newest first)
            List<KeyValuePair<string, SaveData>> designsList = new List<KeyValuePair<string, SaveData>>(designsDict);
            designsList.Sort((a, b) => string.Compare(b.Key, a.Key));

            foreach (var kvp in designsList)
            {
                string currentSlotId = kvp.Key;
                SaveData currentData = kvp.Value;

                // 🏗️ Spawn UI Card
                GameObject cardObj = Instantiate(slotCardPrefab, slotsContainer);
                
                // 🧠 Get the Card Brain
                DesignSlotCard cardBrain = cardObj.GetComponent<DesignSlotCard>();
                if (cardBrain != null)
                {
                    cardBrain.Setup(currentSlotId, currentData);
                }
                else
                {
                    Debug.LogWarning("Prefab missing DesignSlotCard component! Please attach it to avoid mismatches.");
                }
            }
        }
        finally
        {
            isReloading = false;
        }
    }
    public void LoadDesignDetails(string slotId, SaveData data)
    {
        saveSlotsMenuPanel.SetActive(false);
        //  call RoomSetupManager to spawn the group!
        FindObjectOfType<RoomSetupManager>().LoadSavedLayout(data);
    }

    public void CloseReloadMenu()
    {
        if (saveSlotsMenuPanel != null)
        {
            saveSlotsMenuPanel.SetActive(false);
        }

        // Return to Room Selection ONLY if we haven't finished setup/loaded a design yet
        if (!hasPassedSetup)
        {
            RoomSetupManager setup = FindObjectOfType<RoomSetupManager>();
            if (setup != null && setup.roomPanel != null)
                setup.roomPanel.SetActive(true);
        }
    }
}
