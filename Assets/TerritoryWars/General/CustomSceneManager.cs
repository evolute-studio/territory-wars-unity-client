using System.Collections;
using System.Threading.Tasks;
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
            }
        }
        #endregion
        
        
        
        private float _progress;
        private bool _isLoading;

        public delegate void SimpleEvent();

        public SimpleEvent OnLoadingStart;
        public SimpleEvent OnLoadingFinish;
        
        public string Menu = "Menu";
        public string Session = "Session";
        
        public string CurrentScene => SceneManager.GetActiveScene().name;
        
        public delegate void LoadSceneEvent(string name);
        public event LoadSceneEvent OnLoadScene;

        public void LoadLobby(float wait = 0)
        {
            //Game.SetLobbyState();
            LoadSceneWithDealay(Menu, wait);
        }

        public void LoadSession(float wait = 0)
        {
            Debug.Log("LoadSession");
           // Game.SetSessionState();
            LoadSceneWithDealay(Session, wait);
        }

        public void LoadScene(string name, float wait = 0)
        {
            if (_isLoading)
                return;
            StartCoroutine(LoadSceneAsync(name, wait));
        }
        
        public async void LoadSceneWithDealay(string name, float delay)
        {
            await Task.Delay((int) (delay * 1000));
            LoadScene(name);
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