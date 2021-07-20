using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using steveToReadXML;

namespace SteveToFoodSql.Pages
{
    public class readXMLModel : PageModel
    {
        private IHostingEnvironment _environment;

        public readXMLModel(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public IDirectoryContents DirectoryContents { get; private set; }
        public string Message { get; set; }
        public bool cheShowList { get; set; }
        
        [BindProperty]
        public IFormFile xmlSteve { get; set; }
        public List<CheeseVM> cheLists = new List<CheeseVM>();
        public string invoiceFileName = "";
        public string invoiceFileDate = "";
        public List<string> datePlus = new List<string>(10);

        public void OnGet()
        {
            Message = "This is an XML file read page. Please select XML file using the [Choose File] button";
            cheShowList = false;
        }


        //public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        public async Task OnPostUploadAsync(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    Message = "[Error] : Please select XML file using the [Choose File] button";
                }
                else
                {

                    cheShowList = true;

                    invoiceFileName = file.FileName;
                    invoiceFileDate = invoiceFileName.Substring(12, 2) + "/" + invoiceFileName.Substring(14, 2) + "/" + invoiceFileName.Substring(16, 4);

                    Cal_date(invoiceFileDate);

                    var filePath = Path.Combine("https://localhost:44382/xmlFolder/", invoiceFileName);

                    if (file != null && file.Length > 0)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(filePath);
                        XmlNodeList nodeList = xmlDoc.SelectNodes("Items/Item");
                        foreach (XmlNode node in nodeList)
                        {

                            CheeseVM cheeseData = new CheeseVM();

                            foreach (XmlNode child in node.ChildNodes)
                            {
                                // check node name to decide how to handle the values               
                                if (child.Name == "Name")
                                {
                                    cheeseData.Name = child.InnerText;
                                }
                                else if (child.Name == "Type")
                                {
                                    cheeseData.Type = child.InnerText;
                                }
                                else if (child.Name == "Price")
                                {
                                    cheeseData.Price = float.Parse(child.InnerText);
                                }
                                else if (child.Name == "DaysToSell")
                                {
                                    if (child.InnerText == null || child.InnerText == "")
                                    {
                                        cheeseData.DaysToSell = null;
                                    }
                                    else
                                    {
                                        cheeseData.DaysToSell = Int32.Parse(child.InnerText);
                                    }
                                }
                                else if (child.Name == "BestBeforeDate")
                                {
                                    if (child.InnerText == null || child.InnerText == "")
                                    {
                                        List<string> listInvDate = new List<string>();
                                        cheeseData.BestBeforeDate = "";
                                        listInvDate = "2021-12-31".ToString().Split('-').ToList();
                                        string tempDate = listInvDate[2] + "/" + listInvDate[1] + "/" + listInvDate[0];

                                        List<string> calPriceValue = new List<string>(10);
                                        calPriceValue = Cal_price(tempDate, cheeseData.DaysToSell, cheeseData.Type, cheeseData.Price);
                                        cheeseData.d01Price = calPriceValue[1];
                                        cheeseData.d02Price = calPriceValue[2];
                                        cheeseData.d03Price = calPriceValue[3];
                                        cheeseData.d04Price = calPriceValue[4];
                                        cheeseData.d05Price = calPriceValue[5];
                                        cheeseData.d06Price = calPriceValue[6];
                                        cheeseData.d07Price = calPriceValue[7];

                                    }
                                    else
                                    {
                                        List<string> listInvDate = new List<string>();
                                        //cheeseData.BestBeforeDate = DateTime.Parse(child.InnerText);
                                        cheeseData.BestBeforeDate = child.InnerText;
                                        listInvDate = child.InnerText.ToString().Split('-').ToList();
                                        string tempDate = listInvDate[2] + "/" + listInvDate[1] + "/" + listInvDate[0];

                                        List<string> calPriceValue = new List<string>(10);
                                        calPriceValue = Cal_price(tempDate, cheeseData.DaysToSell, cheeseData.Type, cheeseData.Price);
                                        cheeseData.d01Price = calPriceValue[1];
                                        cheeseData.d02Price = calPriceValue[2];
                                        cheeseData.d03Price = calPriceValue[3];
                                        cheeseData.d04Price = calPriceValue[4];
                                        cheeseData.d05Price = calPriceValue[5];
                                        cheeseData.d06Price = calPriceValue[6];
                                        cheeseData.d07Price = calPriceValue[7];
                                    }

                                }
                            }

                            cheLists.Add(cheeseData);


                        }
                    }

