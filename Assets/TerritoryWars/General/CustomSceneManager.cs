using System;
using System.Collections;
using System.Threading.Tasks;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace TerritoryWars.General
{
    public class CustomSceneManager : MonoBehaviour
    {
        #region Singleton
        public static CustomSceneManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion
        
        
        
        private float _progress;
        private bool _isLoading;

        public LoadingScreen LoadingScreen;

        public delegate void SimpleEvent();

        public SimpleEvent OnLoadingStart;
        public SimpleEvent OnLoadingFinish;
        
        public string Menu = "Menu";
        public string Session = "Session";
        
        public string CurrentScene => SceneManager.GetActiveScene().name;
        
        public delegate void LoadSceneEvent(string name);
        public event LoadSceneEvent OnLoadScene;

        
        public void LoadLobby(Action startAction = null, Action finishAction = null)
        {
            CursorManager.Instance.SetCursor("default");
            ApplicationState.SetState(ApplicationStates.Menu);
            LoadSceneWithDealay(Menu, startAction, finishAction);
        }

        public void LoadSession(Action startAction = null, Action finishAction = null)
        {
            Debug.Log("LoadSession");
            ApplicationState.SetState(ApplicationStates.Session);
            LoadSceneWithDealay(Session, startAction, finishAction);
        }
        
        public async void LoadSceneWithDealay(string name, Action startAction = null, Action finishAction = null)
        {
            if (startAction != null)
                startAction.Invoke();
            if (finishAction != null)
            {
                OnLoadingFinish += OnFinish;
                void OnFinish()
                {
                    OnLoadingFinish -= OnFinish;
                    finishAction.Invoke();
                }
            }
            
            //await Task.Delay((int) (delay * 1000));
            LoadScene(name);
        }

        public void LoadScene(string name)
        {
            if (_isLoading)
                return;
            StartCoroutine(LoadSceneAsync(name));
        }
        
        public IEnumerator LoadSceneAsync(string sceneName, float wait = 0)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            OnLoadingStart?.Invoke();
            _isLoading = true;
            float time = Time.time;
            
            while( !operation.isDone && Time.time - time < 1)
            {
                _progress = operation.progress;
                   
                if( _progress >= 0.9f )
                {
                    // Almost done.
                    break;
                }
     
                yield return null;
            }
            
            // Allow new scene to start.
            if(_progress != 0)
                operation.allowSceneActivation = true;
            else 
                SceneManager.LoadScene(sceneName);
            yield return new WaitForSeconds(1f);
            _isLoading = false; 
            OnLoadingFinish?.Invoke();
            OnLoadScene?.Invoke(sceneName);
            yield return null;
        }
    }
}