namespace H3Control
{
    public class H3PasswordConfig
    {
        public static string Hash;

        public static bool IsStricted
        {
            get
            {
                return !string.IsNullOrEmpty(Hash) && Hash.Length == 40; 
            }
        }
    }
}