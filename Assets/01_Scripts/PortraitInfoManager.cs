using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortraitInfoManager : MonoBehaviour
{
    public string selectedFolder = "";
    public string selectedSubFolder = "";
    public string currentPortraitName = "";

    #region SingleTon Pattern
    public static PortraitInfoManager Instance { get; private set; }
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

        Init();
    }
    #endregion

    private void Init()
    {
        selectedSubFolder = "01";
    }

}
