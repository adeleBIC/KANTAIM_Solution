using System.Text.RegularExpressions;

namespace KANTAIM.APK.Services
{
    public class ScanService
    {
        public string[] scanCode(string txt)
        {
            if(txt != null)
            {
                Regex pattern = new Regex(@"^[1-9]+#[0-9]+(#[0-9]+)*\$$");
                if(pattern.IsMatch(txt))
                {
                    string[] v = txt.Split('$');
                    return v[0].Split('#');
                }
            }
           
            return null;
        }
    }
}
