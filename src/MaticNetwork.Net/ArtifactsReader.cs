using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace MaticNetwork.Net
{

  public class ArtifactsReader {
    public JObject jObject;

    // static ArtifactsReader()
    // {
    //     var thisAssembly = Assembly.GetExecutingAssembly();
    //     Console.WriteLine(string.Join("\n", thisAssembly.GetManifestResourceNames()));
    // }

    public ArtifactsReader(string filename) {
      var path = $"Matic.Net.ArtifactsJson.{filename}.json";
      this.jObject = this.ReadJson(path);
    }

    /// <summary>
    /// Read contents of an embedded resource file
    /// </summary>
    private string ReadResourceFile(string path)
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        var stream = thisAssembly.GetManifestResourceStream(path);
        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private JObject ReadJson(string path) {
      var text = this.ReadResourceFile(path);
      return JObject.Parse(text);
    }

    public string GetStringValue(string key) {
      if (this.jObject != null) {
        return this.jObject.GetValue(key).ToString();
      } else {
        return "";
      }
    }

    public string GetABI()
    {
      return this.GetStringValue("abi");
    }

  }
}
