using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;

namespace BlobSASDemo.Pages;


public class IndexModel : PageModel
{
    public List<string> LstBlobContents { get; set; }

    // requires using Microsoft.Extensions.Configuration;
    private readonly IConfiguration Configuration;

    public IndexModel(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void OnGet()
    {
        // Create a service level SAS that only allows reading from service level APIs
        AccountSasBuilder sas = new AccountSasBuilder
        {
            // Allow access to blobs
            Services = AccountSasServices.Blobs,

            // Allow access to the service level APIs
            ResourceTypes = AccountSasResourceTypes.All,

            // Access expires in 1 hour!
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };
        // Allow read access
        sas.SetPermissions(AccountSasPermissions.All);

        // Create a SharedKeyCredential that we can use to sign the SAS token
        string StorageAccountName = Configuration["K_StorageAccountName"];
        string StorageAccountKey = Configuration["K_StorageAccountKey"];
        StorageSharedKeyCredential credential = new StorageSharedKeyCredential(StorageAccountName, StorageAccountKey);

        // Build a SAS URI
        string StorageAccountBlobUri = Configuration["K_StorageAccountBlobUri"];
        UriBuilder sasUri = new UriBuilder(StorageAccountBlobUri);
        sasUri.Query = sas.ToSasQueryParameters(credential).ToString();

        // Create a client that can authenticate with the SAS URI
        string containerName = "data";
        BlobServiceClient service = new BlobServiceClient(sasUri.Uri);

        List<string> numberList = new List<string>();
        foreach (BlobItem blob in service.GetBlobContainerClient(containerName).GetBlobs())
        {
            BlobClient blobClient = service.GetBlobContainerClient(containerName).GetBlobClient(blob.Name);
            numberList.Add(blobClient.Uri.ToString());
        }
        LstBlobContents = numberList;
    }
}
