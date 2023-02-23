using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReactiveVariable<T> {
    public ReactiveVariable(T value) {
        this.value = value;
    }

    private T _value;
    public T value { 
        get {
            return _value;
        }
        set {
            if (!value.Equals(_value)) {
                _value = value;
                if (onValueChanged != null) {
                    onValueChanged(_value);
                    Debug.Log("Value changed to " + _value);
                }
            }
        } 
    }

    public delegate void OnValueChanged(T newValue);
    public event OnValueChanged onValueChanged =  (T newValue) => { };

    public void SetValue(T newValue) {
        value = newValue;
    }
}
