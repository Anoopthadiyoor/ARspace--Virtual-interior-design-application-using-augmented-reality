using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CostManager : MonoBehaviour
{
    public static CostManager Instance;

    public TMP_Text totalCostText;

    [Header("Budget Settings")]
    public float maxBudget = 50000f; // Default budget. You can change this in the Unity Inspector!

    private float totalCost = 0f;

    // Store price info for items without FurnitureData
    private string pendingItemName = "";
    private float pendingItemPrice = 0f;

    private List<string> itemNames = new List<string>();
    private List<float> itemPrices = new List<float>();

    void Awake()
    {
        Instance = this;
        Debug.Log("✅ CostManager Initialized");
    }

    void Start()
    {
        // Force the layout dynamically to prevent off-screen issues on mobile
        if (totalCostText != null)
        {
            RectTransform textRt = totalCostText.GetComponent<RectTransform>();
            if (textRt != null)
            {
                if (totalCostText.transform.parent != null)
                {
                    RectTransform parentRt = totalCostText.transform.parent.GetComponent<RectTransform>();
                    if (parentRt != null)
                    {
                        // Anchor to Bottom-Right to avoid notches
                        parentRt.anchorMin = new Vector2(0.5f, 0f);
                        parentRt.anchorMax = new Vector2(1f, 0f);
                        parentRt.pivot = new Vector2(0.5f, 0.5f);
                        parentRt.anchoredPosition3D = new Vector3(-10, 150, 0);
                        parentRt.sizeDelta = new Vector2(-20, 120);
                        parentRt.localRotation = Quaternion.identity;
                        parentRt.localScale = Vector3.one;
                    }
                }
                
                // Make text fill the background
                textRt.anchorMin = new Vector2(0f, 0f);
                textRt.anchorMax = new Vector2(1f, 1f);
                textRt.pivot = new Vector2(0.5f, 0.5f);
                textRt.anchoredPosition3D = Vector3.zero;
                textRt.sizeDelta = Vector2.zero;
                textRt.localRotation = Quaternion.identity;
                textRt.localScale = Vector3.one;
                textRt.offsetMin = new Vector2(10, 10);
                textRt.offsetMax = new Vector2(-10, -10);
            }
        }
        UpdateUI();
    }

    // =========================
    // SET PENDING PRICE (called before placement)
    // =========================
    public void SetPendingPrice(string itemName, float price)
    {
        pendingItemName = itemName;
        pendingItemPrice = price;
        Debug.Log("💲 Pending price set: " + itemName + " = Rs. " + price);
    }

    // =========================
    // ADD ITEM (FROM PLACEMENT)
    // =========================
    public void AddItem(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("❌ AddItem received NULL object");
            return;
        }

        Debug.Log("🔥 AddItem CALLED for: " + obj.name);

        // Try to get FurnitureData from the object
        FurnitureData data = obj.GetComponentInChildren<FurnitureData>(true);

        if (data != null)
        {
            // Use FurnitureData if available
            itemNames.Add(data.furnitureName);
            itemPrices.Add(data.price);
            totalCost += data.price;

            Debug.Log("💰 Added: " + data.furnitureName + " | Rs. " + data.price);
        }
        else if (pendingItemPrice > 0)
        {
            // Use pending price from FurnitureCardUI
            itemNames.Add(pendingItemName);
            itemPrices.Add(pendingItemPrice);
            totalCost += pendingItemPrice;

            Debug.Log("💰 Added (from card): " + pendingItemName + " | Rs. " + pendingItemPrice);

            // Clear pending
            pendingItemName = "";
            pendingItemPrice = 0f;
        }
        else
        {
            // 🔥 SMART FALLBACK: If no price data, try to guess from the object name (Safe for Reloading)
            string cleanedName = obj.name.Replace("(Clone)", "").Trim();
            string lowerName = cleanedName.ToLower();
            float fallbackPrice = 0f;
            string displayName = cleanedName;

            if (lowerName.Contains("table")) { fallbackPrice = 10000f; displayName = "Study Table"; }
            else if (lowerName.Contains("bed")) { fallbackPrice = 15000f; displayName = "Bed"; }
            else if (lowerName.Contains("sofa") || lowerName.Contains("chair")) { fallbackPrice = 5000f; displayName = "Seating"; }

            itemNames.Add(displayName);
            itemPrices.Add(fallbackPrice);
            totalCost += fallbackPrice;

            Debug.Log($"⚠️ Smart Fallback for {cleanedName}: Rs. {fallbackPrice}");
        }

        Debug.Log("💰 New Total: " + totalCost);
        UpdateUI();
    }

    // =========================
    // REMOVE ITEM (OPTIONAL)
    // =========================
    public void RemoveItem(GameObject obj)
    {
        if (obj == null) return;

        // Try FurnitureData first
        FurnitureData data = obj.GetComponentInChildren<FurnitureData>(true);

        if (data != null && itemNames.Count > 0)
        {
            int idx = itemNames.IndexOf(data.furnitureName);
            if (idx >= 0)
            {
                totalCost -= itemPrices[idx];
                itemNames.RemoveAt(idx);
                itemPrices.RemoveAt(idx);
                Debug.Log("🗑️ Removed: " + data.furnitureName);
            }
        }
        else if (itemNames.Count > 0)
        {
            // Remove the last item if we can't identify it
            int lastIdx = itemNames.Count - 1;
            totalCost -= itemPrices[lastIdx];
            Debug.Log("🗑️ Removed: " + itemNames[lastIdx]);
            itemNames.RemoveAt(lastIdx);
            itemPrices.RemoveAt(lastIdx);
        }

        Debug.Log("💰 New Total: " + totalCost);
        UpdateUI();
    }

    // =========================
    // UPDATE UI
    // =========================
    public void UpdateUI()
    {
        if (totalCostText != null)
        {
            totalCostText.enableAutoSizing = true;
            totalCostText.fontSizeMin = 14;
            totalCostText.fontSizeMax = 43;
            totalCostText.color = Color.white; // Force text explicitly to White

            // Also ensure the parent background is clearly colored
            if (totalCostText.transform.parent != null)
            {
                UnityEngine.UI.Image bgImage = totalCostText.transform.parent.GetComponent<UnityEngine.UI.Image>();
                if (bgImage != null)
                {
                    if (maxBudget > 0 && totalCost > maxBudget)
                    {
                        // Exceeded Budget - Red Warning
                        totalCostText.text = "Total Cost: Rs. " + totalCost.ToString("N0") + "\n<color=#FF5555>⚠️ OVER BUDGET!</color>";
                        bgImage.color = new Color(0.8f, 0f, 0f, 0.8f); // Flashy semi-transparent Red
                    }
                    else
                    {
                        // Under Budget - Normal
                        totalCostText.text = "Total Cost: Rs. " + totalCost.ToString("N0") + "\n<size=50%><color=#AAAAAA>Budget: Rs. " + maxBudget.ToString("N0") + "</color></size>";
                        bgImage.color = new Color(0f, 0f, 0f, 0.6f); // Normal semi-transparent Black
                    }
                }
            }
        }
        else
        {
            Debug.LogError("❌ totalCostText not assigned!");
        }
    }

    // =========================
    // RESET COST
    // =========================
    public void ResetCost()
    {
        itemNames.Clear();
        itemPrices.Clear();
        totalCost = 0f;

        Debug.Log("🔄 Cost reset");

        UpdateUI();
    }

    // =========================
    // EXPORT DATA FOR SAVING
    // =========================
    public float GetTotalCost()
    {
        return totalCost;
    }

    public int GetPlacedItemsCount()
    {
        return itemNames.Count;
    }

    public Dictionary<string, int> GetInventorySummary()
    {
        Dictionary<string, int> summary = new Dictionary<string, int>();
        foreach (string name in itemNames)
        {
            if (summary.ContainsKey(name)) summary[name]++;
            else summary[name] = 1;
        }
        return summary;
    }
}