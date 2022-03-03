using Newtonsoft.Json;

using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OzCodeReview.ClientApi
{
    public class BaseService
    {
        public static string BaseUrl { get; set; }
                   
        public static EventHandler OnAuthorizeRequired { get; set; }

        public static string BearerToken { get; private set; } = null;

        public static void SetBearerToken(string token)
        {
            BearerToken = token;
        }

        protected static HttpClient GetHttpClient(bool anonymous)
        {
            HttpClient httpClient = new();
            httpClient.Timeout = new TimeSpan(0, 2, 0);

            if (!anonymous && !string.IsNullOrEmpty(BearerToken))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + BearerToken);
            }

            return httpClient;
        }

        protected async Task<T> GetAsync<T>(string url, bool anonymous = false, bool encrypted = false, bool deserializeInheritance = false)
        {
            Uri uri = url.StartsWith("http") ? new Uri(url) : new Uri(BaseUrl + url);

            return await this.GetAsync<T>(uri, anonymous,  encrypted, deserializeInheritance);
        }

        protected async Task<T> GetAsync<T>(Uri uri, bool anonymous = false, bool encrypted = false, bool deserializeInheritance = false)
        {
            try
            {
                using HttpClient httpClient = BaseService.GetHttpClient(anonymous);
                using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);                

                using HttpResponseMessage result = await httpClient.SendAsync(requestMessage);
                string json = await result.Content.ReadAsStringAsync();

                if (result.IsSuccessStatusCode)
                {
                    if (encrypted)
                    {
                        json = this.Decrypt(json);
                    }

                    try
                    {
                        if (deserializeInheritance)
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.All
                            };

                            return JsonConvert.DeserializeObject<T>(json, settings);
                        }

                        return JsonConvert.DeserializeObject<T>(json);
                    }
                    catch (JsonSerializationException ex)
                    {
                        throw new BusinessException($"Error deserializing : {json}", ex);
                    }
                }

                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    OnAuthorizeRequired?.Invoke(this, new EventArgs());
                    return default;
                }

                try
                {
                    var errors = JsonConvert.DeserializeObject<string[]>(json) ?? new string[] { "Une erreur imprévue s'est produite" };
                    throw new BusinessException(errors);

                }
                catch (JsonReaderException ex)
                {
                    throw new BusinessException($"Error deserializing : {json}", ex);
                }

                throw new BusinessException($"Erreur {result.StatusCode} lors de la requête");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during getting {uri}", ex?.Message);
                throw;
            }
        }

        protected async Task<OperationResult<T>> PostAsync<T>(string url, object data, bool anonymous = false,
             Expression<Action> onConnectionRetrievedCallBack = null, bool handleTypeName = false)
        {
            Uri uri = url.StartsWith("http") ? new Uri(url) : new Uri(BaseService.BaseUrl + url);

            return await PostAsync<T>(uri, data, anonymous, onConnectionRetrievedCallBack, handleTypeName);
        }

        protected async Task<OperationResult<T>> PostAsync<T>(Uri uri, object data, bool anonymous = false,
            Expression<Action> onConnectionRetrievedCallBack = null, bool handleTypeName = false)
        {

            try
            {
                var result = await this.PostAsync(uri, data, anonymous, handleTypeName);
                string json = await result.Content.ReadAsStringAsync();

                if (result.IsSuccessStatusCode)
                {
                    try
                    {
                        return new OperationResult<T>
                        {
                            Data = JsonConvert.DeserializeObject<T>(json),
                            Success = true
                        };
                    }
                    catch (JsonReaderException ex)
                    {
                        throw new BusinessException($"Error deserializing : {json}", ex);
                    }
                }
                else
                {
                    try
                    {
                        return new OperationResult<T>
                        {
                            Success = false,
                            Data = default,
                            Errors = JsonConvert.DeserializeObject<string[]>(json) ?? new string[] { "Une erreur imprévue s'est produite" }
                        };
                    }
                    catch (JsonReaderException ex)
                    {
                        throw new BusinessException($"Error deserializing : {json}", ex);
                    }
                }
            }
            catch (HttpRequestException ex)
            {               
                return new OperationResult<T>
                {
                    Success = false,
                    Data = default,
                    Errors = new[] { "une erreur est survenue lors de l'appel au service web", ex.Message }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during posting {uri} : {ex.GetType()} - {ex.Message} \n {ex.StackTrace}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri, object data, bool anonymous, bool handleTypeName)
        {
            using HttpClient httpClient = BaseService.GetHttpClient(anonymous);
            httpClient.Timeout = TimeSpan.FromMilliseconds(400000);

            string jsonReq = string.Empty;

            if (data != null)
            {
                if (handleTypeName)
                {
                    jsonReq = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        MaxDepth = 10,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    });
                }
                else
                {
                    jsonReq = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                }
            }

            using StringContent stringContent = new(jsonReq, Encoding.UTF8, "application/json");
            return await httpClient.PostAsync(uri, stringContent);
        }

        protected async Task<OperationResult<T>> PutAsync<T>(string url, object data, bool anonymous = false,
            Expression<Action> onConnectionRetrievedCallBack = null, bool handleTypeName = false)
        {
            Uri uri = url.StartsWith("http") ? new Uri(url) : new Uri(BaseService.BaseUrl + url);

            return await PutAsync<T>(uri, data, anonymous, onConnectionRetrievedCallBack, handleTypeName);
        }

        protected async Task<OperationResult<T>> PutAsync<T>(Uri uri, object data, bool anonymous = false,
            Expression<Action> onConnectionRetrievedCallBack = null, bool handleTypeName = false)
        {
            try
            {
                var result = await this.PutAsync(uri, data, anonymous, handleTypeName);
                string json = await result.Content.ReadAsStringAsync();

                if (result.IsSuccessStatusCode)
                {
                    try
                    {
                        return new OperationResult<T>
                        {
                            Data = JsonConvert.DeserializeObject<T>(json),
                            Success = true
                        };
                    }
                    catch (JsonReaderException ex)
                    {
                        throw new BusinessException($"Error deserializing : {json}", ex);
                    }
                }
                else
                {
                    try
                    {
                        return new OperationResult<T>
                        {
                            Success = false,
                            Data = default,
                            Errors = JsonConvert.DeserializeObject<string[]>(json) ?? new string[] { "Une erreur imprévue s'est produite" }
                        };
                    }
                    catch (JsonReaderException ex)
                    {
                        throw new BusinessException($"Error deserializing : {json}", ex);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                return new OperationResult<T>
                {
                    Success = false,
                    Data = default,
                    Errors = new[] { "An error occured", ex.Message }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during putting {uri} : {ex.GetType()} - {ex.Message} \n {ex.StackTrace}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync(Uri uri, object data, bool anonymous, bool handleTypeName)
        {
            using HttpClient httpClient = BaseService.GetHttpClient(anonymous);
            httpClient.Timeout = TimeSpan.FromMilliseconds(400000);

            string jsonReq = string.Empty;

            if (data != null)
            {
                if (handleTypeName)
                {
                    jsonReq = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.All,
                        NullValueHandling = NullValueHandling.Ignore,
                        MaxDepth = 10,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    });
                }
                else
                {
                    jsonReq = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                }
            }

            using StringContent stringContent = new(jsonReq, Encoding.UTF8, "application/json");
            return await httpClient.PutAsync(uri, stringContent);
        }

        protected async Task<OperationResult<T>> DeleteAsync<T>(string url, bool anonymous = false, Expression<Action> onConnectionRetrievedCallBack = null)
        {
            Uri uri = url.StartsWith("http") ? new Uri(url) : new Uri(BaseService.BaseUrl + url);

            return await DeleteAsync<T>(uri, anonymous, onConnectionRetrievedCallBack);
        }

        protected async Task<OperationResult<T>> DeleteAsync<T>(Uri url, bool anonymous = false, Expression<Action> onConnectionRetrievedCallBack = null)
        {
            try
            {
                using HttpResponseMessage result = await this.DeleteAsync(url, anonymous);
                string json = await result.Content.ReadAsStringAsync();

                if (result.IsSuccessStatusCode)
                {
                    return new OperationResult<T> { Data = JsonConvert.DeserializeObject<T>(json), Success = true };
                }
                else
                {
                    return new OperationResult<T>
                    {
                        Success = false,
                        Data = default,
                        Errors = JsonConvert.DeserializeObject<string[]>(json)
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new OperationResult<T>
                {
                    Success = false,
                    Data = default,
                    Errors = new[] { "une erreur est survenue lors de l'appel au service web", ex.Message }
                };
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(Uri url, bool anonymous = false)
        {
            using HttpClient httpClient = BaseService.GetHttpClient(anonymous);
            httpClient.Timeout = TimeSpan.FromMilliseconds(400000);

            return await httpClient.DeleteAsync(url);
        }     

        

        private string Decrypt(string encrypted)
        {
            var nobase64Encrypted = Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));

            var base64decrypted = XorIt(BearerToken, nobase64Encrypted);

            return Encoding.UTF8.GetString(Convert.FromBase64String(base64decrypted));
        }

        /// <summary>
        /// https://stackoverflow.com/questions/14971836/xor-ing-strings-in-c-sharp
        /// </summary>
        /// <param name="key"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string XorIt(string key, string input)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                stringBuilder.Append((char)(input[i] ^ key[(i % key.Length)]));
            }

            return stringBuilder.ToString();
        }
    }
}
