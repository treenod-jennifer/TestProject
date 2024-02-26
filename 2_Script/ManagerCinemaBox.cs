using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerCinemaBox : MonoBehaviour
{
    public static ManagerCinemaBox _instance = null;
    public UISprite _boxUp;
    public UISprite _boxDown;
    public AnimationCurve _curve;

    readonly float _maxSize = 430f;
    float size = 0f;
    bool _soundPlay = true;

    void Awake()
    {
        if (_instance == null)
            _instance = this;
    }
	// Use this for initialization
	void Start () {
        _boxUp.gameObject.SetActive(false);
        _boxDown.gameObject.SetActive(false);

        _boxUp.color = Color.black;
        _boxDown.color = Color.black;
	}

    public void OnBox(float in_speed = 1f,bool in_sound = true)
    {
        _soundPlay = in_sound;
        StartCoroutine(CoOnBox(in_speed));
    }
    public void OffBox(float in_speed)
    {
        StartCoroutine(CoOffBox(in_speed));
    }

    IEnumerator CoOnBox(float in_speed)
    {
        float timer = 0f;
        _boxUp.gameObject.SetActive(true);
        _boxDown.gameObject.SetActive(true);

        if (_soundPlay)
            ManagerSound.AudioPlay(AudioLobby.LetterBox);
     
        while (true)
        {
            if (!_boxUp.gameObject.active)
            {
                _boxUp.gameObject.SetActive(true);
                _boxDown.gameObject.SetActive(true);
            }

            size = _curve.Evaluate(timer) * _maxSize;

            _boxUp.SetDimensions(1000, (int)size);
            _boxDown.SetDimensions(1000, (int)size);

            timer += Global.deltaTimeLobby * in_speed;
            if (timer > 1f)
            {
                _boxUp.SetDimensions(1000, (int)_maxSize);
                _boxDown.SetDimensions(1000, (int)_maxSize);
                break;
            }
            yield return null;
        }
    }
    IEnumerator CoOffBox(float in_speed)
    {
        float timer = 0f;
        _boxUp.gameObject.SetActive(true);
        _boxDown.gameObject.SetActive(true);

        while (true)
        {

            size = _curve.Evaluate(timer) * _maxSize;

            _boxUp.SetDimensions(1000, (int)(_maxSize - size));
            _boxDown.SetDimensions(1000, (int)(_maxSize - size));

            timer += Global.deltaTimeLobby * in_speed;
            if (timer > 1f)
            {
                _boxUp.SetDimensions(1000, 0);
                _boxDown.SetDimensions(1000, 0);
                break;
            }
            yield return null;
        }
        _boxUp.gameObject.SetActive(false);
        _boxDown.gameObject.SetActive(false);

    }
}
