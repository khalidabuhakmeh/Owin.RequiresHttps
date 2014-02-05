using System.Linq;
using System.Text.RegularExpressions;

namespace Owin.RequiresHttps
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RequiresHttps
    {
        private readonly Func<IDictionary<string, object>, Task> nextFunc;
        private readonly RequiresHttpsOptions options;

        public RequiresHttps(Func<IDictionary<string, object>, Task> nextFunc, RequiresHttpsOptions options)
        {
            this.nextFunc = nextFunc;
            this.options = options ?? new RequiresHttpsOptions();
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var scheme = (string)environment["owin.RequestScheme"];

            if (scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                return nextFunc(environment);

            var headers = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
            headers = headers ?? new Dictionary<string, string[]>();

            if (!string.IsNullOrWhiteSpace(options.RedirectToHttpsPath)) {
                headers.Add("Location", new[] { options.RedirectToHttpsPath });
                environment["owin.ResponseStatusCode"] = 302;
                return ReturnCompletedTask();
            }

            if (options.Force401) {
                environment["owin.ResponseStatusCode"] = 401;
                return ReturnCompletedTask();
            }

            // Will attempt to redirect to an https version of the current url
            // by changing the scheme, and adding a specified port if it is
            // not the standard 443
            var hostWithPort = headers["Host"].First();
            var removePort = new Regex(@":{1}\d{1,}");

            var hostname = removePort.Replace(hostWithPort, string.Empty);
            var httpUri = string.Format("https://{0}", hostname);

            if (options.HttpsPortNumber != 443)
                httpUri += string.Format(":{0}", options.HttpsPortNumber);

            httpUri += string.Join((string)environment["owin.RequestPathBase"], (string)environment["owin.RequestPath"]);

            if (environment["owin.RequestQueryString"] != "")
                httpUri += "?" + (string)environment["owin.RequestQueryString"];

            headers.Add("Location", new[] { httpUri });
            environment["owin.ResponseStatusCode"] = 302;
            return ReturnCompletedTask();
        }

        private Task ReturnCompletedTask()
        {
            return Task.FromResult(0);
        }
    }
}
