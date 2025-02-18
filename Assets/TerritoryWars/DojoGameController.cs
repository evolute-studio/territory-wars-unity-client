using System;
using Dojo.Starknet;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars
{
    public class DojoGameController : MonoBehaviour
    {
        public Game gameSystem;
        [FormerlySerializedAs("gameManagerDojo")] public DojoGameManager dojoGameManager;
        
        private Account localPlayer => dojoGameManager.LocalBurnerAccount;

        private string logMessages = "";
        private Vector2 scrollPosition;
        private GUIStyle buttonStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle logStyle;
        private bool stylesInitialized;
        
        private void Awake()
        {
            // Підписка на події логування
            Application.logMessageReceived += HandleLog;
        }
        
        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }
        
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string prefix = type == LogType.Error ? "ERROR: " : 
                           type == LogType.Warning ? "WARNING: " : "INFO: ";
            logMessages = $"{prefix}{logString}\n{logMessages}";
        }

        public async void CreateGame()
        {
            try
            {
                var txHash = await gameSystem.create_game(localPlayer);
                Debug.Log($"Create Game: {txHash}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public async void CancelGame()
        {
            try
            {
                var txHash = await gameSystem.cancel_game(localPlayer);
                Debug.Log($"Cancel Game: {txHash}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async void JoinGame(string hostPlayer)
        {
            try
            {
                var txHash = await gameSystem.join_game(localPlayer, new FieldElement(hostPlayer));
                Debug.Log($"Join Game: {txHash}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        private void InitStyles()
        {
            if (!stylesInitialized)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = (int)(GUI.skin.button.fontSize * 1.5f);
                
                textFieldStyle = new GUIStyle(GUI.skin.textField);
                textFieldStyle.fontSize = (int)(GUI.skin.textField.fontSize * 1.5f);
                
                logStyle = new GUIStyle(GUI.skin.textArea);
                logStyle.fontSize = (int)(GUI.skin.textArea.fontSize * 1.5f);
                
                stylesInitialized = true;
            }
        }

        private void OnGUI()
        {
            InitStyles();
            
            // Область для кнопок та вводу
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            
            if (GUILayout.Button("Create Game", buttonStyle))
            {
                CreateGame();
            }
            
            if (GUILayout.Button("Cancel Game", buttonStyle))
            {
                CancelGame();
            }
            
            GUILayout.Label("Host Address:", buttonStyle);
            hostAddress = GUILayout.TextField(hostAddress, textFieldStyle, GUILayout.Width(280));
            
            if (GUILayout.Button("Join Game", buttonStyle))
            {
                if (!string.IsNullOrEmpty(hostAddress))
                {
                    JoinGame(hostAddress);
                }
                else
                {
                    Debug.LogWarning("Please enter the host address");
                }
            }
            
            GUILayout.EndArea();
            
            // Область для логів
            GUILayout.BeginArea(new Rect(10, 220, 500, 400));
            GUILayout.Label("Logs:", buttonStyle);
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, 
                GUI.skin.box, GUILayout.Width(480), GUILayout.Height(380));
            
            GUILayout.TextArea(logMessages, logStyle);
            
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private string hostAddress = "";
    }
}