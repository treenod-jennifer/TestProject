using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILabelPlus : UILabel
{
    private static class PKCodeDatabase
    {
        public struct PKCode
        {
            public const char START_CHAR = '[';
            public const char END_CHAR = ']';
            public const char SEPARATOR_CHAR = ':';

            public readonly string key;
            public readonly string value;
            public string FullCode
            {
                get
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return $"{START_CHAR}{key}{END_CHAR}";
                    }
                    else
                    {
                        return $"{START_CHAR}{key}{SEPARATOR_CHAR}{value}{END_CHAR}";
                    }
                }
            }

            public PKCode(string text)
            {
                text = text.Remove(0, 1);
                text = text.Remove(text.Length - 1, 1);

                string[] test = text.Split(SEPARATOR_CHAR);

                if(test.Length == 1)
                {
                    key = test[0];
                    value = string.Empty;
                }
                else if(test.Length == 2 && !string.IsNullOrEmpty(test[1]))
                {
                    key = test[0];
                    value = test[1];
                }
                else
                {
                    key = string.Empty;
                    value = string.Empty;
                }
            }
        }

        public delegate void PKCodeFunc(UILabel target, string value);

        /// <summary>
        /// [Size:36] 사이즈를 36으로 설정 합니다. 정수값만 지원 합니다.
        /// [Color:255,255,0] 컬러를 설정 합니다. 정수값만 지원 합니다.
        /// [Alignment:C] 가운데 정렬을 합니다. (L : 왼쪽 정렬, C : 가운데 정렬, R : 오른쪽 정렬)
        /// [OutLine:2/255,255,0] 외곽선을 설정 합니다. 
        /// 2 : 외곽선 굵기. 실수값을 지원합니다. 
        /// 255,255,0 : 외곽선 컬러값. 정수값만 지원합니다.
        /// [SpacingX:3] 글자사이 가로 간격을 조절합니다. 기본값은 0. 정수값만 지원합니다.
        /// [SpacingY:3] 글자사이 세로 간격을 조절합니다. 기본값은 0. 정수값만 지원합니다.
        /// </summary>
        private static readonly Dictionary<string, PKCodeFunc> database = new Dictionary<string, PKCodeFunc>()
        {
            { "Test", TestProcess },
            { "Size", SizeProcess },
            { "Color", ColorProcess },
            { "Alignment", AlignmentProcess },
            { "OutLine", OutLineProcess },
            { "SpacingX", SpacingXProcess },
            { "SpacingY", SpacingYProcess },
        };

        public static PKCode[] Eviction(string target)
        {
            List<PKCode> codes = new List<PKCode>();

            int startIndex = -1;

            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] == PKCode.START_CHAR)
                {
                    startIndex = i;
                }

                if (target[i] == PKCode.END_CHAR)
                {
                    if(startIndex != -1)
                    {
                        PKCode pkCode = new PKCode(target.Substring(startIndex, i - startIndex + 1));

                        if (database.ContainsKey(pkCode.key))
                        {
                            codes.Add(pkCode);
                        }
                        
                        startIndex = -1;
                    }
                }
            }

            return codes.ToArray();
        }
        
        public static void Apply(UILabel target, PKCode pKCode)
        {
            if (database.TryGetValue(pKCode.key, out PKCodeFunc pKCodeFunc))
            {
                target.text = target.text.Replace(pKCode.FullCode, string.Empty);
                pKCodeFunc?.Invoke(target, pKCode.value);
            }
        }

        #region PKCode Process Function

        private static void TestProcess(UILabel target, string value)
        {
            target.color = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        }

        private static void SizeProcess(UILabel target, string value)
        {
            if (int.TryParse(value, out int result))
            {
                target.fontSize = result;
            }
        }

        private static void ColorProcess(UILabel target, string value)
        {
            Color? color = StringToColor(value);

            if (color.HasValue)
            {
                target.color = color.Value;
            }
        }

        private static void AlignmentProcess(UILabel target, string value)
        {
            switch (value)
            {
                case "C":
                    target.alignment = NGUIText.Alignment.Center;
                    break;
                case "L":
                    target.alignment = NGUIText.Alignment.Left;
                    break;
                case "R":
                    target.alignment = NGUIText.Alignment.Right;
                    break;
                default:
                    break;
            }
        }

        private static void OutLineProcess(UILabel target, string value)
        {
            target.effectStyle = Effect.Outline8;
            string[] sizeAndColor = value.Split('/');
            if(!float.TryParse(sizeAndColor[0], out float effectSize))
            {
                effectSize = 1.0f;
            }

            target.effectDistance = Vector2.one * effectSize;

            Color? color = StringToColor(sizeAndColor[1]);

            if (color.HasValue)
            {
                target.effectColor = color.Value;
            }
        }

        private static void SpacingXProcess(UILabel target, string value)
        {
            if (int.TryParse(value, out int result))
            {
                target.spacingX = result;
            }
        }

        private static void SpacingYProcess(UILabel target, string value)
        {
            if (int.TryParse(value, out int result))
            {
                target.spacingY = result;
            }
        }

        #endregion

        private static Color? StringToColor(string value)
        {
            string[] rgbaText = value.Split(',');
            int[] rbga = new int[rgbaText.Length];

            for (int i = 0; i < rgbaText.Length; i++)
            {
                int.TryParse(rgbaText[i], out rbga[i]);
            }

            Color? textColor;

            if (rgbaText.Length == 4)
            {
                textColor = new Color(rbga[0] / 255.0f, rbga[1] / 255.0f, rbga[2] / 255.0f, rbga[3] / 255.0f);
            }
            else if(rgbaText.Length == 3)
            {
                textColor = new Color(rbga[0] / 255.0f, rbga[1] / 255.0f, rbga[2] / 255.0f);
            }
            else
            {
                textColor = null;
            }

            return textColor;
        }
    }

    public override void MarkAsChanged()
    {
        base.MarkAsChanged();

        //Debug.Log("MarkAsChanged");

        ApplyPKCode();
    }

    //protected override void OnEnable()
    //{
    //    base.OnEnable();

    //    MarkAsChanged();
    //}

    private void ApplyPKCode()
    {
        var pkCodeData = PKCodeDatabase.Eviction(text);

        foreach (var pkCode in pkCodeData)
        {
            PKCodeDatabase.Apply(this, pkCode);
        }
    }
}
