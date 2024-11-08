using Google.Cloud.AIPlatform.V1;

using Type = Google.Cloud.AIPlatform.V1.Type;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace GeminiToolProblems
{
    public static class FunctionDeclerationProvider
    {
        public static FunctionDeclaration GetSummaryTool() =>
            new FunctionDeclaration
            {
                Name = "retrieve_standard_summary",
                Description = "Use this tool to get a general summary of the entire document. When the user needs more nuanced details from the contents of the document, use `query_documents` tool.\r\n    Returns a summarization of a document's contents.",
                Parameters = new OpenApiSchema
                {
                    Type = Type.Object,
                    Properties =
                    {
                        ["documents"] = new()
                        {
                            Type = Type.Array,
                            Description = "The list of document ids and pages to get standard summary of",
                            Items = new()
                            {
                                Type = Type.Object,
                                Properties =
                                {
                                    ["document_id"] = new()
                                    {
                                        Type = Type.String,
                                        Description = "Use the `document_id` of the document"
                                    },
                                    ["pages"] = new()
                                    {
                                        Type = Type.String,
                                        Description = "A string with comma-separated page numbers and/or ranges (e.g., '1,3,5,7-10,20-22'). Use a hyphen (-) for ranges (e.g., '2-5' will get all pages inclusively between 2 and 5)"
                                    }
                                },
                                Required = { "document_id" }
                            }
                        }
                    },
                    Required = { "documents" }
                }
            };

        public static FunctionDeclaration GetQueryTool() =>
            new FunctionDeclaration
            {
                Name = "query_documents",
                Description = "Use this tool to retrieve the entire contents of a document or relevant parts of documents (chunks) based on a query.\r\n    Returns relevant information as chunks of content from documents.\r\n    **Use the Correct Data Structures:** Adhere to the specified structure for the `model` dictionary and its nested components. The `model` must be a dictionary, `documents` should be a list of dictionaries, and each dictionary within `documents` should have `\"document_id\"` and `\"pages\"` keys.  The `query` should always be a string.",
                Parameters = new OpenApiSchema
                {
                    Type = Type.Object,
                    Properties =
                    {
                        ["query"] = new()
                        {
                            Type = Type.String,
                            Description = "To get all of the content of a specific page range, set to '*'. Otherwise, provide the search phrase (query) to use when in retrieving information from documents. The search phrase must be formulated so it includes all of the nuances of the user's query.",
                            Default = new Value { StringValue = "*" }
                        },
                        ["documents"] = new()
                        {
                            Type = Type.Array,
                            Description = "The list of document ids and pages extracted from the user query",
                            Items = new()
                            {
                                Type = Type.Object,
                                Properties =
                                {
                                    ["document_id"] = new()
                                    {
                                        Type = Type.String,
                                        Description = "Use the `document_id` of the document"
                                    },
                                    ["pages"] = new()
                                    {
                                        Type = Type.String,
                                        Description = "A string with comma-separated page numbers and/or ranges (e.g., '1,3,5,7-10,20-22'). Use a hyphen (-) for ranges (e.g., '2-5' will get all pages inclusively between 2 and 5)"
                                    }
                                },
                                Required = { "document_id" }
                            }
                        }
                    },
                    Required = { "query", "documents" }
                }
            };

        public static FunctionDeclaration GetSummaryModelTool() =>
            new FunctionDeclaration
            {
                Name = "retrieve_standard_summary",
                Description = "Use this tool to get a general summary of the entire document. When the user needs more nuanced details from the contents of the document, use `query_documents` tool.\r\n    Returns a summarization of a document's contents.",
                Parameters = new OpenApiSchema
                {
                    Type = Type.Object,
                    Properties =
                    {
                        ["model"] = new()
                        {
                            Type = Type.Object,
                            Properties =
                            {
                                ["documents"] = new()
                                {
                                    Type = Type.Array,
                                    Description = "The list of document ids and pages to get standard summary of",
                                    Items = new()
                                    {
                                        Type = Type.Object,
                                        Properties =
                                        {
                                            ["document_id"] = new()
                                            {
                                                Type = Type.String,
                                                Description = "Use the `document_id` of the document"
                                            },
                                            ["pages"] = new()
                                            {
                                                Type = Type.String,
                                                Description = "A string with comma-separated page numbers and/or ranges (e.g., '1,3,5,7-10,20-22'). Use a hyphen (-) for ranges (e.g., '2-5' will get all pages inclusively between 2 and 5)"
                                            }
                                        },
                                        Required = { "document_id" }
                                    }
                                }
                            },
                            Required = { "documents" }
                        }
                    }
                }
            };

        public static FunctionDeclaration GetQueryModelTool() =>
            new FunctionDeclaration
            {
                Name = "query_documents",
                Description = "Use this tool to retrieve the entire contents of a document or relevant parts of documents (chunks) based on a query.\r\n    Returns relevant information as chunks of content from documents.\r\n    **Use the Correct Data Structures:** Adhere to the specified structure for the `model` dictionary and its nested components. The `model` must be a dictionary, `documents` should be a list of dictionaries, and each dictionary within `documents` should have `\"document_id\"` and `\"pages\"` keys.  The `query` should always be a string.",
                Parameters = new OpenApiSchema
                {
                    Type = Type.Object,
                    Properties =
                    {
                        ["model"] = new()
                        {
                            Type = Type.Object,
                            Properties =
                            {
                                ["query"] = new()
                                {
                                    Type = Type.String,
                                    Description = "To get all of the content of a specific page range, set to '*'. Otherwise, provide the search phrase (query) to use when in retrieving information from documents. The search phrase must be formulated so it includes all of the nuances of the user's query.",
                                    Default = new Value { StringValue = "*" }
                                },
                                ["documents"] = new()
                                {
                                    Type = Type.Array,
                                    Description = "The list of document ids and pages extracted from the user query",
                                    Items = new()
                                    {
                                        Type = Type.Object,
                                        Properties =
                                        {
                                            ["document_id"] = new()
                                            {
                                                Type = Type.String,
                                                Description = "Use the `document_id` of the document"
                                            },
                                            ["pages"] = new()
                                            {
                                                Type = Type.String,
                                                Description = "A string with comma-separated page numbers and/or ranges (e.g., '1,3,5,7-10,20-22'). Use a hyphen (-) for ranges (e.g., '2-5' will get all pages inclusively between 2 and 5)"
                                            }
                                        },
                                        Required = { "document_id" }
                                    }
                                }
                            },
                            Required = { "query", "documents" }
                        }
                    }
                }
            };

        public static string SummaryToolResponse =>
            @"[
    {
        ""3ry4ko9zms"": {
            ""summary"": ""This document emphasizes the importance of a balanced diet for maintaining good health and well-being. It highlights the benefits, including consistent energy levels, a strong immune system, reduced risk of chronic diseases, and improved mental clarity. The document also provides a table outlining the recommended daily intake of essential nutrients like carbohydrates, proteins, fats, vitamins, and minerals, along with their primary sources. It encourages readers to consume a variety of whole, nutrient-dense foods for optimal health."",
            ""sectionSummaries"": [
                {
                    ""startPage"": 1,
                    ""endPage"": 1,
                    ""sectionHeader"": ""The Importance of a Balanced Diet: An Overview"",
                    ""sectionSummary"": ""A balanced diet is crucial for overall health, providing essential nutrients like carbohydrates, proteins, fats, vitamins, and minerals. It ensures consistent energy, supports the immune system, and reduces chronic disease risks.""
                },
                {
                    ""startPage"": 1,
                    ""endPage"": 2,
                    ""sectionHeader"": ""Understanding Recommended Daily Nutrient Intake"",
                    ""sectionSummary"": ""This section provides a detailed table outlining the recommended daily intake for various nutrients. It highlights the importance of consuming the right amounts of carbohydrates, proteins, and fats. Additionally, it emphasizes the significance of incorporating vitamins and minerals from sources like fruits, vegetables, dairy, and meat for a balanced diet.""
                }
            ]
        }
    }
]";

        public static string QueryToolResponse =>
            @"[
    {
        ""entry_id"": ""3ry4ko9zms_1"",
        ""score"": 1,
        ""document_id"": ""3ry4ko9zms"",
        ""raw_text"": ""<h3>2. Vital Nutrients</h3><p>It provides the body with vital nutrients, including carbohydrates, proteins, fats, vitamins, and minerals.</p>"",
        ""page_number"": 1
    },
    {
        ""entry_id"": ""3ry4ko9zms_4"",
        ""score"": 1,
        ""document_id"": ""3ry4ko9zms"",
        ""raw_text"": ""<h3>5. Nutrient-dense Foods</h3><p>Emphasizing whole, nutrient-dense foods over processed ones can make a significant difference in how the body functions and feels every day.</p>"",
        ""page_number"": 1
    },
    {
        ""entry_id"": ""3ry4ko9zms_2"",
        ""score"": 1,
        ""document_id"": ""3ry4ko9zms"",
        ""raw_text"": ""<h3>3. Health Benefits</h3><p>Eating a variety of foods in appropriate quantities ensures consistent energy levels, supports a healthy immune system, and reduces the risk of chronic diseases.</p>"",
        ""page_number"": 1
    },
    {
        ""entry_id"": ""3ry4ko9zms_0"",
        ""score"": 1,
        ""document_id"": ""3ry4ko9zms"",
        ""raw_text"": ""<h1>The Importance of a Balanced Diet</h1><h3>1. Introduction</h3><p>A balanced diet is essential for maintaining good health and well-being.</p>"",
        ""page_number"": 1
    },
    {
        ""entry_id"": ""3ry4ko9zms_5"",
        ""score"": 1,
        ""document_id"": ""3ry4ko9zms"",
        ""raw_text"": ""<h3>6. Recommended Daily Nutrient Intake</h3><p>A balanced diet involves consuming the right amounts of each nutrient daily. Here's a simple table:</p><br>\n<table><tr><th>Nutrient</th><th>Recommended Daily Intake</th><th>Sources</th></tr><tr><td>Carbohydrates</td><td>225-325 grams</td><td>Whole grains, fruits, vegetables</td></tr><tr><td>Proteins</td><td>46-56 grams</td><td>Beans, nuts, poultry, fish</td></tr><tr><td>Fats</td><td>44-77 grams</td><td>Avocados, olive oil, nuts</td></tr><tr><td>Vitamins</td><td>Varies per type</td><td>Fruits, vegetables, dairy</td></tr><tr><td>Minerals</td><td>Varies per type</td><td>Meat, dairy, vegetables</td></tr></table><p>Consider varying your diet to include these items for balanced nutrition.</p><br>\n"",
        ""page_number"": 1
    },
    {
        ""entry_id"": ""3ry4ko9zms_3"",
        ""score"": 1,
        ""document_id"": ""3ry4ko9zms"",
        ""raw_text"": ""<h3>4. Mental Clarity</h3><p>A balanced diet promotes mental clarity and emotional stability, crucial for overall life satisfaction.</p>"",
        ""page_number"": 1
    }
]";
    }
}
