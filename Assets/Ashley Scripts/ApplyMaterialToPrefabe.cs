using UnityEngine;

public class ApplyMaterialToPrefab : MonoBehaviour
{
    public Material materialToApply;

    [ContextMenu("Apply Material To All Renderers")]
    void ApplyMaterialToAllRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            // Apply to all material slots
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialToApply;
            }
            renderer.sharedMaterials = materials;
        }
    }
}