using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore.Text;

public class GoogleSpreadSheetManager : MonoBehaviour
{
    #region SingleTon Pattern
    public static GoogleSpreadSheetManager Instance { get; private set; }
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

        // Initialize other components or variables if needed
    }
    #endregion

    // 링크 뒤 export ~ 부분을 빼고 export?format=tsv 추가하기
    const string userDataURL = "https://docs.google.com/spreadsheets/d/15yMnm_0sphfWg-48sAUYLBPRtZAhszSkn6mgD7jx92s/export?format=tsv"; // UserData

    [SerializeField]
    public List<UserData> userDatas = new List<UserData>();


    IEnumerator Start()
    {
        //UserData
        UnityWebRequest www = UnityWebRequest.Get(userDataURL);
        yield return www.SendWebRequest();

        string data = www.downloadHandler.text;
        print(data);
        ParseUserData(data);


    }
    private void ParseUserData(string data)
    {
        string[] lines = data.Split('\n');
        for (int i = 1; i < lines.Length; i++) // 첫 번째 줄은 헤더이므로 건너뜁니다.
        {
            string[] fields = lines[i].Split('\t');
            if (fields.Length >= 4) // 필드가 충분한지 확인
            {
                UserData userData = new UserData()
                {
                    Name = fields[0],
                    Email = fields[1],
                    FolderName = fields[2],
                    FolderPassword = fields[3],

                };
                userDatas.Add(userData);
            }
        }
    }
    
    
}

[System.Serializable]
public class UserData
{
    public string Name;
    public string Email;
    public string FolderName;
    public string FolderPassword;
}
