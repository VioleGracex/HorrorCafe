using UnityEngine;
using System;

namespace LostFromLight.Core.Variables
{
    [CreateAssetMenu(menuName = "LostFromLight/Variables/BoolVariable")]
    public class BoolVariable : ScriptableObject
    {
        [SerializeField] private bool _value;
        public bool Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<bool> OnValueChanged;

        public void SetValue(bool value) => Value = value;
        public void Toggle() => Value = !_value;
    }
}