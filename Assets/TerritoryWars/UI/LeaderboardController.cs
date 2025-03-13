using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] private Image[] _leadersImages;
    [SerializeField] private GameObject _placeHolder;
    [SerializeField] private Transform ListItemParent;


    // public LeaderboardItem CreateLeaderboardItem()
    // {
    //     
    // }
    
    
    
    public void SetActivePlaceHolder(bool isActive)
    {
        _placeHolder.SetActive(isActive);
    }
    
    


    public class LeaderboardItem
    {
        public GameObject ListItem;
        public string PlayerName;
        public int EvoluteCount;

        private TextMeshProUGUI _playerNameText;
        private TextMeshProUGUI _evoluteCount;
        private Image _leaderPlaceImage;

        public LeaderboardItem(GameObject listItem)
        {
            ListItem = listItem;
            _playerNameText = listItem.transform.Find("Name/NameText").GetComponent<TextMeshProUGUI>();
            _evoluteCount = listItem.transform.Find("EvoluteCount/Count/EvoluteCountText")
                .GetComponent<TextMeshProUGUI>();
            _leaderPlaceImage = listItem.transform.Find("LeaderPlace/LeaderPlaceImage").GetComponent<Image>();
        }

        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }

        public void SetLeaderPlace(int place)
        {
            
        }
    }
}
