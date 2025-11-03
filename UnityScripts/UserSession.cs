using System.Collections.Generic;

public static class UserSession
{
    public static int UserID { get; set; }
    public static string UserName { get; set; }
    public static string PasswordHash { get; set; }
    public static int QRCodeNumber { get; set; }

    // NEW: Currently selected level
    public static Dictionary<string, object> ChoosenLevel { get; set; }

    public static void ClearSession()
    {
        UserID = 0;
        UserName = null;
        PasswordHash = null;
        QRCodeNumber = 0;

        ChoosenLevel = null;
    }
}
