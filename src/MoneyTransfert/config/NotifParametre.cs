using System;
using System.Collections.Generic;
using System.Linq;

namespace MoneyTransfert.Models
{
    public class NotifParametre
    {
        public string cpm_site_id { get; set; }
        public string signature { get; set; }
        public string cpm_amount { get; set; }
        public string cpm_trans_id { get; set; }
        public string cpm_custom { get; set; }
        public string cpm_currency { get; set; }
        public string cpm_payid { get; set; }
        public string cpm_payment_date { get; set; }
        public string cpm_payment_time { get; set; }
        public string cpm_error_message { get; set; }
        public string payment_method { get; set; }
        public string cpm_phone_prefixe { get; set; }
        public string cel_phone_num { get; set; }
        public string cpm_ipn_ack { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string cpm_result { get; set; }
        public string cpm_trans_status { get; set; }
        public string cpm_designation { get; set; }
        public string buyer_name { get; set; }
    }

    public class Status
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class JsonResponse
    {
        public JsonResponse()
        {
        }

        public Status Status { get; set; }

        public NotifParametre transaction { get; set; }
    }
}