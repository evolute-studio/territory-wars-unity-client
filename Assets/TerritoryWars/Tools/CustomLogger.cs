using System.Collections.Generic;

namespace TerritoryWars.Tools
{
    public enum LogType
    {
        Info,
        Warning,
        Event,
        Error,
        Important
    }
    
    public static class CustomLogger
    {
        public static Dictionary<LogType, string> LogTypeColors = new Dictionary<LogType, string>
        {
            {LogType.Info, "#808080"},          // Сірий колір для звичайної інформації
            {LogType.Warning, "#FFA500"},       // Помаранчевий для попереджень
            {LogType.Event, "#32CD32"},   // Лайм-зелений для оновлень моделі
            {LogType.Error, "#DC143C"},         // Темно-червоний для помилок  
            {LogType.Important, "#9441e0"}      // Фіолетовий для важливих повідомлень
        };
        
        public static Dictionary<LogType, bool> LogTypeEnabled = new Dictionary<LogType, bool>
        {
            {LogType.Info, true},
            {LogType.Warning, true},
            {LogType.Event, true},
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
        
        public static void LogEvent(string message)
        {
            Log(LogType.Event, message);
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