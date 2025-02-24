using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnapshotTabController : MonoBehaviour
{
    public GameObject PanelGameObject;
    public GameObject SnapshotListItemPrefab;
    public Transform ListItemParent;
    
    private List<SnapshotListItem> _snapshotListItems = new List<SnapshotListItem>();
    
    public void Start() => Initialize();
    
    public void Initialize()
    {
        
        
    }
    
    public SnapshotListItem CreateListItem()
    {
        GameObject listItem = Instantiate(SnapshotListItemPrefab, ListItemParent);
        SnapshotListItem snapshotListItem = new SnapshotListItem(listItem);
        _snapshotListItems.Add(snapshotListItem);
        return snapshotListItem;
    }
    
}

public class SnapshotListItem
{
    public GameObject ListItem;
    public string CreatorPlayerName;
    public int CreatorPlayerEvoluteCount;
    
    private TextMeshProUGUI _creatorPlayerNameText;
    private TextMeshProUGUI _creatorPlayerEvoluteCountText;
    private TextMeshProUGUI _tileLeftCountText;
    private Button _seeMapButton;
    private Button _restoreButton;

    public SnapshotListItem(GameObject listItem)
    {
        ListItem = listItem;
        _creatorPlayerNameText = listItem.transform.Find("PlayerName/PlayerNameText").GetComponent<TextMeshProUGUI>();
        _creatorPlayerEvoluteCountText = listItem.transform.Find("EvoluteCount/Count").GetComponent<TextMeshProUGUI>();
        _tileLeftCountText = listItem.transform.Find("TileLeft/TilesLeftText").GetComponent<TextMeshProUGUI>();
        _seeMapButton = listItem.transform.Find("Buttons/MapButton").GetComponent<Button>();
        _restoreButton = listItem.transform.Find("Buttons/RestoreButton").GetComponent<Button>();
    }

    public void UpdateItem(string creatorPlayerName, int creatorPlayerEvoluteCount, int tileLeftCount)
    {
        CreatorPlayerName = creatorPlayerName;
        CreatorPlayerEvoluteCount = creatorPlayerEvoluteCount;
        
        _creatorPlayerNameText.text = creatorPlayerName;
        _creatorPlayerEvoluteCountText.text = creatorPlayerEvoluteCount.ToString();
        _tileLeftCountText.text = tileLeftCount.ToString();
    }
    
    public void SetActive(bool isActive)
    {
        ListItem.SetActive(isActive);
    }
}
