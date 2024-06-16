using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    private Dictionary<string, int> imageCounts = new Dictionary<string, int>();
    private Dictionary<string, int> processedImages = new Dictionary<string, int>();

    #region SingleTon Pattern
    public static ProgressManager Instance { get; private set; }
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

    public void CountImages(string subFolderName, int count)
    {
        if (!imageCounts.ContainsKey(subFolderName))
        {
            imageCounts[subFolderName] = count;
            processedImages[subFolderName] = 0;
        }
    }

    public void ImageProcessed(string subFolderName)
    {
        if (processedImages.ContainsKey(subFolderName))
        {
            processedImages[subFolderName]++;
            CheckProgress(subFolderName);
        }
    }

    private void CheckProgress(string subFolderName)
    {
        if (!imageCounts.ContainsKey(subFolderName)) return;

        int totalCount = imageCounts[subFolderName];
        int processedCount = processedImages[subFolderName];

        Debug.Log($"Processing {subFolderName}: {processedCount}/{totalCount} images processed.");

        if (processedCount >= totalCount)
        {
            Debug.Log($"{subFolderName} folder: All images have been processed.");
            // 필요하다면 여기서 이벤트나 콜백을 트리거할 수 있습니다.
        }
    }
}
