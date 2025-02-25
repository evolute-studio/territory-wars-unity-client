using System;
using System.Text;
using System.Linq;
using Dojo.Starknet;
using TerritoryWars.Tools;

namespace TerritoryWars.ModelsDataConverters
{
    public static class CairoFieldsConverter
    {
        private const int MAX_SHORT_STRING_LENGTH = 31;

        public static string GetStringFromFieldElement(FieldElement fieldElement)
        {
            try
            {
                var hexString = fieldElement.Hex().ToString();
                
                // Пропускаємо префікс "0x" якщо він є
                if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    hexString = hexString.Substring(2);
                }

                // Перевіряємо чи hex-рядок має парну кількість символів
                if (hexString.Length % 2 != 0)
                {
                    hexString = "0" + hexString;
                }

                // Конвертуємо hex-рядок в байти
                var bytes = Enumerable.Range(0, hexString.Length / 2)
                    .Select(x => Convert.ToByte(hexString.Substring(x * 2, 2), 16))
                    .ToArray();

                // Конвертуємо байти в ASCII рядок та видаляємо нульові символи
                var result = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                
                // Видаляємо неприпустимі ASCII символи
                return new string(result.Where(c => c >= 32 && c <= 126).ToArray());
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error while converting FieldElement to string: {e.Message}");
                return string.Empty; // Повертаємо пустий рядок у випадку помилки
            }
        }
        
        public static FieldElement GetFieldElementFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new FieldElement("0");

            if (value.Length > MAX_SHORT_STRING_LENGTH)
                throw new ArgumentException($"String is too long. Maximum length is {MAX_SHORT_STRING_LENGTH} characters");

            // Перевіряємо чи всі символи є ASCII
            if (value.Any(c => c > 127))
                throw new ArgumentException("String contains non-ASCII characters");

            // Конвертуємо рядок в ASCII байти
            var bytes = Encoding.ASCII.GetBytes(value);
            
            // Конвертуємо байти в hex рядок
            var hexString = "0x" + BitConverter.ToString(bytes).Replace("-", "");
            
            return new FieldElement(hexString);
        }
    }
}