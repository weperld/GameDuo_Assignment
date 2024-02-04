using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_DmgFont : UIBase
{
    public float fadeOutDelay = 2f;
    public float speed = 10f;
    public AnimationCurve alphaCurve;

    private float fadeTime = 0f;

    private TextMeshProUGUI tmpu_Dmg;
    private TextMeshProUGUI _Tmpu_Dmg
    {
        get
        {
            if (tmpu_Dmg == null)
            {
                if (!gameObject.TryGetComponent<TextMeshProUGUI>(out tmpu_Dmg))
                    tmpu_Dmg = gameObject.AddComponent<TextMeshProUGUI>();
            }
            return tmpu_Dmg;
        }
    }

    public void Set(float dmg, Vector2 appearPosition)
    {
        fadeTime = 0f;
        _Tmpu_Dmg.text = $"-{(int)dmg}";
        rectTransform.anchoredPosition = appearPosition;
    }
    public void Set(float dmg, Vector2 appearPosition, Color color)
    {
        Set(dmg, appearPosition);
        _Tmpu_Dmg.color = color;
    }

    private void Update()
    {
        if (fadeTime > fadeOutDelay)
        {
            if (SpawnManager.IsDestroying) Destroy(gameObject);
            else SpawnManager.Instance.Despawn(PathOfResources.Prefabs.UI.DmgFont, gameObject);
        }

        fadeTime += Time.deltaTime;
        float alpha = alphaCurve.Evaluate(fadeTime / fadeOutDelay);
        var color = _Tmpu_Dmg.color;
        color.a = alpha;
        _Tmpu_Dmg.color = color;
        rectTransform.anchoredPosition += Vector2.up * speed * Time.deltaTime;
    }
}
