using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class TriangleCounterWindow : EditorWindow
{
    // Hier kannst du einstellen, wie viele Objekte angezeigt werden sollen
    private const int TopCount = 10;

    [MenuItem("Tools/Find Objects with Most Triangles")]
    public static void FindMostComplexObjects()
    {
        // Liste für alle gefundenen Objekte und ihre Dreiecksanzahl
        var meshDataList = new List<(GameObject obj, int triangleCount)>();

        // 1. Normale Meshes durchsuchen
        MeshFilter[] meshFilters = GameObject.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        foreach (var filter in meshFilters)
        {
            if (filter.sharedMesh != null)
            {
                int triCount = filter.sharedMesh.triangles.Length / 3;
                meshDataList.Add((filter.gameObject, triCount));
            }
        }

        // 2. Skinned Meshes (z.B. Charaktere) durchsuchen
        SkinnedMeshRenderer[] skinnedRenderers = GameObject.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
        foreach (var skinned in skinnedRenderers)
        {
            if (skinned.sharedMesh != null)
            {
                int triCount = skinned.sharedMesh.triangles.Length / 3;
                meshDataList.Add((skinned.gameObject, triCount));
            }
        }

        // 3. Nach Dreiecksanzahl absteigend sortieren und die Top X nehmen
        var topObjects = meshDataList
            .OrderByDescending(item => item.triangleCount)
            .Take(TopCount)
            .ToList();

        // 4. Ergebnis in der Konsole ausgeben
        if (topObjects.Count > 0)
        {
            Debug.Log($"=== <b>TOP {topObjects.Count} OBJEKTE MIT DEN MEISTEN DREIECKEN</b> ===");
            
            for (int i = 0; i < topObjects.Count; i++)
            {
                var item = topObjects[i];
                // Das Objekt wird am Ende der Log-Zeile übergeben, damit man es in der Szene durch Anklicken highlighten kann
                Debug.Log($"<b>#{i + 1}</b>: {item.obj.name} -> <b>{item.triangleCount:N0}</b> Tris", item.obj);
            }
            
            // Wählt das allerhöchste Objekt automatisch in der Hierarchie aus
            Selection.activeGameObject = topObjects[0].obj;
        }
        else
        {
            Debug.LogWarning("Keine Meshes in der Szene gefunden.");
        }
    }
}
