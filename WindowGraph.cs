using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WindowGraph : MonoBehaviour
{
    [Header("--- BAÐLANTILAR ---")]
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;
    [SerializeField] private Sprite circleSprite;

    [Header("--- TARÝH SEÇÝCÝ ---")]
    public TextMeshProUGUI dateText;
    public Button prevDayBtn;
    public Button nextDayBtn;

    [Header("--- RENK VE GÖRSEL AYARLAR ---")]
    // Pastel Yeþil (Mutlu - Happy)
    [SerializeField] private Color goodColor = new Color(0.3f, 0.85f, 0.4f, 1f);
    // Pastel Mercan (Üzgün - Sad)
    [SerializeField] private Color badColor = new Color(1f, 0.4f, 0.4f, 1f);

    // Çizgi Rengi (Siyah - Black)
    [SerializeField] private Color connectionColor = Color.black;

    [SerializeField] private float lineThickness = 6f;

    [Header("--- IZGARA AYARLARI ---")]
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private float gridThickness = 2f;

    private System.DateTime currentDate;

    private void Awake()
    {
        currentDate = System.DateTime.Now;
        UpdateDateUI();

        prevDayBtn.onClick.AddListener(() => ChangeDate(-1));
        nextDayBtn.onClick.AddListener(() => ChangeDate(1));

        labelTemplateX.gameObject.SetActive(false);
        labelTemplateY.gameObject.SetActive(false);

        LoadDataForDate(currentDate);
    }

    void ChangeDate(int days)
    {
        currentDate = currentDate.AddDays(days);
        UpdateDateUI();
        LoadDataForDate(currentDate);
    }

    // --- DEÐÝÞÝKLÝK 1: TARÝH FORMATI ÝNGÝLÝZCE ---
    void UpdateDateUI()
    {
        // Kültürü "en-US" yaptýk (January, Monday vs. yazar)
        System.Globalization.CultureInfo enCulture = new System.Globalization.CultureInfo("en-US");
        // Format: "January 28, 2026, Wednesday"
        dateText.text = currentDate.ToString("MMMM d, yyyy, dddd", enCulture);
    }

    void LoadDataForDate(System.DateTime date)
    {
        List<int> scoreList = new List<int>();

        if (ParentDataManager.Instance != null)
        {
            scoreList = ParentDataManager.Instance.GetScoresForDate(date);
        }

        if (scoreList.Count == 0)
        {
            int pointCount = 7;
            Random.InitState(date.DayOfYear * date.Year);
            int[] allowedScores = { 0, 25, 50, 75, 100 };
            for (int i = 0; i < pointCount; i++)
            {
                scoreList.Add(allowedScores[Random.Range(0, allowedScores.Length)]);
            }
        }

        ShowGraph(scoreList);
    }

    public void ShowGraph(List<int> valueList)
    {
        foreach (Transform child in graphContainer)
        {
            if (child != labelTemplateX && child != labelTemplateY)
                Destroy(child.gameObject);
        }

        float graphHeight = graphContainer.rect.height;
        float graphWidth = graphContainer.rect.width;
        float yMaximum = 100f;
        float xSize = graphWidth / (valueList.Count + 1);

        GameObject lastCircleGameObject = null;

        // --- DEÐÝÞÝKLÝK 2: Y EKSENÝ ETÝKETLERÝ ÝNGÝLÝZCE ---
        int separatorCount = 4;
        for (int i = 0; i <= separatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = (float)i / separatorCount;
            float yOffset = -15f;
            labelY.anchoredPosition = new Vector2(-10f, (normalizedValue * graphHeight) + yOffset);

            string moodLabel = "";
            if (normalizedValue == 0) moodLabel = "Bad";         // Kötü -> Bad
            else if (normalizedValue == 0.25f) moodLabel = "Sad"; // Üzgün -> Sad
            else if (normalizedValue == 0.5f) moodLabel = "Neutral"; // Nötr -> Neutral
            else if (normalizedValue == 0.75f) moodLabel = "Happy"; // Mutlu -> Happy
            else moodLabel = "Great!";                              // Harika -> Great!

            labelY.GetComponent<TextMeshProUGUI>().text = moodLabel;

            CreateGridLine(new Vector2(0, normalizedValue * graphHeight),
                           new Vector2(graphWidth, normalizedValue * graphHeight));
        }

        // --- NOKTALAR VE ÇÝZGÝLER ---
        for (int i = 0; i < valueList.Count; i++)
        {
            int currentScore = valueList[i];
            float xPosition = xSize + i * xSize;
            float yPosition = (currentScore / yMaximum) * graphHeight;
            Vector2 pointPosition = new Vector2(xPosition, yPosition);

            Color dotColor = (currentScore >= 50) ? goodColor : badColor;

            CreateGridLine(new Vector2(xPosition, 0), pointPosition);

            GameObject circleGameObject = CreateCircle(pointPosition, dotColor);

            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
                                    circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }

            lastCircleGameObject = circleGameObject;

            // Saat (Format zaten evrensel: 10:00)
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPosition, -20f);
            labelX.GetComponent<TextMeshProUGUI>().text = (10 + (i * 2)).ToString("00") + ":00";
        }
    }

    private GameObject CreateCircle(Vector2 anchoredPosition, Color color)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        Image image = gameObject.GetComponent<Image>();
        image.sprite = circleSprite;
        image.color = color;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(25, 25);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.SetAsLastSibling();
        return gameObject;
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        Image image = gameObject.GetComponent<Image>();
        image.color = connectionColor;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, lineThickness);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        rectTransform.SetSiblingIndex(graphContainer.childCount - 2);
    }

    private void CreateGridLine(Vector2 start, Vector2 end)
    {
        GameObject gameObject = new GameObject("gridLine", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        Image image = gameObject.GetComponent<Image>();
        image.color = gridColor;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (end - start).normalized;
        float distance = Vector2.Distance(start, end);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, gridThickness);
        rectTransform.anchoredPosition = start + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        rectTransform.SetAsFirstSibling();
    }
}