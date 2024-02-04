using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorSwatchSO", menuName = "Scriptable Object/Color Swatch Object")]
public class ColorSwatchObject : ScriptableObject
{
    [SerializeField]
    private List<Color> colorSwatches = new List<Color>();

    public List<Color> _ColorSwatches => colorSwatches;
}