using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CategorySelectionManager : MonoBehaviour
{
    [Header("--- UI ELEMANLARI ---")]
    public GameObject backgroundElements;
    public Button continueButton;
    public GridLayoutGroup iconGrid;

    [Header("--- ÝKONLAR VE HEDEFLER ---")]
    public RectTransform[] categoryIcons;
    public RectTransform[] bottomTargets;
    public float shrinkScale = 0.5f;

    [Header("--- SAHNE HARÝTASI AYARLARI ---")]
    public Transform worldContainer; // UI deðil, normal Transform!
    public float[] worldPositionsX; // Sahnede adalarýn durduðu X koordinatlarý (0, 20, 40...)
    public float slideSpeed = 5f;

    private int selectedIndex = -1;
    private bool hasAnimated = false;

    void Start()
    {
        continueButton.interactable = false;
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void OnIconClicked(int index)
    {
        selectedIndex = index;

        // Ýkon görsel geri bildirimi
        for (int i = 0; i < categoryIcons.Length; i++)
        {
            CanvasGroup cg = categoryIcons[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = categoryIcons[i].gameObject.AddComponent<CanvasGroup>();
            cg.alpha = (i == index) ? 1f : 0.5f;
        }

        if (!hasAnimated) continueButton.interactable = true;
        else
        {
            StopAllCoroutines();
            StartCoroutine(SlideWorld(worldPositionsX[selectedIndex]));
        }
    }

    public void OnContinueClicked()
    {
        if (selectedIndex == -1) return;
        hasAnimated = true;

        backgroundElements.SetActive(false);
        continueButton.gameObject.SetActive(false);
        if (iconGrid != null) iconGrid.enabled = false;

        StartCoroutine(AnimateIconsToBottom());
        // Seçilen adaya git
        StartCoroutine(SlideWorld(worldPositionsX[selectedIndex]));
    }

    IEnumerator AnimateIconsToBottom()
    {
        float elapsed = 0f;
        float duration = 0.6f;
        Vector3[] startPos = new Vector3[categoryIcons.Length];
        Vector3 startScale = categoryIcons[0].localScale;
        Vector3 targetScale = new Vector3(shrinkScale, shrinkScale, 1f);

        for (int i = 0; i < categoryIcons.Length; i++)
            startPos[i] = categoryIcons[i].position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            for (int i = 0; i < categoryIcons.Length; i++)
            {
                categoryIcons[i].position = Vector3.Lerp(startPos[i], bottomTargets[i].position, t);
                categoryIcons[i].localScale = Vector3.Lerp(startScale, targetScale, t);
            }
            yield return null;
        }
    }

    // --- SAHNE OBJESÝNÝ KAYDIRAN YENÝ FONKSÝYON ---
    IEnumerator SlideWorld(float targetX)
    {
        // Y ve Z deðerlerini koruyoruz, sadece X'i deðiþtiriyoruz
        Vector3 targetPos = new Vector3(targetX, worldContainer.position.y, worldContainer.position.z);

        while (Vector3.Distance(worldContainer.position, targetPos) > 0.01f)
        {
            worldContainer.position = Vector3.Lerp(worldContainer.position, targetPos, Time.deltaTime * slideSpeed);
            yield return null;
        }
        worldContainer.position = targetPos;
    }
}