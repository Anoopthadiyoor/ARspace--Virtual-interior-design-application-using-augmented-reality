using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RoomSetupManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown roomDropdown;
    public GameObject roomPanel;

    [Header("Room Recommendation Panels")]
    public GameObject bedroomPanel;
    public GameObject livingRoomPanel;
    public GameObject diningPanel;

    [Header("Budget UI")]
    public TMP_InputField budgetInputField;

    [Header("AR UI")]
    public GameObject addMoreButton;
    public GameObject costPanel;

    void Start()
    {
        roomPanel.SetActive(true);
        HideAllPanels();

        if (addMoreButton != null)
            addMoreButton.SetActive(false);

        if (costPanel != null)
            costPanel.SetActive(false);

        // 🔥 Auto-Fix Text Size for Mobile Readability
        SetDropdownTextSize(24);
    }

    private void SetDropdownTextSize(float newSize)
    {
        if (roomDropdown == null) return;

        // 1. Fix the main label (Caption)
        if (roomDropdown.captionText != null)
        {
            roomDropdown.captionText.fontSize = newSize;
        }

        // 2. Fix the list items (Template)
        if (roomDropdown.itemText != null)
        {
            roomDropdown.itemText.fontSize = newSize;
        }
        
        Debug.Log($"✅ Dropdown text size updated to {newSize}");
    }

    // =========================
    // CONFIRM ROOM SELECTION
    // =========================
    public void OnConfirmClicked()
    {
        // 🔥 FALLBACK: If user forgot to assign budgetInputField, find it dynamically before hiding the panel!
        if (budgetInputField == null)
        {
            TMP_InputField foundField = FindObjectOfType<TMP_InputField>();
            if (foundField != null)
            {
                Debug.Log("🔍 Auto-found TMP_InputField: " + foundField.gameObject.name);
                budgetInputField = foundField;
            }
        }
        string incomingBudgetText = "";

        if (budgetInputField != null)
        {
            incomingBudgetText = budgetInputField.text;
        }
        else
        {
            // FALLBACK 2: Check for Legacy InputField if TMP_InputField isn't used
            UnityEngine.UI.InputField legacyField = FindObjectOfType<UnityEngine.UI.InputField>();
            if (legacyField != null)
            {
                Debug.Log("🔍 Auto-found Legacy InputField: " + legacyField.gameObject.name);
                incomingBudgetText = legacyField.text;
            }
        }
        // 🔥 Apply User Budget if provided
        if (!string.IsNullOrEmpty(incomingBudgetText))
        {
            // Strip out any currency letters, commas, or spaces (e.g. "Rs. 50,000" -> "50000")
            string cleanNum = System.Text.RegularExpressions.Regex.Replace(incomingBudgetText, @"[^0-9.]", "");
            
            float newBudget = 0f;
            if (float.TryParse(cleanNum, out newBudget))
            {
                if (CostManager.Instance != null && newBudget > 0)
                {
                    CostManager.Instance.maxBudget = newBudget;
                    CostManager.Instance.UpdateUI();
                    Debug.Log("💰 Custom Budget Set: " + newBudget);
                }
            }
            else
            {
                Debug.LogWarning("❌ Could not parse budget number from: " + incomingBudgetText);
            }
        }

        string selectedRoom = roomDropdown.options[roomDropdown.value].text;

        roomPanel.SetActive(false);
        HideAllPanels();

        switch (selectedRoom)
        {
            case "Bedroom":
                bedroomPanel.SetActive(true);
                break;

            case "Living Room":
                livingRoomPanel.SetActive(true);
                break;

            case "Dining":
                diningPanel.SetActive(true);
                break;

            default:
                Debug.LogWarning("Unknown room selected: " + selectedRoom);
                break;
        }

        if (addMoreButton != null)
            addMoreButton.SetActive(false);

        if (costPanel != null)
            costPanel.SetActive(true);

        // Tell SaveLoadUI that setup is done
        if (SaveLoadUIManager.Instance != null)
        {
            SaveLoadUIManager.Instance.OnSetupConfirmed();
        }

        Debug.Log("Selected Room: " + selectedRoom);
    }

    // =========================
    // SHOW PANEL BASED ON ROOM
    // =========================
    public void ShowRecommendationPanel()
    {
        string selectedRoom = roomDropdown.options[roomDropdown.value].text;

        HideAllPanels();

        switch (selectedRoom)
        {
            case "Bedroom":
                bedroomPanel.SetActive(true);
                break;

            case "Living Room":
                livingRoomPanel.SetActive(true);
                break;

            case "Dining":
                diningPanel.SetActive(true);
                break;
        }

        if (addMoreButton != null)
            addMoreButton.SetActive(false);
    }

    // =========================
    // 🔥 ADD MORE BUTTON FIX
    // =========================
    public void OnAddMoreClicked()
    {
        ShowRecommendationPanel();

        // 🔥 Clear previously selected furniture
        if (FurnitureManager.Instance != null)
        {
            FurnitureManager.Instance.ClearSelection();
        }

        Debug.Log("Add More clicked → selection cleared 🔄");
    }

    // =========================
    // HIDE ALL PANELS
    // =========================
    public void HideAllPanels()
    {
        bedroomPanel.SetActive(false);
        livingRoomPanel.SetActive(false);
        diningPanel.SetActive(false);
    }

    // =========================
    // LOAD SAVED LAYOUT
    // =========================
    public void LoadSavedLayout(SaveData data)
    {
        Debug.Log("Loading saved layout...");

        // 1. Hide the Room Selection and Recommendation UI
        HideAllPanels();
        if (roomPanel != null) roomPanel.SetActive(false);
        if (addMoreButton != null) addMoreButton.SetActive(true);
        if (costPanel != null) costPanel.SetActive(true);
        
        // 2. Adjust AR Buttons: Hide Reload (we are done loading), Show Save
        if (SaveLoadUIManager.Instance != null)
        {
            if (SaveLoadUIManager.Instance.reloadDesignButton != null)
                SaveLoadUIManager.Instance.reloadDesignButton.SetActive(false);
            
            if (SaveLoadUIManager.Instance.saveDesignButton != null)
                SaveLoadUIManager.Instance.saveDesignButton.SetActive(true);
        }
        
        // 3. Clear existing scene
        if (FurnitureManager.Instance != null)
        {
            List<GameObject> toDestroy = new List<GameObject>(FurnitureManager.Instance.GetPlacedObjects());
            foreach (GameObject obj in toDestroy)
            {
                FurnitureManager.Instance.DeletePlacedObject(obj);
            }
        }
        if (CostManager.Instance != null)
        {
            CostManager.Instance.ResetCost();
            // 💰 Restore Saved Budget
            if (data.savedBudget > 0)
            {
                CostManager.Instance.maxBudget = data.savedBudget;
                Debug.Log($"💰 Budget Restored: {data.savedBudget}");
            }
            CostManager.Instance.UpdateUI();
        }
        // 🏠 Restore Room Context for "+ Add More"
        if (!string.IsNullOrEmpty(data.roomCategory))
        {
            for (int i = 0; i < roomDropdown.options.Count; i++)
            {
                if (roomDropdown.options[i].text == data.roomCategory)
                {
                    roomDropdown.value = i;
                    break;
                }
            }
            HideAllPanels(); // Keep UI clean on reload, but the dropdown value is now 'remembered'
        }
        // 3. Create Group Root
        GameObject layoutGroup = new GameObject("LoadedLayoutGroup");
        layoutGroup.tag = "Furniture"; 
        BoxCollider bc = layoutGroup.AddComponent<BoxCollider>();
        bc.size = new Vector3(5, 5, 5); 
        // 4. 🛰️ Anchor to CURRENT User Position & Orientation
        Transform camTransform = Camera.main.transform;
        // Match the user's horizontal heading
        Quaternion camHeading = Quaternion.Euler(0, camTransform.eulerAngles.y, 0);
        layoutGroup.transform.position = camTransform.position;
        layoutGroup.transform.rotation = camHeading;
        // 5. 🌊 Snap to Floor (Find the real surface)
        float floorY = camTransform.position.y - 1.4f;
        ARRaycastManager raycastManager = FindObjectOfType<ARRaycastManager>();
        if (raycastManager != null)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(new Ray(camTransform.position, Vector3.down), hits, TrackableType.PlaneWithinPolygon))
            {
                floorY = hits[0].pose.position.y;
                Debug.Log($"✅ Found floor at {floorY}. Anchoring group.");
            }
        }
        layoutGroup.transform.position = new Vector3(camTransform.position.x, floorY, camTransform.position.z);
        // 6. Spawn Items as Children
        foreach (SavedFurnitureItem item in data.furnitureItems)
        {
            GameObject prefab = Resources.Load<GameObject>(item.prefabName); 
            if (prefab != null)
            {
                // Instantiate as child so it automatically inherits the group's relative orientation!
                GameObject obj = Instantiate(prefab, layoutGroup.transform);

                // Use the relative offsets we saved (Relative to the Phone)
                obj.transform.localPosition = item.localPosition;
                obj.transform.localRotation = item.localRotation;
                obj.transform.localScale = item.localScale;
                
                FurnitureManager.Instance.RegisterPlacedObject(obj);
                CostManager.Instance.AddItem(obj);
            }
        }

        // Attach controller to allow manual sliding/rotating if needed
        layoutGroup.AddComponent<LayoutAlignmentController>();
    }
}