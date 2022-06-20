namespace ContactService.Models;

public class ContactName
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public ContactName()
    {

    }

    public ContactName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}

