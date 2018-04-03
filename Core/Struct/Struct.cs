public class SessionData
{
    public string latestVersion = "";
    public long AverageDownloadSpeed;
    //DATI utente
    public string username = "";
    public bool premium = false;
    public string uuidPremium = "0";
    public string accessToken = "0";
    public bool isLegacy = false;
}

public class AuthenticateJSON
{
    public string username;
    public string password;
    public string clientToken;
    public bool requestUser;
    public Agent agent;
}
public class Agent
{
    public string name;
    public int version;
}

public static class MineCraftInfo
{
    //마크 버전을 제외한 포지버전 
    public static string MinimalForgeVersion = "10.13.4.1566";
    public static string ForgeVersion = "1.7.10-10.13.4.1566-1.7.10";
    public static string ForgeFileName = "forge-1.7.10-10.13.4.1566-1.7.10.jar";
    public static string MinecraftFolderName = "App/.minecraft";
}