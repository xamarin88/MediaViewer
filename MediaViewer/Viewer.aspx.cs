using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Xml;
using System.Net;
using System.Xml.Linq;

namespace MediaViewer
{
    public partial class Viewer : System.Web.UI.Page
    {
        static string baseUri = "http://maps.googleapis.com/maps/api/" +
                          "geocode/xml?latlng={0},{1}&sensor=false";
        public static string latitude;
        public static string longitude;
        public static string address = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                ddlImages.DataSource = GetData("SELECT imageId, filePath as filename FROM BlobStorage WHERE fileExt = 'jpg'");
                ddlImages.DataTextField = "filename";
                ddlImages.DataValueField = "imageId";
                ddlImages.DataBind();

                ddlVideos.DataSource = GetData("SELECT imageId, filePath as filename FROM BlobStorage WHERE fileExt = 'mp4'");
                ddlVideos.DataTextField = "filename";
                ddlVideos.DataValueField = "imageId";
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
            lblImgLocation.Text = "";
            string id = ddlImages.SelectedItem.Value;
            DataTable dt = new DataTable();
            dt = GetData("SELECT filePath, senderLat, senderLong FROM BlobStorage WHERE imageId =" + id + " order by imageId");
            string path = dt.Rows[0]["filePath"].ToString();
            latitude = dt.Rows[0]["senderLat"].ToString();
            longitude = dt.Rows[0]["senderLong"].ToString();
           
            if (path != null)
            {
                imgView.Visible = true;
                string filename = path.Split('/').Last();
                CloudBlockBlob blob = GetBlobInContainer("images", filename);
                imgView.ImageUrl = blob.Uri.AbsoluteUri.ToString();

                if (latitude != null && longitude!= null)
                {
                    RetrieveFormatedAddress(latitude, longitude, ref address);
                    lblImgLocation.Text = address;
                }
                else
                {
                    lblImgLocation.Text = "No Location information available";
                }
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
            lblVideoLocation.Text = "";
            string id = ddlVideos.SelectedItem.Value;
            DataTable dt = new DataTable();
            dt = GetData("SELECT filePath, senderLat, senderLong FROM BlobStorage WHERE imageId =" + id + " order by imageId");
            string path = dt.Rows[0]["filePath"].ToString();
            latitude = dt.Rows[0]["senderLat"].ToString();
            longitude = dt.Rows[0]["senderLong"].ToString();

            if (path != null)
            {
                vdView.Visible = true;
                string filename = path.Split('/').Last();
                CloudBlockBlob blob = GetBlobInContainer("images", filename);
                vdView.Src = blob.Uri.AbsoluteUri.ToString();

                if (latitude != null && longitude != null)
                {
                    RetrieveFormatedAddress(latitude, longitude, ref address);
                    lblVideoLocation.Text = address;
                }
                else
                {
                    lblVideoLocation.Text = "No Location information available";
                }
            }
        }

        public static void RetrieveFormatedAddress(string lat, string lng, ref string address)
        {
            string requestUri = string.Format(baseUri, lat, lng);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(requestUri);

            XmlNodeList xNodelst = xDoc.GetElementsByTagName("result");
            XmlNode xNode = xNodelst.Item(0);
            if (xNode != null)
            { 
                address = xNode.SelectSingleNode("formatted_address").InnerText;
            }
            else
            {
                address = "ERROR: No address components located in XML.";
            }
        }

        //public static void RetrieveFormatedAddress2(string lat, string lng, ref string address)
        //{
        //    string requestUri = string.Format(baseUri, lat, lng);

        //    WebRequest request = WebRequest.Create(requestUri);
        //    WebResponse response = request.GetResponse();
        //    XDocument xdoc = XDocument.Load(response.GetResponseStream());

        //    XElement result = xdoc.Element("GeocodeResponse").Element("result");
        //    XElement locationElement = result.Element("geometry").Element("location");
        //}
    }
}