using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyEntryFocus : MonoBehaviour {

    [Header("포커스 우선순위(높을수록 우선), (에리어의 경우 자동으로 잡힌경우 Area번호 * 1000 + 미션씬) ")]
    public int focusPriority = 000000;

    [Header("카메라 포커스")]
    public FocusData focusData;

    [Header("말풍선 띄우는 조건")]
    public BubbleDisplayOption bubbleDisplayAt = BubbleDisplayOption.TOP_PRIORITY_ONCE;

    [Header("알림판 말풍선")]
    public Transform bubbleTransform;
    public float bubbleHeightOffset = 0f;
    public string bubbleTextKey = "";
    public float bubbleDulation = 1.5f;

    public enum BubbleDisplayOption
    {
        TOP_PRIORITY_EVERY,
        TOP_PRIORITY_ONCE,
        EVERY_LOBBY_IN,
        LOGIN_ONCE
    }

    private void Awake()
    {
        focusCandidates.Add(this);
    }

    private void OnDestroy()
    {
        
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public int GetFocusPriority()
    {
        return this.focusPriority;
    }

    ////////////////////////////////

    class AreaComp : IComparer<LobbyEntryFocus>
    {
        public int Compare(LobbyEntryFocus a, LobbyEntryFocus b)
        {
            long aTs = a.GetFocusPriority();
            long bTs = b.GetFocusPriority();

            int ret = 0;
            if (aTs < bTs)
                ret = 1;
            else if (aTs > bTs)
                ret = -1;
            
            return ret;
        }
    }

    static List<LobbyEntryFocus> focusCandidates = new List<LobbyEntryFocus>();

    public static void DoFocus(bool firstLobby)
    {
        List<LobbyEntryFocus> validCandidates = focusCandidates.FindAll(x => 
        {
            if (x.enabled == false)
                return false;

            return true;
        } );

        if (validCandidates.Count == 0)
        {
            focusCandidates.Clear();
            return;
        }

        validCandidates.Sort(new AreaComp());
        {
            var focusObj = validCandidates[0];

            if ( focusObj.enabled )
            {
                if (string.IsNullOrEmpty(focusObj.bubbleTextKey) == false )
                {
                    bool displayBubble = false;
                    switch (focusObj.bubbleDisplayAt)
                    {
                        case BubbleDisplayOption.EVERY_LOBBY_IN:
                        case BubbleDisplayOption.TOP_PRIORITY_EVERY:
                            {
                                displayBubble = true;
                            }
                            break;

                        case BubbleDisplayOption.TOP_PRIORITY_ONCE:
                        case BubbleDisplayOption.LOGIN_ONCE:
                            {
                                displayBubble = firstLobby;
                            }
                            break;
                    }

                    if (displayBubble)
                    {
                        var bubbleText = Global._instance.GetString(focusObj.bubbleTextKey);
                        var lobbyChat = UILobbyChat.MakeLobbyChat(focusObj.bubbleTransform, bubbleText, focusObj.bubbleDulation);
                        lobbyChat.heightOffset = focusObj.bubbleHeightOffset;
                    }
                }


                if (focusObj.focusData.camTransform != null)
                {
                    var cameraPosition = new Vector3(focusObj.focusData.camTransform.position.x + focusObj.focusData.camOffset.x, 0f, focusObj.focusData.camTransform.position.z + focusObj.focusData.camOffset.y);
                    CameraController._instance.SetCameraPosition(cameraPosition);
                }

                if (focusObj.focusData.zoom > 0f)
                    CameraController._instance.SetFieldOfView(focusObj.focusData.zoom);
            }
        }


        if (validCandidates.Count > 1)
        {
            for (int i = 1; i < validCandidates.Count; ++i)
            {
                var focusObj = validCandidates[i];

                if (focusObj.enabled)
                {
                    if (string.IsNullOrEmpty(focusObj.bubbleTextKey) == false &&
                        ((focusObj.bubbleDisplayAt == BubbleDisplayOption.EVERY_LOBBY_IN) || (firstLobby && focusObj.bubbleDisplayAt == BubbleDisplayOption.LOGIN_ONCE)))
                    {
                        var bubbleText = Global._instance.GetString(focusObj.bubbleTextKey);
                        var lobbyChat = UILobbyChat.MakeLobbyChat(focusObj.bubbleTransform, bubbleText, focusObj.bubbleDulation);
                        lobbyChat.heightOffset = focusObj.bubbleHeightOffset;
                    }
                }
            }
        }

        focusCandidates.Clear();
    }

    internal static void ResetFocusCandidates()
    {
        focusCandidates.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (focusData.charPos != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(focusData.charPos.position + new Vector3(focusData.charOffset.x, focusData.charOffset.y), new Vector3(0.8f, 1.4f, 0.8f));
        }

        if (focusData.camTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(focusData.camTransform.position + new Vector3(focusData.camOffset.x, focusData.camOffset.y), new Vector3(0.8f, 1.4f, 0.8f));
        }

        if (string.IsNullOrEmpty(this.bubbleTextKey) == false)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(bubbleTransform.position + Vector3.up * ( bubbleHeightOffset + 5f), 0.5f);
        }
    }
}

[System.Serializable]
public class FocusData
{
    public Transform camTransform;
    public Vector2 camOffset;
    [Header("CharPos/Offset은 Area용으로 존재하며, 이벤트용으로는 현재 동작안함")]
    public Transform charPos = null;
    public Vector2 charOffset;
    public float zoom = 0f;
}