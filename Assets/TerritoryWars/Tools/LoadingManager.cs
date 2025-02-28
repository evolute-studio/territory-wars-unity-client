using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace TerritoryWars.Tools
{


public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;
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
    
    public List<GameObject> LoadingObjects = new List<GameObject>();
    [SerializeField] private Image _loadingBarFill;
    [SerializeField] private TextMeshProUGUI _loadingText;
    public float LoadingTime = 5f;
    public bool PlayOnAwake = false;

    public void Start()
    {
        // add children to list
        foreach (Transform child in transform)
        {
            LoadingObjects.Add(child.gameObject);
        }
        if (PlayOnAwake)
        {
            Play();
        }
    }
    public void Play(float loadingTime = default)
    {
        if (loadingTime != default)
        {
            LoadingTime = loadingTime;
        }
        SetActiveScreen(true);
        StartCoroutine(FillProgressBar());
        StartCoroutine(LoadingTextCoroutine());
    }
    
    public void SetActiveScreen(bool isActive)
    {
        foreach (var loadingObject in LoadingObjects)
        {
            loadingObject.SetActive(isActive);
        }
    }
    

    // public void Awake()
    // {
    //     // initialize
    //     Initialize();
    // }

    private IEnumerator FillProgressBar()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < LoadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / LoadingTime);
            _loadingBarFill.fillAmount = progress;
            yield return null;
        }
        SetActiveScreen(false);
    }
    
    // Кожні 0,5 секунд точка
    
    private IEnumerator LoadingTextCoroutine()
    {
        string text = "Loading";
        float delay = 0.5f;
        
        while (true)
        {
            _loadingText.text = text;
            yield return new WaitForSeconds(delay);
            text += ".";
            if (text.Length > 10)
            {
                text = "Loading";
            }
        }


    }

}
}