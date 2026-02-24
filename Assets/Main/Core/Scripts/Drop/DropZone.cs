using UnityEngine;

public class DropZone : MonoBehaviour
{
    [SerializeField]
    string zoneName = "Drop Zone";
    [SerializeField]
    float requiredRadius = 15f;
    [SerializeField]
    Color gizmoColor = Color.yellow;

    public string ZoneName => zoneName;
    public float RequiredRadius => requiredRadius;
    public bool IsCompleted { get; private set; }

    public bool IsInsideZone(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= requiredRadius;
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsCompleted ? Color.green : gizmoColor;
        Gizmos.DrawWireSphere(transform.position, requiredRadius);
    }
}