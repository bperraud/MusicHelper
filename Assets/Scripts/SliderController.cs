using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderController : MonoBehaviour, IEndDragHandler, IPointerDownHandler
{
    [SerializeField] private TMP_Text m_SliderValueText;

    private Slider m_Slider;
    private GameObject m_SliderGameObject;

    private bool m_IsDragging;

    private void Awake()
    {
        m_Slider = GetComponent<Slider>();
        m_SliderGameObject = m_SliderValueText.gameObject;
    }

    private void Update()
    {
        if (m_SliderGameObject.activeSelf && m_IsDragging)
        {
            m_SliderValueText.text = m_Slider.value.ToString("G");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        m_SliderGameObject.SetActive(true);
        m_IsDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_SliderGameObject.SetActive(false);
        m_IsDragging = false;
    }
}