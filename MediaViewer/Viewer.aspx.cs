using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MediaViewer
{
    public partial class Viewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                ddlImages.DataSource = GetData("SELECT imageId, filePath as filename FROM BlobStorage WHERE fileExt = 'jpg'");
                ddlImages.DataTextField = "filename";
                ddlImages.DataValueField = "filename";
                ddlImages.DataBind();

                ddlVideos.DataSource = GetData("SELECT imageId, filePath as filename FROM BlobStorage WHERE fileExt = 'mp4'");
                ddlVideos.DataTextField = "filename";
                ddlVideos.DataValueField = "filename";
                ddlVideos.DataBind();
            }
        }

        private DataTable GetData(string query)
        {
            DataTable dt = new DataTable();
            
            string constr = ConfigurationManager.AppSettings["ConnectionString"];
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        cmd.CommandTimeout = 1000;
                        sda.SelectCommand = cmd;
                        sda.Fill(dt);
                    }
                }
                return dt;
            }
        }

        protected void FetchImage(object sender, EventArgs e)
        {
            string path = ddlImages.SelectedItem.Value;
            if (path != null)
            {
                imgView.Visible = true;
                string filename = path.Split('/').Last();
                CloudBlockBlob blob = GetBlobInContainer("images", filename);
                imgView.ImageUrl = blob.Uri.AbsoluteUri.ToString();
            }
        }

        CloudBlockBlob GetBlobInContainer(string container, string fileName)
        {
            //use web.config appSetting to get connection setting .NET Framework's ConfigurationManager class can also be used for this
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["BlobStorageConnectionString"]);
            //create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            //retrieve a refernce to a container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(ConfigurationManager.AppSettings["ImagesContainer"]);
            //set permission to show to public
            blobContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            blobContainer.CreateIfNotExists();
            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
            blockBlob.Properties.ContentType = "image/jpg";
            return blockBlob;
        }

        protected void FetchVideo(object sender, EventArgs e)
        {
            string path = ddlVideos.SelectedItem.Value;
            if (path != null)
            {
                vdView.Visible = true;
                string filename = path.Split('/').Last();
                CloudBlockBlob blob = GetBlobInContainer("images", filename);
                vdView.Src = blob.Uri.AbsoluteUri.ToString();
            }
        }
    }
}