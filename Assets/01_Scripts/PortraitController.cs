using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitController : MonoBehaviour
{
    public Button selfObject;

    public void Start()
    {
        selfObject = GetComponent<Button>();
        selfObject.onClick.AddListener(OnClickPortrait);
    }

    private void OnClickPortrait()
    {
        UIManager.Instance.faceImage.GetComponent<RawImage>().texture = GoogleDriveManager.Instance.textureDictionary[gameObject.name];
        PortraitInfoManager.Instance.currentPortraitName = gameObject.name;
    }

}
