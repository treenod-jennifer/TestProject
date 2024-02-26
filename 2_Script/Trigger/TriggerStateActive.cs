using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStateActive : TriggerState
{
    public Transform _defaultCharacterPosition = null;
    public Vector2 _defaultCharacterOffset;
    public Transform _defaultCameraPosition = null;
    public Vector2 _defaultCameraOffset;
    public float _defaultZoom = 0f;

    void Awake()
    {
        _type = TypeTriggerState.Active;
        for (int i = 0; i < _conditions.Count; i++)
            _conditions[i]._stateType = _type;
    }

    void OnDrawGizmosSelected()
    {
        if (_defaultCharacterPosition != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_defaultCharacterPosition.position + new Vector3(_defaultCharacterOffset.x, _defaultCharacterOffset.y), new Vector3(0.8f, 1.4f, 0.8f));
        }

        if (_defaultCameraPosition != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(_defaultCameraPosition.position + new Vector3(_defaultCameraOffset.x, _defaultCameraOffset.y), 0.5f);
        }
    }
}
