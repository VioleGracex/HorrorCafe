using UnityEngine;
using System;

namespace LostFromLight.Core.Variables
{
    [CreateAssetMenu(menuName = "LostFromLight/Variables/Vector3Variable")]
    public class Vector3Variable : ScriptableObject
    {
        [SerializeField] private Vector3 _value;
        public Vector3 Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<Vector3> OnValueChanged;

        public void SetValue(Vector3 value) => Value = value;
    }
}