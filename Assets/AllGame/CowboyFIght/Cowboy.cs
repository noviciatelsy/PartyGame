using UnityEngine;

public class Cowboy : MonoBehaviour
{
    [Header("Cowboy Images")]
    public GameObject idleImage;
    public GameObject winImage;
    public GameObject loseImage;
    public GameObject fireLine;
    public AudioSource audiosource;
    void Start()
    {
        SetIdle();
    }

    // ³õÊ¼×´Ì¬
    public void SetIdle()
    {
        idleImage.SetActive(true);
        winImage.SetActive(false);
        loseImage.SetActive(false);
        audiosource = GetComponent<AudioSource>();
        if (fireLine != null)
            fireLine.SetActive(false);
    }

    // Ê¤Àû
    public void SetWin()
    {
        idleImage.SetActive(false);
        winImage.SetActive(true);
        loseImage.SetActive(false);

        if (fireLine != null && audiosource != null)
        {
            audiosource.Play();
            fireLine.SetActive(true);
        }
    }

    // Ê§°Ü
    public void SetLose()
    {
        idleImage.SetActive(false);
        winImage.SetActive(false);
        loseImage.SetActive(true);

        if (fireLine != null)
            fireLine.SetActive(false);
    }
}