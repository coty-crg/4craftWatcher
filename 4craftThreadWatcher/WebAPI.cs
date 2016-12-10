using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;
using System;

// our own class, so we can have a super tiny timeout (1 second) 
public class ImpatientWebClient : WebClient
{
    protected override WebRequest GetWebRequest(Uri address)
    {
        var req = base.GetWebRequest(address);
        req.Timeout = 1000 * 5;
        return req;
    }
}

public static class WebStuff
{

    // stolen from stackoverflow: http://stackoverflow.com/questions/4926676/mono-webrequest-fails-with-https
    private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        //Return true if the server certificate is ok
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        bool acceptCertificate = true;
        string msg = "The server could not be validated for the following reason(s):\r\n";

        //The server did not present a certificate
        if ((sslPolicyErrors &
             SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable)
        {
            msg = msg + "\r\n    -The server did not present a certificate.\r\n";
            acceptCertificate = false;
        } else
        {
            //The certificate does not match the server name
            if ((sslPolicyErrors &
                 SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                msg = msg + "\r\n    -The certificate name does not match the authenticated name.\r\n";
                acceptCertificate = false;
            }

            //There is some other problem with the certificate
            if ((sslPolicyErrors &
                 SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                foreach (X509ChainStatus item in chain.ChainStatus)
                {
                    if (item.Status != X509ChainStatusFlags.RevocationStatusUnknown &&
                        item.Status != X509ChainStatusFlags.OfflineRevocation)
                        break;

                    if (item.Status != X509ChainStatusFlags.NoError)
                    {
                        msg = msg + "\r\n    -" + item.StatusInformation;
                        acceptCertificate = false;
                    }
                }
            }
        }

        //If Validation failed, present message box
        if (acceptCertificate == false)
        {
            msg = msg + "\r\nDo you wish to override the security check?";
            acceptCertificate = true;
        }

        return acceptCertificate;
    }

    public static void SetupWebStuff()
    {
        ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
    }

    public static string FetchDataFromURLBlocking(string url)
    {
        var data = (string)null;
        var wc = new ImpatientWebClient();

        try
        {
            wc.Encoding = Encoding.UTF8;
            data = wc.DownloadString(url);
        } catch (Exception e)
        {

        }

        return data;
    }

    public static HttpStatusCode PostDataFromURlBlocking(string url, System.Collections.Specialized.NameValueCollection data)
    {

        using (var client = new ImpatientWebClient())
        {
            try
            {
                byte[] responsebytes = client.UploadValues(url, "POST", data);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
            } catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;
                if (response != null)
                {
                    return response.StatusCode;
                } else
                {
                    return HttpStatusCode.SeeOther;
                }
            }
        }

        return HttpStatusCode.OK;
    }
}
