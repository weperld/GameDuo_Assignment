using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Manager<UIManager>
{
    private Dictionary<Enums.CanvasType, Canvas> dict_Canvas = new Dictionary<Enums.CanvasType, Canvas>();

    private ColorSwatchObject colorSwatchSO;
    public List<Color> _ColorSwatches
    {
        get
        {
            if (colorSwatchSO == null)
                colorSwatchSO = Resources.Load<ColorSwatchObject>(PathOfResources.SO.ColorSwatches);

            return colorSwatchSO._ColorSwatches;
        }
    }
    
    public T LoadUI<T>(string path) where T : UIBase
    {
        return ResourceLoader.Load<T>(path);
    }
    public UIBase LoadUI(string path) => LoadUI<UIBase>(path);

    #region ĵ����
    public void RegistCanvas(Enums.CanvasType canvasType, Canvas canvas)
    {
        if (canvas == null) return;

        canvas.sortingOrder = (int)canvasType;
        if (dict_Canvas.TryGetValue(canvasType, out var v))
        {
            if (v != null) Destroy(v.gameObject);
            dict_Canvas[canvasType] = canvas;
        }

        else dict_Canvas.Add(canvasType, canvas);
    }

    private Canvas CreateCanvas(Enums.CanvasType canvasType)
    {
        string canvasName = $"Canvas_{canvasType}";
        int sortOrder = (int)canvasType;
        if (dict_Canvas.TryGetValue(canvasType, out var v)) { v.gameObject.name = canvasName; v.sortingOrder = sortOrder; return v; }

        var canvasGO = new GameObject(canvasName);

        var canvasComp = canvasGO.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = sortOrder;

        var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.transform.SetParent(null);

        dict_Canvas.Add(canvasType, canvasComp);
        return canvasComp;
    }

    /// <summary>
    /// �ش� ĵ������ ���� �����Ǿ� �ִ��� üũ �� ���ٸ� ����
    /// </summary>
    /// <param name="canvasType"></param>
    /// <returns></returns>
    private void CheckCanvasAndCreate(Enums.CanvasType canvasType)
    {
        if (!dict_Canvas.TryGetValue(canvasType, out var v) || v == null)
        {
            v = CreateCanvas(canvasType);
            RegistCanvas(canvasType, v);
        }
    }

    public Canvas GetCanvas(Enums.CanvasType canvasType)
    {
        if (dict_Canvas.ContainsKey(canvasType)) return dict_Canvas[canvasType];
        return null;
    }
    #endregion

    #region ��ǥ ��ȯ
    /// <summary>
    /// ���� ��ǥ�� ĵ���� ��ǥ�� ��ȯ
    /// </summary>
    /// <param name="canvas">���� ĵ����</param>
    /// <param name="worldPosition">��ȯ�� ���� ��ǥ</param>
    /// <returns></returns>
    public Vector2 ConvertWorldPositionToCanvasPosition(Canvas canvas, Vector3 worldPosition)
    {
        if (canvas == null)
        {
            Debug.LogError("UIManager - ConvertWorldPositionToCanvasPosition, Input Canvas is NULL");
            return Vector2.zero;
        }

        var cam = Camera.main;
        var screenPoint = cam.WorldToScreenPoint(worldPosition);

        var canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 ret;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, out ret);

        return ret;
    }
    public Vector2 ConvertWorldPositionToCanvasPosition(Enums.CanvasType canvasType, Vector3 worldPosition)
        => ConvertWorldPositionToCanvasPosition(GetCanvas(canvasType), worldPosition);
    #endregion

    #region ������ ��Ʈ ����
    /// <summary>
    /// ������ ��Ʈ UI ����
    /// </summary>
    /// <param name="dmg">������ ��ġ(�Ҽ��� ����)</param>
    /// <param name="worldSpawnPosition">�ǰ� ���� ��ġ�� ���� ��ǥ</param>
    /// <param name="offset">������ ��Ʈ ui ���� ��ġ�� ������</param>
    public void SpawnDamageEffectUI(float dmg, Vector3 worldSpawnPosition, Vector2 offset, Color color)
    {
        if (SpawnManager.IsDestroying) return;

        CheckCanvasAndCreate(Enums.CanvasType.DMG_FONT);
        var canvas = GetCanvas(Enums.CanvasType.DMG_FONT);
        var dmgFont = SpawnManager.Instance.Spawn<UI_DmgFont>(PathOfResources.Prefabs.UI.DmgFont, canvas.transform);
        if (dmgFont == null) return;

        var canvasPosition = ConvertWorldPositionToCanvasPosition(Enums.CanvasType.DMG_FONT, worldSpawnPosition);
        dmgFont.Set(dmg, canvasPosition + offset, color);
    }
    /// <summary>
    /// ������ ��Ʈ UI ����
    /// </summary>
    /// <param name="dmg">������ ��ġ(�Ҽ��� ����)</param>
    /// <param name="worldSpawnPosition">�ǰ� ���� ��ġ�� ���� ��ǥ</param>
    public void SpawnDamageEffectUI(float dmg, Vector3 worldSpawnPosition, Color color)
        => SpawnDamageEffectUI(dmg, worldSpawnPosition, Vector2.zero, color);
    #endregion
}
