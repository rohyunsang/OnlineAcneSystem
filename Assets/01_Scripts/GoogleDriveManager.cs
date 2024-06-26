using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using System;
using System.Linq;


public class GoogleDriveManager : MonoBehaviour
{
    public static GoogleDriveManager Instance { get; private set; }  // Singleton instance

    public Button cloudFolderLoadButton;
    
    public GameObject filePrefab;
    public GameObject imagePrefab;

    //private List<string> folders = new List<string>();
    public List<string> subFolders = new List<string>();
    public Dictionary<string, string> folderNameToIdMap = new Dictionary<string, string>();

    public string selectedFolderName = "";

    public Dictionary<string, Texture2D> textureDictionary = new Dictionary<string, Texture2D>();  // Image textures


    // Stop Coruntine
    private Coroutine coroutineSelectedSubFolders;
    private Coroutine coroutineDownloadImagesTopFolder;
    private Coroutine coroutineDownloadJpgImages;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Optional: Keep the instance alive across scenes
        }
        else
        {
            Destroy(gameObject);  // Destroy if a duplicate instance is created
        }
    }


    void Start()
    {
        cloudFolderLoadButton.onClick.AddListener(HandlerListAllFoldersButton);
    }

    
     #region ImageUpload

    public void HandlerUploadImage(Texture2D image, string jsonString)
    {
        StartCoroutine(UploadImage(image, jsonString));
        
        UIManager.Instance.ActivateCheckImage();
    }
    public IEnumerator UploadImage(Texture2D image, string jsonString)
    {
        string selectedFolder = PortraitInfoManager.Instance.selectedFolder + "_result";   // Append '_result' to selected folder
        string selectedSubFolder = PortraitInfoManager.Instance.selectedSubFolder;
        string fullPath = $"{selectedFolder}/{selectedSubFolder}";

        // Ensure the folder path exists
        yield return StartCoroutine(EnsureFolderPathExists(fullPath));

        var content = image.EncodeToJPG();
        var file = new UnityGoogleDrive.Data.File()
        {
            Name = PortraitInfoManager.Instance.currentPortraitName,
            Content = content,
            Parents = new List<string> { folderNameToIdMap[selectedSubFolder] }  // Only use the subfolder as the parent
        };

        var request = GoogleDriveFiles.Create(file);
        request.Fields = new List<string> { "id" };
        yield return request.Send();
        Debug.Log(request.IsError ? $"Error: {request.Error}" : $"File uploaded: ID = {request.ResponseData.Id}");

        StartCoroutine(UploadJson(jsonString));
    }
    public IEnumerator UploadJson(string jsonString)
    {
        string selectedFolder = PortraitInfoManager.Instance.selectedFolder + "_result"; // Same folder as the image
        string selectedSubFolder = PortraitInfoManager.Instance.selectedSubFolder;
        string fullPath = $"{selectedFolder}/{selectedSubFolder}";

        // Ensure the folder path exists
        yield return StartCoroutine(EnsureFolderPathExists(fullPath));

        var content = System.Text.Encoding.UTF8.GetBytes(jsonString);
        var file = new UnityGoogleDrive.Data.File()
        {
            Name = PortraitInfoManager.Instance.currentPortraitName.Replace(".jpg",".json"),
            Content = content,
            Parents = new List<string> { folderNameToIdMap[selectedSubFolder] }, // Same parent folder as the image
            MimeType = "application/json"  // Set the MIME type for JSON
        };

        var request = GoogleDriveFiles.Create(file);
        request.Fields = new List<string> { "id" };
        yield return request.Send();
        Debug.Log(request.IsError ? $"Error: {request.Error}" : $"File uploaded: ID = {request.ResponseData.Id}");
    }

    private IEnumerator EnsureFolderPathExists(string fullPath)
    {
        string[] folders = fullPath.Split('/');
        string currentParentId = "root";  // Start with the root ID.

        foreach (var folderName in folders)
        {
            string folderId;
            if (folderNameToIdMap.TryGetValue(folderName, out folderId))
            {
                // Folder ID found in map, use it as the current parent ID for the next iteration
                currentParentId = folderId;
            }
            else
            {
                // Folder ID not in map, check if folder exists on Google Drive
                yield return StartCoroutine(CheckAndCreateFolder(folderName, currentParentId, (newFolderId) => {
                    folderId = newFolderId;  // Update local folder ID with new or found folder ID
                    folderNameToIdMap[folderName] = newFolderId;  // Update the map with the new or found folder ID
                    currentParentId = folderId;  // Update currentParentId for the next iteration
                }));
            }
        }
    }

    private IEnumerator CheckAndCreateFolder(string folderName, string parentId, Action<string> onUpdateFolderId)
    {
        // First check if the folder exists in Google Drive
        var listRequest = GoogleDriveFiles.List();
        listRequest.Fields = new List<string> { "files(id)" };
        listRequest.Q = $"name = '{folderName}' and '{parentId}' in parents and mimeType = 'application/vnd.google-apps.folder'";
        yield return listRequest.Send();

        if (!listRequest.IsError && listRequest.ResponseData.Files.Count > 0)
        {
            // Folder exists, use the existing folder ID
            onUpdateFolderId(listRequest.ResponseData.Files[0].Id);
        }
        else
        {
            // Folder does not exist, create it
            var fileMetadata = new UnityGoogleDrive.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentId }
            };

            var createRequest = GoogleDriveFiles.Create(fileMetadata);
            yield return createRequest.Send();

            if (createRequest.IsError)
            {
                Debug.LogError($"Failed to create folder '{folderName}': {createRequest.Error}");
                yield break;
            }

            // Log the creation and update the ID
            Debug.Log($"Created folder '{folderName}' with ID: {createRequest.ResponseData.Id}");
            onUpdateFolderId(createRequest.ResponseData.Id);  // Use the callback to update the folder ID
        }
    }


    #endregion



    #region ImageDownload
    public void HandlerListAllFoldersButton()
    {
        if (coroutineSelectedSubFolders != null) StopCoroutine(coroutineSelectedSubFolders);
        coroutineSelectedSubFolders = StartCoroutine(RootSubFolders());
    }

    public void HandlerImageDownload()
    {
        if (coroutineDownloadImagesTopFolder != null) StopCoroutine(coroutineDownloadImagesTopFolder);
        coroutineDownloadImagesTopFolder = StartCoroutine(DownloadImagesInTopFolder(selectedFolderName));
    }

    public IEnumerator RootSubFolders()   // Root Folder 안에 있는 폴더들 
    {
        var listRequest = GoogleDriveFiles.List();
        listRequest.Fields = new List<string> { "files(id)", "files(name)" };
        // Query modified to fetch only top-level folders
        listRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and 'root' in parents";
        yield return listRequest.Send();

        if (listRequest.IsError)
        {
            Debug.LogError($"Error: {listRequest.Error}");
            yield break;
        }


        // 역순으로 정렬된 리스트를 이용하여 GameObjects 생성
        var files = new List<UnityGoogleDrive.Data.File>(listRequest.ResponseData.Files);
        files.Reverse();
        
        foreach (var file in files)
        {
            Debug.Log($"Folder ID: {file.Id}, Folder Name: {file.Name}");
            folderNameToIdMap[file.Name] = file.Id;  // Save folder ID

            if (file.Name.Contains("result")) continue;

            GameObject fileObject = Instantiate(filePrefab, UIManager.Instance.folderParent);
            Text fileNameText = fileObject.transform.Find("FileNameText").GetComponent<UnityEngine.UI.Text>();
            fileNameText.text = file.Name;
            fileObject.name = file.Name;

            // add listener

            fileObject.GetComponent<Button>().onClick.AddListener(() => SelectedFileNameListener(fileObject.name));
        }
    }

    public IEnumerator SelectedSubFolders()
    {
        if (!folderNameToIdMap.TryGetValue(selectedFolderName, out var folderId))
        {
            Debug.LogError("Folder ID not found for: " + selectedFolderName);
            yield break;
        }

        var listRequest = GoogleDriveFiles.List();
        listRequest.Fields = new List<string> { "files(id)", "files(name)" };
        listRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and '{folderId}' in parents";
        yield return listRequest.Send();

        if (listRequest.IsError)
        {
            Debug.LogError($"Error: {listRequest.Error}");
            yield break;
        }


        // need sorting and reverse 
        var files = new List<UnityGoogleDrive.Data.File>(listRequest.ResponseData.Files);
        files.Sort((x, y) => String.Compare(x.Name, y.Name));

        foreach (var file in files)
        {
            subFolders.Add(file.Name);
            Debug.Log(file.Name);

            GameObject fileObject = Instantiate(filePrefab, UIManager.Instance.subFolderScrollView);
            Text fileNameText = fileObject.transform.Find("FileNameText").GetComponent<UnityEngine.UI.Text>();
            fileNameText.text = file.Name;
            fileObject.name = file.Name;

            fileObject.GetComponent<Button>().onClick.AddListener(() => 
                UIManager.Instance.ActivateImagesWithName(fileObject.name));


            fileObject.GetComponent<Button>().onClick.AddListener(() => 
                UIManager.Instance.PassSubFileName(fileObject.name));   
        }
        StartCoroutine(DownloadImagesInTopFolder(selectedFolderName));   // Sub Folder들의 이름을 안다음에 실행.
    }

    

    private IEnumerator DownloadImagesInTopFolder(string topFolderName)
    {
        Debug.Log("TopFolderFunction : " + topFolderName);
        // Step 1: Find the top-level folder
        var listRequest = GoogleDriveFiles.List();
        listRequest.Fields = new List<string> { "files(id)", "files(name)" };
        listRequest.Q = $"name = '{topFolderName}' and mimeType = 'application/vnd.google-apps.folder' and 'me' in owners";
        yield return listRequest.Send();

        if (listRequest.IsError || listRequest.ResponseData.Files.Count == 0)
        {
            Debug.LogError($"Error or folder '{topFolderName}' not found: {listRequest.Error}");
            yield break;
        }

        string topLevelFolderId = listRequest.ResponseData.Files[0].Id;
        Debug.Log($"Found top-level folder '{topFolderName}' with ID: {topLevelFolderId}");

        // 시작전에 imageObject 초기화 
        UIManager.Instance.imageObjects.Clear();
        // Step 2: Find each subfolder (01, 02, 03) in the top-level folder
        foreach (string subFolderName in subFolders)
        {
            Debug.Log("is Running? ");
            listRequest = GoogleDriveFiles.List();
            listRequest.Fields = new List<string> { "files(id)", "files(name)" };
            listRequest.Q = $"'{topLevelFolderId}' in parents and name = '{subFolderName}' and mimeType = 'application/vnd.google-apps.folder'";
            yield return listRequest.Send();

            if (listRequest.IsError || listRequest.ResponseData.Files.Count == 0)
            {
                Debug.LogError($"Error or subfolder '{subFolderName}' not found: {listRequest.Error}");
                continue;
            }

            string subFolderId = listRequest.ResponseData.Files[0].Id;
            Debug.Log($"Found subfolder '{subFolderName}' with ID: {subFolderId}");

            // Step 3: List and download images in the subfolder
            yield return StartCoroutine(DownloadJpgImagesInFolder(subFolderId));
        }
    }

    private IEnumerator DownloadJpgImagesInFolder(string folderId)
    {
        var listRequest = GoogleDriveFiles.List();
        listRequest.Fields = new List<string> { "files(id)", "files(name)", "files(mimeType)" };
        listRequest.Q = $"'{folderId}' in parents and mimeType contains 'image/'";
        yield return listRequest.Send();

        if (listRequest.IsError)
        {
            Debug.LogError($"Error: {listRequest.Error}");
            yield break;
        }

        var files = listRequest.ResponseData.Files.ToList();
        files.Reverse();

        foreach (var file in files)
        {
            if (!file.Name.EndsWith(".jpg")) continue; // Skip non-JPG files

            Debug.Log($"Image File ID: {file.Id}, Image File Name: {file.Name}");
            var downloadRequest = GoogleDriveFiles.Download(file.Id);
            yield return downloadRequest.Send();

            if (downloadRequest.IsError)
            {
                Debug.LogError($"Error downloading {file.Name}: {downloadRequest.Error}");
                continue;
            }

            // Convert the downloaded bytes to Texture2D
            byte[] fileContent = downloadRequest.ResponseData.Content;
            Texture2D downloadedTexture = new Texture2D(2, 2);
            downloadedTexture.LoadImage(fileContent);

            // Instantiate the imagePrefab and set the downloaded image
            GameObject imageObject = Instantiate(imagePrefab, UIManager.Instance.portraitScrollView);
            UnityEngine.UI.Image imageComponent = imageObject.GetComponent<Image>();
            imageComponent.sprite = Sprite.Create(downloadedTexture, new Rect(0, 0, downloadedTexture.width, downloadedTexture.height), new Vector2(0.5f, 0.5f));
            imageObject.name = file.Name;
            Text imageNameText = imageObject.transform.Find("ImageName").GetComponent<UnityEngine.UI.Text>();
            imageNameText.text = file.Name;

            // Store the texture in the dictionary
            textureDictionary[file.Name] = downloadedTexture;


            UIManager.Instance.imageObjects.Add(imageObject);

            if (!imageObject.name.Contains("01"))  // 제일 첫 번째 폴더의 사진들은 바로 보이게.
                imageObject.SetActive(false);
        }
    }

    public void StopImageDownload()
    {
        if (coroutineSelectedSubFolders != null) StopCoroutine(coroutineSelectedSubFolders);
        if (coroutineDownloadImagesTopFolder != null) StopCoroutine(coroutineDownloadImagesTopFolder);
        if (coroutineDownloadJpgImages != null) StopCoroutine(coroutineDownloadJpgImages);
    }

    #endregion


    public void SelectedFileNameListener(string folderName)
    {
        selectedFolderName = folderName;
        UIManager.Instance.checkPasswordInfo.SetActive(true);
        UIManager.Instance.selectedFolderNameText.text = "선택된 폴더 이름 :  " + selectedFolderName;

        // PictureInfoManager 에게도 정보 전달. 
        PortraitInfoManager.Instance.selectedFolder = folderName;

    }

    public void RefreshToken()  // 구글 다시 인증 하기 
    {
        AuthController.RefreshAccessToken();
    }

}

