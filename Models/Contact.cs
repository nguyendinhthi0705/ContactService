namespace ContactService.Models;

public class Contact
{
    [JsonProperty("id")]
    public string ContactId { get; set; }
    public ContactName ContactName { get; set; }
    public ContactBirthday ContactBirthday { get; set; }
    public ContactAddress ContactAddress { get; set; }
    public ContactEmail ContactEmail { get; set; }
    public ContactPhone ContactPhone { get; set; }
    public string ContactType { get; set; }
}

