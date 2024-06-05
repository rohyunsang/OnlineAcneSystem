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
        // faceImage의 텍스처 가져오기
        Texture2D faceTexture = faceImage.texture as Texture2D;

        if (faceTexture == null)
        {
            Debug.LogError("Face texture is null.");
            return;
        }

        // faceImage 크기에 맞는 새로운 텍스처 생성
        combinedTexture = new Texture2D((int)PIXEL_WIDTH, (int)PIXEL_HEIGHT, TextureFormat.RGBA32, false);

        // faceTexture 복사
        for (int x = 0; x < faceTexture.width; x++)
        {
            for (int y = 0; y < faceTexture.height; y++)
            {
                combinedTexture.SetPixel(x, y, faceTexture.GetPixel(x, y));
            }
        }

        // 자식 오브젝트들의 이미지를 병합
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

                // 앵커 포인트와 피벗을 고려한 좌표 변환
                Vector2 adjustedPos = new Vector2(
                    anchorPos.x - (size.x * pivot.x),
                    anchorPos.y - (size.y * pivot.y)
                );

                float xOffset = (adjustedPos.x + PIXEL_FACEIMAGE_WIDTH / 2) / PIXEL_FACEIMAGE_WIDTH * PIXEL_WIDTH;
                float yOffset = (adjustedPos.y + PIXEL_FACEIMAGE_HEIGHT / 2) / PIXEL_FACEIMAGE_HEIGHT * PIXEL_HEIGHT;

                // 자식 이미지 복사
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

        // 변경 사항을 적용합니다.
        combinedTexture.Apply();
        SaveJPG();
    }


    public void SaveJPG()
    {
        byte[] bytes = combinedTexture.EncodeToJPG();
        // 텍스처를 JPG 바이트 배열로 인코딩합니다.

        string directoryPath = Path.Combine(desktopPath, "결과이미지");
        string filePath = Path.Combine(directoryPath, "MergedImage.jpg");

        // 디렉토리가 없으면 생성합니다.
        Directory.CreateDirectory(directoryPath);

        // 바이트 배열을 파일로 저장합니다.
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Image saved to: {filePath}");
    }
}