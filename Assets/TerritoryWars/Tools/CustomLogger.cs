using System.Collections.Generic;

namespace TerritoryWars.Tools
{
    public enum LogType
    {
        Info,
        Warning,
        ModelUpdate,
        Error,
        Important
    }
    
    public static class CustomLogger
    {
        public static Dictionary<LogType, string> LogTypeColors = new Dictionary<LogType, string>
        {
            {LogType.Info, "#808080"},          // Сірий колір для звичайної інформації
            {LogType.Warning, "#FFA500"},       // Помаранчевий для попереджень
            {LogType.ModelUpdate, "#32CD32"},   // Лайм-зелений для оновлень моделі
            {LogType.Error, "#DC143C"},         // Темно-червоний для помилок  
            {LogType.Important, "#8A2BE2"}      // Фіолетовий для важливих повідомлень
        };
        
        public static Dictionary<LogType, bool> LogTypeEnabled = new Dictionary<LogType, bool>
        {
            {LogType.Info, true},
            {LogType.Warning, true},
            {LogType.ModelUpdate, true},
            {LogType.Error, true},
            {LogType.Important, true}
        };
        
        public static void Log(LogType logType, string message)
        {
            if (!LogTypeEnabled[logType]) return;
            string color = LogTypeColors[logType];
            string logMessage = $"<color={color}>[{logType}]: {message}</color>";
            UnityEngine.Debug.Log(logMessage);
        }
        
        public static void LogInfo(string message)
        {
            Log(LogType.Info, message);
        }
        
        public static void LogWarning(string message)
        {
            Log(LogType.Warning, message);
        }
        
        public static void LogModelUpdate(string message)
        {
            Log(LogType.ModelUpdate, message);
        }
        
        public static void LogError(string message)
        {
            Log(LogType.Error, message);
        }
        
        public static void LogImportant(string message)
        {
            Log(LogType.Important, message);
        }
        
        
    }
}