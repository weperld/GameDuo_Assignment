using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasRegister : MonoBehaviour
{
    public Enums.CanvasType canvasType;

    private void Awake()
    {
        if (!UIManager.IsDestroying && gameObject.TryGetComponent<Canvas>(out var canvas))
        {
            UIManager.Instance.RegistCanvas(canvasType, canvas);   
        }
    }
}
