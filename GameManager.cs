using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 0: Normal, 1: Kolay (Yorgunsa), 2: Zor (Enerjikse)
    public int currentMoodLevel = 0;

    void Awake()
    {
        // Bu objeden sadece bir tane olsun ve sahneler deðiþse de silinmesin
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arasý yok olma!
        }
        else
        {
            Destroy(gameObject);
        }
    }
}