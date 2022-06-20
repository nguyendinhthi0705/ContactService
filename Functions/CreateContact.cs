namespace ContactManagerAPI.Functions
{
    public class CreateContact
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _config;

        private Database _contactDatabase;
        private Container _contactContainer;

        public CreateContact(
            ILogger<CreateContact> logger,
            CosmosClient cosmosClient,
            IConfiguration config)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _config = config;

            _contactDatabase = _cosmosClient.GetDatabase(_config[Settings.DATABASE_NAME]);
            _contactContainer = _contactDatabase.GetContainer(_config[Settings.CONTAINER_NAME]);

        }

        [FunctionName(nameof(CreateContact))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contacts")] HttpRequest req)
        {
            IActionResult returnValue = null;

            _logger.LogInformation("Creating a new contact");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var input = JsonConvert.DeserializeObject<Contact>(requestBody);

            var contact = new Contact
            {
                ContactId = Guid.NewGuid().ToString(),
                ContactName = new ContactName(input.ContactName.FirstName, input.ContactName.LastName),
                ContactBirthday = new ContactBirthday
                {
                    Birthday = input.ContactBirthday.Birthday
                },
                ContactAddress = new ContactAddress
                {
                    AddressLine1 = input.ContactAddress.AddressLine1,
                    AddressLine2 = input.ContactAddress.AddressLine2,
                    AddressCity = input.ContactAddress.AddressCity,
                    AddressState = input.ContactAddress.AddressState,
                    AddressZIPCode = input.ContactAddress.AddressZIPCode
                },
                ContactEmail = new ContactEmail
                {
                    Email = input.ContactEmail.Email
                },
                ContactPhone = new ContactPhone
                {
                    MobilePhone = input.ContactPhone.MobilePhone,
                    HomePhone = input.ContactPhone.HomePhone,
                    WorkPhone = input.ContactPhone.WorkPhone
                },
                ContactType = input.ContactType
            };

            try
            {
                ItemResponse<Contact> contactResponse = await _contactContainer.CreateItemAsync(
                    contact,
                    new PartitionKey(contact.ContactId));         
                returnValue = new OkObjectResult(contactResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Creating new contact failed. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new OkObjectResult(returnValue); ;
        }
    }
}
