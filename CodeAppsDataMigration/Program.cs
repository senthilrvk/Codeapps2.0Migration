namespace CodeAppsDataMigration
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Super user must sign in before the dashboard opens.
            using (var login = new SuperUserLoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK) return;
            }

            Application.Run(new Dashboard());
        }
    }
}