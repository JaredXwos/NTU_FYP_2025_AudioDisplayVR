using System;
using UnityEditor.Rendering;
using UnityEngine;

public class BaseInitialiser : MonoBehaviour
{
    [SerializeField] int map = 0;
    static readonly int[][] AllConfigurations = new int[][]
    {
        new int[]
        {
            3, 2, 3, 1,
            3, 1, 3, 3,
            2, 2, 1, 1,
            1, 1, 1, 2,
        },
        new int[]{
            1, 1, 1, 1,
            3, 2, 1, 2,
            1, 2, 3, 1,
            3, 3, 3, 3,
        },
        new int[]
        {
            1, 2, 3, 2,
            3, 3, 3, 2,
            1, 1, 1, 2,
            1, 3, 2, 3,
        },
        new int[]
        {
            1, 3, 1, 3,
            2, 1, 1, 2,
            1, 1, 3, 3,
            3, 1, 2, 3,
        },
        new int[]
        {
            1, 3, 3, 1,
            2, 1, 1, 3,
            1, 2, 3, 1,
            2, 2, 2, 3,
        },
        new int[]
        {
            1, 3, 3, 1,
            2, 1, 1, 3,
            1, 2, 3, 1,
            2, 2, 2, 3,
        }
    };
    public int[][] CurrentBase = new int[][] {new int[4], new int[4], new int[4], new int[4] };
    private Transform[] stackTransforms;
    private void Awake()
    {
        stackTransforms = GetComponentsInChildren<Transform>();
        Array.Sort(stackTransforms, (a, b) => (a.localPosition.x + a.localPosition.z * 4).CompareTo(b.localPosition.x + b.localPosition.z * 4));
        for(int i = 0; i < 16; i++)
        {
            Vector3 position = stackTransforms[i].localPosition;
            position.y = AllConfigurations[map][i]/2 - 1;
            stackTransforms[i].localPosition = position;

            Vector3 scale = stackTransforms[i].localScale;
            scale.y = AllConfigurations[map][i];
            stackTransforms[i].localScale = scale;

            CurrentBase[i / 4][i % 4] = AllConfigurations[map][i];
        }
    }
}
