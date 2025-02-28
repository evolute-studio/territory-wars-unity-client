using TerritoryWars.General;
using UnityEngine;

public class StructureDebugger : MonoBehaviour
{
    private StructureChecker structureChecker;
    private Board board;
    private Camera mainCamera;

    public void Initialize(StructureChecker checker, Board board)
    {
        structureChecker = checker;
        this.board = board;
        mainCamera = Camera.main;
    }

    private void OnGUI()
    {
        if (structureChecker == null || board == null) return;

        // first we display roads (they will be at the bottom)
        foreach (var roadEntry in structureChecker.RoadMap)
        {
            DrawStructureInfo(roadEntry.Value, Color.yellow, false);
        }

        // then cities (they will be on top)
        foreach (var cityEntry in structureChecker.CityMap)
        {
            DrawStructureInfo(cityEntry.Value, Color.cyan, true);
        }
    }

    private void DrawStructureInfo(Structure structure, Color color, bool isCity)
    {
        // get the world position of the tile
        Vector3 worldPosition = board.GetTilePosition(structure.Position.x, structure.Position.y);
        
        // convert the world position to screen coordinates
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        
        // if the position is off-screen, don't draw
        if (screenPos.z < 0) return;

        Structure root = structureChecker.FindRoot(structure);
        string status = structureChecker.CheckCityCompletion(root) ? "Fin" : "N Fin";
        
        // create a style for the text with a reduced size
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = color },
            fontSize = 10 // reduced font size
        };

        // calculate the size and position of the rectangle for the text
        string text = $"{(isCity ? "C" : "R")}\n" +
                     $"Open: {structure.OpenEdges}\n" +
                     $"Root: {(root == structure ? "One" : root.Position.ToString())}\n" +
                     $"Status: {status}";
        if (status == "Fin")
        {
            style.normal.textColor = Color.green;
            text = "Structure\nCompleted";
        }
        
        Vector2 textSize = style.CalcSize(new GUIContent(text));
        
        // offset depending on the type of structure
        float xOffset = isCity ? -60 : 60; // cities on top, roads on the bottom
        
        Rect labelRect = new Rect(
            screenPos.x - textSize.x / 2 + xOffset,
            Screen.height - screenPos.y,
            textSize.x + 10,
            textSize.y + 10
        );

        // draw a background for better readability
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(labelRect, Texture2D.whiteTexture);
        
        // return the color back and draw the text
        GUI.color = Color.white;
        GUI.Label(labelRect, text, style);
    }
} 