using MobApp.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace MobApp.Controllers
{
    public class HomeController : Controller
    {
        PostService postService = new PostService();
        Logger _logger = LogManager.GetCurrentClassLogger();

        // GET: Home
        public ActionResult Index(string transactionUID)
        {
            /// Test

            ///transactionUID = "120030932a96bf4779";

            ///Prodaction
            transactionUID = "120031245a31bd6b40";

            ViewBag.TransactionUID = transactionUID;
            ViewBag.Result = (TempData != null && TempData["Result"] != null) ? TempData["Result"] : null;

            return View();
        }

        [HttpPost]
        public ActionResult GetResult(FormCollection formCollection)
        {
            string _result = string.Empty;


            /**/MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress("Plas-Tek<info@plas-tek.ru>");
            mailMessage.To.Add("angelina.krylova@plas-tek.ru");
            mailMessage.Subject = " Новый отзыв";

            /// Формируем тело письма
            var _strBody = new StringBuilder();
            _strBody.AppendLine(formCollection["rating"]);
            _strBody.AppendLine(formCollection["category"]);
            _strBody.AppendLine(formCollection["review"]);
            _strBody.AppendLine(formCollection["recomm"]);



            mailMessage.Body = _strBody.ToString();

            SmtpClient smtpClient = new SmtpClient("195.161.219.163", 25);
            

            smtpClient.Send(mailMessage);


            var feedbackTransactionRecord = new FeedbackTransactionRecord();

            feedbackTransactionRecord.MerchantID = 1;
            feedbackTransactionRecord.StarsCount = Convert.ToInt32(formCollection["rating"]);
            feedbackTransactionRecord.Recommendation = Convert.ToInt32(formCollection["recomm"]);
            feedbackTransactionRecord.LocalDatetime = Convert.ToDateTime(formCollection["LocalDatetime"]);
            feedbackTransactionRecord.LocalTimeOffset = Convert.ToInt32(formCollection["LocalTimeOffset"]);
            feedbackTransactionRecord.Category = formCollection["category"];
            feedbackTransactionRecord.Text = formCollection["review"];
            feedbackTransactionRecord.TransactionUID = formCollection["transactionUID"];

            bool _postResult = false;

            try
            {
                _postResult = postService.PostBoolSystem(new ParamSQL { NameProcedure = "entity.FeedbackTransactionRecord", Obj = feedbackTransactionRecord });
            }
            catch (Exception ex)
            {
                /// Логирование ошибки
            }

            if (!_postResult)
            {
                _result = "Ошибка: некорректная ссылка";
            }
            else
            {
                _result = "Спасибо Вам за отзыв! <br>Ознакомьтесь с новостями и акциями на нашем сайте или в мобильном приложении";
            }

            TempData["Result"] = _result;

            return RedirectToAction("Index");
        }

    }
}