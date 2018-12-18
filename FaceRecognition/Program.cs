using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace FaceRecognition
{
    class Program
    {
        const string subscriptionKey = "8ffd363207ad475ca0436d985f9614db";
        const string uriBase = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

        static void Main( string[] args )
        {
            Console.WriteLine( "Detect faces:" );
            Console.Write( "Enter the path to an image with faces that you wish to analyze: " );
            string imageUrl = Console.ReadLine();

            if ( !String.IsNullOrEmpty( imageUrl ) )
            {
                try
                {
                    MakeAnalysisRequest( imageUrl );
                    Console.WriteLine( "\nWait a moment for the results to appear.\n" );
                }
                catch ( Exception e )
                {
                    Console.WriteLine( "\n" + e.Message + "\nPress Enter to exit...\n" );
                }
            }
            else
            {
                Console.WriteLine( "\nInvalid image url.\nPress Enter to exit...\n" );
            }
            Console.ReadLine();

        }

        static async void MakeAnalysisRequest( string imageUrl )
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey );

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray( imageUrl );

            using ( ByteArrayContent content = new ByteArrayContent( byteData ) )
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue( "application/octet-stream" );

                // Execute the REST API call.
                response = await client.PostAsync( uri, content );

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                // Display the JSON response.
                Console.WriteLine( "\nResponse:\n" );
                Console.WriteLine( JsonPrettyPrint( contentString ) );
                Console.WriteLine( "\nPress Enter to exit..." );
            }
        }

        static byte[] GetImageAsByteArray( string imageUrl )
        {
            using ( var webClient = new WebClient() )
            {
                return webClient.DownloadData( imageUrl );
            }
        }

        // Formats the given JSON string by adding line breaks and indents.
        static string JsonPrettyPrint( string json )
        {
            if ( string.IsNullOrEmpty( json ) )
                return string.Empty;

            json = json.Replace( Environment.NewLine, "" ).Replace( "\t", "" );

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach ( char ch in json )
            {
                switch ( ch )
                {
                    case '"':
                        if ( !ignore ) quote = !quote;
                        break;
                    case '\'':
                        if ( quote ) ignore = !ignore;
                        break;
                }

                if ( quote )
                    sb.Append( ch );
                else
                {
                    switch ( ch )
                    {
                        case '{':
                        case '[':
                            sb.Append( ch );
                            sb.Append( Environment.NewLine );
                            sb.Append( new string( ' ', ++offset * indentLength ) );
                            break;
                        case '}':
                        case ']':
                            sb.Append( Environment.NewLine );
                            sb.Append( new string( ' ', --offset * indentLength ) );
                            sb.Append( ch );
                            break;
                        case ',':
                            sb.Append( ch );
                            sb.Append( Environment.NewLine );
                            sb.Append( new string( ' ', offset * indentLength ) );
                            break;
                        case ':':
                            sb.Append( ch );
                            sb.Append( ' ' );
                            break;
                        default:
                            if ( ch != ' ' ) sb.Append( ch );
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}
