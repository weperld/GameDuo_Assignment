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

    #region 캔버스
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
    /// 해당 캔버스가 씬에 생성되어 있는지 체크 후 없다면 생성
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

    #region 좌표 변환
    /// <summary>
    /// 월드 좌표를 캔버스 좌표로 변환
    /// </summary>
    /// <param name="canvas">기준 캔버스</param>
    /// <param name="worldPosition">변환할 월드 좌표</param>
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

    #region 데미지 폰트 생성
    /// <summary>
    /// 데미지 폰트 UI 생성
    /// </summary>
    /// <param name="dmg">데미지 수치(소수점 버림)</param>
    /// <param name="worldSpawnPosition">피격 당한 위치의 월드 좌표</param>
    /// <param name="offset">데미지 폰트 ui 생성 위치의 오프셋</param>
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
    /// 데미지 폰트 UI 생성
    /// </summary>
    /// <param name="dmg">데미지 수치(소수점 버림)</param>
    /// <param name="worldSpawnPosition">피격 당한 위치의 월드 좌표</param>
    public void SpawnDamageEffectUI(float dmg, Vector3 worldSpawnPosition, Color color)
        => SpawnDamageEffectUI(dmg, worldSpawnPosition, Vector2.zero, color);
    #endregion
}
