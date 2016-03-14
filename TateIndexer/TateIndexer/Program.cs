using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TateIndexer
{
    class Program
    {
        private static string searchServiceName = [Search Service Name];
        private static string apiKey = [Search Service API Key];
        private static SearchServiceClient _searchClient;
        private static SearchIndexClient _indexClient;
        private static string AzureSearchIndex = "tate-art-collection";

        static void Main(string[] args)
        {
            // Create an HTTP reference to the catalog index
            _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
            _indexClient = _searchClient.Indexes.GetClient(AzureSearchIndex);

            Console.WriteLine("{0}", "Deleting index...\n");
            DeleteIndex();

            Console.WriteLine("{0}", "Creating index...\n");
            if (CreateIndex() == false)
            {
                Console.ReadLine();
                return;
            }

            Console.WriteLine("{0}", "Uploading content...\n");
            UploadContent();

            Console.WriteLine("\nPress any key to continue\n");
            Console.ReadLine();

        }

        private static bool DeleteIndex()
        {
            // Delete the index, data source, and indexer.
            try
            {
                _searchClient.Indexes.Delete(AzureSearchIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting index: {0}\r\n", ex.Message);
                Console.WriteLine("Did you remember to set your SearchServiceName and SearchServiceApiKey?\r\n");
                return false;
            }

            return true;
        }

        private static bool CreateIndex()
        {
            // Create the Azure Search index based on the included schema

            // I am enabling all CORS origins so that the JavaScript sample will work from all clients
            // It is highly recommended that you only enable origins that are absolutely necessary
            CorsOptions co = new CorsOptions();
            List<string> origins = new List<string>();
            origins.Add("*");
            co.AllowedOrigins = origins;

            try
            {
                var definition = new Index()
                {
                    Name = AzureSearchIndex,
                    CorsOptions = co,
                    Fields = new[] 
                    { 
                        new Field("acno",                       DataType.String)         { IsKey = true,  IsSearchable = false, IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("acquisitionYear",            DataType.Int32)          { IsKey = false, IsSearchable = false, IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("all_artists",                DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("catalogueGroupCompleteStatus",DataType.String)        { IsKey = false, IsSearchable = true,  IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("catalogueGroupFinbergNumber",DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("catalogueGroupGroupType",    DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("catalogueGroupId",           DataType.Int32)          { IsKey = false, IsSearchable = false, IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("catalogueGroupShortTitle",   DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("classification",             DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("contributorCount",           DataType.Int32)          { IsKey = false, IsSearchable = false, IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("contributors",               DataType.Collection(DataType.String))     { IsSearchable = true, IsFilterable = true,  IsFacetable = true },
                        new Field("creditLine",                 DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("dateRange",                  DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("dateText",                   DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("depth",                      DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("dimensions",                 DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("foreignTitle",               DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("groupTitle",                 DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("height",                     DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("width",                      DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("id",                         DataType.Int32)          { IsKey = false, IsSearchable = false, IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("inscription",                DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("medium",                     DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("movementCount",              DataType.Int32)          { IsKey = false, IsSearchable = false, IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("subjectCount",               DataType.Int32)          { IsKey = false, IsSearchable = false, IsFilterable = true,   IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("subjects",                   DataType.Collection(DataType.String))     { IsSearchable = true, IsFilterable = true,  IsFacetable = true },
                        new Field("thumbnailCopyright",         DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("thumbnailUrl",               DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true},
                        new Field("title",                      DataType.String)         { IsKey = false, IsSearchable = true,  IsFilterable = false,  IsSortable = false, IsFacetable = false, IsRetrievable = true, Analyzer = AnalyzerName.EnMicrosoft},
                        new Field("units",                      DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = true,  IsSortable = true,  IsFacetable = true,  IsRetrievable = true},
                        new Field("url",                        DataType.String)         { IsKey = false, IsSearchable = false,  IsFilterable = false, IsSortable = false, IsFacetable = false, IsRetrievable = true}
                    }
                };
                _searchClient.Indexes.Create(definition);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating index: {0}\r\n", ex.Message);
                return false;
            }
            return true;

        }

        private static void UploadContent()
        {
            // Scan the JSON files from the Tate Art Collection and upload to Azure Search
            var indexOperations = new List<IndexAction>();

            string[] files = Directory.GetFiles(@"..\..\artworks", "*.json", System.IO.SearchOption.AllDirectories);
            int totalCounter = 0;

            try
            {
                foreach (var file in files)
                {
                    using (StreamReader jsonfile = File.OpenText(file))
                    {
                        totalCounter++;
                        Document doc = new Document();
                        string json = jsonfile.ReadToEnd();
                        dynamic array = JsonConvert.DeserializeObject(json);

                        doc.Add("acno", array["acno"].Value);
                        doc.Add("acquisitionYear", array["acquisitionYear"] == null ? -1 : array["acquisitionYear"].Value);
                        doc.Add("all_artists", array["all_artists"].Value);
                        doc.Add("catalogueGroupCompleteStatus", array["catalogueGroupCompleteStatus"] == null ? "" : array["catalogueGroupCompleteStatus"].Value);
                        doc.Add("catalogueGroupFinbergNumber", array["catalogueGroupFinbergNumber"] == null ? "" : array["catalogueGroupFinbergNumber"].Value);
                        doc.Add("catalogueGroupGroupType", array["catalogueGroupGroupType"] == null ? "" : array["catalogueGroupGroupType"].Value);
                        doc.Add("catalogueGroupId", array["catalogueGroupGroupType"] == null ? -1 : array["catalogueGroupGroupType"].Value);
                        doc.Add("catalogueGroupShortTitle", array["catalogueGroupShortTitle"] == null ? "" : array["catalogueGroupShortTitle"].Value);
                        doc.Add("classification", array["classification"] == null ? "" : array["classification"].Value);

                        doc.Add("contributorCount", array["contributorCount"] == null ? -1 : array["contributorCount"].Value);
                        doc.Add("creditLine", array["creditLine"].Value);
                        doc.Add("dateRange", array["dateRange"].Value);
                        doc.Add("dateText", array["dateText"].Value);
                        doc.Add("depth", array["depth"] == null ? "" : array["depth"].Value);
                        doc.Add("width", array["width"] == null ? "" : array["width"].Value);
                        doc.Add("height", array["height"] == null ? "" : array["height"].Value);
                        doc.Add("dimensions", array["dimensions"].Value);
                        doc.Add("foreignTitle", array["foreignTitle"].Value);
                        doc.Add("groupTitle", array["groupTitle"].Value);
                        doc.Add("id", array["id"] == null ? -1 : array["id"].Value);
                        doc.Add("inscription", array["inscription"].Value);
                        doc.Add("medium", array["medium"].Value);
                        doc.Add("movementCount", array["movementCount"] == null ? -1 : array["movementCount"].Value);
                        doc.Add("subjectCount", array["subjectCount"].Value == null ? -1 : array["subjectCount"].Value);
                        doc.Add("thumbnailCopyright", array["thumbnailCopyright"] == null ? "" : array["thumbnailCopyright"].Value);
                        doc.Add("thumbnailUrl", array["thumbnailUrl"].Value);
                        doc.Add("title", array["title"].Value);
                        doc.Add("units", array["units"].Value);
                        doc.Add("url", array["url"].Value);

                        // The below JSON parsing is pretty messy and should be cleaned up with a more succint parsing
                        if (array["contributors"] != null)
                        {
                            List<string> contributorList = new List<string>();

                            foreach (var item in array["contributors"])
                            {
                                contributorList.Add(((string)item["fc"]));
                            }
                            doc.Add("contributors", contributorList);

                        }

                        if (array["subjects"] != null)
                        {
                            JArray subArray = array["subjects"]["children"];
                            List<string> subjectList = new List<string>();
                            if (subArray != null)
                            {
                                foreach (var item in subArray.Children())
                                {
                                    var itemProperties = item.Children<JProperty>();

                                    var myChildren = itemProperties.FirstOrDefault(x => x.Name == "children");
                                    foreach (var subitem in myChildren.Children())
                                    {
                                        foreach (var secondLevelChild in subitem)
                                        {
                                            foreach (var thirdLevelChild in secondLevelChild["children"])
                                            {
                                                var name = thirdLevelChild["name"];
                                                subjectList.Add(name.ToString());
                                            }

                                            var secondLevelItemProperties = secondLevelChild.Children<JProperty>();
                                            var secondLevelElement = secondLevelItemProperties.FirstOrDefault(x => x.Name == "name");
                                            subjectList.Add(secondLevelElement.Value.ToString());

                                        }
                                    }
                                    var rootElement = itemProperties.FirstOrDefault(x => x.Name == "name");
                                    subjectList.Add(rootElement.Value.ToString());

                                }
                            }
                            doc.Add("subjects", subjectList);
                        }

                        indexOperations.Add(IndexAction.Upload(doc));
                        if (indexOperations.Count >= 500)
                        {
                            Console.WriteLine("Writing {0} documents of {1} total documents...", indexOperations.Count, totalCounter);
                            _indexClient.Documents.Index(new IndexBatch(indexOperations));
                            indexOperations.Clear();
                        }
                    }
                }
                if (indexOperations.Count >= 0)
                {
                    Console.WriteLine("Writing {0} documents of {1} total documents...", indexOperations.Count, totalCounter);
                    _indexClient.Documents.Index(new IndexBatch(indexOperations));
                }

            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                Console.WriteLine(
                 "Failed to index some of the documents: {0}",
                        String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
            }

        }

    }
}
