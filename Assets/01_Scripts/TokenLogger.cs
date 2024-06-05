using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;

public class TokenLogger : MonoBehaviour
{
    private void Start()
    {
        var settings = GoogleDriveSettings.LoadFromResources();
        if (settings == null)
        {
            Debug.LogError("GoogleDriveSettings could not be loaded.");
            return;
        }

        string accessToken = settings.CachedAccessToken;
        string refreshToken = settings.CachedRefreshToken;

        Debug.Log($"Access Token: {accessToken}");
        Debug.Log($"Refresh Token: {refreshToken}");
    }
}
