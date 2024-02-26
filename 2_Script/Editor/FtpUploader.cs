using UnityEngine;
using UnityEditor;

using System.Text;
using System.Net;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

[EditorHeight( 4 )]
public class FtpUploader : Uploader
{
    private string url = "";
    private string username = "";
    private string password = "";
    private bool passiveMode = true;

    public override void Upload( byte[] itemBytes, string itemKey, System.Action<bool> callback )
    {
        bool success = true;
        string url = GetUrl( itemKey );

        string status;
        if( !TryUpload( itemBytes, url, out status ) )
        {
            success = CreateFoldersToItem( itemKey, out status ) && TryUpload( itemBytes, url, out status );
        }

        EndUpload( itemKey, success, status, callback );
    }

    private bool TryUpload( byte[] itemBytes, string uri, out string status )
    {
        try
        {
            FtpWebRequest request = CreateFtpWebRequest( uri, WebRequestMethods.Ftp.UploadFile );

            Stream stream = request.GetRequestStream();
            stream.Write( itemBytes, 0, itemBytes.Length );
            stream.Close();

            FtpWebResponse response = request.GetResponse() as FtpWebResponse;
            status = response.StatusDescription;
            return true;
        }
        catch( System.Net.WebException e )
        {
            FtpWebResponse response = e.Response as FtpWebResponse;

            if( response == null )
            {
                status = e.Message;
            }
            else
            {
                status = response.StatusDescription;
            }

            return false;
        }
        catch( System.Exception )
        {
            status = "Could not upload file to FTP server. Please check your FTP URL.";
            return false;
        }
    }

    private bool CreateFoldersToItem( string itemKey, out string status )
    {
        FtpWebRequest request;

        System.Uri uri = new System.Uri( GetUrl( itemKey ) );
        string[] segments = uri.Segments;

        string currentFolder = uri.GetLeftPart( System.UriPartial.Authority );

        for( int i = 0; i < segments.Length - 1; i++ )
        {
            string segment = segments[i];

            try
            {
                currentFolder = string.Concat( currentFolder, segment );

                string requestUri = currentFolder;
                if( currentFolder.Length > 0 && currentFolder[currentFolder.Length - 1] == '/' )
                {
                    requestUri = currentFolder.Substring( 0, currentFolder.Length - 1 );
                }

                request = CreateFtpWebRequest( requestUri, WebRequestMethods.Ftp.MakeDirectory );
                WebResponse response = request.GetResponse();
                response.GetResponseStream().Close();
                response.Close();
            }
            catch( System.Net.WebException e )
            {
                FtpWebResponse response = e.Response as FtpWebResponse;

                if( response == null || response.StatusCode == FtpStatusCode.NotLoggedIn )
                {
                    status = response.StatusDescription;
                    return false;
                }
                else if( response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable )
                {
                    // Folder already exists...
                    continue;
                }
            }
            catch( System.Exception )
            {
                status = "Could not create folder tree in FTP server. Please check your FTP URL.";
                return false;
            }
        }

        status = "OK";
        return true;
    }

    private FtpWebRequest CreateFtpWebRequest( string uri, string method )
    {
        FtpWebRequest request = FtpWebRequest.Create( uri ) as FtpWebRequest;
        request.Credentials = new NetworkCredential( username, password );
        request.Method = method;
        request.UsePassive = passiveMode;
        request.UseBinary = true;
        request.KeepAlive = false;

        return request;
    }

    public override bool HasSettingsNoRetrieve()
    {
        return !string.IsNullOrEmpty( username ) && !string.IsNullOrEmpty( password );
    }

    public override string GetUrl( string itemKey )
    {
        if( url.EndsWith( "/" ) )
        {
            return string.Format( "{0}{1}", url, itemKey );
        }

        return string.Format( "{0}/{1}", url, itemKey );
    }

    public override void OnShowSettingsInspector( Rect rect )
    {
        url = EditorGUI.TextField( LineRect( rect, 0 ), "Url", url );
        username = EditorGUI.TextField( LineRect( rect, 1 ), "Username", username );
        password = EditorGUI.PasswordField( LineRect( rect, 2 ), "Password", password );
        passiveMode = EditorGUI.Toggle( LineRect( rect, 3 ), "Passive Mode", passiveMode );
    }

    public class Serialized
    {
        public string url;
        public string username;
        public string password;
        public bool passiveMode;

        public string Serialize()
        {
            return SerializationHelper.Serialize().Write( url, username, password, passiveMode ).ToString();
        }

        public static Serialized Deserialize( string text )
        {
            Serialized self = new Serialized();
            SerializationHelper.Deserialize( text )
                .ReadString( out self.url )
                .ReadString( out self.username )
                .ReadString( out self.password )
                .ReadBool( out self.passiveMode );
            return self;
        }
    }

    public override void RetrieveSettings( string text )
    {
        Serialized serialized = Serialized.Deserialize( text );

        url = serialized.url;
        username = serialized.username;
        password = serialized.password;
        passiveMode = serialized.passiveMode;
    }

    public override string SaveSettings()
    {
        Serialized serialized = new Serialized { url = this.url, username = this.username, password = this.password, passiveMode = this.passiveMode };
        return serialized.Serialize();
    }

    public override string ToString()
    {
        return url;
    }
}
