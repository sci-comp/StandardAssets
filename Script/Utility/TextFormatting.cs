namespace Game
{
    public static class TextFormatting
    {
        public static string Bars(string msg, int length=64, int prefixCount=4)
        {
            string prefix = new('-', prefixCount);
            string message = $"{prefix} {msg} ";

            if (message.Length >= length)
                return message.Substring(0, length);

            return message + new string('-', length - message.Length);
        }

    }

}

