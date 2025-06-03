using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIDragDropManager : MonoBehaviour
{
    [Header("Canvas & UI Elements")]
    public Canvas canvas;
    public GraphicRaycaster raycaster;

    [Header("Draggable Items (5 buttons)")]
    public GameObject[] draggableItems;

    [Header("Drop Target (UI RectTransform)")]
    public RectTransform dropTarget;

    [Header("Game Over Screen (UI Panel)")]
    public GameObject gameOverScreen;

    [Header("Success Delay (seconds)")]
    public float successDelay = 1f;

    private GameObject currentDragging;
    private int correctIndex = 0;
    private int failCount = 0;

    void Start()
    {
        PlayerPrefs.SetInt("Score", 0);
        gameOverScreen.SetActive(false);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 screenPos = touch.position;

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = screenPos;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    List<RaycastResult> results = new List<RaycastResult>();
                    raycaster.Raycast(pointerData, results);

                    foreach (var result in results)
                    {
                        for (int i = 0; i < draggableItems.Length; i++)
                        {
                            if (result.gameObject == draggableItems[i])
                            {
                                // Clone đúng parent
                                currentDragging = Instantiate(draggableItems[i], draggableItems[i].transform.parent);
                                currentDragging.transform.SetAsLastSibling();

                                // Gán vị trí đúng trong Canvas (Screen Space - Camera)
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                                    canvas.transform as RectTransform,
                                    screenPos,
                                    canvas.worldCamera,
                                    out Vector2 localPos);

                                RectTransform rt = currentDragging.GetComponent<RectTransform>();
                                rt.anchoredPosition = localPos;
                                break;
                            }
                        }
                    }
                    break;

                case TouchPhase.Moved:
                    if (currentDragging != null)
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            canvas.transform as RectTransform,
                            screenPos,
                            canvas.worldCamera,
                            out Vector2 localPos);

                        RectTransform rt = currentDragging.GetComponent<RectTransform>();
                        rt.anchoredPosition = localPos;
                    }
                    break;

                case TouchPhase.Ended:
                    if (currentDragging != null)
                    {
                        RectTransform draggingRT = currentDragging.GetComponent<RectTransform>();
                        RectTransform targetRT = dropTarget;

                        bool inTarget = IsOverlapping(draggingRT, targetRT);
                        bool isCorrect = currentDragging.name.Contains(draggableItems[correctIndex].name);

                        if (inTarget)
                        {
                            if (isCorrect)
                            {
                                PlayerPrefs.SetInt("Score", PlayerPrefs.GetInt("Score", 0) + 1);
                                Invoke(nameof(ShowGameOver), successDelay);
                            }
                            else
                            {
                                failCount++;
                                if (failCount >= 2)
                                {
                                    ShowGameOver();
                                }
                            }
                        }

                        Destroy(currentDragging);
                    }
                    break;
            }
        }
    }

    void ShowGameOver()
    {
        gameOverScreen.SetActive(true);
        Debug.Log("Game Over. Score: " + PlayerPrefs.GetInt("Score", 0));
    }
    
    bool IsOverlapping(RectTransform rt1, RectTransform rt2)
{
    Rect rect1 = GetWorldRect(rt1);
    Rect rect2 = GetWorldRect(rt2);
    return rect1.Overlaps(rect2);
}

Rect GetWorldRect(RectTransform rt)
{
    Vector3[] corners = new Vector3[4];
    rt.GetWorldCorners(corners);
    float xMin = corners[0].x;
    float xMax = corners[2].x;
    float yMin = corners[0].y;
    float yMax = corners[2].y;
    return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
}

}
