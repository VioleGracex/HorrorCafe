using UnityEngine;
using System;

namespace LostFromLight.Core.Variables
{
    [CreateAssetMenu(menuName = "LostFromLight/Variables/FloatVariable")]
    public class FloatVariable : ScriptableObject
    {
        [SerializeField] private float _value;
        public float Value
        {
            get => _value;
            set
            {
                if (Mathf.Approximately(_value, value)) return;
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<float> OnValueChanged;

        public void SetValue(float value) => Value = value;
        public void AddValue(float value) => Value += value;
    }
}