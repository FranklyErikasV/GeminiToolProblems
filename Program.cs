using Google.Cloud.AIPlatform.V1;

using Value = Google.Protobuf.WellKnownTypes.Value;

namespace GeminiToolProblems
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string credentialsFilePath = "<path-to-credentials-json>";
            string projectId = "<project-id>";
            string location = "europe-west1";
            string publisher = "google";
            string model = "gemini-1.5-pro";
            string closeSessionInput = "q"; // An iteration will end if the user input equals this value
            float temperature = 0f;
            int repeatTimes = 5; // Due to Gemini's inconsistency in responses, several retries are required to showcase the issue

            bool useAutoChat = true; // While this is true, the conversation will follow a script from one of the test case lists below
            bool useModelFunctionStructure = true; // When true, the input schema is nested into an additional "model" object

            List<string> queryToolInputStructureMultishotCase = new List<string>
            {
                "Hi there!",
                "What documents do you have available?",
                "What headers does that document contain?"
            };
            List<string> summaryToolInputStructureMultishotCase = new List<string>
            {
                "Hi there!",
                "What documents do you have available?",
                "Can you summarize that document for me?"
            };

            List<string> queryToolInputStructureZeroshotCase = new List<string>
            {
                "Make me a list of headers/sections that the Balanced Diet document has.",
            };
            List<string> summaryToolInputStructureZeroshotCase = new List<string>
            {
                "Summarize the Balanced Diet document.",
            };

            // =================================================================================

            List<string> currentCase = queryToolInputStructureMultishotCase; // Change this to run a different auto-chat case

            // =================================================================================

            var predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{location}-aiplatform.googleapis.com",
                CredentialsPath = credentialsFilePath
            }.Build();

            var systemMessageContent = new Content
            {
                Role = "SYSTEM",
                Parts =
                {
                    new Part { Text = "You are a useful assistant in a professional work environment.\r\nEmojis are forbidden.\r\nYou have access to user uploaded documents via the tools.\r\nYou cannot use Google Search.\r\n\r\n# Available documents: \r\n\r\n|document_id|name|description|number of pages|status|\r\n|-|-|-|-|-|\r\n|3ry4ko9zms|The Importance of a Balanced Diet|-|1|ready|\r\n\r\n\r\nUse tools frequently to make sure you have the necessary knowledge when responding to the user.\r\n" }
                }
            };

            var retrieveStandardSummaryFunction = useModelFunctionStructure ? FunctionDeclerationProvider.GetSummaryModelTool() : FunctionDeclerationProvider.GetSummaryTool();
            var queryDocumentsFunction = useModelFunctionStructure ? FunctionDeclerationProvider.GetQueryModelTool() : FunctionDeclerationProvider.GetQueryTool();
            var functionCallNameList = new List<string>
            {
                retrieveStandardSummaryFunction.Name,
                queryDocumentsFunction.Name
            };

            for (int i = 0; i < repeatTimes; i++)
            {
                Console.WriteLine("=======================");
                Console.WriteLine("||                   ||");
                Console.WriteLine("||   New Iteration   ||");
                Console.WriteLine("||                   ||");
                Console.WriteLine("=======================");

                List<string> autoChatUserInputList = currentCase.Select(i => i).ToList(); // Deep copy

                Google.Protobuf.Collections.RepeatedField<Content> chatHistory = new Google.Protobuf.Collections.RepeatedField<Content>();

                bool isChatting = true;
                while (isChatting)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    string userInput;

                    if (useAutoChat)
                    {
                        userInput = autoChatUserInputList.FirstOrDefault() ?? string.Empty;
                        Console.WriteLine(userInput);
                        if (autoChatUserInputList.Any())
                        {
                            autoChatUserInputList.RemoveAt(0);
                        }
                    }
                    else
                    {
                        userInput = Console.ReadLine() ?? string.Empty;
                    }

                    if (string.Equals(userInput, closeSessionInput, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(userInput))
                    {
                        isChatting = false;
                        break;
                    }

                    chatHistory.Add(new Content
                    {
                        Role = "USER",
                        Parts =
                        {
                            new Part { Text = userInput }
                        }
                    });

                    var generateContentRequest = new GenerateContentRequest
                    {
                        Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
                        GenerationConfig = new GenerationConfig
                        {
                            Temperature = temperature
                        },
                        SystemInstruction = systemMessageContent,
                        Contents = { chatHistory },
                        Tools =
                        {
                            new Tool
                            {
                                FunctionDeclarations = { retrieveStandardSummaryFunction, queryDocumentsFunction }
                            }
                        }
                    };

                    GenerateContentResponse contentResponse = await predictionServiceClient.GenerateContentAsync(generateContentRequest);

                    var functionCall = contentResponse.Candidates[0].Content.Parts[0].FunctionCall;

                    if (functionCall != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine(functionCall);

                        string apiResponse = string.Empty;
                        string expectedDataStructure = string.Empty;

                        if (string.Equals(functionCall.Name, retrieveStandardSummaryFunction.Name))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Expected data structure:");
                            expectedDataStructure = useModelFunctionStructure ?
                                "{ \"name\": \"retrieve_standard_summary\", \"args\": { \"model\": { \"documents\": [ { \"document_id\": \"3ry4ko9zms\" } ] } } }" :
                                "{ \"name\": \"retrieve_standard_summary\", \"args\": { \"documents\": [ { \"document_id\": \"3ry4ko9zms\" } ] } }";
                            Console.WriteLine(expectedDataStructure);
                            Console.WriteLine($"Summary Function Call");

                            apiResponse = FunctionDeclerationProvider.SummaryToolResponse;
                        }

                        if (string.Equals(functionCall.Name, queryDocumentsFunction.Name))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Expected data structure:");
                            expectedDataStructure = useModelFunctionStructure ?
                                "{ \"name\": \"query_documents\", \"args\": { \"model\": { \"documents\": [ { \"pages\": \"1\", \"document_id\": \"3ry4ko9zms\" } ], \"query\": \"*\" } } }" :
                                "{ \"name\": \"query_documents\", \"args\": { \"query\": \"*\", \"documents\": [ { \"pages\": \"1\", \"document_id\": \"3ry4ko9zms\" } ] } }";
                            Console.WriteLine(expectedDataStructure);
                            Console.WriteLine($"Query Documents Function Call");

                            apiResponse = FunctionDeclerationProvider.QueryToolResponse;
                        }

                        if (!functionCallNameList.Contains(functionCall.Name))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Called with incorrect Function Name");
                            Console.WriteLine($"{functionCall.Name} does not exist");
                        }

                        chatHistory.Add(contentResponse.Candidates[0].Content);
                        chatHistory.Add(new Content
                        {
                            Parts =
                            {
                                new Part
                                {
                                    FunctionResponse = new()
                                    {
                                        Name = functionCall.Name,
                                        Response = new()
                                        {
                                            Fields =
                                            {
                                                { "content", new Value { StringValue = apiResponse } }
                                            }
                                        }
                                    }
                                }
                            }
                        });

                        generateContentRequest = new GenerateContentRequest
                        {
                            Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
                            GenerationConfig = new GenerationConfig
                            {
                                Temperature = temperature
                            },
                            SystemInstruction = systemMessageContent,
                            Contents =
                            {
                                chatHistory
                            },
                            Tools =
                            {
                                new Tool
                                {
                                    FunctionDeclarations = { retrieveStandardSummaryFunction, queryDocumentsFunction }
                                }
                            }
                        };

                        contentResponse = await predictionServiceClient.GenerateContentAsync(generateContentRequest);
                    }

                    chatHistory.Add(contentResponse.Candidates[0].Content);

                    string responseText = contentResponse.Candidates[0].Content.Parts[0].Text;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(responseText);
                }

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
