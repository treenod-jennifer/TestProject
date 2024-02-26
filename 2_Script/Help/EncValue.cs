using UnityEngine;

public class EncValue
{
    private const int MIN_SHIFT = 1;
    private const int MAX_SHIFT = 5;
    private const int MIN_SALT = 0xffff;
    private const int MAX_SALT = int.MaxValue;
    private const int SALT_BITS = 0xffff;

    //private T mInvalidValue;
    int _mValue = 0;
    public int mValue
    {
        get
        {
            if (mShift == 1)
                return (_mValue - _sbalue) ^ 0x119342;
            else if (mShift == 2)
                return (_mValue - _sbalue) ^ 0x19641;
            else if (mShift == 3)
                return (_mValue - _sbalue) ^ 0x419649;
            else
                return (_mValue - _sbalue) ^ 0x21940;
        }
        set
        {
            if (mShift == 1)
                _mValue = _sbalue + (value ^ 0x119342);
            else if (mShift == 2)
                _mValue = _sbalue + (value ^ 0x19641);
            else if (mShift == 3)
                _mValue = _sbalue + (value ^ 0x419649);
            else
                _mValue = _sbalue + (value ^ 0x21940);
        }
    }
	
    private int mHash;
    private int mShift;
    private int mSalt;
    private bool mIsInitialized = false;
    private int _sbalue = 0;

    public EncValue(int in_value =  0)
    {
        InitIfNeeded(in_value);
    }

    public int Value
    {
        get {
            InitIfNeeded();

            return IsValid() ? mValue : -1;
        }

        set
        {
            InitIfNeeded();

            int newValue = IsValid() ? value : -1;
            mValue = newValue;
            mHash = MakeHash(newValue);
        }
    }

    private void InitIfNeeded(int in_value = 0)
    {
        if (mIsInitialized)
            return;

        mIsInitialized = true;
        System.Random rr = new System.Random();
        mShift = rr.Next(MIN_SHIFT, MAX_SHIFT);
        mSalt = rr.Next(MIN_SALT, MAX_SALT) & SALT_BITS;
        _sbalue = rr.Next(900, 9999);
        _mValue = _sbalue;
        mValue = in_value;
        mHash = MakeHash(mValue);
    }

    private int MakeHash(int value)
    {
        return (value.GetHashCode() << mShift) + mSalt;
    }

    private bool IsValid()
    {
        int hash = MakeHash(mValue);
        return hash == mHash;
    }

    public override string ToString()
    {
        return Value.ToString();// +"  " + Value.ToString();
    }
}
