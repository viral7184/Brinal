using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OFFICE_AUTOMATION_1.Controllers;
using OFFICE_AUTOMATION_1.Models;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.Helpers;
using PagedList;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Data;
using System.Web.Mvc.Html;
using System.Data.SqlClient;
using HtmlToOpenXml;
using GemBox.Document;
using GemBox.Document.Tables;
using HtmlAgilityPack;
using System.Xml;
using System.IO.Packaging;
using System.Globalization;
    

namespace OFFICE_AUTOMATION_1.Controllers
{
    public class HomeController : Controller
    {
     BrinalEntitiesdatabase1 db = new BrinalEntitiesdatabase1();
        public ActionResult Dashboard()
        {
            try
            {
                if (Session["email"] == null)
                {
                    return RedirectToAction("login");
                }
                else
                {
                    var filess = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                    ViewBag.Counts = filess.Count;

                    var bankcount = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(false)).Count();
                    ViewBag.bank = bankcount;

                    var agentcount = db.agent_master.Where(i => i.IS_DELETED.Value.Equals(false)).Count();
                    ViewBag.agent = agentcount;

                    var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                    ViewBag.villages = village.Count;

                    var deletefile = db.files.Where(i => i.IS_DELETED.Value.Equals(true)).ToList();
                    ViewBag.deletefiles = deletefile.Count;

                    var total = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                    ViewBag.c_file = total;

                    var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                    ViewBag.files = file;

                    ViewBag.Titles = filess.Where(i => i.T == "green").Count();
                    ViewBag.search = filess.Where(i => i.S == "blue").Count();
                    ViewBag.Query = filess.Where(i => i.Q == "blue").Count();
                    ViewBag.public_notice = filess.Where(i => i.PN == "blue").Count();
                    ViewBag.delivered = filess.Where(i => i.D == "green").Count();
                    ViewBag.OVOLD = filess.Where(i => i.OVLOD == "blue").Count();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return View();
        }
        public JsonResult GetAllUsers()
        {
            JsonResult result = new JsonResult();
            try
            {
                // Initialization.   
                string search = Request.Form.GetValues("search[value]")[0];
                string draw = Request.Form.GetValues("draw")[0];
                string order = Request.Form.GetValues("order[0][column]")[0];
                string orderDir = Request.Form.GetValues("order[0][dir]")[0];
                int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
                int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);
                // Loading.   
                List<file> data = db.files.ToList();
                // Total record count.   
                int totalRecords = data.Count;
                // Verification.   
                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    data = data.Where(p => p.R_NO.ToString().ToLower().Contains(search.ToLower()) ||
                        p.PROPERTY_DETAILS.ToLower().Contains(search.ToLower()) ||
                        p.agent_master.NAME.ToString().ToLower().Contains(search.ToLower()) ||
                        p.A_BLOCK_NO.ToLower().Contains(search.ToLower()) ||
                        p.A_REVENUE_SURVEY_NUM.ToLower().Contains(search.ToLower())
                     ).ToList();
                }
                // Sorting.   
                if (!(string.IsNullOrEmpty(order) && string.IsNullOrEmpty(orderDir)))
                {
                    data = data.OrderBy(I => I.R_NO).ToList();
                }
                int recFilter = data.Count;
                data = data.Skip(startRec).Take(pageSize).ToList();
                var modifiedData = data.Select(d =>
                    new
                    {
                        R_NO = d.R_NO,
                        PROPERTY_DETAILS = d.PROPERTY_DETAILS,
                        A_BLOCK_NO = d.A_BLOCK_NO,
                        A_ID = d.agent_master.NAME,
                        B_ID = d.bank_master.NAME
                    }
                    ).ToList();
                ViewBag.file = modifiedData;
                // Loading drop down lists.   
                result = this.Json(new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = modifiedData
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Info   
                Console.Write(ex);
            }
            // Return info.   
            return result;
        }
        public JsonResult getfile()
        {

            var file = db.files.Select(i => new
            {
                fileno = i.R_NO,
                bankname = i.bank_master.NAME,
                agentname = i.agent_master.NAME,
                property = i.PROPERTY_DETAILS,
                ablockno = i.A_BLOCK_NO,
                arevenueno = i.A_REVENUE_SURVEY_NUM,
                customername = i.CUSTOMERNAME,
                inwerdate = i.ENT_DATE,
                assigeto = i.employee_master.NAME,
                remark = i.REMARKS
            }).ToList();
            return Json(file, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Propertity_details()
        {
            ViewBag.propertitydetails = db.propertity_details.OrderByDescending(i => i.ID).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult Propertity_details(propertity_details data,FormCollection frm)
        {
            if (data.ID > 0)
            {
                var propertity = db.propertity_details.Where(i => i.ID == data.ID).SingleOrDefault();
                propertity.PROPERTITY_DETAIL = data.PROPERTITY_DETAIL;
                propertity.A_BLOCK_NO = data.A_BLOCK_NO;
                propertity.A_REVENUE_SURVEY_NUM = data.A_REVENUE_SURVEY_NUM;
                propertity.A_PLOT_FLAT_NUM = data.A_PLOT_FLAT_NUM;
                propertity.REASON = data.REASON;
                db.SaveChanges();              
            }
            else
            {
                var ablockno = Request.Form["A_BLOCK_NO"];
                var flatno = Request.Form["A_PLOT_FLAT_NUM"];
                var revenueno = Request.Form["A_REVENUE_SURVEY_NUM"];

                data.A_BLOCK_NO = ablockno;
                data.A_PLOT_FLAT_NUM = flatno;
                data.A_REVENUE_SURVEY_NUM = revenueno;
                db.propertity_details.Add(data);
                db.SaveChanges();
            }
            return RedirectToAction("Propertity_details");
        }
        public ActionResult Addfile()
        {
            try
            {
                var city = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.citys = city;

                var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                ViewBag.villages = village;

                var dist = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID == 0).ToList();
                ViewBag.District = dist;

                var bank = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.banks = bank;

                var agent = db.agent_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.agents = agent;

                var employee = db.employee_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.emp = employee;

                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.files = file;

                ViewBag.propertity = db.propertity_details.ToList();

                return View();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [HttpPost]
        public ActionResult Addfile(file data, notification noti_data,FormCollection frm)
        {
            try
            {                
                var nec = Request.Form["NEC"];
                var ablockno = Request.Form["A_BLOCK_NO"];
                var flatno = Request.Form["A_PLOT_FLAT_NUM"];
                var revenueno = Request.Form["A_REVENUE_SURVEY_NUM"];

                data.NEC = nec;
                data.A_BLOCK_NO = ablockno;
                data.A_PLOT_FLAT_NUM = flatno;
                data.A_REVENUE_SURVEY_NUM = revenueno;

                var checkrno = db.files.Where(i => i.R_NO == data.R_NO).FirstOrDefault();
                if(checkrno != null)
                {
                    TempData["r_no"] = "R NO is Already used";
                    return RedirectToAction("Addfile");
                }
                data.ENT_DATE = data.ENT_DATE;
                data.INWARD_DATE = data.ENT_DATE;
                data.IS_DELETED = false;
                if(data.A_BLOCK_NO != null || data.A_REVENUE_SURVEY_NUM != null || data.S_BLOCK_NO != null || data.S_REVENUE_SURVEY_NUM != null)
                { 
                     data.S = "blue";
                }
                else
                {
                    data.S = "gray";
                }
                data.S_DATE = DateTime.Now;
                data.Q = "gray";
                data.OVLOD = "gray";
                data.PN = "gray";
                data.D = "gray";
                data.T = "gray";
                //data.T_DATE = DateTime.Now;
                if (data.R_NO == null)
                {
                    var last = db.files.Where(i => i.R_NO == db.files.Max(j => j.R_NO)).FirstOrDefault();
                    if (last == null)
                    {
                        data.R_NO = 5301;
                    }
                    else
                    {
                        data.R_NO = last.R_NO + 1;
                    }
                }
              
                var search = db.files.Add(data);
                db.SaveChanges();

                var A_email = db.agent_master.Where(i => i.ID == data.A_ID).FirstOrDefault();
                var to = A_email.EMAIL;
                if(to == null)
                {
                    to = "";
                }
                var B_email = db.bank_master.Where(i => i.ID == data.B_ID).FirstOrDefault();
                var to1 = B_email.BANK_EMAIL;
                if (to1 == null)
                {
                    to1 = "";
                }
                var cc = B_email.CC;
                if (cc == null)
                {
                    cc = "";
                }
                var no = (data.APPLICATION_NO != null && data.APPLICATION_NO != "") ? data.APPLICATION_NO : data.R_NO.ToString();
                //var subject1 = "your file is inwerd" + " on file no:" + data.R_NO;
                //var subject2 = "Inward file(" + data.PROPERTY_DETAILS + ")( file no:"+ data.R_NO + ") Application / Prospect / File no.:" + no + " Party name: " + data.CUSTOMERNAME + " Property: " + data.PROPERTY_DETAILS + " File Number :" + data.R_NO;
                var subject = "Inward file(" + data.PROPERTY_DETAILS + ") ( file no: " + data.R_NO + ")";
                var msg = "Application / Prospect / File no.: " + data.R_NO + "<br /> " +
                                  "Party name :" + data.CUSTOMERNAME + "<br /> " +
                                  "Property :" + data.PROPERTY_DETAILS + " <br /> " +
                                  " File Number :<strong> "+data.R_NO+ " </strong> <br /> Brinal A. Bangdiwala(Advocate)";

                var confi = db.configurations.FirstOrDefault();
                var Rno = Convert.ToInt32(data.R_NO);
                sendmail(to, to1, cc, subject, msg,Rno, confi);

                var fileno = data.R_NO;
                var year = DateTime.Now.Year;              
                var path = confi.FOLDER_PATH;
                //var path = System.Configuration.ConfigurationManager.AppSettings["path"];
                var folderpath = Directory.CreateDirectory(Path.Combine(path + fileno + "-" + year));
                ViewBag.F_path = folderpath.FullName;
                if (data.ASSIGN_TO > 1)
                {
                    noti_data.EMP_ID = data.ASSIGN_TO;
                    noti_data.ENT_DATE = DateTime.Now;
                    noti_data.IS_DELETED = false;
                    noti_data.IS_READ = false;
                    noti_data.FILE_ID = data.ID;
                    noti_data.MESSAGE = "this file assign to you";
                    db.notifications.Add(noti_data);
                    db.SaveChanges();
                }
                return RedirectToAction("Files");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult Autocomplete(string term)
        {
            try
            {
                var result = new List<KeyValuePair<string, string>>();

                List<file> lists = new List<file>();
                foreach (var item in lists)
                {
                    result.Add(new KeyValuePair<string, string>(item.PROPERTY_DETAILS.ToString(), item.PROPERTY_DETAILS));
                }
                var result3 = result.Where(s => s.Value.ToLower().Contains
                          (term.ToLower())).Select(w => w).ToList();

                return Json(result3, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult GetDetail(int id)
        {
            file model = new file();
            return Json(model);
        }
        [HttpPost]
        public JsonResult getseggectionbox(string Prefix)
        {
            try
            {
                var Countries = (from c in db.files
                                 where c.PROPERTY_DETAILS.StartsWith(Prefix) || c.PROPERTY_DETAILS.Contains(Prefix)

                                 select new { c.PROPERTY_DETAILS, c.R_NO });
                return Json(Countries, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult Files(string id, string sortOrder, string searchString, string currentFilter, int? page, int pageSize = 10)
        {
            try

            {
                db.Configuration.ProxyCreationEnabled = true;
                ViewBag.CurrentSort = sortOrder;

                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
                ViewBag.FileSortParm = sortOrder == "FileNo" ? "FileNo_desc" : "FileNo";
                ViewBag.bankSortParm = sortOrder == "BankName" ? "BankName_desc" : "BankName";
                ViewBag.agentSortParm = sortOrder == "Agent" ? "AgentName_desc" : "Agent";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;

                var search = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).OrderBy(i => i.R_NO).ToList();
                ViewBag.files = search;

                if (!String.IsNullOrEmpty(searchString))
                {
                    DateTime dt;
                    if (DateTime.TryParse(searchString, out dt) == true)
                    {
                        var dat = Convert.ToDateTime(searchString);
                        dat =Convert.ToDateTime(dat.ToString("dd/MM/yyyy"));                    
                        search = db.files.Where(i => DateTime.Compare(i.ENT_DATE.Value, dat) == 0 || i.bank_master.NAME.Contains(searchString) || i.PROPERTY_DETAILS.Contains(searchString) || i.agent_master.NAME.Contains(searchString) || i.employee_master.NAME.Contains(searchString) || i.R_NO.ToString().Contains(searchString)).ToList();
                    }                        
                    else
                    search = db.files.Where(i => i.bank_master.NAME.Contains(searchString) || i.PROPERTY_DETAILS.Contains(searchString) || i.agent_master.NAME.Contains(searchString) || i.employee_master.NAME.Contains(searchString) || i.R_NO.ToString().Contains(searchString)).ToList();
                    ViewBag.files = search;
                }
                switch (sortOrder)
                {
                    case "BankName":
                        search = search.OrderBy(s => s.bank_master.NAME).ToList();
                        ViewBag.files = search;
                        break;
                    case "BankName_desc":
                        search = search.OrderByDescending(s => s.bank_master.NAME).ToList();
                        ViewBag.files = search;
                        break;
                    case "Agent":
                        search = search.OrderBy(s => s.agent_master.NAME).ToList();
                        ViewBag.files = search;
                        break;
                    case "AgentName_desc":
                        search = search.OrderByDescending(s => s.agent_master.NAME).ToList();
                        ViewBag.files = search;
                        break;
                    case "FileNo":
                        search = search.OrderBy(s => s.ID).ToList();
                        ViewBag.files = search;
                        break;
                    case "FileNo_desc":
                        search = search.OrderByDescending(s => s.ID).ToList();
                        ViewBag.files = search;
                        break;
                    default:
                        search = search.OrderBy(s => s.ID).OrderByDescending(i => i.R_NO).ToList();
                        break;
                }

                var files = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.R_NO).ToList();

                if (id == "Title")
                {
                    search = (from f in db.files
                              join fs in db.files on f.ID equals fs.ID
                              where fs.T == "green"
                              select f).Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.R_NO).ToList();
                    ViewBag.files = files;
                }
                else if (id == "Search")
                {
                    search = (from f in db.files
                              join fs in db.files on f.ID equals fs.ID
                              where fs.S == "blue"
                              select f).Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.R_NO).ToList();
                    ViewBag.files = files;
                }
                else if (id == "Query")
                {
                    search = (from f in db.files
                              join fs in db.files on f.ID equals fs.ID
                              where fs.Q == "blue"
                              select f).Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.R_NO).ToList();
                    ViewBag.files = files;
                }
                else if (id == "OVOLD")
                {
                    search = (from f in db.files
                              join fs in db.files on f.ID equals fs.ID
                              where fs.OVLOD == "blue"
                              select f).Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.R_NO).ToList();
                    ViewBag.files = files;
                }
                else if (id == "PN")
                {
                    search = (from f in db.files
                              join fs in db.files on f.ID equals fs.ID
                              where fs.PN == "blue"
                              select f).Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.R_NO).ToList();
                    ViewBag.files = files;
                }
                else if (id == "Delivered")
                {
                    search = (from f in db.files
                              join fs in db.files on f.ID equals fs.ID
                              where fs.D == "green"
                              select f).Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.R_NO).ToList();
                    ViewBag.files = files;
                }
                var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                ViewBag.villages = village;
                var bank = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.banks = bank;
                var agent = db.agent_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.agents = agent;
                var employee = db.employee_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.emp = employee;
                int pageNumber = (page ?? 1);               
                ViewBag.strindx = (pageNumber - 1) * (pageSize) + 1;
                ViewBag.f = search;
                return View();
                //return View(search.ToPagedList(pageNumber, pageSize));
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void ownerinformatipn()
        {
            try
            {
                var info = db.INFORMATION.Where(i => i.ID == 1).FirstOrDefault();
                Session["ownername"] = info.OWNER_NAME;
                Session["study"] = info.STUDY;
                Session["peofession"] = info.PROFESSION;
                Session["address"] = info.ADDRESS;
                Session["telephoneno"] = info.TELEPHONE_NO;
                Session["mobileno"] = info.MOBILE_NO;
                Session["pancardno"] = info.PANCARD_NO;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult CreateFileFromHTML(string html, string fileName)
        {
            var filepath = SaveDOCX(fileName, html, true);
            return Json(filepath);
        }
        private static string SaveDOCX(string fileName, string BodyText, bool IncludeHTML)
        {
            try
            {
                string WordprocessingML = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";


                // create a new package (Open XML document) ...
                Package pkgOutputDoc = null;

                //pkgOutputDoc = Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
                pkgOutputDoc = Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite);

                // create the start part, set up the nested structure ...
                XmlDocument xmlStartPart = new XmlDocument();
                XmlElement tagDocument = xmlStartPart.CreateElement("w:document", WordprocessingML);
                xmlStartPart.AppendChild(tagDocument);
                XmlElement tagBody = xmlStartPart.CreateElement("w:body", WordprocessingML);
                tagDocument.AppendChild(tagBody);
                if (IncludeHTML)
                {
                    string relationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

                    XmlElement tagAltChunk = xmlStartPart.CreateElement("w:altChunk", WordprocessingML);
                    XmlAttribute RelID = tagAltChunk.Attributes.Append(xmlStartPart.CreateAttribute("r:id", relationshipNamespace));
                    RelID.Value = "rId2";
                    tagBody.AppendChild(tagAltChunk);
                }
                else
                {
                    XmlElement tagParagraph = xmlStartPart.CreateElement("w:p", WordprocessingML);
                    tagBody.AppendChild(tagParagraph);
                    XmlElement tagRun = xmlStartPart.CreateElement("w:r", WordprocessingML);
                    tagParagraph.AppendChild(tagRun);
                    XmlElement tagText = xmlStartPart.CreateElement("w:t", WordprocessingML);
                    tagRun.AppendChild(tagText);

                    XmlNode nodeText = xmlStartPart.CreateNode(XmlNodeType.Text, "w:t", WordprocessingML);
                    nodeText.Value = BodyText;
                    tagText.AppendChild(nodeText);
                }
                Uri docuri = new Uri("/word/document.xml", UriKind.Relative);
                //docuri.AbsolutePath.ToString("D:/OFFAUTO");
                PackagePart docpartDocumentXML = pkgOutputDoc.CreatePart(docuri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml");
                StreamWriter streamStartPart = new StreamWriter(docpartDocumentXML.GetStream(FileMode.Create, FileAccess.Write));
                xmlStartPart.Save(streamStartPart);
                streamStartPart.Close();
                pkgOutputDoc.Flush();

                // create the relationship part
                pkgOutputDoc.CreateRelationship(docuri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId1");
                pkgOutputDoc.Flush();

                //this part was taken from the HTMLtoWordML project on CodeProject, authored by Paulo Vaz, which borrowed work from Praveen Bonakurthi
                //original project location for Paulo's project is http://www.codeproject.com/KB/aspnet/HTMLtoWordML.aspx, and it contains a link to the article
                //by Praveen Bonakurthi. I didn't want to have a resident template on the server, so it led to this project

                Uri uriBase = new Uri("/word/document.xml", UriKind.Relative);
                PackagePart partDocumentXML = pkgOutputDoc.GetPart(uriBase);

                Uri uri = new Uri("/word/websiteinput.html", UriKind.Relative);

                //creating the html file from the output of the webform
                string html = string.Concat("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><html><head><title></title></head><body>", BodyText, "</body></html>");
                byte[] Origem = Encoding.UTF8.GetBytes(html);
                PackagePart altChunkpart = pkgOutputDoc.CreatePart(uri, "text/html");
                using (Stream targetStream = altChunkpart.GetStream())
                {
                    targetStream.Write(Origem, 0, Origem.Length);
                }
                Uri relativeAltUri = PackUriHelper.GetRelativeUri(uriBase, uri);

                //create the relationship in the final file
                partDocumentXML.CreateRelationship(relativeAltUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/aFChunk", "rId2");

                //close the document ...
                pkgOutputDoc.Close();
                string sourceFile = System.Configuration.ConfigurationManager.AppSettings["sourchpath"] + fileName;
                string destinationFile = System.Configuration.ConfigurationManager.AppSettings["destpath"] + fileName;
                string backup = System.Configuration.ConfigurationManager.AppSettings["destpath"] +"bckp.docx";
                System.IO.File.Move(sourceFile, destinationFile);
                //System.IO.File.Replace(sourceFile, destinationFile, backup);
                return destinationFile;
            }
            catch (Exception e)
            {
                if (e.Message == "The file exists.\r\n")
                {
                    string path = System.Configuration.ConfigurationManager.AppSettings["destpath"] + fileName;
                    string msg = e.Message + "\n Please check path: " + path;
                    return msg;
                }
                else
                {
                    return e.Message;
                }
                
            }
            // use the Open XML namespace for WordprocessingML:           
        }

        public ActionResult billcreate()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 4)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public ActionResult bill()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 6)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult billformate()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 2)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult createbill4()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 18)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult createbill()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 10)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult getbillcreate()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 14)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult bill1()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 9)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public ActionResult bill2()
        {
            try
            {
               int s= islogin();  
                if(s==0)
                {
                    return RedirectToAction("Login");
                }

                //var formate_id = Convert.ToInt16(Session["formatid"]);
                //if (formate_id == 20)
                //{
                if (Session["bank"] == null)
                {
                    return RedirectToAction("Bank");
                }
                var bankname = Session["bank"].ToString();
                var date1 = Convert.ToDateTime(Session["date1"]);
                var date2 = Convert.ToDateTime(Session["date2"]);
                var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).OrderBy(i => i.R_NO).ToList();
                ViewBag.files = count;
                //    return View();
                //}
                //else
                //{
                //    return RedirectToAction("Bank");
                //}
            }
            catch (Exception e)
            {
                throw e;
            }
            return View();
        }
        public ActionResult bill3()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 8)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult bill4()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 3)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public string ConvertNumbertoWords(int number)
        {
            try
            {
                if (number == 0) return "ZERO";
                if (number < 0) return "minus " + ConvertNumbertoWords(Math.Abs(number));
                string words = "";
                if ((number / 1000000) > 0)
                {
                    words += ConvertNumbertoWords(number / 100000) + " LAKES ";
                    number %= 1000000;
                }
                if ((number / 1000) > 0)
                {
                    words += ConvertNumbertoWords(number / 1000) + " THOUSAND ";
                    number %= 1000;
                }
                if ((number / 100) > 0)
                {
                    words += ConvertNumbertoWords(number / 100) + " HUNDRED ";
                    number %= 100;
                }
                //if ((number / 10) > 0)  
                //{  
                // words += ConvertNumbertoWords(number / 10) + " RUPEES ";  
                // number %= 10;  
                //}  
                if (number > 0)
                {
                    if (words != "") words += "AND ";
                    var unitsMap = new[]
                    {
            "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"
        };
                    var tensMap = new[]
                    {
            "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"
        };
                    if (number < 20) words += unitsMap[number];
                    else
                    {
                        words += tensMap[number / 10];
                        if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                    }
                }
                return words;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult getbill()
        {
            try
            {
                var formate_id = Convert.ToInt16(Session["formatid"]);
                if (formate_id == 1)
                {
                    var bankname = Session["bank"].ToString();
                    var date1 = Convert.ToDateTime(Session["date1"]);
                    var date2 = Convert.ToDateTime(Session["date2"]);
                    var count = db.files.Where(i => i.bank_master.NAME == bankname.ToString() && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                    ViewBag.files = count;
                    return View();
                }
                else
                {
                    return RedirectToAction("Bank");
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult editfile(int id)
        {
            try
            {
                var city = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.citys = city;

                var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                ViewBag.villages = village;

                var dist = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID == 0).ToList();
                ViewBag.District = dist;

                var bank = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.banks = bank;

                var agent = db.agent_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.agents = agent;


                var employee = db.employee_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.emp = employee;

                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.files = file;

                var files = db.files.Where(i => i.ID == id).SingleOrDefault();
                return View(files);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        [HttpPost]
        public ActionResult editfile(file data, notification noti_data)
        {
            try
            {
                var filess = db.files.Where(i => i.ID == data.ID).SingleOrDefault();
                if (filess != null)
                {
                    if (data.UPDATE_DATE != null)
                    {
                        filess.UPDATE_DATE = DateTime.Now;
                    }
                    if (data.R_NO > 0)
                    {
                        filess.R_NO = data.R_NO;
                    }
                    if (data.A_ID > 0)
                    {
                        filess.A_ID = data.A_ID;
                    }
                    if (data.B_ID > 0)
                    {
                        filess.B_ID = data.B_ID;
                    }
                    if (data.CUSTOMERNAME != null)
                    {
                        filess.CUSTOMERNAME = data.CUSTOMERNAME;
                    }                   
                       filess.CERTIFIED_COPY = data.CERTIFIED_COPY;                  
                    if (data.APPLICATION_NO != null)
                    {
                        filess.APPLICATION_NO = data.APPLICATION_NO;
                    }
                    if (data.PROPERTY_DETAILS != null)
                    {
                        filess.PROPERTY_DETAILS = data.PROPERTY_DETAILS;
                    }
                    if (data.ENT_DATE != null)
                    {
                        filess.ENT_DATE = data.ENT_DATE;
                    }
                    if (data.DELIVERY_DATE != null)
                    {
                        filess.DELIVERY_DATE = data.DELIVERY_DATE;
                        filess.D = "green";
                        filess.D_DATE = DateTime.Now;
                    }         
                        filess.A_BLOCK_NO = data.A_BLOCK_NO;                  
                        filess.A_REVENUE_SURVEY_NUM = data.A_REVENUE_SURVEY_NUM;                   
                        filess.A_PLOT_FLAT_NUM = data.A_PLOT_FLAT_NUM;                 
                        filess.S_BLOCK_NO = data.S_BLOCK_NO;                    
                        filess.S_REVENUE_SURVEY_NUM = data.S_REVENUE_SURVEY_NUM;                   
                        filess.S_PLOT_FLAT_NUM = data.S_PLOT_FLAT_NUM;
                    if (data.A_BLOCK_NO != null || data.A_REVENUE_SURVEY_NUM != null || data.S_BLOCK_NO != null || data.S_REVENUE_SURVEY_NUM != null)
                    {
                        filess.S = "blue";
                        filess.S_DATE = DateTime.Now;
                    }
                    else
                    {
                        filess.S = "gray";                     
                    }
                    if (data.DISTRICT != null)
                    {
                        filess.DISTRICT = data.DISTRICT;
                    }
                    if (data.SUBDIST != null)
                    {
                        filess.SUBDIST = data.SUBDIST;
                    }
                    if (data.VILLAGE != null)
                    {
                        filess.VILLAGE = data.VILLAGE;
                    }
                    if (data.NO_OF_YEAR != null)
                    {
                        filess.NO_OF_YEAR = data.NO_OF_YEAR;
                    }
                    if (data.NEC != null)
                    {
                        filess.NEC = data.NEC;
                    }
                    if (data.ASSIGN_TO > 0)
                    {
                        filess.ASSIGN_TO = data.ASSIGN_TO;
                    }      
                    if (data.SEARCH_FEES > 0)
                    {
                        filess.SEARCH_FEES = data.SEARCH_FEES;
                    }
                    if (data.LEGAL_FEES > 0)
                    {
                        filess.LEGAL_FEES = data.LEGAL_FEES;
                    }
                    if (data.IS_PAID != null)
                    {
                        filess.IS_PAID = data.IS_PAID;
                    }
                    filess.REMARKS = data.REMARKS;
                    filess.STATUS_REMARKS = data.STATUS_REMARKS;
                    filess.PAID_REMARKS = data.PAID_REMARKS;                  
                    filess.UPDATE_DATE = DateTime.Now;
                    db.SaveChanges();
                    if (data.ASSIGN_TO > 1)
                    {
                        noti_data.EMP_ID = data.ASSIGN_TO;
                        noti_data.ENT_DATE = DateTime.Now;
                        noti_data.IS_DELETED = false;
                        noti_data.IS_READ = false;
                        noti_data.FILE_ID = data.ID;
                        noti_data.MESSAGE = "this file assign to you";
                        db.notifications.Add(noti_data);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("FileSingle/" + filess.R_NO);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public JsonResult CountNoFile(string BankName, DateTime date1, DateTime date2)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var files = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.bank_master.NAME.Equals(BankName) && i.SEARCH_FEES != null && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                var ispaid = files.Where(i => i.IS_PAID == true).Count();
                var unpaid = files.Where(i => i.IS_PAID == false).Count();
                var count = files.Count;

                string js = ispaid + "," + unpaid + "," + count;
                //var jsn= {"ISPaid": ispaid, "UnPaid": unpaid,"Total":count};
                //ViewBag.count = count;                
                return Json(new { data = js }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
 
        public JsonResult deliverfile(int id)
        {
            try
            {
               // db.Configuration.ProxyCreationEnabled = false;
                var files = db.files.Where(i => i.ID == id).SingleOrDefault();
                if (files.D == "green")
                {
                    files.D = "gray";
                }
                else if (files.D == "gray")
                {
                    files.D = "green";
                    files.D_DATE = DateTime.Now;
                    files.DELIVERY_DATE = DateTime.Now;
                    var to = files.agent_master.EMAIL;
                    if (to == null)
                    {
                        to = "";
                    }
                    var to1 = files.bank_master.BANK_EMAIL;
                    if (to1 == null)
                    {
                        to1 = "";
                    }
                    var cc = files.bank_master.CC;
                    if (cc == null)
                    {
                        cc = "";
                    }
                    var confi = db.configurations.FirstOrDefault();
                    var no = (files.APPLICATION_NO != null && files.APPLICATION_NO != "") ? files.APPLICATION_NO : files.R_NO.ToString();
                    var subject = "Delivered Report(" + files.PROPERTY_DETAILS + ")(File no: " + files.R_NO + ")"; 
                   
                    var msg = "Application / Prospect / File no.: " + no + "<br /> " +
                                  "Party name :" + files.CUSTOMERNAME + "<br /> " +
                                  "Property :" + files.PROPERTY_DETAILS + " <br /> " +
                                  "Message: Legal Report has been Delivered.<br />"
                                  + "<br /> For the security reason title file(pdf) will be sent in separete email.<br />  " +
                                   "<br /> Brinal A. Bangdiwala(Advocate)";

                    var Rno =Convert.ToInt32(files.R_NO);
                    sendmail(to, to1, cc, subject, msg, Rno, confi);
                   // Delivermail(to,to1,cc,confi,files);
                }
                db.SaveChanges();

                var datafile = db.files.Where(i => i.ID == id).Select(i => new
                {
                    id = i.ID,
                    d = i.D
                }).ToList();


                return Json(new { data = datafile }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public ActionResult Bank()
        {
            try
            {
                var bank = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.ID).ToList();
                ViewBag.banks = bank;
            }
            catch (Exception e)
            {

                throw e;
            }

            return View();
        }
        [HttpPost]
        public ActionResult Bank(bank_master data , FormCollection frm)
        {
            try
            {
                if (data.ID > 0)
                {
                    var banks = db.bank_master.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (banks != null)
                    {
                        banks.UPDATE_DATE = DateTime.Now;
                        if (data.NAME != null)
                        {
                            banks.NAME = data.NAME;
                        }
                        if(data.BANK_EMAIL != null)
                        {
                            banks.BANK_EMAIL = data.BANK_EMAIL;
                        }
                        if(data.CC != null)
                        {
                            banks.CC = data.CC;
                        }
                        if (data.PARTICULAR != null)
                        {
                            banks.PARTICULAR = data.PARTICULAR.Trim();
                        }
                        if (data.ADDRESS != null)
                        {
                            banks.ADDRESS = data.ADDRESS.Trim();
                        }
                        if (data.MOBILE != null)
                        {
                            banks.MOBILE = data.MOBILE;
                        }
                        if (data.NO_OF_YEAR != null)
                        {
                            banks.NO_OF_YEAR = data.NO_OF_YEAR;
                        }
                        if (data.NEC != null)
                        {
                            banks.NEC = data.NEC;
                        }
                        if (data.FEES >= 0)
                        {
                            banks.FEES = data.FEES;
                        }
                        if (data.LEGAL_FEES >= 0)
                        {
                            banks.LEGAL_FEES = data.LEGAL_FEES;
                        }
                        banks.BILL_FORMATE_ID = data.BILL_FORMATE_ID;
                        db.SaveChanges();
                    }
                }
                else
                {
                    var CC = Request.Form["cc"];
                    bool ab = false;
                    data.IS_DELETED = ab;
                    data.CC = CC;
                    if (data.PARTICULAR == null)
                    {
                        data.PARTICULAR = "-";
                    }
                    if (data.ADDRESS == null)
                    {
                        data.ADDRESS = "-";
                    }
                    data.ENT_DATE = DateTime.Now;
                    db.bank_master.Add(data);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("Bank");
        }

        public ActionResult billdetailslist()
        {
            var billlist = db.BILLs.OrderByDescending(i => i.Id).ToList();
            ViewBag.billlist = billlist;
            return View();
        }

        public ActionResult Bankdetails(int id)
        {
            try
            {
                var bankdetails = db.bank_master.Where(i => i.ID == id).FirstOrDefault();
                return View(bankdetails);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        [HttpPost]
        public ActionResult Bankdetails(bank_master data, FormCollection frm)
        {
            try
            {
                ownerinformatipn();
                var bank_details = db.bank_master.Where(i => i.ID == data.ID).FirstOrDefault();
                var formatId = bank_details.BILL_FORMATE_ID;

                var date1 = Convert.ToDateTime(Request.Form["DATE1"]);
                var date2 = Convert.ToDateTime(Request.Form["DATE2"]);
                Session["date1"] = date1;
                Session["date2"] = date2;

                var id = bank_details.NAME;
                Session["formatid"] = formatId;
                Session["bank"] = id;
                Session["bankname"] = id;

                if (formatId == 0 && formatId == null)
                {
                    return RedirectToAction("Bank");
                }
                DateTime? date = DateTime.Now;
                var month = date.Value.ToString("MMMM");
                var month1 = date.Value.Month;
                Session["month"] = month;
                var d3 = DateTime.Now.Year.ToString();
                Session["year"] = d3;
                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
                ViewBag.files = file;
                if (file.Count == 0)
                {
                    TempData["error"] = "No File in Bank";
                    var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
                    return RedirectToAction("Bankdetails/" + bankid.ID);
                }
                Session["bank"] = id;
                Session["total"] = db.files.Where(i => i.SEARCH_FEES != null && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).Sum(x => x.SEARCH_FEES);
                Session["bankname"] = id;
                string wordoftotal = db.files.Where(i => i.SEARCH_FEES != null && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).Sum(x => x.SEARCH_FEES).ToString();
                if (wordoftotal != null && wordoftotal != "")
                {
                    var ruppies = ConvertNumbertoWords(Convert.ToInt16(wordoftotal));
                    Session["wordoftotal"] = ruppies;
                }
                else
                {
                    Session["wordoftotal"] = null;
                }
                var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
                if (bank != null)
                {
                    BILL bills = new BILL();
                    bills.B_ID = bank.ID;
                    var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).FirstOrDefault();
                    if (billno == null)
                    {
                        bills.BILL_NO = 1;
                    }
                    else
                    {
                        bills.BILL_NO = billno.BILL_NO + 1;
                    }
                    //bills.BILL_NO = billno.BILL_NO + 1;
                    bills.TO_DATE = date1;
                    bills.END_DATE = date2;                       
                    bills.date = DateTime.Now;
                    db.BILLs.Add(bills);
                    db.SaveChanges();
                    Session["billno"] = bills.BILL_NO;
                }
                return RedirectToAction("bill2");
            }
            //    if (formatId == 1)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if(file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/"+bankid.ID);
            //        }                   
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if(billno == null)
            //            {
            //                bills.BILL_NO = 1;                            
            //            }
            //            else
            //            { 
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("getbill");
            //    }
            //    else if (formatId == 10)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }                    
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }

            //        return RedirectToAction("createbill");
            //    }
            //    else if (formatId == 18)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }
            //        Session["record"] = file.Count;
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("createbill4");
            //    }
            //    else if (formatId == 8)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }
            //        Session["record"] = file.Count;            
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("bill3");
            //    }
            //    else if (formatId == 3)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }               
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("bill4");
            //    }
            //    else if (formatId == 14)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }
            //        Session["bank"] = id;
            //        var total = db.files.Where(i => i.SEARCH_FEES != null && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).Sum(x => x.SEARCH_FEES);
            //        Session["total"] = total;
            //        Session["bankname"] = id;
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("getbillcreate");
            //    }
            //    else if (formatId == 2)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }         
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("billformate");
            //    }
            //    else if (formatId == 6)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }                 
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("bill");
            //    }
            //    else if (formatId == 9)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }
            //        Session["bank"] = id;
            //        Session["total"] = db.files.Where(i => i.SEARCH_FEES != null && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).Sum(x => x.SEARCH_FEES);
            //        Session["bankname"] = id;
            //        string wordoftotal = db.files.Where(i => i.SEARCH_FEES != null && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).Sum(x => x.SEARCH_FEES).ToString();

            //        if (wordoftotal != null && wordoftotal != "")
            //        {
            //            var ruppies = ConvertNumbertoWords(Convert.ToInt16(wordoftotal));
            //            Session["wordoftotal"] = ruppies;
            //        }
            //        else
            //        {
            //            Session["wordoftotal"] = null;
            //        }
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("bill1");
            //    }
            //    else if (formatId == 20)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }
            //        Session["bank"] = id;
            //        Session["total"] = db.files.Where(i => i.SEARCH_FEES != null && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).Sum(x => x.SEARCH_FEES);
            //        Session["bankname"] = id;
            //        string wordoftotal = db.files.Where(i => i.SEARCH_FEES != null && i.IS_DELETED == false && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).Sum(x => x.SEARCH_FEES).ToString();
            //        if (wordoftotal != null && wordoftotal != "")
            //        {
            //            var ruppies = ConvertNumbertoWords(Convert.ToInt16(wordoftotal));
            //            Session["wordoftotal"] = ruppies;
            //        }
            //        else
            //        {
            //            Session["wordoftotal"] = null;
            //        }
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("bill2");
            //    }
            //    else if (formatId == 4)
            //    {
            //        var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_PAID.Value.Equals(false) && i.bank_master.NAME.Equals(id) && i.ENT_DATE >= date1 && i.ENT_DATE <= date2).ToList();
            //        ViewBag.files = file;
            //        if (file.Count == 0)
            //        {
            //            TempData["error"] = "No File in Bank";
            //            var bankid = db.bank_master.Where(i => i.NAME == id).FirstOrDefault();
            //            return RedirectToAction("Bankdetails/" + bankid.ID);
            //        }
            //        var bank = db.bank_master.Where(i => i.NAME == id && i.IS_DELETED == false).FirstOrDefault();
            //        if (bank != null)
            //        {
            //            BILL bills = new BILL();
            //            bills.B_ID = bank.ID;
            //            var billno = db.BILLs.Where(i => i.Id == db.BILLs.Max(j => j.Id)).SingleOrDefault();
            //            if (billno == null)
            //            {
            //                bills.BILL_NO = 1;
            //            }
            //            else
            //            {
            //                bills.BILL_NO = billno.BILL_NO + 1;
            //            }
            //            bills.BILL_NO = billno.BILL_NO + 1;
            //            bills.date = DateTime.Now;
            //            db.BILLs.Add(bills);
            //            db.SaveChanges();
            //            Session["billno"] = bills.BILL_NO;
            //        }
            //        return RedirectToAction("billcreate");
            //    }
            //    else
            //    {
            //        TempData["msg"] = "Please Select Format of Bill";
            //        return RedirectToAction("Bank");
            //    }
            //}
            catch (Exception e)
            {
                throw e;
            }
            return View();
        }
        public ActionResult Ownerinformation()
        {
            try
            {
                var information = db.INFORMATION.FirstOrDefault();
                ViewBag.information = information;
                return View(information);
            }
            catch (Exception e)
            {
                throw e;
            }            
        }
        [HttpPost]
        public ActionResult Ownerinformation(INFORMATION data)
        {
            try
            {
                var information = db.INFORMATION.Where(i => i.ID == data.ID).SingleOrDefault();
                information.OWNER_NAME = data.OWNER_NAME;
                information.PROFESSION = data.PROFESSION;
                information.STUDY = data.STUDY;
                information.ADDRESS = data.ADDRESS;
                information.TELEPHONE_NO = data.TELEPHONE_NO;
                information.MOBILE_NO = data.MOBILE_NO;
                information.PANCARD_NO = data.PANCARD_NO;
                information.ENTRY_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("Ownerinformation");
        }
        public JsonResult fileloction(int id)
        {
            var confi = db.configurations.FirstOrDefault();
            var path = confi.FOLDER_PATH;
            var data = db.files.Where(i => i.ID == id).FirstOrDefault();
            var fileno = data.R_NO;
            var year = DateTime.Now.Year;
            var folderpath = path + fileno + "-" + year;
           
            return Json(new { data = folderpath }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Configuration()
        {
            var config = db.configurations.FirstOrDefault();
            ViewBag.configration = config;
            return View(config);
        }
        [HttpPost]
        public ActionResult Configuration(configuration data)
        {
            try
            {
                var configurations = db.configurations.Where(i => i.Id == data.Id).SingleOrDefault();
                configurations.FOLDER_PATH = data.FOLDER_PATH;
                configurations.FROM_EMAIL = data.FROM_EMAIL;
                configurations.EM_PASS = data.EM_PASS;
                configurations.HOST = data.HOST;
                configurations.PORT = data.PORT;
                configurations.SENDER = data.SENDER;
                configurations.SMS_AUTH_KEY = data.SMS_AUTH_KEY;
                configurations.SEND_SMS_URI = data.SEND_SMS_URI;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("Configuration");
        }
        public ActionResult Agent()
        {
            try
            {
                var agent = db.agent_master.Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.ID).ToList();
                ViewBag.agents = agent;

                var bks = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.banks = bks;

            }
            catch (Exception e)
            {

                throw e;
            }

            return View();
        }
        [HttpPost]
        public ActionResult Agent(agent_master data)
        {
            try
            {
                if (data.ID > 0)
                {
                    var agents = db.agent_master.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (agents != null)
                    {
                        agents.UPDATE_DATE = DateTime.Now;
                        if (data.NAME != null)
                        {
                            agents.NAME = data.NAME.Trim();
                        }
                        if (data.ADDRESS != null)
                        {
                            agents.ADDRESS = data.ADDRESS.Trim();
                        }
                        if (data.MOBILE1 != null)
                        {
                            agents.MOBILE1 = data.MOBILE1.Trim();
                        }
                        if (data.MOBILE2 != null)
                        {
                            agents.MOBILE2 = data.MOBILE2.Trim();
                        }                    
                          agents.EMAIL = data.EMAIL;
                       
                        db.SaveChanges();
                    }
                }
                else
                {
                    bool ab = false;
                    data.IS_DELETED = ab;
                    data.ENT_DATE = DateTime.Now;
                    db.agent_master.Add(data);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("Agent");
        }
        public ActionResult agentdeatails(int id)
        {
            try
            {
                var agent = db.agent_master.Where(i => i.ID == id).FirstOrDefault();
                return View(agent);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public ActionResult forgotpasswrd(FormCollection frm)
        {
            try
            {
                var email = Request.Form["EMAIL"];
                if (email != null)
                {
                    var user = db.employee_master.Where(i => i.EMAIL == email).FirstOrDefault();
                    if (user != null)
                    {
                        var confi = db.configurations.FirstOrDefault();
                        var passtext = user.PASSWORD;
                        Session["pass"] = passtext;
                        var checkmail = sendpassword(email, passtext,confi);
                        Session["check_email"] = checkmail;
                    }
                    else
                    {
                        Session["check_email"] = "Entered email is not matched detabase";
                    }
                }               
            }
            catch (Exception e)
            {

                throw e;
            }

            return RedirectToAction("login");
        }
        public string changepassword(string email, string password, employee_master data, FormCollection frm)
        {
            try
            {
                //var email = Request.Form["EMAIL"];
                //var pass = Request.Form["PASSWORD"];

                var employee = db.employee_master.Where(i => i.EMAIL == email).FirstOrDefault();
                if (employee != null)
                {
                    employee.PASSWORD = password;
                    db.SaveChanges();
                    return "Your password has been successfully changed!";
                }
                return "Please check inserted data.";
            }
            catch (Exception e)
            {

                return "Something went wrong! Please try again.";
            }
        }
        public ActionResult Employee()
        {
            try
            {
                var employee = db.employee_master.Where(i => i.IS_DELETED == false).OrderByDescending(i => i.ID).ToList();
                ViewBag.emp = employee;
            }
            catch (Exception e)
            {
                throw e;
            }

            return View();
        }
        [HttpPost]
        public ActionResult Employee(employee_master data, FormCollection frm, HttpPostedFileBase[] files)
        {

            try
            {
                if (data.FILE_NAME != null)
                {
                    Models.employee_master imgs = new employee_master();
                    imgs.files = data.files;
                    HttpFileCollectionBase file = Request.Files;
                    for (int i = 0; i < file.Count; i++)
                    {
                        HttpPostedFileBase Image = file[i];
                        var filename = "";
                        if (Image != null)
                        {
                            filename = Path.GetFileName(Image.FileName);
                            var path = Path.Combine(Server.MapPath("~/Images"), filename);
                            Image.SaveAs(path);
                            data.FILE_NAME = filename;
                            //db.public_notice.Add(data);
                            db.SaveChanges();
                        }
                    }
                }
                var proof = Request.Form["PROOF"];
                if (data.ID > 0)
                {
                    var employees = db.employee_master.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (employees != null)
                    {

                        employees.UPDATE_DATE = DateTime.Now;
                        if (data.NAME != null)
                        {
                            employees.NAME = data.NAME.Trim();
                        }

                        if (data.ADDRESS1 != null)
                        {
                            employees.ADDRESS1 = data.ADDRESS1.Trim();
                        }
                        if (data.ADDRESS2 != null)
                        {
                            employees.ADDRESS2 = data.ADDRESS2.Trim();
                        }
                        if (data.MOBILE1 != null)
                        {
                            employees.MOBILE1 = data.MOBILE1.Trim();
                        }
                        if (data.MOBILE2 != null)
                        {
                            employees.MOBILE2 = data.MOBILE2.Trim();
                        }
                        if (data.EMAIL != null)
                        {
                            employees.EMAIL = data.EMAIL.Trim();
                        }
                        if (data.ID_PROOF != null)
                        {
                            employees.ID_PROOF = proof + "-" + data.ID_PROOF;
                        }
                        employees.IS_ACTIVE = data.IS_ACTIVE;
                        if (data.FILE_NAME != null)
                        {
                            employees.FILE_NAME = data.FILE_NAME;
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    bool ab = false;
                    data.IS_DELETED = ab;
                    data.ID_PROOF = proof + "-" + data.ID_PROOF;
                    data.ENT_DATE = DateTime.Now;
                    db.employee_master.Add(data);
                    db.SaveChanges();

                    var emp_roll = db.emp_roll_master.Where(i => i.EMP_ID == data.ID).SingleOrDefault();

                    if (emp_roll != null)
                    {

                    }
                    else
                    {
                        var employee_role = new emp_roll_master();
                        if (employee_role != null)
                        {
                            employee_role.EMP_ID = data.ID;
                            employee_role.EMP_ROLE = "Employee";
                            employee_role.ENT_DATE = DateTime.Now;
                            db.emp_roll_master.Add(employee_role);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("Employee");
        }
        public ActionResult employeedetails(int id)
        {
            var emp = db.employee_master.Where(i => i.ID == id).FirstOrDefault();
            return View(emp);
        }
        public ActionResult ManageCity()
        {
            try
            {
                var manage = db.city_master.OrderByDescending(i => i.ID).ToList();
                ViewBag.managecity = manage;

                var city = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.citys = city;

                var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                ViewBag.villages = village;

                var dist = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID == 0).ToList();
                ViewBag.District = dist;

                var cities = (from g1 in db.city_master
                              join g2 in db.city_master on g1.SUB_DIST_ID equals g2.ID
                              join g3 in db.city_master on g1.DIST_ID equals g3.ID
                              select new Villages
                              {
                                  Id = g1.ID,
                                  Village = g1.VILLAGE,
                                  SubDist = g2.VILLAGE,
                                  Dist = g3.VILLAGE,
                                  SUB_DIST_ID = g2.ID,
                                  DIST_ID = g3.ID

                              }).ToList();
                ViewBag.cities = cities;
            }
            catch (Exception e)
            {
                throw e;
            }

            return View();
        }
        [HttpPost]
        public ActionResult ManageCity(city_master data)
        {
            try
            {
                if (data.ID > 0)
                {
                    var managecity = db.city_master.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (managecity != null)
                    {
                        if (data.VILLAGE != null)
                        {
                            managecity.VILLAGE = data.VILLAGE;
                        }
                        if (data.SUB_DIST_ID > 0)
                        {
                            managecity.SUB_DIST_ID = data.SUB_DIST_ID;
                        }
                        if (data.DIST_ID > 0)
                        {
                            managecity.DIST_ID = data.DIST_ID;
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    db.city_master.Add(data);
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("ManageCity");
        }
        public ActionResult employee_role()
        {
            try
            {
                var employee = db.employee_master.ToList();
                ViewBag.emp = employee;

                var employeess = db.emp_roll_master.OrderByDescending(i => i.ID).ToList();
                ViewBag.employees = employeess;

            }
            catch (Exception e)
            {

                throw e;
            }

            return View();
        }
        [HttpPost]
        public ActionResult employee_role(emp_roll_master data)
        {
            try
            {
                if (data.ID > 0)
                {
                    var emp_roll_master = db.emp_roll_master.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (emp_roll_master != null)
                    {
                        emp_roll_master.UPDATE_DATE = DateTime.Now;
                        if (data.EMP_ID > 0)
                        {
                            emp_roll_master.EMP_ID = data.EMP_ID;
                        }
                        if (data != null)
                        {
                            emp_roll_master.EMP_ROLE = data.EMP_ROLE;
                        }
                        db.SaveChanges();
                    }

                }
                else
                {
                    data.ENT_DATE = DateTime.Now;
                    db.emp_roll_master.Add(data);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return RedirectToAction("employee_role");
        }
        public ActionResult Communication()
        {
            try
            {
                var s_year = 2017;
                var currentyear = DateTime.Now.Year;
                var diff_year = currentyear - s_year;
                
                

                var agent = db.agent_master.ToList();
                ViewBag.agents = agent;

                var communication = db.communications.OrderByDescending(i => i.ID).ToList();
                ViewBag.communications = communication;
            }
            catch (Exception e)
            {

                throw e;
            }

            return View();
        }
        [HttpPost]
        public ActionResult Communication(communication data)
        {

            try
            {
                if (data.ID > 0)
                {
                    var communication = db.communications.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (communication != null)
                    {
                        communication.UPDATE_DATE = DateTime.Now;
                        if (data.A_ID > 0)
                        {
                            communication.A_ID = data.A_ID;
                        }

                        communication.SMS = data.SMS;

                        if (data.SMS_CONTENT != null)
                        {
                            communication.SMS_CONTENT = data.SMS_CONTENT.Trim();
                        }
                        if (data.EMAIL_CONTENT != null)
                        {
                            communication.EMAIL_CONTENT = data.EMAIL_CONTENT.Trim();
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    bool ab = false;
                    data.IS_DELETED = ab;
                    data.ENT_DATE = DateTime.Now;
                    db.communications.Add(data);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return RedirectToAction("Communication");
        }
        public ActionResult Notifications()
        {
            try
            {
                if (Session["emp_id"] != null && Session["emp_id"].ToString() != "")
                {
                    int noti_id = Convert.ToInt32(Session["emp_id"]);
                    var noti = db.notifications.Where(i => i.EMP_ID == noti_id && i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.ID).ToList();
                    ViewBag.noti = noti;
                    noti.ForEach(i => i.IS_READ = true);
                    db.SaveChanges();

                    Session["noti"] = null;

                }
                else
                {
                    var notification = db.notifications.Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.ID).ToList();
                    ViewBag.noti = notification;
                }
                var employee = db.employee_master.ToList();
                ViewBag.emp = employee;
            }
            catch (Exception e)
            {
                throw e;
            }


            return View();
        }
        [HttpPost]
        public ActionResult Notifications(notification data)
        {

            try
            {
                if (data.ID > 0)
                {
                    var notification = db.notifications.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (notification != null)
                    {
                        if (data.MESSAGE != null)
                        {
                            notification.MESSAGE = data.MESSAGE.Trim();
                        }
                        if (data.EMP_ID > 0)
                        {
                            notification.EMP_ID = data.EMP_ID;
                        }
                        db.SaveChanges();
                    }

                }
                else
                {
                    data.IS_DELETED = false;
                    data.IS_READ = false;
                    data.ENT_DATE = DateTime.Now;
                    db.notifications.Add(data);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {

                throw;
            }


            return RedirectToAction("Notifications");
        }
        public ActionResult PublicNotice(int? id)
        {
            try
            {
                var date1 = DateTime.Now;
                var public_notification = db.public_notice.Where(i => i.REMINDER_DATE >= i.REMINDER_DATE && i.IS_DELETED == false && i.IS_READ == false).ToList();
                if (public_notification != null)
                {
                    Session["public_notification"] = public_notification.Count();
                }
                else
                {
                    Session["public_notification"] = "";
                }
                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.PN == "gray").ToList();
                ViewBag.files = file;
                if (id > 0)
                {
                    var date = DateTime.Now;
                    var pu_notice = db.public_notice.Where(i => i.REMINDER_DATE >= i.REMINDER_DATE && i.IS_DELETED.Value.Equals(false) && i.IS_READ == false).OrderByDescending(i => i.ID).ToList();
                    ViewBag.public_notice = pu_notice;
                }
                else
                {
                    var pu_notice = db.public_notice.Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.ID).ToList();
                    ViewBag.public_notice = pu_notice;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return View();
        }
        [HttpPost]
        public ActionResult PublicNotice(public_notice data, FormCollection frm, HttpPostedFileBase[] files)
        {
            try
            {
                var day = Request.Form["DAYS"];

                var f_no = db.files.Where(i => i.ID == data.FILE_ID).FirstOrDefault();
                var fileno = f_no.R_NO;
                var year = DateTime.Now.Year;
                if (data.IMAGE_PATH != null)
                {
                    Models.public_notice imgs = new public_notice();
                    imgs.file = data.file;
                    HttpFileCollectionBase file = Request.Files;
                    for (int i = 0; i < file.Count; i++)
                    {
                        HttpPostedFileBase Image = file[i];
                        var filename = "";
                        if (Image != null)
                        {
                            filename = Path.GetFileName(Image.FileName);
                            var path3 = Path.Combine(Server.MapPath("~/Images"), filename);
                            Image.SaveAs(path3);
                            var confi = db.configurations.FirstOrDefault();
                            var path = confi.FOLDER_PATH;
                            //var path = System.Configuration.ConfigurationManager.AppSettings["path"];
                            var path1 = Path.Combine((path + fileno + "-" + year), filename);
                            if (!System.IO.Directory.Exists(path1))
                            {
                                var path2 = confi.FOLDER_PATH;
                                //var path2 = System.Configuration.ConfigurationManager.AppSettings["path"];
                                Directory.CreateDirectory(Path.Combine(path2 + fileno + "-" + year));
                            }
                            Image.SaveAs(path1);
                            data.IMAGE_PATH = filename;
                            db.SaveChanges();
                        }
                    }
                }
                if (data.ID > 0)
                {
                    var publics = db.public_notice.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (publics != null)
                    {

                        if (data.ISSUED_DATE != null)
                        {
                            publics.ISSUED_DATE = data.ISSUED_DATE;
                        }
                        if (data.ISSUED_DATE != null)
                        {
                            if (day != null && day != "")
                            {
                                Int16 addedday = Convert.ToInt16(day);
                                publics.REMINDER_DATE = data.ISSUED_DATE.Value.AddDays(addedday);
                            }
                        }
                        if (data.IMAGE_PATH != null)
                        {
                            publics.IMAGE_PATH = data.IMAGE_PATH;
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    bool ab = false;
                    data.IS_DELETED = ab;
                    data.ENT_DATE = DateTime.Now;
                    if (day != null && day != "")
                    {
                        Int32 addedday = Convert.ToInt32(day);
                        data.REMINDER_DATE = data.ISSUED_DATE.Value.AddDays(addedday);
                    }
                    data.IS_READ = false;
                    db.public_notice.Add(data);
                    db.SaveChanges();
                    var f = db.files.Where(i => i.ID == data.FILE_ID).SingleOrDefault();
                    f.PN = "blue";
                    f.PN_DATE = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("PublicNotice");
        }
        public ActionResult FileSingle(int id)
        {
            try
            {
                var Files = db.files.Where(i => i.R_NO == id).FirstOrDefault();

                Session["green"] = Files.T;
                Session["filename"] = Files.FILE_NAME;

                if (Files != null)
                {
                    ViewBag.deli_date = Files.DELIVERY_DATE;
                    ViewBag.stsclr = Files;

                    var ovlod = db.queries.Where(i => i.FILE_ID == Files.ID).FirstOrDefault();
                    if (ovlod != null)
                    {
                        Session["queryid"] = ovlod;
                        if (ovlod.OV != null)
                        {
                            Session["ov"] = "OV";
                        }
                        else if (ovlod.LOD != null)
                        {
                            Session["lod"] = "LOD";
                        }
                    }
                }
                var date = DateTime.Now;
                var public_notification = db.public_notice.Where(i => i.REMINDER_DATE >= i.REMINDER_DATE && i.IS_DELETED.Value.Equals(false) && i.IS_READ.Value.Equals(false)).ToList();
                if (public_notification != null)
                {
                    Session["public_notification"] = public_notification.Count();
                }
                else
                {
                    Session["public_notification"] = null;
                }
                var selectquery = db.queries.Where(i => i.FILE_ID == Files.ID).FirstOrDefault();
                ViewBag.getquery = selectquery;

                var search = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.searchdate = search;

                var employee = db.employee_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.emp = employee;

                return View(Files);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        [HttpPost]
        public ActionResult FileSingle(file data, notification noti_data)
        {
            try
            {
                if (data.A_ID > 0)
                {
                    var filess = db.files.Where(i => i.ID == data.ID).SingleOrDefault();

                    filess.UPDATE_DATE = DateTime.Now;
                    filess.A_ID = data.A_ID;
                    filess.B_ID = data.B_ID;
                    filess.CUSTOMERNAME = data.CUSTOMERNAME;
                    filess.INWARD_DATE = data.INWARD_DATE;
                    filess.DELIVERY_DATE = data.DELIVERY_DATE;
                    filess.REMARKS = data.REMARKS;
                    filess.A_BLOCK_NO = data.A_BLOCK_NO;
                    filess.A_REVENUE_SURVEY_NUM = data.A_REVENUE_SURVEY_NUM;
                    filess.A_PLOT_FLAT_NUM = data.A_PLOT_FLAT_NUM;
                    filess.S_BLOCK_NO = data.S_BLOCK_NO;
                    filess.S_REVENUE_SURVEY_NUM = data.S_REVENUE_SURVEY_NUM;
                    filess.S_PLOT_FLAT_NUM = data.S_PLOT_FLAT_NUM;
                    filess.VILLAGE = data.VILLAGE;
                    filess.NO_OF_YEAR = data.NO_OF_YEAR;
                    filess.ASSIGN_TO = data.ASSIGN_TO;

                    filess.STATUS_REMARKS = data.STATUS_REMARKS;
                    filess.SEARCH_FEES = data.SEARCH_FEES;
                    filess.IS_PAID = data.IS_PAID;
                    filess.PAID_REMARKS = data.PAID_REMARKS;
                    filess.IS_PN_APPLICABLE = data.IS_PN_APPLICABLE;
                    db.SaveChanges();

                    if (filess.ASSIGN_TO > 0)
                    {
                        noti_data.EMP_ID = data.ASSIGN_TO;
                        noti_data.ENT_DATE = DateTime.Now;
                        noti_data.IS_DELETED = false;
                        noti_data.IS_READ = false;
                        noti_data.MESSAGE = "this file assign you";
                        db.notifications.Add(noti_data);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("FileSingle");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public ActionResult searching(FormCollection frm)
        {
            try
            {
                var dts = Convert.ToDateTime(Request.Form["DATE"]);
                var date = DateTime.Now.Date;
                var day1 = DateTime.Now.Date.AddDays(1);
                var day2 = DateTime.Now.Date.AddDays(-1);
                var serch = new List<file>();
                if (Request.Form["DATE"] == null)
                {
                    serch = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && i.ENT_DATE.Value == date && (i.A_BLOCK_NO != null || i.S_BLOCK_NO != null || i.A_REVENUE_SURVEY_NUM != null|| i.S_REVENUE_SURVEY_NUM != null)).OrderBy(i => i.R_NO).ToList();
                    ViewBag.files = serch;
                }
                else
                {
                    serch = db.files.Where(i => i.IS_DELETED.Value.Equals(false) && DateTime.Compare(i.ENT_DATE.Value, dts) == 0 && (i.A_BLOCK_NO != null || i.S_BLOCK_NO != null || i.A_REVENUE_SURVEY_NUM != null || i.S_REVENUE_SURVEY_NUM != null)).OrderBy(i => i.R_NO).ToList();
                    ViewBag.files = serch;
                }
                List<file> cm = new List<file>();
                List<file> cm1 = new List<file>();
                List<file> cm2 = new List<file>();


                if (frm.GetValues("DISTRICT") != null)
                {
                    int[] b = Array.ConvertAll(frm.GetValues("DISTRICT"), p => Convert.ToInt32(p));
                    foreach (var B in b)
                    {
                        foreach (var dt in serch)
                        {
                            if (dt.DISTRICT == B)
                            {
                                cm.Add(dt);
                            }
                        }
                    }
                    serch = cm;
                }
                if (frm.GetValues("SUBDIST") != null)
                {
                    int[] c = Array.ConvertAll(frm.GetValues("SUBDIST"), p => Convert.ToInt32(p));
                    foreach (var C in c)
                    {
                        foreach (var dt in serch)
                        {
                            if (dt.SUBDIST == C)
                            {
                                cm1.Add(dt);
                            }
                        }
                    }
                    serch = cm1;
                }
                if (frm.GetValues("VILLAGE") != null)
                {
                    int[] a = Array.ConvertAll(frm.GetValues("VILLAGE"), p => Convert.ToInt32(p));
                    foreach (var A in a)
                    {
                        foreach (var dt in serch)
                        {
                            if (dt.VILLAGE == A)
                            {
                                cm2.Add(dt);
                            }
                        }
                    }
                    serch = cm2;
                }
                ViewBag.files = serch;

                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).OrderByDescending(i => i.ID).ToList();
                ViewBag.filess = file;

                var city = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.citys = city;

                var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                ViewBag.villages = village;

                var sub_dist1 = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.SubDist = sub_dist1;

                var dist1 = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID == 0).ToList();
                ViewBag.District = dist1;

                var emp = db.employee_master.ToList();
                ViewBag.employee = emp;

                return View();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public ActionResult editsearch(int id)
        {
            try
            {

                var serch = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.serching = serch;

                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.files = file;

                var city = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.citys = city;

                var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                ViewBag.villages = village;

                var sub_dist = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.SubDist = sub_dist;

                var dist = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID == 0).ToList();
                ViewBag.District = dist;

                var emp = db.employee_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.employee = emp;

                if (id == 0)
                {
                    return View();
                }
                else
                {
                    var search = db.files.Where(i => i.ID == id).SingleOrDefault();
                    return View(search);
                }
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        [HttpPost]
        public ActionResult editsearch(file data)
        {
            try
            {
                var filess = db.files.Where(i => i.ID == data.ID).SingleOrDefault();
                if (filess != null)
                {
                    filess.UPDATE_DATE = DateTime.Now;
                    if (data.ENT_DATE != null)
                    {
                        filess.ENT_DATE = data.ENT_DATE;
                    }
                    if (data.A_ID > 0)
                    {
                        filess.A_ID = data.A_ID;
                    }
                    if (data.B_ID > 0)
                    {
                        filess.B_ID = data.B_ID;
                    }
                    if (data.CUSTOMERNAME != null)
                    {
                        filess.CUSTOMERNAME = data.CUSTOMERNAME;
                    }
                    if (data.INWARD_DATE != null)
                    {
                        filess.INWARD_DATE = data.INWARD_DATE;
                    }
                    if (data.DELIVERY_DATE != null)
                    {
                        filess.DELIVERY_DATE = data.DELIVERY_DATE;
                    }                   
                        filess.REMARKS = data.REMARKS;
                        filess.A_BLOCK_NO = data.A_BLOCK_NO;
                        filess.A_REVENUE_SURVEY_NUM = data.A_REVENUE_SURVEY_NUM; 
                        filess.A_PLOT_FLAT_NUM = data.A_PLOT_FLAT_NUM;                   
                        filess.S_BLOCK_NO = data.S_BLOCK_NO;                 
                        filess.S_REVENUE_SURVEY_NUM = data.S_REVENUE_SURVEY_NUM;                  
                        filess.S_PLOT_FLAT_NUM = data.S_PLOT_FLAT_NUM;                   
                    if (data.VILLAGE != null)
                    {
                        filess.VILLAGE = data.VILLAGE;
                    }
                    if (data.SUBDIST != null)
                    {
                        filess.SUBDIST = data.SUBDIST;
                    }
                    if (data.NO_OF_YEAR != null)
                    {
                        filess.NO_OF_YEAR = data.NO_OF_YEAR;
                    }
                    if (data.ASSIGN_TO > 0)
                    {
                        filess.ASSIGN_TO = data.ASSIGN_TO;
                    }                   
                        //filess.STATUS_REMARKS = data.STATUS_REMARKS;
                   
                    if (data.SEARCH_FEES > 0)
                    {
                        filess.SEARCH_FEES = data.SEARCH_FEES;
                    }
                        data.IS_PAID = data.IS_PAID;                
                        //filess.PAID_REMARKS = data.PAID_REMARKS.Trim();                 

                    if (data.RECEIVER != null)
                    {
                        filess.RECEIVER = data.RECEIVER;
                    }
                    filess.UPDATE_DATE = DateTime.Now;

                    db.SaveChanges();
                }
                return RedirectToAction("searching");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public ActionResult query(int? id)
        {
            try
            {
                if (id > 0)
                {
                    var querys = db.queries.Where(i => i.FILE_ID == id && i.IS_DELETED.Value.Equals(false)).ToList();
                    ViewBag.queryes = querys;

                    var status = db.queries.Where(i => i.FILE_ID == id && i.IS_DELETED.Value.Equals(false) && i.IS_RESOLVE == false).ToList();
                    var filestatus = db.files.Where(i => i.ID == id).FirstOrDefault();
                   // filestatus.Q_DATE = DateTime.Now;
                    if (status.Count == 0)
                    {
                        filestatus.Q = "green";
                    }
                    else
                    {
                        filestatus.Q = "blue";
                    }
                    db.SaveChanges();
                }
                else
                {
                    var querys = db.queries.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_RESOLVE == false).OrderByDescending(i => i.ID).ToList();
                    ViewBag.queryes = querys;
                }      
                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.files = file;
                var agent = db.agent_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.agents = agent;
                var bank = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(false)).ToList();
                ViewBag.bank = bank;
            }
            catch (Exception e)
            {
                throw e;
            }
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult query(query data, communication comm_data, FormCollection frm)
        {
            try
            {
                var TO = Request.Form["TO"];
                if (TO == null)
                {
                    TO = "";
                }
                var CC = Request.Form["CC"];
                if (CC == null)
                {
                    CC = "";
                }
                var BCC = Request.Form["BCC"];
                if (BCC == null)
                {
                    BCC = "";
                }
                var mobile_no = Request.Form["MOBILE_NO"];
                var Q = (frm["EMAIL_CONTENT"]);
                if (data.ID > 0)
                {
                    var querys = db.queries.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (querys != null)
                    {
                        querys.UPDATE_DATE = DateTime.Now;
                        if (data.QUERY_REMARK != null)
                        {
                            querys.QUERY_REMARK = data.QUERY_REMARK.Trim();
                        }
                        querys.IS_RESOLVE = data.IS_RESOLVE;
                        if (data.IS_RESOLVE == true)
                        {
                            var querys1 = db.queries.Where(i => i.ID == data.ID).SingleOrDefault();
                            var f_id = Convert.ToInt16(querys.FILE_ID);
                            var filedata1 = db.files.Where(i => i.ID == f_id).FirstOrDefault();
                            var R_no = Convert.ToInt32(filedata1.R_NO);
                            var agent = db.agent_master.Where(i => i.ID == querys1.SENT_TO).FirstOrDefault();
                            var agentname = agent.NAME;
                            var confi = db.configurations.FirstOrDefault();
                            //if (mobile_no!="" || mobile_no != null)
                            //{
                            //    sendmsg(mobile_no, data.SMS_CONTENT,confi);
                            //}     
                            //var subject = "Query resolved" + " on file no " + R_no;
                            //sendmail(TO, CC, BCC,subject, data.EMAIL_CONTENT, R_no, confi);
                        }
                        db.SaveChanges();
                    }
                    var status = db.queries.Where(i => i.FILE_ID == querys.FILE_ID && i.IS_DELETED.Value.Equals(false) && i.IS_RESOLVE == false).ToList();
                    var filestatus = db.files.Where(i => i.ID == querys.FILE_ID).FirstOrDefault();
                    filestatus.Q_DATE = DateTime.Now;
                    if (status.Count == 0)
                    {
                        filestatus.Q = "green";
                    }
                    else
                    {
                        filestatus.Q = "blue";
                    }
                    db.SaveChanges();
                }
                else
                {
                    var sentemail = db.agent_master.Where(i => i.ID == data.SENT_TO).FirstOrDefault();
                    var filedata = db.files.Where(i => i.ID == data.FILE_ID).FirstOrDefault();
                    var R_no = Convert.ToInt32(filedata.R_NO);
                    var confi = db.configurations.FirstOrDefault();
                    if (sentemail != null)
                    {
                        var no = (filedata.APPLICATION_NO != null && filedata.APPLICATION_NO != "") ? filedata.APPLICATION_NO : filedata.R_NO.ToString();
                        if (mobile_no != "" || mobile_no != null)
                        {
                            sendmsg(mobile_no, data.SMS_CONTENT,confi);
                        }
                        var msg = "Application / Prospect / File no.: " + no + "<br /> " +
                                  "Party name :" + filedata.CUSTOMERNAME + "<br /> " +
                                  "Property :" + filedata.PROPERTY_DETAILS + " <br /> " +
                                  "Query :<br /> " + data.EMAIL_CONTENT + "<br /> Brinal A. Bangdiwala(Advocate)";
                       
                        var subject = "Query of(" + filedata.PROPERTY_DETAILS + ")( file no:" + filedata.R_NO + ")";

                        sendmail(TO, CC, BCC,subject, msg, R_no, confi);

                        comm_data.A_ID = data.SENT_TO;
                        comm_data.F_ID = data.FILE_ID;
                        comm_data.SMS_CONTENT = data.SMS_CONTENT;
                        comm_data.EMAIL_CONTENT = data.EMAIL_CONTENT;
                        comm_data.TO = TO;
                        comm_data.CC = CC;
                        comm_data.BCC = BCC;
                        comm_data.SUBJECT = "Query of(" + filedata.PROPERTY_DETAILS + ")( file no: " + filedata.R_NO + ")Application / Prospect / File no. " + no + ": Party name :" + filedata.CUSTOMERNAME + ", Property: " + filedata.PROPERTY_DETAILS + " Query: " + data.QUERY_REMARK;
                        comm_data.BODY = msg;
                        comm_data.MOBILE_NO = mobile_no;
                        comm_data.ATTACHMENT = comm_data.ATTACHMENT;
                        comm_data.ENT_DATE = DateTime.Now;
                        db.communications.Add(comm_data);
                        db.SaveChanges();
                    }
                    var queryfile = db.files.Where(i => i.ID == data.FILE_ID).SingleOrDefault();
                    if (queryfile != null)
                    {
                        queryfile.Q = "blue";
                        queryfile.Q_DATE = DateTime.Now;
                    }
                    else
                    {

                    }
                    data.IS_DELETED = false;
                    data.ENT_DATE = DateTime.Now;
                    db.queries.Add(data);
                    db.SaveChanges();

                    if (data.OV != null || data.LOD != null)
                    {
                        var chst = db.files.Where(i => i.ID == data.FILE_ID).SingleOrDefault();
                        if (chst != null)
                        {
                            chst.ID = data.FILE_ID;
                            chst.OV_DATE = DateTime.Now;
                            chst.OVLOD = "blue";
                            db.SaveChanges();
                        }
                    }
                    var status = db.queries.Where(i => i.FILE_ID == data.FILE_ID && i.IS_DELETED.Value.Equals(false) && i.IS_RESOLVE == false).ToList();
                    var filestatus = db.files.Where(i => i.ID == data.FILE_ID).FirstOrDefault();
                    filestatus.Q_DATE = DateTime.Now;
                    if (status.Count == 0)
                    {
                        filestatus.Q = "green";
                    }
                    else
                    {
                        filestatus.Q = "blue";
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("query");
        }
        public ActionResult Login()
        {
            if (Request.UrlReferrer != null)
            {
                TempData["ReturnUrl"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Login(employee_master data)
        {
            try
            {
                var users = db.employee_master.Where(i => i.EMAIL == data.EMAIL && i.PASSWORD == data.PASSWORD).FirstOrDefault();
                if (users == null)
                {
                    TempData["msg"] = "Email id Or Password is wrong";
                    return RedirectToAction("Login");
                }
                else
                {
                    var roll = users.emp_roll_master.FirstOrDefault();

                    Session["name"] = users.NAME.Trim();
                    Session["roll"] = roll.EMP_ROLE;
                    Session["emp_id"] = users.ID;
                    Session["email"] = users.EMAIL;

                    var notification = db.notifications.Where(i => i.EMP_ID == users.ID && i.IS_READ != true).Count();
                    {
                        if (notification > 0)
                        {
                            Session["noti"] = notification;
                        }
                        else
                        {
                            Session["noti"] = null;
                        }
                    }
                    var pu_notice = db.public_notice.Where(i => i.IS_DELETED.Value.Equals(false) && i.IS_READ.Value.Equals(false)).OrderByDescending(i => i.ID).ToList();
                    ViewBag.public_notice = pu_notice;

                    if (pu_notice != null)
                    {
                        var date = DateTime.Now;
                        var public_notification = db.public_notice.Where(i => i.REMINDER_DATE >= i.REMINDER_DATE && i.IS_DELETED.Value.Equals(false) && i.IS_READ.Value.Equals(false)).Count();
                        if (public_notification != 0)
                        {
                            Session["public_notification"] = public_notification;
                        }
                        else
                        {
                            Session["public_notification"] = "";
                        }
                        if (TempData["ReturnUrl"] != null && !TempData["ReturnUrl"].ToString().Contains("Login") && TempData["ReturnUrl"].ToString() != "http://localhost:55138/")
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }
                    }
                    return RedirectToAction("DashBoard");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [AllowAnonymous]
        public ActionResult logout()
        {
            try
            {
                Session.Abandon();
                Session.Clear();
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("Login");
        }
        public JsonResult delete(int id, string c_del, string a_del, string em_del, string noti, string pn_del, string qry_del, string src_del, string bk_del, string filedelete)
        {
            try
            {
                if (a_del != null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    var agent = db.agent_master.Where(i => i.ID == id).SingleOrDefault();

                    var usefile = db.files.Where(i => i.A_ID == id).Count();
                    if (usefile == 0)
                    {
                        bool ab = true;
                        agent.IS_DELETED = ab;
                        db.SaveChanges();
                    }
                    return Json(new { data = agent }, JsonRequestBehavior.AllowGet);
                }
                else if (em_del != null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    var employee = db.employee_master.Where(i => i.ID == id).SingleOrDefault();
                    var usefile = db.files.Where(i => i.ASSIGN_TO == id).Count();
                    if (usefile == 0)
                    {
                        bool ab = true;
                        employee.IS_DELETED = ab;
                        db.SaveChanges();
                    }
                    return Json(new { data = employee }, JsonRequestBehavior.AllowGet);
                }
                else if (noti != null)
                {
                    var notification = db.notifications.Where(i => i.ID == id).SingleOrDefault();
                    bool ab = true;
                    notification.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (pn_del != null)
                {
                    var pb_n = db.public_notice.Where(i => i.ID == id).SingleOrDefault();
                    bool ab = true;
                    pb_n.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (qry_del != null)
                {
                    var query = db.queries.Where(i => i.ID == id).SingleOrDefault();
                    bool ab = true;
                    query.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (bk_del != null)
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    var bank = db.bank_master.Where(i => i.ID == id).SingleOrDefault();
                    var usefile = db.files.Where(i => i.B_ID == id).Count();
                    if (usefile == 0)
                    {
                        bool ab = true;
                        bank.IS_DELETED = ab;
                        db.SaveChanges();
                    }
                    return Json(new { data = bank }, JsonRequestBehavior.AllowGet);
                }
                else if (filedelete != null)
                {
                    var filedelete1 = db.files.Where(i => i.ID == id).SingleOrDefault();
                    bool ab = true;
                    filedelete1.IS_DELETED = ab;
                    db.SaveChanges();
                }
                return Json("");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void Recoverd(int ID, string values)
        {
            try
            {
                if (values == "d_agent")
                {
                    var agent = db.agent_master.Where(i => i.ID == ID).SingleOrDefault();
                    bool ab = false;
                    agent.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (values == "d_employee")
                {
                    var employee = db.employee_master.Where(i => i.ID == ID).SingleOrDefault();
                    bool ab = false;
                    employee.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (values == "d_noti")
                {
                    var notification = db.notifications.Where(i => i.ID == ID).SingleOrDefault();
                    bool ab = false;
                    notification.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (values == "d_p_notice")
                {
                    var pb_n = db.public_notice.Where(i => i.ID == ID).SingleOrDefault();
                    bool ab = false;
                    pb_n.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (values == "d_query")
                {
                    var query = db.queries.Where(i => i.ID == ID).SingleOrDefault();
                    bool ab = false;
                    query.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (values == "d_bank")
                {
                    var bank = db.bank_master.Where(i => i.ID == ID).SingleOrDefault();
                    bool ab = false;
                    bank.IS_DELETED = ab;
                    db.SaveChanges();
                }
                else if (values == "d_file")
                {
                    var bank = db.files.Where(i => i.ID == ID).SingleOrDefault();
                    bool ab = false;
                    bank.IS_DELETED = ab;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public void isdelete(int ID, string values)
        {
            try
            {
                if (values == "d_agent")
                {
                    var agent = db.agent_master.Where(i => i.ID == ID).SingleOrDefault();

                    agent.IS_DELETED = null;
                    db.SaveChanges();
                }
                else if (values == "d_employee")
                {
                    var employee = db.employee_master.Where(i => i.ID == ID).SingleOrDefault();

                    employee.IS_DELETED = null;
                    db.SaveChanges();
                }
                else if (values == "d_noti")
                {
                    var notification = db.notifications.Where(i => i.ID == ID).SingleOrDefault();

                    notification.IS_DELETED = null;
                    db.SaveChanges();
                }
                else if (values == "d_p_notice")
                {
                    var pb_n = db.public_notice.Where(i => i.ID == ID).SingleOrDefault();

                    pb_n.IS_DELETED = null;
                    db.SaveChanges();
                }
                else if (values == "d_query")
                {
                    var query = db.queries.Where(i => i.ID == ID).SingleOrDefault();

                    query.IS_DELETED = null;
                    db.SaveChanges();
                }
                else if (values == "d_bank")
                {
                    var bank = db.bank_master.Where(i => i.ID == ID).SingleOrDefault();

                    bank.IS_DELETED = null;
                    db.SaveChanges();
                }
                else if (values == "d_file")
                {
                    var bank = db.files.Where(i => i.ID == ID).SingleOrDefault();

                    bank.IS_DELETED = null;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public ActionResult deletedrecord()
        {
            try
            {
                var agent3 = db.agent_master.Where(i => i.IS_DELETED.Value.Equals(true)).ToList();
                ViewBag.agent = agent3;

                var bank3 = db.bank_master.Where(i => i.IS_DELETED.Value.Equals(true)).ToList();
                ViewBag.bank = bank3;

                var emp = db.employee_master.Where(i => i.IS_DELETED == true).ToList();
                ViewBag.employee = emp;

                var notifi = db.notifications.Where(i => i.IS_DELETED.Value.Equals(true)).ToList();
                ViewBag.notification = notifi;

                var employee4 = db.employee_master.ToList();
                ViewBag.emp = employee4;

                var notice = db.public_notice.Where(i => i.IS_DELETED.Value.Equals(true)).ToList();
                ViewBag.publicnotice = notice;

                var file = db.files.Where(i => i.IS_DELETED.Value.Equals(true)).ToList();
                ViewBag.files = file;

                var querys = db.queries.Where(i => i.IS_DELETED.Value.Equals(true)).ToList();
                ViewBag.queryes = querys;



                var file2 = db.files.ToList();
                ViewBag.files1 = file2;

                var village = db.city_master.Where(i => i.SUB_DIST_ID > 0 && i.DIST_ID > 0).ToList();
                ViewBag.villages = village;

                var sub_dist = db.city_master.Where(i => i.SUB_DIST_ID == 0 && i.DIST_ID > 0).ToList();
                ViewBag.SubDist = sub_dist;
            }
            catch (Exception e)
            {

                throw e;
            }


            return View();
        }
        [System.Web.Mvc.HttpGet]
        public JsonResult loadbank(int Id)
        {
            try
            {
                int ID = Convert.ToInt32(Id);

                // var bankid = db.agent_master.Where(i => i.ID == ID).Select(i => i.ID).SingleOrDefault();
                //var bank = db.bank_master.Where(i => i.ID == bankid).FirstOrDefault();
                var bank = db.bank_master.Where(j => j.ID == ID).Select(i => new
                {
                    NAME = i.NAME,
                    NO_OF_YEAR = i.NO_OF_YEAR,
                    ID = i.ID,
                    FEES = i.FEES,
                    NEC = i.NEC,
                    LEGAL_FEES = i.LEGAL_FEES
                }).FirstOrDefault();
                return Json(new { data = bank }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [System.Web.Mvc.HttpGet]
        public JsonResult Loadagent(int Id)
        {
            try
            {
                int ID = Convert.ToInt32(Id);
                var agent = db.files.Where(j => j.ID == ID).Select(i => new
                {
                    Agentid = i.agent_master.ID,
                    agentemail = i.agent_master.EMAIL,
                    mobile_no = i.agent_master.MOBILE1,
                    bank_email = i.bank_master.BANK_EMAIL,
                    cc = i.bank_master.CC
                }).FirstOrDefault();
                return Json(new { data = agent }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [System.Web.Mvc.HttpGet]
        public JsonResult loadcitydata(int Id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var dist = db.city_master.Where(i => i.DIST_ID == Id && i.SUB_DIST_ID == 0).Select(i => new
                {
                    ID = i.ID,
                    sub_dist = i.VILLAGE
                }).ToList();

                return Json(new { data = dist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [System.Web.Mvc.HttpGet]
        public JsonResult loadvillagedata(int Id)
        {
            try
            {
                var vlg = db.city_master.Where(i => i.SUB_DIST_ID == Id).Select(i => new
                {
                    ID = i.ID,
                    village = i.VILLAGE
                }).ToList();

                return Json(new { data = vlg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }

        [System.Web.Mvc.HttpPost]
        public JsonResult loadcitydatas(int[] Id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var dist = db.city_master.Where(i => i.DIST_ID > 0 && i.SUB_DIST_ID == 0 && i.SUB_DIST_ID != 1).ToList();

                List<city_master> cm = new List<city_master>();
                foreach (var ids in Id)
                {
                    foreach (var dt in dist)
                    {
                        if (dt.DIST_ID == ids)
                        {
                            cm.Add(dt);
                        }
                    }
                }
                return Json(new { data = cm });
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [System.Web.Mvc.HttpPost]
        public JsonResult loadvillagedatas(int[] Id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var dist = db.city_master.Where(i => i.DIST_ID > 1 && i.SUB_DIST_ID > 0 && i.SUB_DIST_ID != 1).ToList();

                List<city_master> cm = new List<city_master>();
                foreach (var ids in Id)
                {
                    foreach (var dt in dist)
                    {
                        if (dt.SUB_DIST_ID == ids)
                        {
                            cm.Add(dt);
                        }
                    }
                }
                return Json(new { data = cm }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }

        [System.Web.Mvc.HttpGet]
        public JsonResult loadvillage(int Id)
        {
            try
            {
                int ID = Convert.ToInt32(Id);

                var villageid = db.city_master.Where(i => i.ID == ID).Select(i => i.SUB_DIST_ID).SingleOrDefault();

                var village = db.city_master.Where(j => j.ID == villageid).Select(i => new
                {
                    GEONAME = i.VILLAGE,
                    GEOLVLID = i.SUB_DIST_ID,
                    dist = i.DIST_ID,
                    ID = i.ID,

                }).FirstOrDefault();

                return Json(new { data = village }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        [System.Web.Mvc.HttpGet]
        public JsonResult loaddist(int Id)
        {
            try
            {
                int ID = Convert.ToInt32(Id);
                var subdistid = db.city_master.Where(i => i.ID == ID).Select(i => i.DIST_ID).SingleOrDefault();

                var subdist = db.city_master.Where(j => j.ID == subdistid).Select(i => new
                {
                    Dist = i.VILLAGE,
                    GEOLVLID = i.DIST_ID,
                    ID = i.ID,

                }).FirstOrDefault();

                return Json(new { data = subdist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        [HttpPost]
        public ActionResult updatepn(public_notice data, FormCollection frm)
        {
            try
            {
                var day = Request.Form["DAYS"];
                Int16 addedday = Convert.ToInt16(day);

                data.ENT_DATE = DateTime.Now;
                data.REMINDER_DATE = data.ISSUED_DATE.Value.AddDays(addedday);
                db.public_notice.Add(data);
                db.SaveChanges();

                return RedirectToAction("FileSingle/" + data.FILE_ID);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        [HttpPost]
        public ActionResult quary(query data)
        {
            try
            {
                data.ENT_DATE = DateTime.Now;
                var quary = db.queries.Add(data);
                db.SaveChanges();
                return RedirectToAction("FileSingle/" + data.FILE_ID);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public ActionResult template()
        {
            try
            {
                var temp = db.TEMPLATEs.ToList();
                ViewBag.template = temp;
            }
            catch (Exception e)
            {

                throw e;
            }


            return View();
        }
        [HttpPost]
        public ActionResult template(TEMPLATE data, FormCollection frm, HttpPostedFileBase[] files)
        {
            try
            {
                if (data.ID > 0)
                {
                    var template = db.TEMPLATEs.Where(i => i.ID == data.ID).SingleOrDefault();
                    if (template != null)
                    {

                        template.UPDATE_DATE = DateTime.Now;
                        if (data.MATTER != null)
                        {
                            template.MATTER = data.MATTER.Trim();
                        }
                        if (data.DESCRIPTION != null)
                        {
                            template.DESCRIPTION = data.DESCRIPTION.Trim();
                        }
                        if (data.HTML != null)
                        {
                            template.HTML = data.HTML;
                        }
                        template.IS_ACTIVE = data.IS_ACTIVE;
                        db.SaveChanges();
                    }
                }
                else
                {
                    data.ENT_DATE = DateTime.Now;
                    db.TEMPLATEs.Add(data);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {

                throw e;
            }

            return RedirectToAction("template");
        }

        public ActionResult ChangeStatusNow(public_notice data1, query data2, file data3, agent_master data4, communication comm_data, FormCollection frm, HttpPostedFileBase[] files, string mo, string msg)
        {
            try
            {
                var search = db.files.ToList();
                ViewBag.searchdate = search;
                if (data1.IMAGE_PATH != null)
                {
                    Models.public_notice imgs = new public_notice();
                    HttpFileCollectionBase file = Request.Files;
                    for (int i = 0; i < file.Count; i++)
                    {
                        HttpPostedFileBase Image = file[i];
                        var filename = "";
                        if (Image != null)
                        {
                            var fileno = data3.R_NO;
                            var year = DateTime.Now.Year;
                            filename = Path.GetFileName(Image.FileName);

                            var path3 = Path.Combine(Server.MapPath("~/Images"), filename);
                            Image.SaveAs(path3);

                            var confi = db.configurations.FirstOrDefault();
                            var path = confi.FOLDER_PATH;
                            //var path = System.Configuration.ConfigurationManager.AppSettings["path"];
                            var path1 = Path.Combine((path + fileno + "-" + year), filename);

                            if (!System.IO.Directory.Exists(path1))
                            {
                                var path2 = confi.FOLDER_PATH;
                                //var path2 = System.Configuration.ConfigurationManager.AppSettings["path"];
                                Directory.CreateDirectory(Path.Combine(path2 + fileno + "-" + year));
                            }
                            Image.SaveAs(path1);
                            data1.IMAGE_PATH = filename;
                            db.SaveChanges();
                        }
                    }
                }
                if (data3.FILE_NAME != null)
                {
                    Models.public_notice imgs = new public_notice();

                    HttpFileCollectionBase file = Request.Files;
                    for (int i = 0; i < file.Count; i++)
                    {
                        HttpPostedFileBase Image = file[i];
                        var filename = "";
                        if (Image != null)
                        {
                            var fileno = data3.R_NO;
                            var year = DateTime.Now.Year;
                            filename = Path.GetFileName(Image.FileName);

                            var path3 = Path.Combine(Server.MapPath("~/DocxFile"), filename);
                            Image.SaveAs(path3);

                            var confi = db.configurations.FirstOrDefault();
                            var path = confi.FOLDER_PATH;
                            //var path = System.Configuration.ConfigurationManager.AppSettings["path"];
                            var path1 = Path.Combine((path + fileno + "-" + year), filename);
                            if (!System.IO.Directory.Exists(path1))
                            {
                                var path2 = confi.FOLDER_PATH;
                                //var path2 = System.Configuration.ConfigurationManager.AppSettings["path"];
                                Directory.CreateDirectory(Path.Combine(path2 + fileno + "-" + year));
                            }
                            Image.SaveAs(path1);
                            data3.FILE_NAME = filename;
                            db.SaveChanges();
                        }
                    }
                }
                if (data3.ID > 0)
                {
                    var chst = db.files.Where(i => i.ID == data3.ID).SingleOrDefault();

                    if (chst != null)
                    {
                        if (data3.T != null)
                        {
                            chst.T = data3.T;
                            if (data3.FILE_NAME != null)
                            {
                                chst.FILE_NAME = data3.FILE_NAME;
                                chst.T = "green";
                                chst.T_DATE = DateTime.Now;
                            }
                            if (data3.T == "gray")
                            {
                                chst.T_DATE = null;
                            }
                            else
                            {
                                chst.T_DATE = DateTime.Now;
                            }
                            db.SaveChanges();
                            if (chst.T == "green")
                            {
                                Session["green"] = chst.T;
                            }
                            else
                            {
                                Session["grey"] = chst.T;
                            }
                        }
                        else if (data3.S != null)
                        {
                            chst.S = data3.S;
                            if (data3.S == "green")
                            {
                                if (data3.RECEIVER > 0)
                                {
                                    chst.RECEIVER = data3.RECEIVER;
                                }
                            }
                            else
                            {
                                if (data3.RECEIVER > 0)
                                {
                                    chst.RECEIVER = null;
                                }
                            }
                            if (data3.S != "gray")
                            {
                                chst.S_DATE = DateTime.Now;
                            }
                            db.SaveChanges();
                        }
                        else if (data3.OVLOD != null)
                        {
                            chst.OVLOD = data3.OVLOD;
                            chst.OV_DATE = DateTime.Now;
                            db.SaveChanges();
                        }
                        else if (data3.D != null)
                        {
                            chst.D = data3.D;
                            if (data3.D == "green")
                            {
                                var to = chst.agent_master.EMAIL;
                                if (to == null)
                                {
                                    to = "";
                                }
                                var to1 = chst.bank_master.BANK_EMAIL;
                                if (to1 == null)
                                {
                                    to1 = "";
                                }
                                var cc = chst.bank_master.CC;
                                if (cc == null)
                                {
                                    cc = "";
                                }
                                var confi = db.configurations.FirstOrDefault();
                                var no = (chst.APPLICATION_NO != null && chst.APPLICATION_NO != "") ? chst.APPLICATION_NO : chst.R_NO.ToString();
                                var subject = "Delivered Report(" + chst.PROPERTY_DETAILS + ")(File no: " + chst.R_NO + ")";

                                var msg1 = "Application / Prospect / File no.: " + no + "<br /> " +
                                              "Party name :" + chst.CUSTOMERNAME + "<br /> " +
                                              "Property :" + chst.PROPERTY_DETAILS + " <br /> " +
                                              "Message: Legal Report has been Delivered.<br />"
                                              + "<br /> For the security reason title file(pdf) will be sent in separete email.<br />  " +
                                               "<br /> Brinal A. Bangdiwala(Advocate)";

                                var Rno = Convert.ToInt32(chst.R_NO);
                                sendmail(to, to1, cc, subject, msg1, Rno, confi);
                                chst.DELIVERY_DATE = DateTime.Now;
                                chst.D_DATE = DateTime.Now;
                            }
                            else
                            {
                                chst.DELIVERY_DATE = null;
                                chst.D_DATE = null;
                            }
                            db.SaveChanges();
                        }
                        else if (data3.Q != null)
                        {
                            var sentemail = db.agent_master.Where(i => i.ID == data3.A_ID).FirstOrDefault();
                            if (sentemail != null)
                            {

                            }
                            chst.Q = data3.Q;
                            if (data3.Q != "gray")
                            {
                                chst.Q_DATE = DateTime.Now;
                            }
                            db.SaveChanges();

                            var query = db.queries.Where(i => i.FILE_ID == data3.ID).SingleOrDefault();
                            if (query != null)
                            {
                                query.FILE_ID = data3.ID;
                                if (data2.QUERY_REMARK != null)
                                {
                                    query.QUERY_REMARK = data2.QUERY_REMARK;
                                }
                                if (data2.LOD != null)
                                {
                                    query.LOD = data2.LOD;
                                }
                                if (data2.OV != null)
                                {
                                    query.OV = data2.OV;
                                }
                                if (data2.SMS_CONTENT != null)
                                {
                                    query.SMS_CONTENT = data2.SMS_CONTENT;
                                }
                                if (data2.EMAIL_CONTENT != null)
                                {
                                    query.EMAIL_CONTENT = data2.EMAIL_CONTENT;
                                }
                                if (data2.SENT_TO != null)
                                {
                                    query.SENT_TO = data2.SENT_TO;
                                }
                                query.UPDATE_DATE = DateTime.Now;
                                db.SaveChanges();

                            }
                            else
                            {
                                data2.FILE_ID = data3.ID;
                                data2.ENT_DATE = DateTime.Now;
                                data2.IS_DELETED = false;
                                db.queries.Add(data2);
                                db.SaveChanges();
                            }
                            if (data2.OV != null || data2.LOD != null)
                            {
                                if (chst != null)
                                {
                                    chst.ID = data3.ID;
                                    chst.OV_DATE = DateTime.Now;
                                    chst.OVLOD = "blue";
                                    db.SaveChanges();
                                }
                            }
                        }
                        else if (data3.PN != null)
                        {
                            chst.PN = data3.PN;
                            if (data3.PN != "gray")
                            {
                                chst.PN_DATE = DateTime.Now;
                            }
                            else
                            {
                                chst.PN_DATE = null;
                            }
                            db.SaveChanges();

                            var publicnotice = db.public_notice.Where(i => i.FILE_ID == data3.ID).SingleOrDefault();
                            
                            if (publicnotice != null)
                            {
                                if(data3.PN == "green")
                                {
                                    publicnotice.IS_READ = true;
                                }
                                else
                                {
                                    publicnotice.IS_READ = false;
                                }
                                var day = Request.Form["DAYS"];
                                int addedday = 0;
                                if (day != null && day != "")
                                {
                                    addedday = Convert.ToInt16(day);
                                }
                                if (data1.REMARK != null)
                                {
                                    publicnotice.REMARK = data1.REMARK;
                                }
                                if (data1.ISSUED_DATE != null)
                                {
                                    publicnotice.ISSUED_DATE = data1.ISSUED_DATE;
                                }
                                if (data1.IMAGE_PATH != null)
                                {
                                    publicnotice.IMAGE_PATH = data1.IMAGE_PATH;
                                }
                                if (day != null && day != "")
                                {
                                    publicnotice.REMINDER_DATE = data1.ISSUED_DATE.Value.AddDays(addedday);
                                }
                                db.SaveChanges();
                                                       
                                return RedirectToAction("FileSingle/" + data3.R_NO);
                            }
                            else
                            {
                                var day = Request.Form["DAYS"];
                                if (day != null && day != "")
                                {
                                    Int16 addedday = Convert.ToInt16(day);
                                    data1.REMINDER_DATE = data1.ISSUED_DATE.Value.AddDays(addedday);
                                }
                                data1.FILE_ID = data3.ID;
                                data1.IS_DELETED = false;
                                data1.IS_READ = false;
                                data1.ENT_DATE = DateTime.Now;
                                db.public_notice.Add(data1);
                                db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        chst.ID = data3.ID;
                        chst.T = data3.T;
                        if (data3.T != "gray")
                        {
                            chst.T_DATE = DateTime.Now;
                        }
                        db.files.Add(chst);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return RedirectToAction("FileSingle/" + data3.R_NO);
        }

        [HttpGet]
        public JsonResult Receiver(int R_NO)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;

                var receiver = db.files.Where(i => i.R_NO == R_NO).Select(i => new
                {
                    receivename = i.employee_master1.NAME
                }).FirstOrDefault();
                return Json(new { data = receiver }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public JsonResult getemployee(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var emp = db.employee_master.Where(i => i.ID == id).SingleOrDefault();
                return Json(new { data = emp }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getagent(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var agent = db.agent_master.Where(i => i.ID == id).SingleOrDefault();
                return Json(new { data = agent }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getbank(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var bank = db.bank_master.Where(i => i.ID == id).SingleOrDefault();
                return Json(new { data = bank }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getemployeerole(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var emprole = db.emp_roll_master.Where(i => i.ID == id).SingleOrDefault();
                return Json(new { data = emprole }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getnotification(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var notifications = db.notifications.Where(i => i.ID == id).SingleOrDefault();
                return Json(new { data = notifications }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getpublicnotice(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var publicnotice = db.public_notice.Where(i => i.ID == id).SingleOrDefault();
                var dt = Json(new { data = publicnotice }, JsonRequestBehavior.AllowGet);
                return Json(new { data = publicnotice }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getpublicnoticefile(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var publicnotice = db.public_notice.Where(i => i.FILE_ID == id).SingleOrDefault();
                return Json(new { data = publicnotice }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        [System.Web.Mvc.HttpGet]
        public JsonResult getquery(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var querys = (from q in db.queries join f in db.files on q.FILE_ID equals f.ID
                              join b in db.bank_master on f.B_ID equals b.ID
                              where q.ID == id
                              select new
                              {
                                  ID = q.ID,
                                  QUERY_REMARK = q.QUERY_REMARK,
                                  IS_RESOLVE = q.IS_RESOLVE,
                                  mobile_no = q.agent_master.MOBILE1,
                                  SENT_TO = q.agent_master.EMAIL,
                                  B_mail= b.BANK_EMAIL,
                                  cc= b.CC
                              }).ToList();   
                return Json(new { data = querys }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public ActionResult getqueryfile(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var querys = db.queries.Where(i => i.FILE_ID == id).ToList();
                ViewBag.queryes = querys;
                //return Json(new { data = querys }, JsonRequestBehavior.AllowGet);
                // return RedirectToAction("query");
            }
            catch (Exception e)
            {

                throw e;
            }
            return RedirectToAction("query");
        }
        public JsonResult gettemplate(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var template = db.TEMPLATEs.Where(i => i.ID == id).SingleOrDefault();
                return Json(new { data = template }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getfilestatus(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var status = db.files.Where(i => i.R_NO.Equals(id)).FirstOrDefault();
                return Json(new { data = status }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getinformation(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var information = db.INFORMATION.Where(i => i.ID == id).SingleOrDefault();
                return Json(new { data = information }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getConfoguration(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var config = db.configurations.Where(i => i.Id == id).SingleOrDefault();
                return Json(new { data = config }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                throw e;
            }

        }
        public JsonResult getpropertitydetails(int id)
        {
            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                var propertitydetails = db.propertity_details.Where(i => i.ID == id).FirstOrDefault();
                //var dt = Json(new { data = propertitydetails }, JsonRequestBehavior.AllowGet);
                return Json(new { data = propertitydetails }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e)
            {

                throw e;
            }

        }

        public JsonResult openfolder(int id)
        {
            try
            {
                //{
                //    ProcessStartInfo ProcessInfo;
                //    Process Process;
                //    var Command = "Your command here";
                //    ProcessInfo = new ProcessStartInfo("cmd.exe", "/K " + Command);
                //    ProcessInfo.CreateNoWindow = true;
                //    ProcessInfo.UseShellExecute = true;
                //    Process = Process.Start(ProcessInfo);
                //}

                var confi = db.configurations.FirstOrDefault();
                var fileno = db.files.Where(i => i.ID == id).FirstOrDefault();
                var Rno = fileno.R_NO;
                var path = confi.FOLDER_PATH;
                var year = DateTime.Now.Year;
                //  Process.Start("explorer.exe", path + "" + Rno + "-" + year);
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                // processStartInfo.WorkingDirectory = @"C:\Users\pcustance\Desktop\";
                //processStartInfo.WorkingDirectory = path + "" + Rno + "-" + year;
                processStartInfo.WorkingDirectory = "D:/";
                var F_path = path + "" + Rno + "-" + year;
                //processStartInfo.FileName = @"notepad.exe";
                //processStartInfo.Arguments = "test.txt";
                processStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                processStartInfo.CreateNoWindow = true;
              //  Process.Start("explorer.exe", String.Format("/n, /e, {0}", "d:/"));
               // Process process = Process.Start("explorer.exe", F_path);  
                return Json(new { fpath = F_path }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void configuration_setting()
        {
            var config = db.configurations.Where(i => i.Id == 1).FirstOrDefault();
            Session["frommail"] = config.FROM_EMAIL;
            return;
        }
        public static string sendpassword(string email, string pass,configuration data1)
        {
            try
            {
                //var frmemail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"];
                //var frmpass = System.Configuration.ConfigurationManager.AppSettings["EMPass"];
                //var host = System.Configuration.ConfigurationManager.AppSettings["Host"];
                //int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["port"]);

                var frmemail = data1.FROM_EMAIL;
                var frmpass = data1.EM_PASS;
                var host = data1.HOST;
                int port = Convert.ToInt32(data1.PORT);

                var fromAddress = new MailAddress(frmemail, "Brinal A. Bangdiwala(Advocate)");
                var toAddress = new MailAddress(email);
                string password = pass;
                string fromPassword = frmpass;
                string subject = "Forgot Password";
                string body = "your password is :" + password + "";

                SmtpClient smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
                return "email send successfully to " + toAddress + "!";
            }
            catch (Exception e)
            {
                return "there is some error plz try again!";
            }
        }

        public static string sendmail(string TO, string CC, string BCC,string subject, string msg, int R_No,configuration data)
        {
            try
            {                                
                var _to = TO.Split(',');
                var _cc = CC.Split(',');
                var _bcc = BCC.Split(',');               
                var frmemail = data.FROM_EMAIL;                
                var frmpass = data.EM_PASS;
                var host = data.HOST;                
                int port =Convert.ToInt32(data.PORT);                
                MailAddress from = new MailAddress(frmemail, "Brinal A. Bangdiwala(Advocate)");                
                MailMessage message = new MailMessage();
                message.From=from;                
                foreach (var item in _to)
                {
                    if (item != "" && item != null)
                    {
                        MailAddress cp = new MailAddress(item);
                        message.To.Add(cp);
                    }
                }
                foreach (var item in _cc)
                {
                    if (item != "" && item != null)
                    {
                        MailAddress cp = new MailAddress(item);
                        message.CC.Add(cp);
                    }
                }
                foreach (var item in _bcc)
                {
                    if (item != "" && item != null)
                    {
                        MailAddress cp = new MailAddress(item);
                        message.Bcc.Add(cp);
                    }
                }
                message.Bcc.Add(frmemail);
                message.Subject = subject;
                message.Body = msg;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(from.Address, frmpass)
                };
                try
                {
                    smtp.Send(message);
                }
                catch (Exception ex)
                {

                }
                return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static string sendmsg(string mo, string sms,configuration configuration)
        {
            try
            {
                //string authKey = System.Configuration.ConfigurationManager.AppSettings["SMSauthKey"]; 
                //string senderId = System.Configuration.ConfigurationManager.AppSettings["senderId"]; 
            
                string authKey = configuration.SMS_AUTH_KEY;
                string senderId = configuration.SENDER;
                string message = HttpUtility.UrlEncode(sms);

                System.Text.StringBuilder sbPostData = new System.Text.StringBuilder();
                sbPostData.AppendFormat("authkey={0}", authKey);
                sbPostData.AppendFormat("&mobiles={0}", mo);
                sbPostData.AppendFormat("&message={0}", message);
                sbPostData.AppendFormat("&sender={0}", senderId);
                sbPostData.AppendFormat("&route={0}", 4);

                string sendSMSUri = System.Configuration.ConfigurationManager.AppSettings["sendSMSUri"];

                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);

                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] data = encoding.GetBytes(sbPostData.ToString());

                httpWReq.Method = "POST";
                httpWReq.ContentType = "application/x-www-form-urlencoded";
                httpWReq.ContentLength = data.Length;
                using (Stream stream = httpWReq.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();

                reader.Close();
                response.Close();
            }
            catch (SystemException ex)
            {
                throw ex;
            }
            return "";
        }
        public static string Delivermail(string to,string to1,string cc,configuration config,file data)
        {
            try
            {
               
                var _to = to.Split(',');
                var _to1 = to1.Split(',');
                var _cc = cc.Split(',');
                //var frmemail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"];
                var frmemail = config.FROM_EMAIL;
                //var frmpass = System.Configuration.ConfigurationManager.AppSettings["EMPass"];
                var frmpass = config.EM_PASS;
                var host = config.HOST;
                //var host = System.Configuration.ConfigurationManager.AppSettings["Host"];               
                //int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["port"]);
                int port = Convert.ToInt32(config.PORT);
                // var fromAddress = new MailAddress(frmemail, "Brinal Bangdiwala");
                MailAddress from = new MailAddress(frmemail, "Brinal A. Bangdiwala(Advocate)");
                //MailAddress to = new MailAddress(TO,);
                MailMessage message = new MailMessage();
                message.From = from;
                //var emails = email.Split(',');
                foreach (var item in _to)
                {
                    if (item != "" && item != null)
                    {
                        MailAddress cp = new MailAddress(item);
                        message.To.Add(cp);
                    }
                }
                foreach (var item in _to1)
                {
                    if (item != "" && item != null)
                    {
                        MailAddress cp = new MailAddress(item);
                        message.To.Add(cp);
                    }
                }
                foreach (var item in _cc)
                {
                    if (item != "" && item != null)
                    {
                        MailAddress cp = new MailAddress(item);
                        message.CC.Add(cp);
                    }
                }
              
                message.Bcc.Add(frmemail);
                message.Subject = "File No: " + data.R_NO + " is ready for deliver";
                message.Body = "Hello,<br /> " + data.agent_master.NAME + "<br />" +
                          " File No. " + data.R_NO + " is ready for deliver. <br />Brinal A. Bangdiwala(Advocate)";
                message.IsBodyHtml = true;
                var smtp = new SmtpClient
                {
                    Host = host,
                    Port = port,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(from.Address, frmpass)
                };
                try
                {
                    smtp.Send(message);
                }
                catch (Exception ex)
                {

                }
                return "";

            }
            catch (Exception e)
            {

                return "there is some error plz try again!" + e;
            }
        }

        public int islogin()
        {
            if (Session["email"] == null)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
