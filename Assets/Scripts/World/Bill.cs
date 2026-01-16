using UnityEngine;

public class Bill : MonoBehaviour
{
    public GameObject[] billLeft, billRight;
    public int count;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int randLeft = Random.Range(0, billLeft.Length + count);
        for (int i = 0; i < billLeft.Length; i++)
        {
            if (i == randLeft) billLeft[i].SetActive(true);
            else billLeft[i].SetActive(false);
        }

        int randRight = Random.Range(0, billRight.Length + count);
        for (int i = 0; i < billRight.Length; i++)
        {
            if (i == randRight) billRight[i].SetActive(true);
            else billRight[i].SetActive(false);
        }
    }
}
