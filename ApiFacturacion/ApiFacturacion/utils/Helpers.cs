using System.Text;

namespace ApiFacturacion.utils
{
    public class Helpers
    {
    }
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => new UTF8Encoding(false);
    }
}
