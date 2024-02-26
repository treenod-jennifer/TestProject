using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TriggerStateActive : TriggerState
{
    public Transform _defaultCharacterPosition = null;
    public Vector2 _defaultCharacterOffset;
    public Transform _defaultCameraPosition = null;
    public Vector2 _defaultCameraOffset;
    public float _defaultZoom = 0f;

    [System.NonSerialized]
    public int areaOrderIndex = 0;
    [System.NonSerialized]
    public int sceneIndex = 0;

    void Awake()
    {
        _type = TypeTriggerState.Active;
        for (int i = 0; i < _conditions.Count; i++)
            _conditions[i]._stateType = _type;

        var lobbyFocus = gameObject.GetComponent<LobbyEntryFocus>();
        if (lobbyFocus == null && (_defaultCameraPosition != null || _defaultCharacterPosition != null) )
        {
            lobbyFocus = gameObject.AddMissingComponent<LobbyEntryFocus>();
            lobbyFocus.focusData = new FocusData()
            {
                camTransform = _defaultCameraPosition,
                camOffset = _defaultCameraOffset,
                charPos = _defaultCharacterPosition,
                charOffset = _defaultCharacterOffset,
                zoom = _defaultZoom
            };

            var areaBase = AreaBase.ScanNearestAreaBase(this.transform);
            if( areaBase != null )
            {
                var area = areaBase as Area;

                var p = this.transform.parent;

                if( area != null && p != null && p.name.Contains("Scene"))
                {
                    MatchCollection matches = Regex.Matches(p.name, "[0-9]+");
                    if( matches.Count > 0)
                        sceneIndex = int.Parse(matches[0].Value);

                    areaOrderIndex = area.areaOrder;
                    lobbyFocus.focusPriority = 1000 * areaOrderIndex + sceneIndex;
                    lobbyFocus.bubbleDisplayAt = LobbyEntryFocus.BubbleDisplayOption.TOP_PRIORITY_EVERY;
                }
            }
        }
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
