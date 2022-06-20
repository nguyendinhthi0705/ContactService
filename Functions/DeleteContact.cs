namespace ContactManagerAPI.Functions
{
    public class DeleteContact
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly IConfiguration _config;

        private Database _contactDatabase;
        private Container _contactContainer;

        public DeleteContact(
            ILogger<DeleteContact> logger,
            CosmosClient cosmosClient,
            IConfiguration config)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _config = config;

            _contactDatabase = _cosmosClient.GetDatabase(_config[Settings.DATABASE_NAME]);
            _contactContainer = _contactDatabase.GetContainer(_config[Settings.CONTAINER_NAME]);
        }

        [FunctionName(nameof(DeleteContact))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "contacts/{id}")] HttpRequest req,
            string id)
        {
            IActionResult returnValue = null;

            try
            {
                QueryDefinition getContactQueryDefinition = new QueryDefinition(
                    $"SELECT * FROM {_contactContainer.Id} c WHERE c.id = @id")
                    .WithParameter("@id", id);

                FeedIterator<Contact> getResultSet = _contactContainer.GetItemQueryIterator<Contact>
                    (
                        getContactQueryDefinition,
                        requestOptions: new QueryRequestOptions()
                        {
                            MaxItemCount = 1
                        }
                    );

                while (getResultSet.HasMoreResults)
                {
                    FeedResponse<Contact> response = await getResultSet.ReadNextAsync();
                    Contact contact = response.First();
                    ItemResponse<Contact> itemResponse = await _contactContainer.DeleteItemAsync<Contact>
                        (id: id, partitionKey: new PartitionKey(contact.ContactType));
                    returnValue = new OkResult();
                }

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
