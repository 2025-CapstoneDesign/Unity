using TMPro;
using UnityEngine;

public class CountTextDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void ShowCompressionCount(int count)
    {
        gameObject.SetActive(true);
        text.text = $"가슴 압박 횟수: {count}회";
    }

    public void ShowBreathCount(int count)
    {
        gameObject.SetActive(true);
        text.text = $"인공호흡 횟수: {count}회";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        text.text = string.Empty;
    }
}
