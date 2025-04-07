using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 



public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance;
    public TextMeshProUGUI alertText;

    public GameObject alertPanel;

    public GameObject alertPanelPrefab; // gắn Prefab từ Inspector
    public Transform alertCanvasParent; // nơi để spawn alert (thường là UI Canvas)


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        alertPanel.SetActive(false); // Ẩn panel ngay từ đầu
    }


    public void ShowAlert(string message)
    {
        if (alertPanelPrefab != null && alertCanvasParent != null)
        {
            // Dùng prefab nếu có
            alertCanvasParent.gameObject.SetActive(true);
            GameObject alertInstance = Instantiate(alertPanelPrefab, alertCanvasParent);
            alertInstance.SetActive(true);
            TextMeshProUGUI tmp = alertInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.text = message;

            Destroy(alertInstance, 3f);
        }
        else
        {
            // Nếu prefab chưa được gán, dùng panel cũ
            if (alertText != null && alertPanel != null)
            {
                alertText.text = message;
                alertPanel.SetActive(true);
                StopAllCoroutines();
                StartCoroutine(HideAfterSeconds(3));
            }
            else
            {
                Debug.LogWarning("AlertManager: Không có prefab hoặc alertPanel fallback!");
            }
        }
    }


    private IEnumerator HideAfterSeconds(float time)
    {
        yield return new WaitForSeconds(time);
        alertPanel.SetActive(false);
    }
}
