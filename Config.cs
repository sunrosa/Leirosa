public class Config
{
    public string Token {get; set;}
    public string SuggestionsPath {get; set;}
    public string ReportsPath {get; set;}
    public string VrchatPath {get; set;}
    public ulong VrchatRoleId {get; set;}
    public string DefaultGelbooruTags {get; set;}
    public string Prefix {get; set;}
    public bool EmbedColorFromUserAvatar {get; set;}
    public string Status {get; set;}
    public bool UseCustomStatus {get; set;}
    public string InviteURL {get; set;}
    public string SourceURL {get; set;}
    public string BotName {get; set;}
    public List<ulong> DeveloperIds {get; set;}
    public string DatetimeFormat {get; set;}
}
