using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyComboCount : MonoBehaviour {

    public UILabel _textScore;

    Transform _transform;
    float timer = 0f;
    Vector3 startPos;
    float StartScale = 1.2f;

    public bool MoveCombo = false;
    //int comboCount = 0;
    public GameObject particleObj;

    public static int ComboCount = 0;



    public void initScore(Vector3 startPos, string score)
    {
        _transform.position = startPos;
        _textScore.text = score;
        _textScore.alpha = 1;
        _textScore.color = Color.red;
        
        foreach (var temp in AdventureManager.instance.ComboObjList)
        {
            if (Vector3.Distance(temp._transform.localPosition, _transform.localPosition) < 120f &&
                Mathf.Abs(temp._transform.localPosition.y - _transform.localPosition.y) < 26f)
            {
                if(temp._transform.localPosition.y >= _transform.localPosition.y)
                    _transform.localPosition -= Vector3.up * 26;
                else
                    _transform.localPosition += Vector3.up * 26;
            }
        } 
        
        _textScore.depth = 25 + ManagerBlock.instance.comboCount;        
        ComboCount++;

        //StartScale = 1.3f + 0.05f * comboCount;
        _transform.localScale = new Vector3(0, 1 * StartScale, 1);// Vector3.zero;// Vector3.one * StartScale;
    }

    void Awake()
    {
        _transform = transform;
    }

    const float delay = 0.5f;
    float waitTimer = 0f;
    float ComboTimer = 0f;

    void Update()
    {
        if (MoveCombo)
            return;

        timer += Global.deltaTimePuzzle * 1.2f;

        _textScore.color = Color.Lerp(Color.red, Color.yellow, 0.5f + 0.5f * (Mathf.Sin(Mathf.PI * timer * 6f) + 1) * 0.5f);


        if (delay > waitTimer)
        {
            float ratio = ManagerBlock.instance._curveBlockPopUp.Evaluate(waitTimer*2);
            //_transform.localScale = Vector3.Lerp(new Vector3(0, 1 * StartScale, 1), Vector3.one * StartScale, waitTimer * 2);
            _transform.localScale = Vector3.one * StartScale * ratio;
            waitTimer += Global.deltaTimePuzzle*2;
            return;
        }        

       
        _transform.localPosition += Vector3.up * Global.deltaTimePuzzle * 10f;

        
        startPos = _transform.localPosition;

        //if (timer > 1f)
        //    StartCombo();
        //일정시간 이후에 점수로 변화고 추가
    }

    public void StartCombo()
    {
        startPos = _transform.localPosition;
        MoveCombo = true;
        StartCoroutine(DoMoveCombo());
    }


    IEnumerator DoMoveCombo()
    {
        //AdventureManager.instance.AddComboPoint();
        _textScore.text = "Bonus +5%";
        _textScore.color = Color.white;

        while (ComboTimer < 1f)
        {
            float posY = ManagerBlock.instance._curveBlockJump.Evaluate(ComboTimer);// AdventureManager.instance.animCurveTextPosY.Evaluate(ComboTimer);
            float scaleRatio = AdventureManager.instance.animCurveTextScale.Evaluate(ComboTimer);

            _transform.localPosition = new Vector3(startPos.x, startPos.y + posY * 20, 0);

            _textScore.color = Color.Lerp(Color.white, new Color(0.5f, 0.5f, 0.5f, 1), (Mathf.Sin(Mathf.PI * ComboTimer * 2  * 5) + 1) * 0.5f);

            /*
            //float scale = StartScale * (1f + ComboTimer);
            //if (scale > 3f)
            //    scale = 3f;
         
            _transform.localScale = Vector3.one * StartScale;
            //_transform.localPosition += Vector3.up * Global.deltaTimePuzzle * 15f;

            //_textScore.color = Color.Lerp(Color.red, Color.yellow, 0.5f + 0.5f * (Mathf.Sin(Mathf.PI * timer * 10f) + 1) * 0.5f);
            //_transform.position = Vector3.Lerp(startPos, GameUIManager.instance.adventureComboLabel.gameObject.transform.position, ComboTimer);

            //_transform.position = Vector3.MoveTowards(_transform.position, GameUIManager.instance.adventureComboLabel.gameObject.transform.position, 0.07f);

            _transform.position = Vector3.Lerp(_transform.position, GameUIManager.instance.adventureComboLabel.gameObject.transform.position, (ComboTimer*0.75f));
            

            //if (ComboTimer > 1f)
            //if (particleObj.activeSelf == false && ComboTimer > 1f)
            //    particleObj.SetActive(true);

            if (ComboTimer > 1f)                            
                _textScore.alpha = 1.5f- (ComboTimer - 0.3f)* 4;
            */

            if (ComboTimer > 0.5f)
                _textScore.alpha = 2 - ComboTimer*2;
            
            ComboTimer += Global.deltaTimePuzzle * 1.6f;
            yield return null;
        }
        /*
        ComboTimer = 0f;
        while (ComboTimer < 1f)
        {
            ComboTimer += Global.deltaTimePuzzle*3f;
            _textScore.alpha = 1 - ComboTimer;
            yield return null;
        }
        */

        //_transform.position = GameUIManager.instance.adventureComboLabel.gameObject.transform.position;


        //GameUIManager.instance.adventureComboLabel.gameObject.SetActive(true);
        //AdventureManager.instance.ComboObjList.Remove(this);
        //AdventureManager.instance.AddComboPoint();

        particleObj.SetActive(false);
        ComboCount--;
        Destroy(gameObject);
        yield return null;
    }
}
