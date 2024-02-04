using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour, IUI
{
    private RectTransform rectTf;
    public RectTransform rectTransform
    {
        get
        {
            if (rectTf == null)
            {
                if (!gameObject.TryGetComponent<RectTransform>(out rectTf))
                    rectTf = gameObject.AddComponent<RectTransform>();
            }

            return rectTf;
        }
    }

    public Canvas rootCanvas
    {
        get
        {
            var ret = transform.GetComponentInChildren<Canvas>();
            return ret;
        }
    }
}
