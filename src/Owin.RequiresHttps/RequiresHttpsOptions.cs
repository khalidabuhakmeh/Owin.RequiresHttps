namespace Owin.RequiresHttps
{
    public class RequiresHttpsOptions
    {
        public RequiresHttpsOptions()
        {
            HttpsPortNumber = 443;
            Force401 = false;
        }

        public bool Force401 { get; set; }
        public string RedirectToHttpsPath { get; set; }
        public int HttpsPortNumber { get; set; }
    }
}
