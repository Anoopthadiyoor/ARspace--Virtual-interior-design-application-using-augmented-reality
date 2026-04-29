using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DesignSlotCard : MonoBehaviour
{
    [Header("UI References")]
    public RawImage thumbnailImage;
    public TMP_Text costText;
    public TMP_Text inventoryText;
    public Button loadButton;
    public Button deleteButton;

    private string slotId;
    private SaveData designData;

    public void Setup(string id, SaveData data)
    {
        this.slotId = id;
        this.designData = data;

        // 🛡️ FINGERPRINT HANDSHAKE: Verify that this data actually matches this slot!
        bool isLegacy = string.IsNullOrEmpty(data.slotId);
        bool isMatch = !isLegacy && data.slotId == id;

        // 1. Set Text Data immediately
        if (costText != null)
            costText.text = data.totalCost > 0 ? "Rs. " + data.totalCost : "No Cost Data";

        if (inventoryText != null)
        {
            string summary = isLegacy ? "<b>[LEGACY DESIGN]</b>\n" : "";
            
            if (!isMatch && !isLegacy)
            {
                summary += "<color=red>⚠️ DATA INTEGRITY ERROR!</color>\nThis data does not match the photo.";
            }
            else if (data.inventorySummary != null && data.inventorySummary.Count > 0)
            {
                foreach (var item in data.inventorySummary)
                    summary += $"{item.Key} x{item.Value}\n";
            }
            else
            {
                summary += "No Items Found";
            }
            inventoryText.text = summary;
        }

        // 2. Clear old click listeners
        if (loadButton != null)
        {
            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(OnLoadClicked);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        // 3. Start Loading Thumbnail privately
        if (thumbnailImage != null)
        {
            // Set a tiny placeholder color or clear it
            thumbnailImage.texture = null;
            thumbnailImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light Gray
            LoadThumbnailAsync();
        }
    }

    private async void LoadThumbnailAsync()
    {
        // 🧪 Verification: Hold local reference to slotId to prevent mid-task swaps
        string targetId = this.slotId;
        
        byte[] imgData = await CloudSaveManager.Instance.GetScreenshotAsync(targetId);
        
        // Safety check: is this card still relevant and not destroyed?
        if (this == null || thumbnailImage == null) return;
        
        // Final Sync Check: Did the ID change while we were waiting?
        if (targetId != this.slotId) return;

        if (imgData != null && imgData.Length > 0)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imgData);
            thumbnailImage.texture = tex;
            thumbnailImage.color = Color.white;
        }
    }

    private void OnLoadClicked()
    {
        // Call back to the Manager to handle the actual loading logic
        SaveLoadUIManager.Instance.LoadDesignDetails(slotId, designData);
    }

    private async void OnDeleteClicked()
    {
        bool success = await CloudSaveManager.Instance.DeleteDesignAsync(slotId);
        if (success)
        {
            // Destroy this card instantly from the UI
            Destroy(gameObject);
        }
    }
}
