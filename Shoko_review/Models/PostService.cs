using Newtonsoft.Json;
using NLog;
using OauthModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace MobApp.Models
{
    public class PostService
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Получение системного токена
        /// </summary>
        /// <param name="_systemLogin"></param>
        /// <param name="_serviceUrl"></param>
        /// <returns></returns>
        public TokenModel GetTokenSystem()
        {
            TokenModel _tokenModel = null;

            using (var httpClient = new HttpClient())
            {
                try
                {
                    string login = "plas-portal-new";
                    string password = "f6yjAmk5@jk7eeCq";
                    string _serviceUrl = "https://sh.svc.sw.dc-2.plas-tek.ru/sh/SmartSupportTest/";

                    if (ConfigurationManager.AppSettings["IsTest"] == "false")
                    {
                        password = "aK#eMRNHPX>2*Y3ZTYks*QF-FwK@tXm$";
                        _serviceUrl = "https://sh.svc.sw.dc-2.plas-tek.ru/sh/SmartSupport/";
                    }

                    var authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", login, password)));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);

                    var form = new Dictionary<string, string>
                        {
                           {"grant_type", "password"},
                           {"username", login},
                           {"password", password},
                       };

                    var address = _serviceUrl + "accesstoken";
                    var tokenResponse = httpClient.PostAsync(address, new FormUrlEncodedContent(form)).Result;

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        _tokenModel = new TokenModel();
                        _tokenModel.Token = tokenResponse.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() }).Result;
                        _tokenModel.DateCreateToken = DateTime.Now;

                    }
                    else
                    {
                        var _errorPost = tokenResponse.Content.ReadAsAsync<ErrorPost>().Result;

                        string _errorMess = _errorPost.ErrorDescription;

                        throw new Exception(_errorMess);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("[Type][GetToken],[Mess][" + ex.Message + "]");
                }
            }

            return _tokenModel;
        }

        /// <summary>
        /// Получение информации о том прошла операция или нет без возврата данных
        /// </summary>
        /// <param name="_paramSql">Параметры для процедуры БД</param>
        /// <returns></returns>
        public bool PostBoolSystem(ParamSQL _paramSql)
        {
            bool _result = false;

            var _tokenModel = GetTokenSystem();

            if (_tokenModel == null)
            {
                return _result;
            }

            HttpResponseMessage _response = new HttpResponseMessage();

            int _attempt = 0;

            while (_response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _attempt++;

                if (_attempt > 2)
                {
                    _logger.Error("[Type][PostBool],[Mess][Превышено количество попыток],[Param][" + GetParamSqlToString(_paramSql) + "]");
                    break;
                }

                using (var _httpClient = new HttpClient())
                {
                    try
                    {
                        string _serviceUrl = "https://sh.svc.sw.dc-2.plas-tek.ru/sh/SmartSupportTest/";

                        if (ConfigurationManager.AppSettings["IsTest"] == "false")
                        {
                            _serviceUrl = "https://sh.svc.sw.dc-2.plas-tek.ru/sh/SmartSupport/"; 
                        }

                        _httpClient.BaseAddress = new Uri(_serviceUrl);

                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenModel.Token.AccessToken);

                        _response = _httpClient.PostAsJsonAsync("api/OauthSmart/PostString", _paramSql).Result;

                        if (_response.IsSuccessStatusCode)
                        {
                            _result = true;
                            break;
                        }
                        else if (_response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                        {
                            string _errorMess = Encoding.GetEncoding("windows-1251").GetString(Encoding.GetEncoding(1252).GetBytes(_response.ReasonPhrase));

                            throw new Exception(_errorMess);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("[Type][PostBool],[Mess][" + ex.Message + "],[Param][" + GetParamSqlToString(_paramSql) + "]");
                    }
                }
            }

            return _result;
        }

        /// <summary>
        /// Преобразование объекта в строку JSON
        /// </summary>
        /// <param name="paramSQL"></param>
        /// <returns></returns>
        private static string GetParamSqlToString(ParamSQL paramSQL)
        {
            return JsonConvert.SerializeObject(paramSQL);
        }

    }

    public sealed class ParamSQL
    {
        public string NameProcedure { get; set; }
        public object Obj { get; set; }

        public int? ReplicationID { get; set; }
    }

    public class TokenModel
    {
        public DateTime DateCreateToken { get; set; }
        public Token Token { get; set; }
    }

    public class ErrorPost
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

    }
}