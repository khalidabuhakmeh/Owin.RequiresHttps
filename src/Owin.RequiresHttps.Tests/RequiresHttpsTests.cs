namespace Owin.RequiresHttps.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class OwinHttpsTests
    {
        [Fact]
        public void Should_Execute_Next_If_Https()
        {
            //Given
            var owinhttps = GetOwinHttps(GetNextFunc());
            var environment = new Dictionary<string, object>
            {
                { "owin.RequestScheme", "https" },
                { "owin.ResponseHeaders", new Dictionary<string, string[]>()}
            };

            //When
            var task = owinhttps.Invoke(environment);

            //Then
            Assert.Equal(true, task.IsCompleted);
            Assert.Equal(123, ((Task<int>)task).Result);
        }

        [Fact]
        public void Should_Return_401_If_Http()
        {
            //Given
            var owinhttps = GetOwinHttps(GetNextFunc(), new RequiresHttpsOptions { Force401 = true });
            var environment = new Dictionary<string, object>
            {
                { "owin.RequestScheme", "http" },
                { "owin.ResponseHeaders", new Dictionary<string, string[]>()}
            };

            //When
            owinhttps.Invoke(environment);

            //Then
            Assert.Equal(401, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void Should_Return_Completed_Task_If_Http()
        {
            //Given
            var owinhttps = GetOwinHttps(GetNextFunc(), new RequiresHttpsOptions { Force401 = true });
            var environment = new Dictionary<string, object>
            {
                { "owin.RequestScheme", "http" },
                { "owin.ResponseHeaders", new Dictionary<string, string[]>()}
            };

            //When
            var task = owinhttps.Invoke(environment);

            //Then
            Assert.Equal(true, task.IsCompleted);
            Assert.Equal(0, ((Task<int>)task).Result);
        }

        [Fact]
        public void Should_Return_302_If_Options_Provided()
        {
            //Given
            var owinhttps = GetOwinHttps(GetNextFunc(),
                new RequiresHttpsOptions() { RedirectToHttpsPath = "http://www.google.co.uk" });

            var environment = new Dictionary<string, object>
            {
                {"owin.RequestScheme", "http"},
                {"owin.ResponseHeaders", new Dictionary<string, string[]>()}
            };

            //When
            owinhttps.Invoke(environment);

            //Then
            var headers = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;

            Assert.Equal("http://www.google.co.uk", headers["Location"].First());
            Assert.Equal(302, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void Should_Return_Completed_Task_If_Http_And_Options_Provided()
        {
            //Given
            var owinhttps = GetOwinHttps(GetNextFunc(),
                new RequiresHttpsOptions() { RedirectToHttpsPath = "http://www.google.co.uk" });

            var environment = new Dictionary<string, object>
            {
                {"owin.RequestScheme", "http"},
                {"owin.ResponseHeaders", new Dictionary<string, string[]>()}
            };

            //When
            var task = owinhttps.Invoke(environment);

            //Then
            Assert.Equal(true, task.IsCompleted);
            Assert.Equal(0, ((Task<int>)task).Result);
        }

        [Fact]
        public void Should_Return_401_If_Options_RedirectURL_Not_Specified()
        {
            //Given
            var owinhttps = GetOwinHttps(GetNextFunc(),
                new RequiresHttpsOptions() { RedirectToHttpsPath = "", Force401 = true });

            var environment = new Dictionary<string, object>
            {
                {"owin.RequestScheme", "http"},
                {"owin.ResponseHeaders", new Dictionary<string, string[]>()}
            };

            //When
            owinhttps.Invoke(environment);

            //Then
            Assert.Equal(401, environment["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void Should_Redirect_To_Https_Version_Of_Http_Url()
        {
            var owinhttps = GetOwinHttps(GetNextFunc());

            var environment = new Dictionary<string, object>
            {
                {"owin.RequestScheme", "http"},
                {"owin.RequestPathBase", ""},
                {"owin.RequestPath", "" },
                {"owin.RequestQueryString", "" },
                {"owin.ResponseHeaders", new Dictionary<string, string[]>{ {"Host", new []{ "localhost" }}}}
            };

            //When
            owinhttps.Invoke(environment);

            //Then
            
            Assert.Equal(302, environment["owin.ResponseStatusCode"]);
            var headers = environment["owin.ResponseHeaders"] as Dictionary<string, string[]>;
            Assert.Equal("https://localhost", headers["Location"].First());
        }

        [Fact]
        public void Should_Redirect_To_Https_Version_Of_Http_Url_With_Port_Number()
        {
            var owinhttps = GetOwinHttps(GetNextFunc(), new RequiresHttpsOptions() { HttpsPortNumber = 2014 });

            var environment = new Dictionary<string, object>
            {
                {"owin.RequestScheme", "http"},
                {"owin.RequestPathBase", ""},
                {"owin.RequestPath", "" },
                {"owin.RequestQueryString", "" },
                {"owin.ResponseHeaders", new Dictionary<string, string[]>{ {"Host", new []{ "localhost" }}}}
            };

            //When
            owinhttps.Invoke(environment);

            //Then

            Assert.Equal(302, environment["owin.ResponseStatusCode"]);
            var headers = environment["owin.ResponseHeaders"] as Dictionary<string, string[]>;
            Assert.Equal("https://localhost:2014", headers["Location"].First());
        }

        [Fact]
        public void Should_Redirect_To_Https_Version_Of_Http_Url_With_QueryString()
        {
            var owinhttps = GetOwinHttps(GetNextFunc());

            var environment = new Dictionary<string, object>
            {
                {"owin.RequestScheme", "http"},
                {"owin.RequestPathBase", ""},
                {"owin.RequestPath", "" },
                {"owin.RequestQueryString", "awesome=yes&cool=definitely" },
                {"owin.ResponseHeaders", new Dictionary<string, string[]>{ {"Host", new []{ "localhost" }}}}
            };

            //When
            owinhttps.Invoke(environment);

            //Then

            Assert.Equal(302, environment["owin.ResponseStatusCode"]);
            var headers = environment["owin.ResponseHeaders"] as Dictionary<string, string[]>;
            Assert.Equal("https://localhost?awesome=yes&cool=definitely", headers["Location"].First());
        }


        public Func<IDictionary<string, object>, Task> GetNextFunc()
        {
            return objects => Task.FromResult(123);
        }

        public RequiresHttps GetOwinHttps(Func<IDictionary<string, object>, Task> nextFunc, RequiresHttpsOptions options = null)
        {
            return new RequiresHttps(nextFunc, options);
        }
    }
}
