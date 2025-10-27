using UnityEngine;
using System.Collections.Generic;

namespace Ouiki.Restaurant
{
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Customer Stats")]
        public List<CustomerTypeSO> customerTypes;

        [Header("Customer Prefabs")]
        public List<BaseCustomer> humanCustomerPrefabs;
        public List<GhoulCustomer> ghoulCustomerPrefabs;

        [Header("Seats and Spawning")]
        public CustomerSeat[] seats;
        public float spawnInterval = 8f;
        [Tooltip("Delay before spawning the very first customer (seconds).")]
        public float firstCustomerDelay = 0f;
        public int maxActiveCustomers = 3;
        public Vector3 spawnAreaMin = new Vector3(-10, 0, -10);
        public Vector3 spawnAreaMax = new Vector3(-5, 0, 5);
        public Vector3 spawnAreaOffset = Vector3.zero;

        private float timer = 0f;
        private bool firstCustomerSpawned = false;
        private readonly List<BaseCustomer> activeCustomers = new List<BaseCustomer>();
        private int spawnPatternIndex = 0;
        private readonly List<string> spawnPattern = new List<string> { "Human", "Human", "Ghoul" };
        private bool ghoulActive = false;

        public IReadOnlyList<BaseCustomer> ActiveCustomers => activeCustomers;

        void Start()
        {
            timer = 0f;
            firstCustomerSpawned = false;
        }

        void Update()
        {
            timer += Time.deltaTime;

            if (!firstCustomerSpawned)
            {
                if (timer >= firstCustomerDelay)
                {
                    TrySpawnCustomer();
                    timer = 0f;
                    firstCustomerSpawned = true;
                }
            }
            else
            {
                if (timer >= spawnInterval)
                {
                    TrySpawnCustomer();
                    timer = 0f;
                }
            }
        }

        void TrySpawnCustomer()
        {
            Debug.Log("[CustomerSpawner] TrySpawnCustomer called.");

            if (activeCustomers.Count >= maxActiveCustomers)
            {
                Debug.LogWarning("[CustomerSpawner] Max active customers reached.");
                return;
            }
            if (ghoulActive)
            {
                Debug.LogWarning("[CustomerSpawner] Ghoul already active, skipping spawn.");
                return;
            }

            CustomerSeat seat = GetRandomAvailableSeat();
            if (seat == null)
            {
                Debug.LogWarning("[CustomerSpawner] No available seat to assign.");
                return;
            }

            string type = spawnPattern[spawnPatternIndex % spawnPattern.Count];
            spawnPatternIndex++;

            if (customerTypes == null || customerTypes.Count == 0)
            {
                Debug.LogWarning("[CustomerSpawner] customerTypes list is empty!");
                return;
            }
            CustomerTypeSO typeSO = customerTypes[Random.Range(0, customerTypes.Count)];

            Vector3 spawnPos = GetRandomSpawnPosition();
            BaseCustomer customer = null;

            if (type == "Human")
            {
                if (humanCustomerPrefabs == null || humanCustomerPrefabs.Count == 0)
                {
                    Debug.LogWarning("[CustomerSpawner] No humanCustomerPrefabs assigned.");
                    return;
                }
                var prefab = humanCustomerPrefabs[Random.Range(0, humanCustomerPrefabs.Count)];
                customer = Instantiate(prefab, spawnPos, Quaternion.identity);
            }
            else if (type == "Ghoul")
            {
                if (ghoulCustomerPrefabs == null || ghoulCustomerPrefabs.Count == 0)
                {
                    Debug.LogWarning("[CustomerSpawner] No ghoulCustomerPrefabs assigned.");
                    return;
                }
                var prefab = ghoulCustomerPrefabs[Random.Range(0, ghoulCustomerPrefabs.Count)];
                customer = Instantiate(prefab, spawnPos, Quaternion.identity);
            }

            if (customer == null)
            {
                Debug.LogWarning("[CustomerSpawner] Failed to instantiate customer prefab.");
                return;
            }

            customer.Initialize(typeSO, this);

            customer.exitPoint = spawnPos;

            if (type == "Ghoul")
            {
                ghoulActive = true;
            }

            seat.Reserve();
            customer.AssignSeat(seat);
            customer.OnLeave += OnCustomerLeft;
            activeCustomers.Add(customer);

            Debug.Log($"[CustomerSpawner] Spawned {type} customer at {spawnPos}");
        }

        Vector3 GetRandomSpawnPosition()
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
            Vector3 localRandom = new Vector3(x, y, z) + spawnAreaOffset;
            return transform.TransformPoint(localRandom);
        }

        CustomerSeat GetRandomAvailableSeat()
        {
            List<CustomerSeat> available = new List<CustomerSeat>();
            foreach (var seat in seats)
                if (!seat.IsOccupied) available.Add(seat);

            if (available.Count == 0) return null;
            return available[Random.Range(0, available.Count)];
        }

        void OnCustomerLeft(BaseCustomer customer)
        {
            if (customer.assignedSeat != null)
                customer.assignedSeat.Release();

            if (customer is GhoulCustomer)
                ghoulActive = false;

            activeCustomers.Remove(customer);
            Destroy(customer.gameObject);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = (spawnAreaMin + spawnAreaMax) * 0.5f + spawnAreaOffset;
            Vector3 size = spawnAreaMax - spawnAreaMin;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = oldMatrix;
        }
#endif
    }
}