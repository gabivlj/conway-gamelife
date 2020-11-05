using System.Text;

namespace utilsconway
{
    public static class Str
    {
        public static string Repeat(string str, int times)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < times; i++)
            {
                builder.Append(str);
            }
            
            return builder.ToString();
        }
    }
}
