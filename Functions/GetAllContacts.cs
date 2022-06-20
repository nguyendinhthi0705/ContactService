using Microsoft.Azure.WebJobs;

namespace ContactManagerAPI.Functions
{
    public class GetAllContacts
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _config;

        private Database _contactDatabase;
        private Container _contactContainer;

        public GetAllContacts(
            ILogger<GetAllContacts> logger,
            CosmosClient cosmosClient,
            IConfiguration config)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _config = config;

            _contactDatabase = _cosmosClient.GetDatabase(_config[Settings.DATABASE_NAME]);
            _contactContainer = _contactDatabase.GetContainer(_config[Settings.CONTAINER_NAME]);
        }

        [FunctionName(nameof(GetAllContacts))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contacts")] HttpRequest req)
        {
            IActionResult returnValue = null;

            try
            {
                QueryDefinition query = new QueryDefinition($"SELECT * FROM {_contactContainer.Id} c");

                FeedIterator<Contact> resultSet = _contactContainer.GetItemQueryIterator<Contact>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        MaxItemCount = 10
                    });

                while (resultSet.HasMoreResults)
                {
                    _logger.LogInformation("Retreving all contacts");
                    FeedResponse<Contact> response = await resultSet.ReadNextAsync();
                    returnValue = new OkObjectResult(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not retrieve all contacts. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }
    }
}
