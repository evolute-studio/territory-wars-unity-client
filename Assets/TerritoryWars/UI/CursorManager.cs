using System;
using UnityEngine;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;
    
    // Different cursor types you might want
    public Texture2D defaultCursor;
    public Texture2D pointerCursor;
    // etc.

    // Corresponding hotspots
    public Vector2 defaultHotspot;
    public Vector2 pointerHotspot;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
 
    }

    private void Start()
    {
        SetCursor("default");
    }

    public void SetCursor(string cursorType)
    {
        switch(cursorType.ToLower())
        {
            case "pointer":
                Cursor.SetCursor(pointerCursor, pointerHotspot, CursorMode.Auto);
                break;
            default:
                Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
                break;
        }
    }
}