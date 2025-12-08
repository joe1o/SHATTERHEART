using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    
    [Header("Custom Cursor")]
    public Texture2D cursorTexture;
    
    [Header("Cursor Size")]
    public int maxCursorSize = 32; // Maximum size for the cursor
    
    [Header("Hotspot Position")]
    [Tooltip("Where the click happens. (0,0) = top-left, (0.5,0.5) = center")]
    [Range(0f, 1f)] public float hotspotX = 0.5f; // 0 = left, 1 = right
    [Range(0f, 1f)] public float hotspotY = 0.5f; // 0 = top, 1 = bottom
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetCustomCursor();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void SetCustomCursor()
    {
        if (cursorTexture != null)
        {
            // Scale down the cursor if it's too large
            Texture2D scaledCursor = cursorTexture;
            int finalWidth = Mathf.Min(cursorTexture.width, maxCursorSize);
            int finalHeight = Mathf.Min(cursorTexture.height, maxCursorSize);
            
            if (cursorTexture.width > maxCursorSize || cursorTexture.height > maxCursorSize)
            {
                scaledCursor = ScaleTexture(cursorTexture, finalWidth, finalHeight);
            }
            
            // Calculate hotspot based on percentage of final cursor size
            Vector2 hotspot = new Vector2(
                finalWidth * hotspotX,
                finalHeight * hotspotY
            );
            
            Cursor.SetCursor(scaledCursor, hotspot, CursorMode.Auto);
            Debug.Log($"Custom cursor set! Size: {scaledCursor.width}x{scaledCursor.height}, Hotspot: {hotspot}");
        }
        else
        {
            Debug.LogWarning("Cursor texture is not assigned!");
        }
    }
    
    Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        
        float ratioX = ((float)source.width) / targetWidth;
        float ratioY = ((float)source.height) / targetHeight;
        
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                int sourceX = Mathf.FloorToInt(x * ratioX);
                int sourceY = Mathf.FloorToInt(y * ratioY);
                result.SetPixel(x, y, source.GetPixel(sourceX, sourceY));
            }
        }
        
        result.Apply();
        return result;
    }
    
    // Call this if you need to refresh the cursor
    public void RefreshCursor()
    {
        SetCustomCursor();
    }
}
