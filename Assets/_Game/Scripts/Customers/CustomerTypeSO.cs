using UnityEngine;

namespace Ouiki.Restaurant
{
    [CreateAssetMenu(menuName = "Restaurant/CustomerType", fileName = "CustomerTypeSO")]
    public class CustomerTypeSO : ScriptableObject
    {
        public string customerTypeName;
        public float patienceTime = 15f;
        public float chaseTime = 6f;
        public float attackDistance = 1.5f;
        public float chaseSpeed = 3.5f;
        public float walkSpeed = 2f;
        public float fleeSpeed = 4.5f;
    }
}