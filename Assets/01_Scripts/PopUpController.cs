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
        rectTr = GetComponent<RectTransform>(); //스크립트 위치의 Rect Transform
        OnClickOriginPositionBtn(); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) // 마우스 왼쪽 클릭되었는지 확인
        {
            rectTr.anchoredPosition += eventData.delta * 2; // 드래그 이벤트 함수 선언 및 등록
        }
    }

    public void OnClickOriginPositionBtn()
    {
        rectTr.localPosition = new Vector3(600f, 0f, 0f);   // localPosition
        rectTr.localScale = Vector3.one;
    }

}
