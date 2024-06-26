using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    #region SingleTon Pattern
    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        // Set this as the instance and ensure it persists across scenes
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

    }
    #endregion

    [Header("Panels")]
    public GameObject initPanel;
    public GameObject mainPanel;
    public GameObject settingPanel;

    [Header("InitPanel")]
    public Transform folderParent;  // InitPanel - ScrollView - Content
    public Text selectedFolderNameText;   // InitPanel - CheckPasswordInfo
    public GameObject checkFolderInfo;
    public Button checkFolderButton;
    public GameObject reLoginInfo;

    [Header("MainPanel")]
    public Transform subFolderScrollView;  // ScrollView - content 
    public Transform portraitScrollView;   // ScrollView - content 
    public RawImage faceImage;      // 작업 캔버스 이미지 

    [Header("SettingPanel")]
    public Button goInitPanelButton;

    [Header("SingleTon Datas")]
    public List<GameObject> imageObjects = new List<GameObject>();

    void Start()
    {
        checkFolderButton.onClick.AddListener(SelectedFolderDownload);
        goInitPanelButton.onClick.AddListener(Init);
        goInitPanelButton.onClick.AddListener(InitSceneObject);
    }

    #region InitPanel

    public void ReLoginInfoSetActivate()
    {
        reLoginInfo.SetActive(true);
    }

    public void SelectedFolderDownload()
    {
        initPanel.SetActive(false);
        StartCoroutine(GoogleDriveManager.Instance.SelectedSubFolders());
    }

    /*
    public void CheckPassword()
    {
        bool flag = false;


        foreach (UserData userData in GoogleSpreadSheetManager.Instance.userDatas)
        {
            if (userData.FolderName == GoogleDriveManager.Instance.selectedFolderName)
            {
                Debug.Log(userData.FolderName);
                Debug.Log(GoogleDriveManager.Instance.selectedFolderName);
                Debug.Log(userData.FolderPassword);
                Debug.Log(passwordInputFieldText.text);

                if (userData.FolderPassword.Trim().Equals(passwordInputFieldText.text.Trim()))
                {
                    
                    flag = true;
                    break;
                }
            }
        }

        if (flag)  // Correct Password
        {
            // 
            initPanel.SetActive(false);
            StartCoroutine(GoogleDriveManager.Instance.SelectedSubFolders());
        }
        else
        {
            failPasswordInfo.SetActive(true);
        }

    }
     
     */
    #endregion

    #region MainPanel


    //fileObject.GetComponent<Button>().onClick.AddListener(() => UIManager.Instance.ActivateImagesWithName(fileObject.name));
    // -> Listener
    public void ActivateImagesWithName(string selectedName)  
    {
        foreach (GameObject imgObj in imageObjects)
        {
            // 파일 이름이 선택된 이름을 포함하면 활성화, 그렇지 않으면 비활성화
            imgObj.SetActive(imgObj.name.Contains(selectedName));
        }
    }

    public void PassSubFileName(string fileObjectName)
    {
        PortraitInfoManager.Instance.selectedSubFolder = fileObjectName;
    }

    public void ActivateCheckImage()
    {
        string currentPortraitName = PortraitInfoManager.Instance.currentPortraitName;  // 현재 포트레이트 이름 가져오기

        // ScrollView의 자식들을 검색
        foreach (Transform child in portraitScrollView)
        {
            if (child.name == currentPortraitName)  // 현재 포트레이트 이름과 일치하는 자식 찾기
            {
                // 해당 자식에서 "CheckImage"라는 이름의 자식 오브젝트 찾기
                Transform checkImageTransform = child.Find("CheckImage");
                if (checkImageTransform != null)
                {
                    checkImageTransform.gameObject.SetActive(true);  // "CheckImage" 오브젝트를 활성화
                }
                else
                {
                    Debug.LogError("CheckImage 오브젝트를 찾을 수 없습니다.");
                }
                break;  // 찾았으면 루프 종료
            }
        }
    }

    public void DeleteAllPimples()
    {
        // faceImage의 Transform 컴포넌트를 가져옵니다.
        Transform faceImageTransform = faceImage.transform;

        // faceImageTransform의 자식 오브젝트들을 순회하며 각각을 삭제합니다.
        // 주의: List 또는 Array로 변환 없이 직접 삭제하는 것은 열거 도중 컬렉션을 변경하기 때문에 오류를 일으킬 수 있습니다.
        // 따라서 안전하게 삭제하기 위해 임시 리스트를 생성한 후 해당 리스트를 순회하여 삭제합니다.
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in faceImageTransform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);  // 자식 오브젝트를 삭제합니다.
        }
    }


    #endregion

    #region SettingPanel

    public void Init()
    {
        // folderParent, subFolderScrollView, portraitScrollView의 자식 오브젝트를 삭제합니다.
        ClearChildren(folderParent);
        ClearChildren(subFolderScrollView);
        ClearChildren(portraitScrollView);
    }

    // Transform의 모든 자식 오브젝트를 삭제하는 메서드
    private void ClearChildren(Transform parent)
    {
        // 자식 오브젝트들을 순회하며 삭제
        // List로 변환하여 삭제하는 방법을 사용하여 열거 도중 컬렉션 수정 오류를 방지합니다.
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parent)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }

    private void InitSceneObject()  
    {
        // GoogleDriveManager.Instance.StopImageDownload();

        GoogleDriveManager.Instance.subFolders.Clear();
        GoogleDriveManager.Instance.folderNameToIdMap.Clear();
        GoogleDriveManager.Instance.textureDictionary.Clear();

        checkFolderInfo.SetActive(false);


        initPanel.SetActive(true);
        settingPanel.SetActive(false);

        PortraitInfoManager.Instance.Init();
    }

    #endregion
}

