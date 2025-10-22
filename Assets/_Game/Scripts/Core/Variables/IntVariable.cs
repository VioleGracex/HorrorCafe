using UnityEngine;
using System;

namespace LostFromLight.Core.Variables
{
    [CreateAssetMenu(menuName = "LostFromLight/Variables/IntVariable")]
    public class IntVariable : ScriptableObject
    {
        [SerializeField] private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<int> OnValueChanged;

        public void SetValue(int value) => Value = value;
        public void AddValue(int value) => Value += value;
    }
}