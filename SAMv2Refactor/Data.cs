namespace IngameScript
{
    partial class Program
    {
        // Methods appear to be unused
        private static class Data
        {
            public static string str = "-";
            private static int i = 0;
            public static void AppendString(string newString)
            {
                str += newString + "\n";
            }

            public static void ClearSomething()
            {
                i++;
                if (i >= 10000000)
                {
                    i = 0;
                }
                str = "Clears: " + i.ToString() + "\n";
            }
        }
    }
}
