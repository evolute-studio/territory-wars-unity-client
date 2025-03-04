using System.Collections.Generic;

namespace TerritoryWars.Tools
{
    public enum LogType
    {
        Info,
        Warning,
        Event,
        Error,
        Important,
        DojoLoop,
    }
    
    public static class CustomLogger
    {
        public static Dictionary<LogType, string> LogTypeColors = new Dictionary<LogType, string>
        {
            {LogType.Info, "#808080"},      
            {LogType.Warning, "#FFA500"},    
            {LogType.Event, "#32CD32"}, 
            {LogType.Error, "#DC143C"},        
            {LogType.Important, "#9441e0"},
            {LogType.DojoLoop, "#FFD700"}
        };
        
        public static Dictionary<LogType, bool> LogTypeEnabled = new Dictionary<LogType, bool>
        {
            {LogType.Info, true},
            {LogType.Warning, true},
            {LogType.Event, true},
            {LogType.Error, true},
            {LogType.Important, true},
            {LogType.DojoLoop, true}
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
        
        public static void LogDojoLoop(string message)
        {
            Log(LogType.DojoLoop, message);
        }
        
        
    }
}