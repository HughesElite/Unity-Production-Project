using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class NavMeshExcluder : MonoBehaviour
{
    [Header("NavMesh Exclusion Settings")]
    [Tooltip("Automatically apply exclusion when the component is added")]
    public bool autoApplyOnAdd = true;

    [Tooltip("Apply exclusion to all child objects as well")]
    public bool applyToChildren = true;

    [Tooltip("Show visual indicator in Scene view")]
    public bool showVisualIndicator = true;
    public Color indicatorColor = new Color(1f, 0f, 0f, 0.3f);

    void Awake()
    {
        if (autoApplyOnAdd)
        {
            ApplyNavMeshExclusion();
        }
    }

    void OnValidate()
    {
        // Reapply when settings change in inspector
        ApplyNavMeshExclusion();
    }

    [ContextMenu("Apply NavMesh Exclusion")]
    public void ApplyNavMeshExclusion()
    {
        // Exclude this object
        ExcludeFromNavMesh(gameObject);

        // Exclude children if enabled
        if (applyToChildren)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child != transform)
                {
                    ExcludeFromNavMesh(child.gameObject);
                }
            }
        }

        Debug.Log($"NavMesh exclusion applied to {gameObject.name}" +
                  (applyToChildren ? " and its children" : ""));
    }

    [ContextMenu("Remove NavMesh Exclusion")]
    public void RemoveNavMeshExclusion()
    {
        // Re-enable this object for NavMesh
        IncludeInNavMesh(gameObject);

        // Re-enable children if setting is enabled
        if (applyToChildren)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child != transform)
                {
                    IncludeInNavMesh(child.gameObject);
                }
            }
        }

        Debug.Log($"NavMesh exclusion removed from {gameObject.name}" +
                  (applyToChildren ? " and its children" : ""));
    }

    void ExcludeFromNavMesh(GameObject obj)
    {
#if UNITY_EDITOR
        // Method 1: Use NavMeshModifier component (recommended)
        NavMeshModifier modifier = obj.GetComponent<NavMeshModifier>();
        if (modifier == null)
        {
            modifier = obj.AddComponent<NavMeshModifier>();
        }
        modifier.overrideArea = true;
        modifier.area = 1; // 1 = Not Walkable

        // Method 2: Set Navigation Area directly
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty navMeshArea = serializedObject.FindProperty("m_NavMeshArea");
        if (navMeshArea != null)
        {
            navMeshArea.intValue = 1; // 1 = Not Walkable
            serializedObject.ApplyModifiedProperties();
        }
#endif
    }

    void IncludeInNavMesh(GameObject obj)
    {
#if UNITY_EDITOR
        // Remove NavMeshModifier if it exists
        NavMeshModifier modifier = obj.GetComponent<NavMeshModifier>();
        if (modifier != null)
        {
            DestroyImmediate(modifier);
        }

        // Set the Navigation Area back to "Walkable" (0)
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty navMeshArea = serializedObject.FindProperty("m_NavMeshArea");
        if (navMeshArea != null)
        {
            navMeshArea.intValue = 0; // 0 = Walkable
            serializedObject.ApplyModifiedProperties();
        }
#endif
    }

    // Visual indicator in Scene view
    void OnDrawGizmos()
    {
        if (!showVisualIndicator) return;

        Gizmos.color = indicatorColor;

        // Draw a cube for box colliders or a sphere for others
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else
            {
                Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
            }
        }
        else
        {
            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
        }

        // Draw an X to indicate exclusion
        Gizmos.color = Color.red;
        float size = 0.5f;
        Gizmos.DrawLine(transform.position - Vector3.right * size, transform.position + Vector3.right * size);
        Gizmos.DrawLine(transform.position - Vector3.forward * size, transform.position + Vector3.forward * size);
    }

    // Show the label in Scene view
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Handles.Label(transform.position + Vector3.up, "NavMesh Excluded");
#endif
    }
}