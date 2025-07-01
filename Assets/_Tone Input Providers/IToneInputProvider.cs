using Unity.Mathematics;
using UnityEngine;

public interface IToneInputProvider
{
    public int[] GetToneInput();
    public void RequestUpdate();
}