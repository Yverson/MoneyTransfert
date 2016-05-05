using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MoneyTransfert
{
    public static class HelperSMS
    {
        public static void SendSMS(string No, string Message)
        {

            try
            {
                No = No.Trim();

                string respStr = "";
                Uri uri = new Uri(String.Format("https://mmg.symtel.biz:8443/AMMG/SymtelMMG?username=UTELECOM&password=telecom@2014&from=TUSEV&to=225{0}&dlr-mask=31&text={1}", No, Message));
                HttpWebRequest requestFile = (HttpWebRequest)WebRequest.Create(uri);
                requestFile.ContentType = "application/html";

                // Attaching the Certificate To the request

                System.Net.ServicePointManager.CertificatePolicy =
                                       new TrustAllCertificatePolicy();

                HttpWebResponse webResp = requestFile.GetResponse() as HttpWebResponse;
                if (requestFile.HaveResponse)
                {
                    if (webResp.StatusCode == HttpStatusCode.OK || webResp.StatusCode == HttpStatusCode.Accepted)
                    {
                        StreamReader respReader = new StreamReader(webResp.GetResponseStream());
                        respStr = respReader.ReadToEnd();

                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
    }

    public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
    {
        public TrustAllCertificatePolicy()
        { }
        public bool CheckValidationResult(ServicePoint sp,
           System.Security.Cryptography.X509Certificates.
            X509Certificate cert, WebRequest req, int problem)
        {

            return true;
        }
    }
}
