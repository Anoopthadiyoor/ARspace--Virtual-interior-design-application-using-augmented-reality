using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotManager : MonoBehaviour
{
    public static ScreenshotManager Instance;

    public GameObject[] uiElementsToHide; // Assign the Canvas or main UI panel here

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CaptureScreenshotAndSave(System.Action<byte[]> onCaptureComplete)
    {
        StartCoroutine(CaptureRoutine(onCaptureComplete));
    }

    private IEnumerator CaptureRoutine(System.Action<byte[]> onCaptureComplete)
    {
        // 1. Hide UI
        foreach (var ui in uiElementsToHide)
        {
            if (ui != null) ui.SetActive(false);
        }

        // 2. Wait for UI to actually hide and frame to finish rendering
        yield return new WaitForEndOfFrame();

        // 3. Take screenshot
        Texture2D screenImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenImage.Apply();

        // 4. Resize to a thumbnail to save cloud space (e.g. 512x288 depending on aspect ratio)
        Texture2D thumbnail = ResizeTexture(screenImage, 512);

        // 5. Encode to JPG (highly compressed compared to PNG)
        byte[] imageBytes = thumbnail.EncodeToJPG(75);

        // Clean up unmanaged texture memory
        Destroy(screenImage);
        Destroy(thumbnail);

        // 6. Restore UI
        foreach (var ui in uiElementsToHide)
        {
            if (ui != null) ui.SetActive(true);
        }

        // 7. Return bytes
        onCaptureComplete?.Invoke(imageBytes);
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth)
    {
        float ratio = (float)source.height / source.width;
        int targetHeight = Mathf.RoundToInt(targetWidth * ratio);

        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }
}
