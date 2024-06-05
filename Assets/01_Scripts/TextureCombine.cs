using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TextureCombine : MonoBehaviour
{
    public RawImage faceImage;
    public GameObject imagesParent;   // faceImage is imagesParent
    private Texture2D combinedTexture;
    private string desktopPath;

    public float PIXEL_WIDTH = 2136f;
    public float PIXEL_HEIGHT = 3216f;
    public float PIXEL_FACEIMAGE_WIDTH = 715f;
    private const float PIXEL_FACEIMAGE_HEIGHT = 1080f;

    private void Start()
    {
        desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
    }

    public void Texture2DCombine()   // using Button MainPanel - SaveImageButton 
    {
        // faceImage�� �ؽ�ó ��������
        Texture2D faceTexture = faceImage.texture as Texture2D;

        if (faceTexture == null)
        {
            Debug.LogError("Face texture is null.");
            return;
        }

        // faceImage ũ�⿡ �´� ���ο� �ؽ�ó ����
        combinedTexture = new Texture2D((int)PIXEL_WIDTH, (int)PIXEL_HEIGHT, TextureFormat.RGBA32, false);

        // faceTexture ����
        for (int x = 0; x < faceTexture.width; x++)
        {
            for (int y = 0; y < faceTexture.height; y++)
            {
                combinedTexture.SetPixel(x, y, faceTexture.GetPixel(x, y));
            }
        }

        // �ڽ� ������Ʈ���� �̹����� ����
        foreach (Transform child in imagesParent.transform)
        {
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                Texture2D childTexture = image.sprite.texture;
                RectTransform rectTransform = image.GetComponent<RectTransform>();
                Vector2 size = rectTransform.sizeDelta;
                Vector2 pivot = rectTransform.pivot;
                Vector2 anchorPos = rectTransform.anchoredPosition;

                if (childTexture == null)
                {
                    Debug.LogError($"Texture for {image.name} is null.");
                    continue;
                }

                // ��Ŀ ����Ʈ�� �ǹ��� ����� ��ǥ ��ȯ
                Vector2 adjustedPos = new Vector2(
                    anchorPos.x - (size.x * pivot.x),
                    anchorPos.y - (size.y * pivot.y)
                );

                float xOffset = (adjustedPos.x + PIXEL_FACEIMAGE_WIDTH / 2) / PIXEL_FACEIMAGE_WIDTH * PIXEL_WIDTH;
                float yOffset = (adjustedPos.y + PIXEL_FACEIMAGE_HEIGHT / 2) / PIXEL_FACEIMAGE_HEIGHT * PIXEL_HEIGHT;

                // �ڽ� �̹��� ����
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        int targetX = Mathf.FloorToInt(xOffset + x);
                        int targetY = Mathf.FloorToInt(yOffset + y);

                        if (targetX >= 0 && targetX < combinedTexture.width && targetY >= 0 && targetY < combinedTexture.height)
                        {
                            combinedTexture.SetPixel(targetX, targetY, Color.red);
                        }
                    }
                }
            }
        }

        // ���� ������ �����մϴ�.
        combinedTexture.Apply();
        SaveJPG();
    }


    public void SaveJPG()
    {
        byte[] bytes = combinedTexture.EncodeToJPG();
        // �ؽ�ó�� JPG ����Ʈ �迭�� ���ڵ��մϴ�.

        string directoryPath = Path.Combine(desktopPath, "����̹���");
        string filePath = Path.Combine(directoryPath, "MergedImage.jpg");

        // ���丮�� ������ �����մϴ�.
        Directory.CreateDirectory(directoryPath);

        // ����Ʈ �迭�� ���Ϸ� �����մϴ�.
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Image saved to: {filePath}");
    }
}