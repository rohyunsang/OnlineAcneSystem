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

        //////////////////////////////////////////////////////////
        Debug.Log("faceTexture WithHeight");
        Debug.Log(faceTexture.width);   // 2136
        Debug.Log(faceTexture.height);  // 3216 

        if (faceTexture == null)
        {
            Debug.LogError("Face texture is null.");
            return;
        }

        // faceImage ũ�⿡ �´� ���ο� �ؽ�ó ����
        combinedTexture = new Texture2D((int)PIXEL_WIDTH, (int)PIXEL_HEIGHT, TextureFormat.RGBA32, false);

        //////////////////////////////////////////////////////////
        Debug.Log("combinedTexture WithHeight");
        Debug.Log(combinedTexture.width);   // 2136
        Debug.Log(combinedTexture.height);  // 3216

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

                // rectTransform.rect�� ����Ͽ� ���� ũ�⸦ �����ɴϴ�.
                float width = rectTransform.rect.width;
                float height = rectTransform.rect.height;

                Vector2 pivot = rectTransform.pivot;
                Vector2 anchorPos = rectTransform.anchoredPosition;

                if (childTexture == null)
                {
                    Debug.LogError($"Texture for {image.name} is null.");
                    continue;
                }

                // ��Ŀ ����Ʈ�� �ǹ��� ����� ��ǥ ��ȯ
                Vector2 adjustedPos = new Vector2(
                    anchorPos.x - (width * pivot.x),
                    anchorPos.y - (height * pivot.y)
                );

                float xOffset = (adjustedPos.x + PIXEL_FACEIMAGE_WIDTH / 2) / PIXEL_FACEIMAGE_WIDTH * PIXEL_WIDTH;
                float yOffset = (adjustedPos.y + PIXEL_FACEIMAGE_HEIGHT / 2) / PIXEL_FACEIMAGE_HEIGHT * PIXEL_HEIGHT;

                /////////////////////////////////////
                Debug.Log("x, y Offset");
                Debug.Log(xOffset);
                Debug.Log(yOffset);

                // �ڽ� �̹��� ����
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int targetX = Mathf.FloorToInt(xOffset + x);
                        int targetY = Mathf.FloorToInt(yOffset + y);

                        Debug.Log("Mathf.FloorToInt(xOffset + x, y);");
                        Debug.Log(targetX);
                        Debug.Log(targetY);

                        if (targetX >= 0 && targetX < combinedTexture.width && targetY >= 0 && targetY < combinedTexture.height)
                        {
                            Color pixelColor = childTexture.GetPixelBilinear((float)x / width, (float)y / height);

                            // Check if the pixel is transparent; skip if it is
                            if (pixelColor.a < 0.01f) // Consider pixels with very low alpha as transparent
                                continue;
                            /////////////////////////////////////////////////////////
                            Debug.Log(pixelColor); // Optional: log the pixel color value
                            combinedTexture.SetPixel(targetX, targetY, pixelColor);
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