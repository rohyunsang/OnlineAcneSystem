using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplicationQuit : MonoBehaviour
{
    public Text saveText;
    public Text copySaveText;
    public GameObject checkQuitImage;

    public void IsQuitApplication()
    {
        checkQuitImage.SetActive(true);
        copySaveText.text = saveText.text + " \n" + "정말로 종료하시겠습니까? ";
    }

    public void OffCheckQuitImage()
    {
        checkQuitImage.SetActive(false);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}