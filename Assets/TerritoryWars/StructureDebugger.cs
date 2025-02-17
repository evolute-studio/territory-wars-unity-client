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

        // Спочатку відображаємо дороги (вони будуть знизу)
        foreach (var roadEntry in structureChecker.RoadMap)
        {
            DrawStructureInfo(roadEntry.Value, Color.yellow, false);
        }

        // Потім міста (вони будуть зверху)
        foreach (var cityEntry in structureChecker.CityMap)
        {
            DrawStructureInfo(cityEntry.Value, Color.cyan, true);
        }
    }

    private void DrawStructureInfo(Structure structure, Color color, bool isCity)
    {
        // Отримуємо світову позицію тайлу
        Vector3 worldPosition = board.GetTilePosition(structure.Position.x, structure.Position.y);
        
        // Конвертуємо світову позицію в екранні координати
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Якщо позиція поза екраном, не малюємо
        if (screenPos.z < 0) return;

        Structure root = structureChecker.FindRoot(structure);
        string status = structureChecker.CheckCityCompletion(root) ? "Fin" : "N Fin";
        
        // Створюємо стиль для тексту зі зменшеним розміром
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = color },
            fontSize = 10 // Зменшений розмір шрифту
        };

        // Розраховуємо розмір та позицію прямокутника для тексту
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
        
        // Зміщення в залежності від типу структури
        float xOffset = isCity ? -60 : 60; // Міста зверху, дороги знизу
        
        Rect labelRect = new Rect(
            screenPos.x - textSize.x / 2 + xOffset,
            Screen.height - screenPos.y,
            textSize.x + 10,
            textSize.y + 10
        );

        // Малюємо фон для кращої читабельності
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(labelRect, Texture2D.whiteTexture);
        
        // Повертаємо колір назад і малюємо текст
        GUI.color = Color.white;
        GUI.Label(labelRect, text, style);
    }
} 