using UnityEngine;

namespace TerritoryWars
{

    [System.Serializable]
    public class TileData
    {
        public string id;
        private char[] sides;
        public int rotationIndex = 0;

        public TileData(string tileCode)
        {
            sides = tileCode.ToCharArray();
            UpdateId();
        }

        private void UpdateId()
        {
            // Створюємо новий id на основі поточного повороту
            char[] rotatedSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                // Для повороту за годинниковою стрілкою:
                // Якщо rotationIndex = 1, то:
                // Top(0) -> Right(1)
                // Right(1) -> Bottom(2)
                // Bottom(2) -> Left(3)
                // Left(3) -> Top(0)
                int sourceIndex = (i - rotationIndex + 4) % 4;
                rotatedSides[i] = sides[sourceIndex];
            }
            id = new string(rotatedSides);
        }

        public void Rotate(int times = 1)
        {
            // Додаємо поворот за годинниковою стрілкою
            rotationIndex = (rotationIndex + times) % 4;
            UpdateId();
            Debug.Log($"Rotated tile. New rotation index: {rotationIndex}, New ID: {id}");
        }

        public LandscapeType GetSide(Side side)
        {
            // Враховуємо поворот при отриманні типу сторони
            int index = ((int)side - rotationIndex + 4) % 4;
            return CharToLandscape(sides[index]);
        }

        private LandscapeType CharToLandscape(char c)
        {
            return c switch
            {
                'C' => LandscapeType.City,
                'R' => LandscapeType.Road,
                'F' => LandscapeType.Field,
                _ => throw new System.ArgumentException($"Invalid landscape type: {c}")
            };
        }
    }
}