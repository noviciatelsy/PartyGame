using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cowboy : MonoBehaviour
{
    [Header("Fire Visual")]
    public Transform fireImage;     // 开火图片
    public GameObject bulletLine;   // 枪线图片

    public float rotate = 10.0f;
    void Start()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        // 初始化
        if (fireImage != null)
        {
            fireImage.localPosition = new Vector3(0f, 0f, 1f);
        }

        if (bulletLine != null)
        {
            bulletLine.SetActive(false);
        }
    }

    // =========================
    // 开火接口
    // =========================
    public void Fire()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, rotate);

        if (fireImage != null)
        {
            fireImage.localPosition = new Vector3(0f, 0f, -1f);
        }

        StartCoroutine(FireCoroutine());
    }

    IEnumerator FireCoroutine()
    {
        yield return new WaitForSeconds(0.05f);

        if (bulletLine != null)
        {
            bulletLine.SetActive(true);
        }
    }

    public void OnShoot()
    {
        if (fireImage != null)
        {
            fireImage.localPosition = new Vector3(0f, 0f, -1f);
        }
    }
}
