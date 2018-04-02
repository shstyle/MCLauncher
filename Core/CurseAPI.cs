using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


public class CurseApis
{
    public class ModpackVersions
    {
        public string Name { get; set; }
        public string URL { get; set; }
        public string GameVersion { get; set; }

    }
    public class CategorySection
    {
        public string Name { get; set; }
    }
    public class Attachments
    {
        public string ThumbnailUrl { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public bool IsDefault { get; set; }
    }
    public class Authors
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    public class Categories
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class CurseRoot
    {
        public List<data> Data { get; set; }
    }
    public class Minecraft
    {
        public List<ModLoader> modLoaders { get; set; }
        public string version { get; set; }
    }
    public class ModLoader
    {
        public string id { get; set; }
        public bool primary { get; set; }
    }
    public class ModpackManifest
    {
        public string author { get; set; }
        public List<Fileino> files { get; set; }
        public string manifestType { get; set; }
        public int manifestVersion { get; set; }
        public Minecraft minecraft { get; set; }
        public string name { get; set; }
        public string overrides { get; set; }
        public int projectID { get; set; }
        public string version { get; set; }
    }

    public class Fileino
    {
        public int fileID { get; set; }
        public int projectID { get; set; }
        public bool required { get; set; }
    }

    public class data
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CategorySection CategorySection { get; set; }
        public Attachments[] Attachments { get; set; }
        public Authors[] Authors { get; set; }
        public string PrimaryCategoryAvatarUrl { get; set; }
        public string PrimaryCategoryName { get; set; }
        public List<Categories> Categories { get; set; }

        public string DownloadCount { get; set; }
        public int IsFeatured { get; set; }
        public double PopularityScore { get; set; }
        public string Summary { get; set; }
        public string webSiteURL { get; set; }
    }
    public static string defaultURL = "https://cursemeta.nikky.moe";

    public static async Task<List<ModpackVersions>> getVersions(int id)
    {
        var list = new List<ModpackVersions>();

        HttpClient client = new HttpClient();

        var response = await client.GetAsync(defaultURL + "/api/addon/" + id + "/files");

        var responseString = await response.Content.ReadAsStringAsync();

        dynamic x = JsonConvert.DeserializeObject(responseString);

        foreach (var y in x)
        {
            list.Add(new ModpackVersions
            {
                Name = y.fileNameOnDisk,
                URL = y.downloadURL,
                GameVersion = y.gameVersion[0]
            });
        }

        return list;
    }

    public static async Task<string> getDownloadURL(int projectId, int fileId)
    {
        var client = new WebClient();
        try
        {

            var response =
                await client.DownloadStringTaskAsync(defaultURL + "/api/addon/" + projectId +
                                                     "/files/" + fileId);
            dynamic x = JsonConvert.DeserializeObject(response);
            return x.downloadURL.ToString();
            /*if (x.dependencies != null || x.dependencies.Length != 0)
            {
                foreach (var loc in x.dependencies)
                {
                    list.AddRange(ResolveDependancies(loc.addOnId, x.gameVersion));
                }
            }*/

        }
        catch (Exception e)
        {
            Console.WriteLine("Could not download " + defaultURL + "/api/addon/" + projectId +
                              "/files/" + fileId + e.Message);
        }

        return null;
    }

    public static async Task<data> FetchModpackInfo(int id)
    {
        var list = new List<string>();
        var client = new WebClient();
        try
        {

            var response =
                await client.DownloadStringTaskAsync(defaultURL + "/api/addon/" + id);
            data x = JsonConvert.DeserializeObject<data>(response);
            return x;

        }
        catch (Exception e)
        {
            Console.WriteLine("Error downloading modpack info");
        }

        return null;
    }

    public static async Task<List<string>> ResolveDependancies(int projectId, string[] versions)
    {
        var list = new List<string>();
        var client = new WebClient();
        try
        {
            var response =
                await client.DownloadStringTaskAsync(defaultURL + "/api/addon/" + projectId + "/files");
            dynamic x = JsonConvert.DeserializeObject(response);
            foreach (var version in versions)
            {
                var found = false;
                foreach (var loc in x)
                {
                    var pos = Array.FindIndex(loc, version);
                    if (pos != -1)
                    {
                        list.Add(loc[pos].downloadURL);
                        if (loc[pos].dependencies != null || loc[pos].dependencies.Length != 0)
                        {
                            foreach (var loc1 in loc[pos].dependencies)
                            {
                                list.AddRange(ResolveDependancies(loc1, loc[pos].gameVersion));
                            }
                        }
                        found = true;
                        break;
                    }
                }
                if (found)
                    break;
            }
        }
        catch (Exception e)
        {
           
            Console.WriteLine("Could not download " + defaultURL + "/api/addon/" + projectId +
                              "/files");
        }

        return list;
    }
}