using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FurnitureCardUI : MonoBehaviour
{
    public Image furnitureImage;
    public TMP_Text furnitureName;
    public TMP_Text furniturePrice;
    public Button viewARButton;

    [Header("Assign Prefab In Inspector")]
    public GameObject furniturePrefab;

    void OnEnable()
    {
        if (viewARButton != null)
        {
            viewARButton.onClick.RemoveListener(OnViewARClicked);
            viewARButton.onClick.AddListener(OnViewARClicked);
        }
        else
        {
            Debug.LogWarning("View AR Button not assigned on " + gameObject.name);
        }
    }

    void OnDisable()
    {
        if (viewARButton != null)
        {
            viewARButton.onClick.RemoveListener(OnViewARClicked);
        }
    }

    void OnViewARClicked()
    {
        // CHECK PREFAB
        if (furniturePrefab == null)
        {
            Debug.LogWarning("Furniture Prefab not assigned on " + gameObject.name);
            return;
        }
        // SET SELECTED FURNITURE
        if (FurnitureManager.Instance != null)
        {
            FurnitureManager.Instance.SetFurniture(furniturePrefab);

            // 🔥 UPDATED LOG (IMPORTANT)
            Debug.Log("NEW furniture selected ✅: " + furniturePrefab.name);

            // PASS PRICE TO COST MANAGER
            if (CostManager.Instance != null && furniturePrice != null)
            {
                string priceText = furniturePrice.text;
                // Parse price from text like "Rs. 15000" or "RS. 15000"
                string numStr = System.Text.RegularExpressions.Regex.Replace(priceText, @"[^0-9.]", "");
                float price = 0f;
                float.TryParse(numStr, out price);
                string itemNameStr = furnitureName != null ? furnitureName.text : furniturePrefab.name;
                CostManager.Instance.SetPendingPrice(itemNameStr, price);
            }
            // HIDE PANELS + SHOW ADD MORE
            RoomSetupManager manager = FindObjectOfType<RoomSetupManager>();
            if (manager != null)
            {
                manager.HideAllPanels();

                if (manager.addMoreButton != null)
                    manager.addMoreButton.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("FurnitureManager instance not found!");
        }
    }
}