using UnityEngine;

public interface IColourable
{
    /// <summary>
    /// Sets the colour of the component.
    /// </summary>
    /// <param name="colour">The new colour to set.</param>
    /// <param name="key"> The authorisation key to be passed in if requested </param>
    public void SetMaterialColor(Color color, object key);

    public Color GetMaterialColor();
}