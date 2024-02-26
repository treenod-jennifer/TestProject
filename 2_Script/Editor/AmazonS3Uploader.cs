using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.Security.Cryptography;

[EditorHeight( 4 )]
public class AmazonS3Uploader : Uploader
{
    public enum Region
    {
        us_east_1,
        us_west_1,
        us_west_2,
        eu_west_1,
        ap_southeast_1,
        ap_southeast_2,
        ap_northeast_1,
        ap_northeast_2, //  KTW ADD
        sa_east_1
    }

    private string accessKey = "";
    private string secretKey = "";

    private Region region = Region.us_east_1;
    private int bucketIndex = 0;
    private List<string> buckets = new List<string>();

    public override void Upload( byte[] itemBytes, string itemKey, System.Action<bool> callback )
    {
        string url = GetUrl( itemKey );

        RemotePackageManagerHTTP.Request request = new RemotePackageManagerHTTP.Request( "PUT", url, itemBytes );
        request.synchronous = true;

        string dateString = System.DateTime.UtcNow.ToString( "ddd, dd MMM yyyy HH:mm:ss " ) + "GMT";
        string canonicalString = string.Format( "PUT\n\n\n\nx-amz-acl:public-read\nx-amz-date:{0}\n/{1}/{2}", dateString, GetBucket(), itemKey );

        request.AddHeader( "x-amz-acl", "public-read" );
        request.AddHeader( "x-amz-date", dateString );
        request.AddHeader( "Authorization", string.Format( "AWS {0}:{1}", accessKey, GetEncodedSignature( canonicalString ) ) );

        request.Send( r => {
            if( r.exception == null )
            {
                if( string.IsNullOrEmpty( r.response.Text ) )
                {
                    EndUpload( itemKey, true, "", callback );
                }
                else
                {
                    EndUpload( itemKey, false, r.response.Text, callback );
                }
            }
            else
            {
                EndUpload( itemKey, false, r.exception.Message, callback );
            }
        } );
    }

    private void RetrieveBuckets()
    {
        string url = GetBaseUrl();

        RemotePackageManagerHTTP.Request request = new RemotePackageManagerHTTP.Request( "GET", url );
        request.synchronous = true;

        string dateString = System.DateTime.UtcNow.ToString( "ddd, dd MMM yyyy HH:mm:ss " ) + "GMT";
        string canonicalString = string.Format( "GET\n\n\n\nx-amz-date:{0}\n/", dateString );

        request.AddHeader( "x-amz-date", dateString );
        request.AddHeader( "Authorization", string.Format( "AWS {0}:{1}", accessKey, GetEncodedSignature( canonicalString ) ) );

        request.Send( r => {
            if( r.exception == null )
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml( r.response.Text );

                buckets.Clear();

                foreach( XmlNode bucketName in doc.GetElementsByTagName( "Name" ) )
                {
                    if( bucketName.ParentNode.LocalName == "Bucket" )
                    {
                        buckets.Add( bucketName.InnerText );
                    }
                }
            }
            else
            {
                Debug.LogError( string.Format( "Failed to retrieve s3 buckets list: {0}", r.exception.Message ) );
            }
        } );
    }

    public override bool HasSettingsNoRetrieve()
    {
        return !string.IsNullOrEmpty( accessKey ) && !string.IsNullOrEmpty( secretKey );
    }

    public override void OnShowSettingsInspector( Rect rect )
    {
        accessKey = EditorGUI.TextField( LineRect( rect, 0 ), "Access Key", accessKey );
        secretKey = EditorGUI.TextField( LineRect( rect, 1 ), "Secret Key", secretKey );
        region = ( Region ) EditorGUI.EnumPopup( LineRect( rect, 2 ), "Region", region );

        Rect r = LineRect( rect, 3 );
        float rightWidth = 64.0f;
        Rect leftRect = new Rect( r.x, r.y, r.width - rightWidth, r.height );
        Rect rightRect = new Rect( r.x + leftRect.width, r.y, rightWidth, r.height );

        if( buckets.Count > 0 )
        {
            bucketIndex = Mathf.Clamp( bucketIndex, 0, buckets.Count );
            bucketIndex = EditorGUI.Popup( leftRect, "Bucket", bucketIndex, buckets.ToArray() );
        }
        else
        {
#if !UNITY_4
            EditorGUI.LabelField( leftRect, GUIContent.none, EditorStyles.helpBox );
            EditorGUI.LabelField( leftRect, "Could not find any S3 Bucket!", EditorStyles.miniLabel );
#else
            EditorGUI.HelpBox( leftRect, "Could not find any S3 Bucket!", MessageType.None );
#endif
        }

        EditorGUI.BeginDisabledGroup( !HasSettingsNoRetrieve() );
        if( GUI.Button( rightRect, "Refresh" ) )
        {
            Uploader.RequireWeb( () => RetrieveBuckets() );
        }
        EditorGUI.EndDisabledGroup();
    }

    private string GetEncodedSignature( string canonicalString )
    {
        UTF8Encoding encoding = new UTF8Encoding();
        HMACSHA1 hash = new HMACSHA1();
        hash.Key = encoding.GetBytes( secretKey );
        byte[] canonicalBytes = encoding.GetBytes( canonicalString );
        byte[] signature = hash.ComputeHash( canonicalBytes );

        return System.Convert.ToBase64String( signature );
    }

    private string GetBucket()
    {
        if( buckets.Count > 0 )
        {
            bucketIndex = Mathf.Clamp( bucketIndex, 0, buckets.Count );
            return buckets[bucketIndex];
        }

        return "";
    }

    private string GetBaseUrl()
    {
        string regionString = "s3";

        if( region != Region.us_east_1 )
        {
            regionString = string.Format( "{0}-{1}", regionString, region.ToString().Replace( '_', '-' ) );
        }

        return string.Format( "http://{0}.amazonaws.com", regionString );
    }

    public override string GetUrl( string itemKey )
    {
        // KTW CHG
        return string.Format( "{0}/{1}/{2}", GetBaseUrl(), GetBucket(), itemKey );
//        return string.Format("{0}/{1}/{2}/{3}", GetBaseUrl(), GetBucket(), "puzzle", itemKey);
    }

    public class Serialized
    {
        public string accessKey;
        public string secretKey;
        public int region;
        public int bucketIndex;
        public string[] buckets;

        public string Serialize()
        {
            return SerializationHelper.Serialize().Write( accessKey, secretKey, region, bucketIndex ).WriteArray( buckets ).ToString();
        }

        public static Serialized Deserialize( string text )
        {
            Serialized self = new Serialized();
            SerializationHelper.Deserialize( text )
                .ReadString( out self.accessKey )
                .ReadString( out self.secretKey )
                .ReadInt( out self.region )
                .ReadInt( out self.bucketIndex )
                .ReadStringArray( out self.buckets );
            return self;
        }
    }

    public override void RetrieveSettings( string text )
    {
        Serialized serialized = Serialized.Deserialize( text );

        accessKey = serialized.accessKey;
        secretKey = serialized.secretKey;
        region = ( Region ) serialized.region;
        bucketIndex = serialized.bucketIndex;
        buckets = serialized.buckets.ToList();
    }

    public override string SaveSettings()
    {
        Serialized serialized = new Serialized();

        serialized.accessKey = accessKey;
        serialized.secretKey = secretKey;
        serialized.region = ( int ) region;
        serialized.bucketIndex = bucketIndex;
        serialized.buckets = buckets.ToArray();

        return serialized.Serialize();
    }
}
