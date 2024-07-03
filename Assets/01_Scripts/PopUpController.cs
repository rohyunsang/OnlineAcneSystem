using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopUpController : MonoBehaviour, IDragHandler
{
    //Drag 
    private RectTransform rectTr;

    private void Start()
    {
        rectTr = GetComponent<RectTransform>(); //��ũ��Ʈ ��ġ�� Rect Transform
        OnClickOriginPositionBtn(); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) // ���콺 ���� Ŭ���Ǿ����� Ȯ��
        {
            rectTr.anchoredPosition += eventData.delta * 2; // �巡�� �̺�Ʈ �Լ� ���� �� ���
        }
    }

    public void OnClickOriginPositionBtn()
    {
        rectTr.localPosition = new Vector3(600f, 0f, 0f);   // localPosition
        rectTr.localScale = Vector3.one;
    }

}
