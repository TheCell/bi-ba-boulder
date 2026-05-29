namespace Thecell.Bibaboulder.Common.Appsettings;

public class EmailSettings
{
    public const string SectionName = "Email";
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required bool UseSsl { get; set; }
    public required bool UseStartTls { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string ContactAddress { get; set; }
    public required string FromAddress { get; set; }
    public required string FromName { get; set; }
}
