using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;

namespace RepositoryLayer.Data
{
    public class CloudStorageClient
    {
        private readonly StorageClient _storageClient;

        public CloudStorageClient()
        {
            _storageClient = StorageClient.Create();
        }

        /// <summary>
        public StorageClient GetStorageClient() => _storageClient;
    }
}
