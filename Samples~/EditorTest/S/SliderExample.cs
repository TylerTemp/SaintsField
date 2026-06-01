using UnityEngine;

[ExecuteInEditMode]
public class SliderExample : MonoBehaviour
{
    public Vector3 targetPosition { get { return m_TargetPosition; } set { m_TargetPosition = value; } }
    [SerializeField]
    private Vector3 m_TargetPosition = new Vector3(1f, 0f, 2f);

    public virtual void Update()
    {
        transform.LookAt(m_TargetPosition);
    }
}
