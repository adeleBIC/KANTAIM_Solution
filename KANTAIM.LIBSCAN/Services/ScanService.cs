using System.Text.RegularExpressions;

namespace KANTAIM.LIBSCAN.Services
{
    public class ScanService
    {
        public string[] ParseCode(string txt)
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
