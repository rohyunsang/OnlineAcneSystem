using System.Collections;
using System.Collections.Generic;
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

    [Header("InitPanel")]
    public Transform folderParent;  // InitPanel - ScrollView - Content
    public Text selectedFolderNameText;   // InitPanel - CheckPasswordInfo
    public GameObject checkPasswordInfo;
    public GameObject failPasswordInfo;
    public Text passwordInputFieldText;
    public Button checkPasswordButton;

    [Header("MainPanel")]
    public Transform subFolderScrollView;
    public Transform portraitScrollView;
    public RawImage faceImage;      // �۾� ĵ���� �̹��� 

    [Header("SingleTon Datas")]
    public List<GameObject> imageObjects = new List<GameObject>();

    void Start()
    {
        checkPasswordButton.onClick.AddListener(CheckPassword);
    }

    #region InitPanel
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

    #endregion
}

