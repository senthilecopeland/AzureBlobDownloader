using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobDownloader
{
    internal class BlobSettings
    {
       public string ConnectionString { get; set;}
       public string ContainerName { get; set; }
       public string DownloadPath { get; set; }
    }
}
