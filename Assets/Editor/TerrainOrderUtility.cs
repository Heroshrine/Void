using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TerrainOrderUtility
{
    private const float SortingMultiplier = 10f;

    [MenuItem("Void/Set Terrain Order")]
    public static void SetTerrainOrder()
    {
        var terrainSpriteRenderers = new List<SpriteRenderer>();
        var transforms = Object.FindObjectsOfType<Transform>(true);

        foreach (var transform in transforms)
        {
            if (!transform.CompareTag("Terrain"))
            {
                continue;
            }

            terrainSpriteRenderers.AddRange(transform.GetComponents<SpriteRenderer>());
        }

        if (terrainSpriteRenderers.Count == 0)
        {
            Debug.LogWarning("No Terrain-tagged SpriteRenderers found in the scene to update.");
            return;
        }

        var spriteRendererArray = terrainSpriteRenderers.ToArray();
        Undo.RecordObjects(spriteRendererArray, "Set Terrain Order");

        foreach (var renderer in spriteRendererArray)
        {
            if (renderer == null)
            {
                continue;
            }

            var worldPosition = renderer.transform.position;
            var sortingOrder = Mathf.CeilToInt(-worldPosition.y * SortingMultiplier);
            renderer.sortingOrder = sortingOrder;
            EditorUtility.SetDirty(renderer);
        }

        Debug.Log($"Updated sorting order for {spriteRendererArray.Length} Terrain SpriteRenderers.");
    }
}
