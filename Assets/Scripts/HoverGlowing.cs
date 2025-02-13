using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HoverGlowing : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button _button;
    private TextMeshProUGUI _text;
    private MaterialPropertyBlock _propertyBlock;
    
    private void Start()
    {
        _button = GetComponent<Button>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _propertyBlock = new MaterialPropertyBlock();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        _text.fontMaterial.EnableKeyword("GLOW_ON");
        _text.fontMaterial.SetFloat("_GlowPower", 0.4f);
        _text.fontMaterial.SetFloat("_GlowOffset", 1f);
        _text.fontMaterial.SetFloat("_GlowOuter", 0.785f);
        _text.fontMaterial.SetFloat("_GlowInner", 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _text.fontMaterial.SetFloat("_GlowPower", 0f);
    }
}
