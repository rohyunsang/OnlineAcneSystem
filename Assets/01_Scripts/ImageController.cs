using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Script attach   Canvas - MainPanel - FaceImage
public class ImageController : MonoBehaviour, IDragHandler, IScrollHandler, IPointerDownHandler
{
    //Drag 
    private RectTransform rectTr;

    //Mouse Scorll Zoom In/Out
    private Vector3 minitialScale;
    private float zoomSpeed = 0.1f;
    private float maxZoom = 5.0f;

    public Button resetImageButton;

    public Transform faceImage; // faceImage 부모 오브젝트 참조 self reference
    public Transform backGround;

    public GameObject circlePrefab;
    public GameObject blackheadPrefab;
    public GameObject whiteheadPrefab;
    public GameObject papulePrefab;
    public GameObject pustuePrefab;
    public GameObject nodulePrefab;

    public GameObject namingButtonPrefabs; // Reference to the button prefab
    public GameObject statusButtonPrefabs;
    public GraphicRaycaster graphicRaycaster;

    private void Start()
    {
        rectTr = GetComponent<RectTransform>(); //스크립트 위치의 Rect Transform
        minitialScale = transform.localScale;  //현재 Local Scale을 저장

        resetImageButton.onClick.AddListener(OnClickOriginPositionBtn);  // 버튼 클릭 리스너 추가
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) // 마우스 왼쪽 클릭되었는지 확인
        {
            rectTr.anchoredPosition += eventData.delta * 2; // 드래그 이벤트 함수 선언 및 등록
        }
    }

    public void OnClickOriginPositionBtn()
    {
        rectTr.localPosition = new Vector3(20.1841f, 0f, 0f);   // localPosition
        rectTr.localScale = Vector3.one;
    }

    public void OnScroll(PointerEventData eventData) //마우스 스크롤 이벤트 등록
    {
        var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
        var desiredScale = transform.localScale + delta;

        desiredScale = ClampDesiredScale(desiredScale);

        transform.localScale = desiredScale;
    }

    private Vector3 ClampDesiredScale(Vector3 desiredScale) // 마우스 스크롤의 최대 줌인/아웃
    {
        desiredScale = Vector3.Max(minitialScale, desiredScale);
        desiredScale = Vector3.Min(minitialScale * maxZoom, desiredScale);

        return desiredScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Check for overlapping UI elements using raycasting
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(eventData, results);

            // Check specifically if anything other than the faceImage was hit
            if (results.Any(result => result.gameObject != faceImage.gameObject)) return;

            RectTransform faceImageRect = faceImage as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(faceImageRect, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
            {
                Debug.Log(localPointerPosition);

                #region Delete Part

                if (IsCircleAtPosition(localPointerPosition))
                {
                    // If there's a circle, create the statusButtons
                    GameObject statusButtonsInstance = Instantiate(statusButtonPrefabs, backGround);
                    statusButtonsInstance.GetComponent<RectTransform>().SetAsLastSibling();

                    Vector2 convertedPosition = ConvertToBackgroundLocalCoordinates(localPointerPosition, faceImage, backGround);
                    statusButtonsInstance.GetComponent<RectTransform>().anchoredPosition = convertedPosition + new Vector2(130f, 5f);

                    // Find the circle object at the specified position
                    GameObject statusImage = GetCircleAtPosition(localPointerPosition);

                    // Pass the circle's name to the Text component of NameButton's child
                    Transform nameButton = statusButtonsInstance.transform.Find("NameButton");
                    if (nameButton != null)
                    {
                        Text nameText = nameButton.GetChild(0).GetComponent<Text>();
                        if (nameText != null && statusImage != null)
                        {
                            nameText.text = statusImage.name;
                        }
                    }

                    // Add a listener to the Delete button to destroy the circle when clicked
                    Button deleteButton = statusButtonsInstance.transform.Find("DeleteButton").GetComponent<Button>();
                    if (deleteButton != null && statusImage != null)
                    {
                        deleteButton.onClick.AddListener(() => {
                            Destroy(statusImage);
                            Destroy(statusButtonsInstance);
                        });
                    }

                    Button cancelButton = statusButtonsInstance.transform.Find("CancelButton").GetComponent<Button>();
                    if (cancelButton != null)
                    {
                        cancelButton.onClick.AddListener(() => Destroy(statusButtonsInstance));
                    }
                }
                #endregion

                #region Instantiate Part
                else
                {
                    GameObject statusImage = Instantiate(circlePrefab, faceImage); // 
                    statusImage.GetComponent<RectTransform>().anchoredPosition = localPointerPosition;

                    Vector2 originPosition = localPointerPosition;

                    GameObject buttonsInstance = Instantiate(namingButtonPrefabs, backGround);
                    buttonsInstance.GetComponent<RectTransform>().SetAsLastSibling();
                    Vector2 convertedPosition = ConvertToBackgroundLocalCoordinates(localPointerPosition, faceImage, backGround);
                    buttonsInstance.GetComponent<RectTransform>().anchoredPosition = convertedPosition + new Vector2(130f, 5f);

                    foreach (Transform child in buttonsInstance.transform)
                    {
                        Button btn = child.GetComponent<Button>();
                        if (btn != null)
                        {
                            // Clear previous listeners
                            btn.onClick.RemoveAllListeners();

                            btn.onClick.AddListener(() =>
                            {
                                string buttonText = btn.transform.GetChild(0).GetComponent<Text>().text;
                                ChangeCircleShape(statusImage, buttonText, localPointerPosition, buttonsInstance);
                                if(buttonsInstance != null) Destroy(buttonsInstance);
                            });
                        }
                    }
                }
                #endregion

            }
        }
    }
    void ChangeCircleShape(GameObject tmpStatusImage, string newName, Vector2 originPosition, GameObject buttonsInstance)
    {
        Destroy(tmpStatusImage);

        GameObject statusImage = null;
        

        if (newName.Equals("Blackhead"))
        {
            statusImage = Instantiate(blackheadPrefab, faceImage);
        }
        else if (newName.Equals("Whitehead"))
        {
            statusImage = Instantiate(whiteheadPrefab, faceImage);
        }
        else if (newName.Equals("Papule"))
        {
            statusImage = Instantiate(papulePrefab, faceImage);
        }
        else if (newName.Equals("Pustule"))
        {
            statusImage = Instantiate(pustuePrefab, faceImage);
        }
        else if (newName.Equals("Nodule"))
        {
            statusImage = Instantiate(nodulePrefab, faceImage);
        }
        else if (newName.Equals("Cancel"))
        {
            Destroy(buttonsInstance);
            return;
        }

        statusImage.name = newName;
        statusImage.GetComponent<RectTransform>().anchoredPosition = originPosition;
    }
    bool IsCircleAtPosition(Vector2 position)
    {
        Debug.Log("OnIsCircleAtPosition");
        foreach (Transform child in faceImage)
        {
            if (child.gameObject.CompareTag("StatusImage"))
            {
                RectTransform circleRect = child as RectTransform;
                // Directly check if the position is within the bounds of the circleRect
                if (circleRect.rect.Contains(position - (Vector2)circleRect.anchoredPosition))
                {
                    return true;
                }
            }
        }
        return false;
    }
    GameObject GetCircleAtPosition(Vector2 position)
    {
        foreach (Transform child in faceImage)
        {
            if (child.gameObject.CompareTag("StatusImage"))  // Assuming circle objects have a "StatusImage" tag
            {
                RectTransform circleRect = child as RectTransform;
                if (circleRect.rect.Contains(position - (Vector2)circleRect.anchoredPosition))
                {
                    return child.gameObject;
                }
            }
        }
        return null;
    }

    public Vector2 ConvertToBackgroundLocalCoordinates(Vector2 originalLocalPos, Transform originalParent, Transform targetParent)
    {
        Vector3 worldPos = originalParent.TransformPoint(originalLocalPos); // Convert to world coordinates
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos); // Convert world to screen

        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent as RectTransform, screenPos, null, out Vector2 localPos); // Convert screen to local of target

        return localPos;
    }

}