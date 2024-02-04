using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_WaveStartAlarm : UIBase
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI tmpu_Wave;
    public GameObject go_ExclamationYellow;
    public GameObject go_ExclamationRed;

    public AnimationCurve blendCurve;
    public float fadeOutDelay;

    private float fadeInTime;

    private void Awake()
    {
        if (!WaveManager.IsDestroying) WaveManager.Instance._ActionOnStartWave.RegistAction(OnStartWave);
    }

    private void Update()
    {
        fadeInTime += Time.deltaTime;
        if (fadeInTime > fadeOutDelay)
        {
            gameObject.SetActive(false);
            return;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = blendCurve.Evaluate(fadeInTime / fadeOutDelay);
    }

    private void OnStartWave(int stage, int wave)
    {
        if (WaveManager.IsDestroying) return;

        gameObject.SetActive(true);
        if (tmpu_Wave != null) tmpu_Wave.text = $"{stage} - {wave} 웨이브 시작!";
        if (go_ExclamationYellow != null) go_ExclamationYellow.SetActive(!WaveManager.Instance._IsBossWave);
        if (go_ExclamationRed != null) go_ExclamationRed.SetActive(WaveManager.Instance._IsBossWave);
        fadeInTime = 0f;
    }
}
