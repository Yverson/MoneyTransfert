using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.ApplicationInsights;
using MoneyTransfert.Models;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;
using PayPal.Api;

namespace MoneyTransfert.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private TelemetryClient telemetry = new TelemetryClient();
        protected string URISignature = "http://api.sandbox.cinetpay.com/v1/?method=getSignatureByPost";
        protected string URIStatus = "http://api.sandbox.cinetpay.com/v1/?method=checkPayStatus";

        private ApplicationDbContext _dbContext;
        public string currentUserId { get; set; }
        public ApplicationUser currentUser { get; set; }
        public string Name { get; set; }
        string signature;
        public string message { get; set; }

        public HomeController(ApplicationDbContext dbContext, TelemetryClient Telemetry)
        {
            _dbContext = dbContext;
            telemetry = Telemetry;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Paiement(Transactions trans)
        {

            ViewBag.messageVIP = "";
            currentUserId = HttpContext.User.GetUserId();
            trans.Id = Guid.NewGuid().ToString();
            trans.Date = DateTime.UtcNow.Date;
            trans.DateTransaction = DateTime.UtcNow;
            trans.Utilisateur = currentUserId;
            //trans.TypeTransaction = "MP";
            trans.Etat = "ACTIF";

            if (trans.TypeTransaction == "MP")
            {
                trans.Pourcentage = 0;
                trans.Total = trans.Montant + (trans.Montant * trans.Pourcentage * 0.01);
                trans.MontantEuro = trans.Montant / 655;
            }
            else
            {
                trans.Pourcentage = 7;
                trans.Montant = trans.MontantEuro * 655;
                trans.Total = trans.Montant - (trans.Montant * trans.Pourcentage * 0.01);
                
            }

            trans.status = "En Attente du Paiement";
            _dbContext.Transactions.Add(trans);
            _dbContext.SaveChanges();

            if (trans.TypeTransaction == "MP")
            {
                return View(PaiementRapide(trans, "Transfert d'argent Mobile money -> Paypal"));
            }
            else
            {
                return Redirect(PaiementPaypal(trans, "Transfert d'argent Paypal -> Mobile money"));
            }



        }

        public IActionResult ValidationPaiement(string paymentId, string PayerID, string token)
        {

            Transactions trans = _dbContext.Transactions.Where(c => c.PaypalId == paymentId && c.Etat == "ACTIF" && c.status != "Terminer").FirstOrDefault();

            if (trans != null)
            {
                Dictionary<string, string> sdkConfig = new Dictionary<string, string>();
                sdkConfig.Add("mode", "sandbox");
                string accessToken = new OAuthTokenCredential("AcZHMbdEwvd1QXQ-vdF-H0Kbe-IX-cM4kHhhc51w-SchOjO7lXIPmKlxBiNc-TTaHXf5TRjx5_0-TFRG", "EP71e33QUhJnEYGj4kh1SaV10MdQUVOyP3HArttAJ3jWPYJS8zN8QH3Y-AcNnD739Y2KpzrfHS_KVr8x", sdkConfig).GetAccessToken();

                APIContext apiContext = new APIContext(accessToken);
                apiContext.Config = sdkConfig;



                // Using the information from the redirect, setup the payment to execute.
                var paymentExecution = new PaymentExecution() { payer_id = PayerID };
                var payment = new Payment() { id = paymentId };

                try
                {
                    // Execute the payment.
                    var executedPayment = payment.Execute(apiContext, paymentExecution);
                    trans.log = "REUSSI";
                    trans.status = "Terminer";

                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Notification(NotifParametre notif)
        {
            //HelperSMS.SendSMS(Config.adminNumber, "Notif");
            ViewBag.Message = "Notification";
            telemetry.TrackEvent("Notification");

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data.Add("apikey", Config.apikey);
                data.Add("cpm_site_id", notif.cpm_site_id);
                data.Add("cpm_trans_id", notif.cpm_trans_id);

                byte[] responsebytes = client.UploadValues(Config.URIStatus, "POST", data);
                string res = Encoding.UTF8.GetString(responsebytes);
                JsonResponse ci = JsonConvert.DeserializeObject<JsonResponse>(res);

                //debut test si ok cpm_result==00

                if (ci.transaction.cpm_result == "00")
                {
                    try
                    {
                        Transactions trans = _dbContext.Transactions.Where(c => c.Id == ci.transaction.cpm_trans_id && c.Etat == "ACTIF" && c.status != "Terminer").FirstOrDefault();

                        if (trans != null)
                        {
                            if (trans.TypeTransaction == "MP")
                            {
                                telemetry.TrackEvent("Notification:MP");
                                trans.status = "Payé, En Attente de transfert.";
                                trans.statuscinetpay = ci.transaction.cpm_error_message;
                                trans.buyer_name = ci.transaction.buyer_name;
                                _dbContext.SaveChanges();

                                try
                                {
                                    Dictionary<string, string> sdkConfig = new Dictionary<string, string>();
                                    sdkConfig.Add("mode", "sandbox");
                                    string accessToken = new OAuthTokenCredential("AcZHMbdEwvd1QXQ-vdF-H0Kbe-IX-cM4kHhhc51w-SchOjO7lXIPmKlxBiNc-TTaHXf5TRjx5_0-TFRG", "EP71e33QUhJnEYGj4kh1SaV10MdQUVOyP3HArttAJ3jWPYJS8zN8QH3Y-AcNnD739Y2KpzrfHS_KVr8x", sdkConfig).GetAccessToken();

                                    APIContext apiContext = new APIContext(accessToken);
                                    apiContext.Config = sdkConfig;

                                    var payout = new Payout
                                    {
                                        // #### sender_batch_header
                                        // Describes how the payments defined in the `items` array are to be handled.
                                        sender_batch_header = new PayoutSenderBatchHeader
                                        {
                                            sender_batch_id = "batch_" + System.Guid.NewGuid().ToString().Substring(0, 8),
                                            email_subject = "Transfert d'argent par moneytransfert"

                                        },
                                        // #### items
                                        // The `items` array contains the list of payout items to be included in this payout.
                                        // If `syncMode` is set to `true` when calling `Payout.Create()`, then the `items` array must only
                                        // contain **one** item.  If `syncMode` is set to `false` when calling `Payout.Create()`, then the `items`
                                        // array can contain more than one item.
                                        items = new List<PayoutItem>
                                        {
                                            new PayoutItem
                                            {
                                                recipient_type = PayoutRecipientType.EMAIL,
                                                amount = new Currency
                                                {
                                                    //value = trans.MontantEuro.ToString(),
                                                    value = "5",
                                                    currency = "EUR"
                                                },
                                                receiver = trans.Email,
                                                note = "Thank you.",
                                                sender_item_id = trans.Id.ToString().Substring(0,8)
                                            }
                                        }
                                    };

                                    var createdPayout = payout.Create(apiContext, false);
                                    trans.log = "REUSSI";
                                    trans.status = "Terminer";

                                    _dbContext.SaveChanges();

                                }
                                catch (Exception ex)
                                {
                                    trans.log = ex.Message;
                                    _dbContext.SaveChanges();

                                }
                            }
                            else
                            {
                                try
                                {


                                }
                                catch (Exception ex)
                                {
                                    trans.log = ex.Message;
                                    _dbContext.SaveChanges();

                                }

                            }
                        }
                        else
                        {
                            HelperSMS.SendSMS(Config.adminNumber, "Trans null");

                        }
                    }
                    catch (Exception)
                    {

                        HelperSMS.SendSMS(Config.adminNumber, "Trans null");
                    }

                }
                else
                {
                    //log
                }

                //HelperSMS.SendSMS(Config.adminNumber, ci.transaction.buyer_name + " " + ci.transaction.cel_phone_num + " " + ci.transaction.cpm_custom + " " + ci.transaction.cpm_error_message + " " + ci.transaction.cpm_payid + " " + ci.transaction.cpm_result + " " + ci.transaction.cpm_trans_status);

                ViewBag.Notif = res;
            }


            return null;
        }

        public IActionResult Historique()
        {

            return View();
        }

        public IActionResult Rapport()
        {

            return View();
        }

        public IActionResult Help()
        {

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public PaiementData PaiementRapide(Transactions trans, string Description)
        {
            string signature;
            string id = DateTime.UtcNow.ToString("yyyyMMddhhmmss");

            using (WebClient client = new WebClient())
            {

                Config.cpm_designation = Description;

                NameValueCollection data = new NameValueCollection();
                data.Add("apikey", "106612574455953b2d0e7775.94466351");
                data.Add("cpm_site_id", "721335");
                data.Add("cpm_currency", "CFA");
                data.Add("cpm_page_action", "PAYMENT");
                data.Add("cpm_payment_config", "SINGLE");
                data.Add("cpm_version", "V1");
                data.Add("cpm_language", "fr");
                data.Add("cpm_trans_date", id);
                data.Add("cpm_trans_id", trans.Id.ToString());
                data.Add("cpm_designation", Config.cpm_designation);
                data.Add("cpm_amount", trans.Total.ToString());
                data.Add("cpm_custom", HttpContext.User.Identity.Name);

                byte[] responsebytes = client.UploadValues(URISignature, "POST", data);
                signature = Encoding.UTF8.GetString(responsebytes);
                signature = JsonConvert.DeserializeObject<string>(signature);

            }

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("apikey", Config.apikey);
            postData.Add("cpm_site_id", Config.cpm_site_id);
            postData.Add("cpm_currency", Config.cpm_currency);
            postData.Add("cpm_page_action", Config.cpm_page_action);
            postData.Add("cpm_payment_config", Config.cpm_payment_config);
            postData.Add("cpm_version", Config.cpm_version);
            postData.Add("cpm_language", Config.cpm_language);
            postData.Add("cpm_trans_date", id);
            postData.Add("cpm_trans_id", trans.Id.ToString());
            postData.Add("cpm_designation", Config.cpm_designation);
            postData.Add("cpm_amount", trans.Total.ToString());
            postData.Add("cpm_custom", HttpContext.User.Identity.Name);
            postData.Add("signature", signature);


            PaiementData pay = new PaiementData();
            pay.data = postData;

            return pay;
        }

        public string PaiementPaypal(Transactions trans, string Description)
        {
            Dictionary<string, string> sdkConfig = new Dictionary<string, string>();
            sdkConfig.Add("mode", "sandbox");
            string accessToken = new OAuthTokenCredential("AcZHMbdEwvd1QXQ-vdF-H0Kbe-IX-cM4kHhhc51w-SchOjO7lXIPmKlxBiNc-TTaHXf5TRjx5_0-TFRG", "EP71e33QUhJnEYGj4kh1SaV10MdQUVOyP3HArttAJ3jWPYJS8zN8QH3Y-AcNnD739Y2KpzrfHS_KVr8x", sdkConfig).GetAccessToken();

            APIContext apiContext = new APIContext(accessToken);
            apiContext.Config = sdkConfig;

            Amount amnt = new Amount();
            amnt.currency = "EUR";
            //amnt.total = trans.Total.ToString();
            amnt.total = trans.MontantEuro.ToString();

            List<Transaction> transactionList = new List<Transaction>();
            Transaction tran = new Transaction();
            tran.description = Description;
            tran.amount = amnt;
            transactionList.Add(tran);

            Payer payr = new Payer();
            payr.payment_method = "paypal";

            RedirectUrls redirUrls = new RedirectUrls();
            redirUrls.cancel_url = "http://etransfert.net";
            redirUrls.return_url = "http://localhost:63918/Home/ValidationPaiement";

            Payment pymnt = new Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactionList;
            pymnt.redirect_urls = redirUrls;

            Payment createdPayment = pymnt.Create(apiContext);

            trans.PaypalId = createdPayment.id;
            _dbContext.SaveChanges();

            string url = createdPayment.GetApprovalUrl();
            return url;

        }

    }
}
