using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class config
    {
        public static string minecraftfolder = "App/.minecraft";
        public static string mainclass = "";
        public static string arguments = "";
        public static string forgefilepath = "";
        public static string logfile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + minecraftfolder + @"\launcher_logs.txt";


        public static string M_F_P = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + minecraftfolder + @"\";

        public static string javaLocal = AppDomain.CurrentDomain.BaseDirectory +"/Java";


        public static string LocalModpacks = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + minecraftfolder + @"\LocalModpacks\";
        public static string jre32FileName = "jre1.8.0_161";
        public static string jre64FileName = "jre1.8.0_161";
        public static string jre32URL = "http://download.oracle.com/otn-pub/java/jdk/8u161-b12/2f38c3b165be4555a1fa6e98c45e0808/jre-8u161-windows-i586.tar.gz";
        public static string jre64URL = "http://download.oracle.com/otn-pub/java/jdk/8u161-b12/2f38c3b165be4555a1fa6e98c45e0808/jre-8u161-windows-x64.tar.gz";
        public static string modpackDownload = "dummy";
        public static string modpackExtract  = modpackDownload+"/extract";

}