                    //return RedirectToPage("./readXML");
                }
            }
            catch (NullReferenceException e)
            {
                Message = "[Error] Please contact system admin";

                //return RedirectToPage("./readXML");
                //Console.WriteLine("File not found...\n");
            }

        }


        private void Cal_date (string tempDate)
        {
            DateTime intDate = DateTime.ParseExact(tempDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            for (int i = 0; i < 8; i++)
            {
                datePlus.Add(intDate.AddDays(i).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
            }

        }

        private List<string> Cal_price(string tempDate, int? tempNum, string tempType, float tempPrice)
        {
            float intPrice = tempPrice;
            DateTime intDate = DateTime.ParseExact(invoiceFileDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime bestEndDate = DateTime.ParseExact(tempDate, "d/M/yyyy", CultureInfo.InvariantCulture);

            List<string> returnValue = new List<string>(10);
            float currentPrice = intPrice;
            float dec5Num = (float)((float)intPrice * 0.05);
            float dec10Num = (float)((float)intPrice * 0.1);

            // ==================
            // Start logic here
            // ==================
                returnValue.Add(intPrice.ToString());


                for (int i = 1; i < 8; i++)
                {
                    DateTime currentDate = DateTime.ParseExact(datePlus[i], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var compareBestResult = DateTime.Compare(bestEndDate, currentDate);

                    var numberOfDays = (DateTime.ParseExact(datePlus[i], "dd/MM/yyyy", CultureInfo.InvariantCulture) - bestEndDate).TotalDays;

                    if (tempType.ToUpper() == "STANDARD")
                    {
                        if (compareBestResult > 0 || compareBestResult == 0)
                        {
                            currentPrice = ((float)(currentPrice - dec5Num));
                        }
                        else
                        {
                            currentPrice = ((float)(currentPrice - dec10Num));
                        }
                    } else if (tempType.ToUpper() == "AGED")
                    {
                            currentPrice = ((float)(currentPrice + dec5Num));

                    } else if (tempType.ToUpper() == "UNIQUE")
                    {
                            currentPrice = currentPrice;

                    } else if (tempType.ToUpper() == "FRESH")
                    {
                            if (compareBestResult > 0 || compareBestResult == 0)
                            {
                                currentPrice = ((float)(currentPrice - (dec5Num * 2)));
                            }
                            else
                            {
                                currentPrice = ((float)(currentPrice - (dec10Num * 2)));
                            }

                    } else if (tempType.ToUpper() == "SPECIAL")
                    {
                            DateTime expireDate = bestEndDate.AddDays((double)tempNum);
                            var compareExpireResult = DateTime.Compare(expireDate, currentDate);
                            var expireOfDays = (expireDate - DateTime.ParseExact(datePlus[i], "dd/MM/yyyy", CultureInfo.InvariantCulture)).TotalDays;

                        if (expireOfDays < 10)
                        {
                            if (expireOfDays < 5)
                            {
                                currentPrice = ((float)(currentPrice + (dec10Num)));
                            } else
                            {
                                currentPrice = ((float)(currentPrice + (dec5Num)));
                            }
                        } else
                        {
                            if (compareBestResult > 0 || compareBestResult == 0)
                            {
                                currentPrice = ((float)(currentPrice - (dec5Num)));
                            }
                            else
                            {
                                currentPrice = ((float)(currentPrice - (dec10Num)));
                            }

                        }

                }


                if (currentPrice < 0)
                    {
                        currentPrice = 0;
                    } else if (currentPrice > 20)
                    {
                        currentPrice = 20;
                    }

                    float currentPriceString = (float)Math.Round(currentPrice * 100f) / 100f;
                
                    if (tempNum < (numberOfDays) && compareBestResult < 0)
                    {
                        returnValue.Add("expires");

                    } else
                    {
                        returnValue.Add(currentPriceString.ToString());
                    }

                } // end for loop

            return returnValue;
        }




    }
}
