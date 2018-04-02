using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
//Additional includes
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Http;
using Properties;

namespace MCLoginLib
{
    public class Launcher
    {

        public delegate void OnFailedLogin();
        public delegate void OnSuccessLogin();


        public static SessionData DATA = new SessionData();
        static string appData;
        static string path;
        
        public static async Task<string> Authenticate(string username, string password)
        {
            var vjson = new AuthenticateJSON
            {
                username = username,
                password = password,
                clientToken = Settings.Default.clientToken,
                requestUser = true,
                agent = new Agent
                {
                    name = "Minecraft",
                    version = 1
                }
            };

            HttpClient client = new HttpClient();

            var content = new StringContent(JsonConvert.SerializeObject(vjson), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://authserver.mojang.com/authenticate", content);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public static async Task<bool> Login(string id, string pwd, OnSuccessLogin loginCallback, OnFailedLogin failedCallback)
        {
            string x = await Launcher.Authenticate(id, pwd);
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(x);
            string accessToken = "";
            string username = "";
            string Puuid = "";
            try
            {
                accessToken = json.accessToken;
                username = json.selectedProfile.name;
                Puuid = json.selectedProfile.id;
                if (accessToken != null)
                {
                    if (loginCallback != null)
                        loginCallback.Invoke();
                }
                else
                {
                    if (failedCallback != null)
                        failedCallback.Invoke();
                    return false;
                }
            }
            catch (Exception eex)
            {
                if (failedCallback != null)
                    failedCallback.Invoke();

                return false;
            }
            DATA.username = username;
            DATA.premium = true;
            DATA.uuidPremium = Puuid;
            DATA.accessToken = accessToken;
            return true;
        }

        private static string MakeForgeData()
        {
            var json = File.ReadAllText(path + @"\" + "se.json");
            ///포지 데이터로드


            dynamic decoded = JsonConvert.DeserializeObject(json);

            //포지 버전     
            var forge = decoded.forgeVersion;

                //경로 및 argument 생성
                string launch = @"-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump -Xmx" +  4 + "G -Xms" + "4" + "G " +
                            "-XX:+DisableExplicitGC -XX:+UseConcMarkSweepGC -XX:+UseParNewGC -XX:+UseNUMA -XX:+CMSParallelRemarkEnabled -XX:MaxTenuringThreshold=15 -XX:MaxGCPauseMillis=30 -XX:GCPauseIntervalMillis=150 -XX:+UseAdaptiveGCBoundary -XX:-UseGCOverheadLimit -XX:+UseBiasedLocking -XX:SurvivorRatio=8 -XX:TargetSurvivorRatio=90 -XX:MaxTenuringThreshold=15 -Dfml.ignorePatchDiscrepancies=true -Dfml.ignoreInvalidMinecraftCertificates=true -XX:+UseFastAccessorMethods -XX:+AggressiveOpts " +
                        "\"-Djava.library.path=" + path + "\\\\natives-win\\\\\" " +

                        @"-cp ";



            //포지 경로추가
               launch += "\"" + path + @"\\libraries\net\minecraftforge\forge\" + MineCraftInfo.ForgeVersion + @"\\" + MineCraftInfo.ForgeFileName + ";";
                    

                //포지 라이브러리 로드
                foreach (var lib in decoded.libs)
                    launch += path+ "\\" + lib.path + ";";
                
                launch += path + @"\\versions\\" + decoded.mc_version + "\\\\" + decoded.mc_version + ".jar";



                //어규먼트 디코드
                string args = decoded.arguments;
                //세션정보 추가
                string arguments = args.Replace(
                    "${auth_player_name}", MCLoginLib.Launcher.DATA.username
                    ).Replace(
                    "${version_name}",
                        (string)decoded.mc_version + "-forge" + (string)decoded.mc_version + "-" + MineCraftInfo.MinimalForgeVersion + "-" + (string)decoded.mc_version + " "
                    ).Replace(
                    "${game_directory}", path
                    ).Replace(
                    "${assets_root}", path + "\\assets\\"
                    ).Replace(
                    "${assets_index_name}", (string)decoded.mc_version
                    ).Replace(
                    "${auth_uuid}", MCLoginLib.Launcher.DATA.uuidPremium
                    ).Replace(
                    "${auth_access_token}", MCLoginLib.Launcher.DATA.accessToken
                    ).Replace(
                    "${user_properties}", "{}"
                    ).Replace(
                    "${user_type}", "mojang"
                    ).Replace(
                    "${version_type}", (string)decoded.version_type
                    );
            
                launch = launch + "\" " + (string)decoded.mainClass + " " + arguments;
            
                return launch;
        }


        public static async Task StartMineCraft()
        {
            appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";
            path = appData + @".minecraft";

            System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + ".minecraft");
            path = AppDomain.CurrentDomain.BaseDirectory + ".minecraft";
   
            Process process = new Process();
            try
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = ComputerInfoDetect.GetJavaInstallationPath();
            
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = MakeForgeData();
               
                process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    if (e.Data != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            
                            if (e.Data.ToString().Contains("Loading tweak class name"))
                            {
                       
                            }
                            if (e.Data.ToString().Contains("Setting user:"))
                            {
                               
                            }
                            if (e.Data.ToString().Contains("LWJGL Version:"))
                            {

                            }

                        }));

                    }
                });
                process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    if (e.Data != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                         //   MessageBox.Show(e.Data.ToString());
                        }));
                    }
                });

                await Task.Run(() => process.Start());
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await Task.Run(() => process.WaitForExit());
               
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Data.ToString());
            }

           

        }

    }
}