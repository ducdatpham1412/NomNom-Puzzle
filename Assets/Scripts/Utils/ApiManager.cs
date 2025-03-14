using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static class ApiManager {
    public enum ERROR_MSG {
        token_expired,
        token_blacklisted,
    }

    static HttpClient Client = new() {
        // BaseAddress = new Uri("http://localhost:8000")
    };
    static string BaseAddress = $"{Configs.Env.api_url}/api/v1";
    static ConcurrentQueue<TaskCompletionSource<string>> taskQueue = new ConcurrentQueue<TaskCompletionSource<string>>();
    static bool isRefreshing = false;

    static ApiManager() {
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var account = Storage.GET<Account>(Storage.Key.account);
        if (account != null) {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.token);
        }
    }


    static string AppendQueryParameters(string url, Dictionary<string, string> parameters) {
        if (parameters == null) {
            return url;
        }

        var uriBuilder = new UriBuilder(url);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var param in parameters) {
            query[param.Key] = param.Value;
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public static void SetAccount(JObject resLogin) {
        string token = resLogin["token"].ToString();
        string refresh_token = resLogin["refresh_token"].ToString();
        Account account = new Account {
            token = token,
            refresh_token = refresh_token,
        };
        Storage.SetAccount(account);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<T> HandleError<T>(HttpResponseMessage res, Func<Task<T>> PendingTask, bool isRetried) {
        string strError = await res.Content.ReadAsStringAsync();
        JObject errObject = JObject.Parse(strError);
        string errMsg = errObject["error_msg"].ToString();

        if (errMsg == ERROR_MSG.token_expired.ToString() && !isRetried) {
            if (isRefreshing) {
                var tcs = new TaskCompletionSource<string>();
                taskQueue.Enqueue(tcs);
                await tcs.Task;
                return await PendingTask();
            }

            isRefreshing = true;
            string refreshToken = "refresh_token";
            Client.DefaultRequestHeaders.Remove("Authorization");
            try {
                JObject refreshRes = await POST<JObject>(
                    path: "/auth/refresh-token",
                    data: new Dictionary<string, object> { { "refresh", refreshToken } }, isRetried: true
                );
                string newToken = refreshRes["access"].ToString();
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                Account account = Storage.GET<Account>(Storage.Key.account);
                account.token = newToken;
                Storage.SetAccount(account);

                while (!taskQueue.IsEmpty) {
                    if (taskQueue.TryDequeue(out var tcs)) {
                        tcs.SetResult("Done");
                    }
                }

                return await PendingTask();
            }
            catch (Exception ex) {
                if (ex.Message == ERROR_MSG.token_blacklisted.ToString()) {
                    JObject resLogin = await POST<JObject>(
                        "/auth/login",
                        data: new Dictionary<string, object> { { "device_id", Helper.DeviceID } },
                        parameters: new Dictionary<string, string> {
                            {"type", "device_id"},
                        }
                    );
                    SetAccount(resLogin);
                    while (!taskQueue.IsEmpty) {
                        if (taskQueue.TryDequeue(out var tcs)) {
                            tcs.SetResult("Done");
                        }
                    }

                    return await PendingTask();
                }
                else {
                    throw new InvalidOperationException(ex.Message);
                }
            }
            finally {
                isRefreshing = false;
            }
        }
        else {
            throw new InvalidOperationException(errMsg);
        }
    }

    public static async Task<T> GET<T>(
        string path,
        Dictionary<string, string> parameters = null,
        bool isRetried = false
    ) {
        string url = AppendQueryParameters($"{BaseAddress}{path}", parameters);
        HttpResponseMessage res = await Client.GetAsync(url);

        if (!res.IsSuccessStatusCode) {
            return await HandleError(
                res,
                PendingTask: async () => await GET<T>(path, parameters, isRetried: true),
                isRetried
            );
        }

        string strContent = await res.Content.ReadAsStringAsync();
        JToken jData = JObject.Parse(strContent)["data"];
        return jData.ToObject<T>();
    }


    public static async Task<T> POST<T>(
        string path,
        Dictionary<string, object> data = null,
        Dictionary<string, string> parameters = null,
        bool isRetried = false
    ) {
        string url = AppendQueryParameters($"{BaseAddress}{path}", parameters);
        HttpContent content = null;
        if (data != null) {
            var json = JsonConvert.SerializeObject(data);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        HttpResponseMessage res = await Client.PostAsync(url, content);

        if (!res.IsSuccessStatusCode) {
            return await HandleError(
                res,
                PendingTask: async () => await POST<T>(path, data, parameters, isRetried: true),
                isRetried
            );
        }

        string strContent = await res.Content.ReadAsStringAsync();
        JToken jData = JObject.Parse(strContent)["data"];
        return jData.ToObject<T>();
    }


    public static async Task<T> PUT<T>(
        string path,
        Dictionary<string, string> data = null,
        Dictionary<string, string> parameters = null,
        bool isRetried = false
    ) {
        string url = AppendQueryParameters($"{BaseAddress}{path}", parameters);
        HttpContent content = null;
        if (data != null) {
            var json = JsonConvert.SerializeObject(data);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        HttpResponseMessage res = await Client.PutAsync(url, content);

        if (!res.IsSuccessStatusCode) {
            return await HandleError(
                res,
                PendingTask: async () => await PUT<T>(path, data, parameters, isRetried: true),
                isRetried
            );
        }

        string strContent = await res.Content.ReadAsStringAsync();
        JToken jData = JObject.Parse(strContent)["data"];
        return jData.ToObject<T>();
    }


    public static async Task<T> DELETE<T>(
       string path,
       Dictionary<string, string> parameters = null,
       bool isRetried = false
   ) {
        string url = AppendQueryParameters($"{BaseAddress}{path}", parameters);
        HttpResponseMessage res = await Client.DeleteAsync(url);

        if (!res.IsSuccessStatusCode) {
            return await HandleError(
                res,
                PendingTask: async () => await DELETE<T>(path, parameters, isRetried: true),
                isRetried
            );
        }

        string strContent = await res.Content.ReadAsStringAsync();
        JToken jData = JObject.Parse(strContent)["data"];
        return jData.ToObject<T>();
    }
}