using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class SerializeRequest
{
    public SerializeRequest( char separator )
    {
        this.separator = separator;
    }

    public SerializeRequest Write( params object[] values )
    {
        foreach( object value in values )
        {
            builder.Append( value );
            builder.Append( separator );
        }

        return this;
    }

    public SerializeRequest WriteArray( object[] values )
    {
        builder.Append( values.Length );
        builder.Append( separator );

        foreach( object value in values )
        {
            builder.Append( value );
            builder.Append( separator );
        }

        return this;
    }

    public override string ToString()
    {
        int length = builder.Length;
        if( length > 1 )
        {
            return builder.ToString( 0, length - 1 );
        }

        return "";
    }

    private char separator;
    private StringBuilder builder = new StringBuilder();
}

public class DeserializeRequest
{
    public DeserializeRequest( string text, char separator )
    {
        textValues = text.Split( separator );
        readIndex = 0;
    }

    public DeserializeRequest ReadInt( out int ret )
    {
        int.TryParse( TryGetTextValue(), out ret );
        return this;
    }

    public DeserializeRequest ReadBool( out bool ret )
    {
        bool.TryParse( TryGetTextValue(), out ret );
        return this;
    }

    public DeserializeRequest ReadString( out string ret )
    {
        ret = TryGetTextValue();
        return this;
    }

    public DeserializeRequest ReadStringArray( out string[] ret )
    {
        int arrayLength;
        ReadInt( out arrayLength );

        ret = new string[arrayLength];
        for( int i = 0; i < ret.Length; i++ )
        {
            string element;
            ReadString( out element );

            ret[i] = element;
        }

        return this;
    }

    private string TryGetTextValue()
    {
        if( readIndex < textValues.Length )
        {
            return textValues[readIndex++];
        }

        return "";
    }

    private string[] textValues;
    private int readIndex;
}

public class SerializationHelper
{
    private const char defaultSeparator = ',';

    public static SerializeRequest Serialize( char separator = defaultSeparator )
    {
        SerializeRequest request = new SerializeRequest( separator );
        return request;
    }

    public static DeserializeRequest Deserialize( string text, char separator = defaultSeparator )
    {
        DeserializeRequest request = new DeserializeRequest( text, separator );
        return request;
    }
}
