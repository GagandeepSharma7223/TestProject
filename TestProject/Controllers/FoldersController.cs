using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using TestProject.App_Start;
using TestProject.Models;

namespace TestProject.Controllers
{
    [RoutePrefix("api/folders")]
    public class FoldersController : ApiController
    {
        string RootDirectory = ConfigurationManager.AppSettings.Get("RootDirectory");

        #region Get
        [HttpGet]
        public IHttpActionResult Get([FromUri] string rootDir = null, [FromUri] string searchText = null)
        {
            try
            {
                bool isRootNode = string.IsNullOrEmpty(rootDir);
                var list = new List<DirectoryDetailViewModel>();
                var rootDirPath = GetRootDirectoryPath();
                IEnumerable<string> folders;
                if (string.IsNullOrEmpty(searchText))
                {
                    folders = Directory.GetDirectories(isRootNode ? rootDirPath : rootDir).Select(d => d.ToLower());
                }
                else
                {
                    folders = Directory.GetDirectories(isRootNode ? rootDirPath : rootDir, "*.*", SearchOption.AllDirectories).Select(d => d.ToLower())
                        .Where(x => string.IsNullOrEmpty(searchText) || Path.GetFileName(x).Contains(searchText.ToLower()));
                }

                foreach (var folder in folders)
                {
                    var dirInfo = new DirectoryInfo(folder);
                    list.Add(new DirectoryDetailViewModel
                    {
                        DirectorySize = dirInfo.GetFiles().Sum(x => x.Length),
                        Path = dirInfo.FullName,
                        Name = dirInfo.Name
                    });
                }
                return Ok(list);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("GetFiles")]
        public IHttpActionResult GetFiles([FromUri] string rootDir = null)
        {
            try
            {
                var result = DirectorySearch(rootDir);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("SearchFile")]
        public IHttpActionResult SearchFile(string query)
        {
            try
            {
                var mappedPath = GetRootDirectoryPath();
                var dirDetailViewModel = new DirectoryDetailViewModel();
                var files = Directory.GetFiles(mappedPath, "*.*", SearchOption.AllDirectories).Select(f => f.ToLower())
                    .Where(f => Path.GetFileName(f).Contains(query.ToLower()));
                foreach (string name in files)
                {
                    var file = new FileInfo(name);
                    dirDetailViewModel.Files.Add(new FileDetailViewModel
                    {
                        FileName = file.Name,
                        FileSize = file.Length,
                        Path = file.FullName
                    });
                }
                return Ok(dirDetailViewModel);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private DirectoryDetailViewModel DirectorySearch(string dir)
        {
            var rootDirectory = GetRootDirectoryPath(dir);
            var dirInfo = new DirectoryInfo(rootDirectory);
            var dirDetailViewModel = new DirectoryDetailViewModel();
            // Directory doesn't exists
            if (!dirInfo.Exists)
            {
                dirDetailViewModel.ErrorMessage = "Directory does not exits.";
                return dirDetailViewModel;
            }

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                dirDetailViewModel.Files.Add(new FileDetailViewModel
                {
                    FileName = file.Name,
                    FileSize = file.Length,
                    Path = file.FullName
                });
            }

            dirDetailViewModel.DirectorySize = dirDetailViewModel.Files.Sum(x => x.FileSize);
            return dirDetailViewModel;
        }
        #endregion

        // Add File
        #region Upload File
        [HttpPost]
        [Route("Upload")]
        public IHttpActionResult Upload()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count > 0)
                {
                    string directoryPath = httpRequest.Form["DirectoryPath"];
                    var rootDirPath = GetRootDirectoryPath(directoryPath);
                    foreach (string fileName in httpRequest.Files.Keys)
                    {
                        var file = httpRequest.Files[fileName];
                        var filePath = Path.Combine(rootDirPath, file.FileName); //HttpContext.Current.Server.MapPath("~/" + rootDir + "/" + file.FileName);
                        file.SaveAs(filePath);
                    }

                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        // Download File
        #region Download
        [HttpPost]
        [Route("Download")]
        public HttpResponseMessage Download(string filePath)
        {
            try
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(File.ReadAllBytes(filePath));
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = Path.GetFileName(filePath);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                throw new HttpResponseException(errorResponse);
            }
        }
        #endregion

        // Add Directory
        #region Add Directory

        [HttpPost]
        [Route("AddDirectory")]
        public IHttpActionResult AddDirectory(AddFolderViewModel model)
        {
            try
            {
                model.RootDirectory = GetRootDirectoryPath(model.RootDirectory);
                var mappedPath = Path.Combine(model.RootDirectory, model.FolderName);
                var dirInfo = new DirectoryInfo(mappedPath);
                var response = new BaseResponseViewModel();
                if (dirInfo.Exists)
                {
                    response.ErrorMessage = "Directory already exits.";
                    return Ok(response);
                }

                //Create new Directory
                Directory.CreateDirectory(mappedPath);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        #endregion

        #region Private Methods
        private string GetRootDirectoryPath()
        {
            return System.Web.Hosting.HostingEnvironment.MapPath("~/" + RootDirectory);
        }

        private string GetRootDirectoryPath(string directoryPath)
        {
            var rootDirPath = GetRootDirectoryPath();
            return string.IsNullOrEmpty(directoryPath) ? rootDirPath : directoryPath;
        }
        #endregion

    }
}