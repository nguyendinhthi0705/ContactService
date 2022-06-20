namespace ContactManagerAPI.Functions
{
    public class GetContact
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _config;

        private Database _contactDatabase;
        private Container _contactContainer;

        public GetContact(
            ILogger<GetContact> logger,
            CosmosClient cosmosClient,
            IConfiguration config)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _config = config;

            _contactDatabase = _cosmosClient.GetDatabase(_config[Settings.DATABASE_NAME]);
            _contactContainer = _contactDatabase.GetContainer(_config[Settings.CONTAINER_NAME]);
        }

        [FunctionName(nameof(GetContact))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contacts/{id}")] HttpRequest req,
            string id)
        {
            IActionResult returnValue = null;

            try
            {
                QueryDefinition getContactQueryDefinition = new QueryDefinition(
                    $"SELECT * FROM {_contactContainer.Id} c WHERE c.id = @id")
                    .WithParameter("@id", id);

                ItemResponse<Contact> itemResponse = await _contactContainer.ReadItemAsync<Contact>(id, new PartitionKey(id));
                   
                returnValue = new OkObjectResult(itemResponse.Resource);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Couldn't delete contact. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }
    }
}
