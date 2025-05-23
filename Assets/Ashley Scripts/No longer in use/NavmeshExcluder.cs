using UnityEngine;
using UnityEngine.AI;
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
#if UNITY_EDITOR
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
#endif
    }

    [ContextMenu("Remove NavMesh Exclusion")]
    public void RemoveNavMeshExclusion()
    {
#if UNITY_EDITOR
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
#endif
    }

    void ExcludeFromNavMesh(GameObject obj)
    {
#if UNITY_EDITOR
        // Get the current static flags
        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);

        // Remove Navigation Static flag
        flags &= ~StaticEditorFlags.NavigationStatic;

        // Apply the updated flags
        GameObjectUtility.SetStaticEditorFlags(obj, flags);

        // Alternative method: Set the Navigation Area to "Not Walkable" (1)
        // This requires the object to be Navigation Static
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
        // Get the current static flags
        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);

        // Add Navigation Static flag
        flags |= StaticEditorFlags.NavigationStatic;

        // Apply the updated flags
        GameObjectUtility.SetStaticEditorFlags(obj, flags);

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