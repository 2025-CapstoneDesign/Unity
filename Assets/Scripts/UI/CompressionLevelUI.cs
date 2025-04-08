using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompressionLevelUI : MonoBehaviour
{
    [Range(0, 100)]
    public float currentStrength = 0f; // 외부에서 이 값만 갱신하면 됨

    public List<Image> levelImages = new List<Image>(); // 5개 색상 블록

    void Start()
    {
        SetAllAlpha(0.2f); // 시작 시 모두 반투명
    }

    void Update()
    {
        int level = GetLevel(currentStrength);

        for (int i = 0; i < levelImages.Count; i++)
        {
            float alpha = (i == level) ? 1f : 0.2f;
            SetImageAlpha(levelImages[i], alpha);
        }
    }

    int GetLevel(float value)
    {
        if (value < 20f) return 0;
        if (value < 40f) return 1;
        if (value < 60f) return 2;
        if (value < 80f) return 3;
        return 4;
    }

    void SetAllAlpha(float alpha)
    {
        foreach (var img in levelImages)
        {
            SetImageAlpha(img, alpha);
        }
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    // 외부에서 강도값 업데이트
    public void SetStrength(float value)
    {
        currentStrength = Mathf.Clamp(value, 0f, 100f);
    }
}
