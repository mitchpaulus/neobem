using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace src;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class Attribute
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("datatype")]
    public string Datatype { get; set; }
}

public class Attributes
{
    [JsonPropertyName("attribute")]
    public List<Attribute> Attribute { get; set; }
}

public class BclFileVersion
{
    [JsonPropertyName("software_program")]
    public string SoftwareProgram { get; set; }

    [JsonPropertyName("identifier")]
    public string Identifier { get; set; }
}

public class BclFile
{
    [JsonPropertyName("version")]
    public BclFileVersion Version { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; }

    [JsonPropertyName("filetype")]
    public string Filetype { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class Files
{
    [JsonPropertyName("file")]
    public List<BclFile> File { get; set; }
}

public class Tags
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
}

public class BclComponentJson
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }

    [JsonPropertyName("version_id")]
    public string VersionId { get; set; }

    [JsonPropertyName("vuuid")]
    public string Vuuid { get; set; }

    [JsonPropertyName("xml_checksum")]
    public string XmlChecksum { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("modeler_description")]
    public string ModelerDescription { get; set; }

    [JsonPropertyName("attributes")]
    public Attributes Attributes { get; set; }

    [JsonPropertyName("openstudio_version")]
    public string OpenstudioVersion { get; set; }

    [JsonPropertyName("files")]
    public Files Files { get; set; }

    [JsonPropertyName("org")]
    public string Org { get; set; }

    [JsonPropertyName("repo")]
    public string Repo { get; set; }

    [JsonPropertyName("release_tag")]
    public string ReleaseTag { get; set; }

    [JsonPropertyName("file_location")]
    public string FileLocation { get; set; }

    [JsonPropertyName("bundle")]
    public string Bundle { get; set; }

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; }

    [JsonPropertyName("repo_url")]
    public string RepoUrl { get; set; }

    [JsonPropertyName("tags")]
    public Tags Tags { get; set; }
}

