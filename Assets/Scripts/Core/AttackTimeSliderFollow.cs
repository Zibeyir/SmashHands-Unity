using UnityEngine;

public class AttackTimeSliderFollow : MonoBehaviour
{
    public Transform target;

    Vector3 offset = Vector3.zero;
    private void Start()
    {
        transform.rotation = Quaternion.identity;
        transform.parent = null;
    }
    public void GetOffset(float _offset)
    {
        offset = new Vector3(0, -_offset-.5f, 0);
    }
    void LateUpdate()
    {
        transform.position = target.position +offset;

    }
}
