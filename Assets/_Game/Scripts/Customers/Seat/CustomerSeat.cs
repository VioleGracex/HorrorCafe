using UnityEngine;
using Ouiki.Items;

namespace Ouiki.Restaurant
{
    public class CustomerSeat : MonoBehaviour
    {
        [Header("Seat Offsets")]
        [SerializeField] private Vector3 sitPointOffset = new Vector3(0, 0, 0.5f);
        [SerializeField] private Vector3 standPointOffset = new Vector3(0, 0, -0.5f);

        [Header("Seat Rotations")]
        [SerializeField] private Vector3 sitPointEuler = Vector3.zero;   
        [SerializeField] private Vector3 standPointEuler = Vector3.zero; 

        [Header("References")]
        public CustomerServiceSlot serviceSlot;
        public bool IsOccupied { get; private set; }

        private Transform _transform;

        public Vector3 SitPointWorld => (_transform ??= transform).TransformPoint(sitPointOffset);
        public Vector3 StandPointWorld => (_transform ??= transform).TransformPoint(standPointOffset);

        public Quaternion SitRotationWorld => (_transform ??= transform).rotation * Quaternion.Euler(sitPointEuler);
        public Quaternion StandRotationWorld => (_transform ??= transform).rotation * Quaternion.Euler(standPointEuler);

        private void Awake()
        {
            _transform = transform;
            Release();
        }

        public void Reserve()
        {
            IsOccupied = true;
        }

        public void Release()
        {
            IsOccupied = false;
            HideServiceIndicator();
        }

        public void ShowServiceIndicator()
        {
            serviceSlot.gameObject.SetActive(true);
        }

        public void HideServiceIndicator()
        {
            serviceSlot.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Transform t = transform;

            // Sit Point
            Vector3 sitWorld = t.TransformPoint(sitPointOffset);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(sitWorld, 0.07f);
            Gizmos.DrawLine(t.position, sitWorld);

            Vector3 standWorld = t.TransformPoint(standPointOffset);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(standWorld, 0.07f);
            Gizmos.DrawLine(t.position, standWorld);

            if (serviceSlot != null)
            {
                Gizmos.color = new Color(0.6f, 0.3f, 0f, 0.2f); 
                Gizmos.DrawCube(serviceSlot.transform.position, Vector3.one * 0.18f);
            }

            // Draw Sit Rotation
            Gizmos.color = Color.cyan;
            Quaternion sitRot = Quaternion.Euler(sitPointEuler);
            Vector3 dir = sitRot * Vector3.forward * 0.28f;
            Gizmos.DrawRay(sitWorld, dir);

            // Draw Stand Rotation
            Gizmos.color = Color.magenta;
            Quaternion standRot = Quaternion.Euler(standPointEuler);
            Vector3 standDir = standRot * Vector3.forward * 0.28f;
            Gizmos.DrawRay(standWorld, standDir);
        }
#endif
    }
}