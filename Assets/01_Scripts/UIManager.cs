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
    public RawImage faceImage;      // �۾� ĵ���� �̹��� 

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
            // ���� �̸��� ���õ� �̸��� �����ϸ� Ȱ��ȭ, �׷��� ������ ��Ȱ��ȭ
            imgObj.SetActive(imgObj.name.Contains(selectedName));
        }
    }

    public void PassSubFileName(string fileObjectName)
    {
        PortraitInfoManager.Instance.selectedSubFolder = fileObjectName;
    }

    public void ActivateCheckImage()
    {
        string currentPortraitName = PortraitInfoManager.Instance.currentPortraitName;  // ���� ��Ʈ����Ʈ �̸� ��������

        // ScrollView�� �ڽĵ��� �˻�
        foreach (Transform child in portraitScrollView)
        {
            if (child.name == currentPortraitName)  // ���� ��Ʈ����Ʈ �̸��� ��ġ�ϴ� �ڽ� ã��
            {
                // �ش� �ڽĿ��� "CheckImage"��� �̸��� �ڽ� ������Ʈ ã��
                Transform checkImageTransform = child.Find("CheckImage");
                if (checkImageTransform != null)
                {
                    checkImageTransform.gameObject.SetActive(true);  // "CheckImage" ������Ʈ�� Ȱ��ȭ
                }
                else
                {
                    Debug.LogError("CheckImage ������Ʈ�� ã�� �� �����ϴ�.");
                }
                break;  // ã������ ���� ����
            }
        }
    }

    public void DeleteAllPimples()
    {
        // faceImage�� Transform ������Ʈ�� �����ɴϴ�.
        Transform faceImageTransform = faceImage.transform;

        // faceImageTransform�� �ڽ� ������Ʈ���� ��ȸ�ϸ� ������ �����մϴ�.
        // ����: List �Ǵ� Array�� ��ȯ ���� ���� �����ϴ� ���� ���� ���� �÷����� �����ϱ� ������ ������ ����ų �� �ֽ��ϴ�.
        // ���� �����ϰ� �����ϱ� ���� �ӽ� ����Ʈ�� ������ �� �ش� ����Ʈ�� ��ȸ�Ͽ� �����մϴ�.
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in faceImageTransform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);  // �ڽ� ������Ʈ�� �����մϴ�.
        }
    }


    #endregion

    #region SettingPanel

    public void Init()
    {
        // folderParent, subFolderScrollView, portraitScrollView�� �ڽ� ������Ʈ�� �����մϴ�.
        ClearChildren(folderParent);
        ClearChildren(subFolderScrollView);
        ClearChildren(portraitScrollView);
    }

    // Transform�� ��� �ڽ� ������Ʈ�� �����ϴ� �޼���
    private void ClearChildren(Transform parent)
    {
        // �ڽ� ������Ʈ���� ��ȸ�ϸ� ����
        // List�� ��ȯ�Ͽ� �����ϴ� ����� ����Ͽ� ���� ���� �÷��� ���� ������ �����մϴ�.
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

