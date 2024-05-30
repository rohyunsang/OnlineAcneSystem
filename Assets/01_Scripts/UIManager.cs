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

    [Header("InitPanel")]
    public Text selectedFolderNameText;   // InitPanel - CheckPasswordInfo
    public GameObject checkPasswordInfo;
    public GameObject failPasswordInfo;
    public Text passwordInputFieldText;
    public Button checkPasswordButton;

    [Header("MainPanel")]
    public Transform subFolderScrollView;
    public Transform portraitScrollView;

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
            StartCoroutine(GoogleDriveManager.Instance.ShowAllSubFolders());
        }
        else
        {
            failPasswordInfo.SetActive(true);
        }

    }
    #endregion

    #region MainPanel



    #endregion
}

