using httpscert.src.util;

namespace httpscert.src.legacy
{
    internal static class Legacy
    {
        internal static int Command(string[] mArgs)
        {
            string? pass = string.Empty;
            string[]? domains = null;
            int year = 1;
            for (int i = 0; i < mArgs.Length; i++)
            {
                var item = mArgs[i].Replace("-", "/");
                if (i + 1 < mArgs.Length)
                {
                    if (item.StartsWith("/d", StringComparison.OrdinalIgnoreCase))
                    {
                        string domain = mArgs[i + 1].Replace('=', '-');
                        if (domain.IndexOf('+') != -1)
                        {
                            domains = domain.Split('+');
                        }
                        else
                        {
                            domains = [domain];
                        }
                    }
                    else if (item.StartsWith("/p", StringComparison.OrdinalIgnoreCase))
                    {
                        pass = mArgs[i + 1];
                    }
                    else if (item.StartsWith("/y", StringComparison.OrdinalIgnoreCase) && !int.TryParse(mArgs[i + 1], out year))
                    {
                        year = 1;
                    }
                }
            }

            if (domains == null)
            {
                return -1;
            }

            Utils.Generate(".", domains, pass, year);
            return 0;
        }
    }
}