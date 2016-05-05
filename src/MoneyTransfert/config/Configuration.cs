using System;
using System.Collections.Generic;
using System.Linq;

namespace MoneyTransfert
{
    public class Config
    {
        public static string baseUrlTest = "http://api.sandbox.cinetpay.com/";
        public static string baseUrlProd = "http://api.cinetpay.com/";
        public static string baseUrlPaiementProd = "http://secure.cinetpay.com";
        public static string baseUrlPaiementTest = "http://secure.sandbox.cinetpay.com";

        public static string adminNumber = "09917435";

        public static string URISignature = baseUrlTest+"v1/?method=getSignatureByPost";
        public static string URIStatus = baseUrlTest +"v1/?method=checkPayStatus";

        public static string apikey = "106612574455953b2d0e7775.94466351";
        public static string cpm_site_id = "721335";
        public static string cpm_currency = "CFA";
        public static string cpm_page_action = "PAYMENT";
        public static string cpm_payment_config = "SINGLE";
        public static string cpm_version = "V1";
        public static string cpm_language = "fr";
        public static string cpm_designation = "test iti";
        public static string cpm_custom = "test custom";
        
        
    }
}