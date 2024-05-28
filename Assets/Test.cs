using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.WSA;
using Image = UnityEngine.UI.Image;


public class Test : MonoBehaviour
{
    public Texture2D image;
    public Button uploadButton;
    public Button showAllFoldersButton;
    public Transform folderParent;

    public GameObject filePrefab;
    public GameObject imagePrefab;
    public Image faceImage;

    private List<string> subFolders = new List<string> { "01", "02", "03" };

    void Start()
    {
        uploadButton.onClick.AddListener(HandlerUploadButton);
        showAllFoldersButton.onClick.AddListener(HandlerListAllFoldersButton);
        HandlerImageDownload();
    }

    public void HandlerUploadButton()
    {
        StartCoroutine(UploadButton());
    }

    public void HandlerListAllFoldersButton()
    {
        StartCoroutine(ShowAllFolders());
    }
    public void HandlerImageDownload()
    {
        StartCoroutine(DownloadImagesInTopFolder("test1"));
    }

    public IEnumerator UploadButton()
    {
        var content = image.EncodeToPNG();
        var file = new UnityGoogleDrive.Data.File() 
            { Name = "testTestImage", Content = content };
        var request = GoogleDriveFiles.Create(file);
        request.Fields = new List<string> { "id" };
        yield return request.Send();
        print(request.IsError);
        print(request.RequestData.Content);
        print(request.RequestData.Id);
    }

    public IEnumerator ShowAllFolders()
    {
        var listRequest = GoogleDriveFiles.List();
        listRequest.Fields = new List<string> { "files(id)", "files(name)" };
        listRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and 'me' in owners";
        yield return listRequest.Send();

        if (listRequest.IsError)
        {
            Debug.LogError($"Error: {listRequest.Error}");
            yield break;
        }

        foreach (var file in listRequest.ResponseData.Files)
        {
            Debug.Log($"Folder ID: {file.Id}, Folder Name: {file.Name}");
            GameObject fileObject = Instantiate(filePrefab, folderParent);
            Text fileNameText = fileObject.transform.Find("FileNameText").GetComponent<UnityEngine.UI.Text>();
            fileNameText.text = file.Name;
        }
    }

    private IEnumerator DownloadImagesInTopFolder(string topFolderName)
    {
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

        // Step 2: Find each subfolder (01, 02, 03) in the top-level folder
        foreach (string subFolderName in subFolders)
        {
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

        foreach (var file in listRequest.ResponseData.Files)
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
            GameObject imageObject = Instantiate(imagePrefab, folderParent);
            UnityEngine.UI.Image imageComponent = imageObject.GetComponent<Image>();
            imageComponent.sprite = Sprite.Create(downloadedTexture, new Rect(0, 0, downloadedTexture.width, downloadedTexture.height), new Vector2(0.5f, 0.5f));
        }
    }
}

