using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScore : MonoBehaviour {

    public UILabel _textScore;
    public float delay = 0;

    Transform _transform;
    float timer = 0f;

    public bool IsCombo = false;
    public bool MoveCombo = false;
    int comboCount = 0;
    public GameObject particleObj;

    public void initScore(Vector3 startPos, string score, float tempDelay = 0, bool isCombo = false)
    {
        _transform.position = startPos;
        _textScore.text = score;
        delay = tempDelay;
        _transform.localPosition += Vector3.up * 37;
        IsCombo = isCombo;

        

        if (delay > 0f) _textScore.alpha = 0;

        if (isCombo)
        {
            delay = ManagerBlock.instance.comboCount * 0.05f;
            _textScore.depth = (int)GimmickDepth.FX_EFFECT + ManagerBlock.instance.comboCount;
        }

        comboCount = ManagerBlock.instance.comboCount;
    }

    void Awake()
    {
        _transform = transform;
    }

    float waitTimer = 0f;

    void Update()
    {
        if (IsCombo)
        {
            if (MoveCombo)
            {
                if (delay > 0 && delay > waitTimer)
                {
                    waitTimer += Global.deltaTimePuzzle;                    
                }
                else
                {
                    if (particleObj.activeSelf == false)
                        particleObj.SetActive(true);

                    _transform.position = Vector3.MoveTowards(_transform.position, GameUIManager.instance.adventureComboLabel.gameObject.transform.position, 0.05f);

                    if (_transform.position == GameUIManager.instance.adventureComboLabel.gameObject.transform.position)
                    {
                        GameUIManager.instance.adventureComboLabel.gameObject.SetActive(true);
                        GameUIManager.instance.adventureComboLabel.text = "Combo " + comboCount;
                        AdventureManager.instance.ComboObjList.Remove(this);
                        AdventureManager.instance.AddComboPoint();
                        
                        Destroy(gameObject);
                    }
                    return;
                }
            }
            //컬러조절
            _textScore.color = Color.Lerp(Color.red, Color.yellow, 0.5f + 0.5f * (Mathf.Sin(Mathf.PI * timer * 6f) + 1) * 0.5f);

            _transform.localScale = Vector3.one * (1.1f + 0.075f * comboCount);// (1.2f + Mathf.Sin(Mathf.PI * timer * 6f) * 0.1f);
            _transform.localPosition += Vector3.up * Global.deltaTimePuzzle * 40f;

            timer += Global.deltaTimePuzzle * 1.2f;
            return; 
        }

        if(delay > 0 && delay > waitTimer)
        {
            waitTimer += Global.deltaTimePuzzle;
            return;
        }
        _textScore.alpha = 1;


        _transform.localPosition += Vector3.up * Global.deltaTimePuzzle * 100f;
        if (timer < 0.3f)
            _transform.localScale = Vector3.one * (1f + Mathf.Sin(Mathf.PI * (timer + 0.1f) / 0.4f) * 0.6f);
        else
            _transform.localScale = Vector3.one;

        if (timer >= 0.7f)
            _textScore.color = new Color(1f, 1f, 1f, 1f - (timer - 0.7f) / 0.3f);

        timer += Global.deltaTimePuzzle * 1.2f;
        
        if (timer >= 1f && IsCombo == false)
            Destroy(gameObject);
    }
}
