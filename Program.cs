﻿using Google.Cloud.AIPlatform.V1;

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
            InputStructure inputStructure = InputStructure.Model; // Determines which input schema to use for the tool calls.
                                                                  // Normal being non-nested, plain objects,
                                                                  // Model - nested in a "model" object (as is currently implemented in the docs reading assistant),
                                                                  // Input - nested in a "input" object

            List<string> queryToolInputStructureMultishotCase = new List<string>
            {
                "Hello there!",
                "What documents do you have available?",
                "What headers does that document contain?"
            };
            List<string> summaryToolInputStructureMultishotCase = new List<string>
            {
                "Hello there!",
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

            List<string> currentCase = summaryToolInputStructureMultishotCase; // Change this to run a different auto-chat case

            // =================================================================================

            var predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{location}-aiplatform.googleapis.com",
                CredentialsPath = credentialsFilePath
            }.Build();

            var systemMessageContent = new Content
            {
                Parts =
                {
                    new Part { Text = @"
You are a useful assistant in a professional work environment who has autonomy in making its own decisions in order to solve the user's query.
Emojis are forbidden.
Document IDs must never be printed to the user.
You have access to user uploaded documents through your functions.
You cannot use Google Search.

# Available documents that you have access to

|document_id|name|description|number of pages|status|
|-|-|-|-|-|
|3ry4ko9zms|The Importance of a Balanced Diet|-|1|ready|

Use your functions frequently to access and analyze the available documents and answer the user's query.
" }
                }
            };

            FunctionDeclaration retrieveStandardSummaryFunction = new FunctionDeclaration();
            FunctionDeclaration queryDocumentsFunction = new FunctionDeclaration();

            switch (inputStructure)
            {
                case InputStructure.Normal:
                    retrieveStandardSummaryFunction = FunctionDeclerationProvider.GetSummaryTool();
                    queryDocumentsFunction = FunctionDeclerationProvider.GetQueryTool();
                    break;
                case InputStructure.Model:
                    retrieveStandardSummaryFunction = FunctionDeclerationProvider.GetSummaryModelTool();
                    queryDocumentsFunction = FunctionDeclerationProvider.GetQueryModelTool();
                    break;
                case InputStructure.Input:
                    retrieveStandardSummaryFunction = FunctionDeclerationProvider.GetSummaryInputTool();
                    queryDocumentsFunction = FunctionDeclerationProvider.GetQueryInputTool();
                    break;
            }

            var functionCallNameList = new List<string>
            {
                retrieveStandardSummaryFunction.Name,
                queryDocumentsFunction.Name
            };

            for (int i = 0; i < repeatTimes; i++)
            {
                Console.WriteLine("==========================");
                Console.WriteLine("||                      ||");
                Console.WriteLine("||   New Conversation   ||");
                Console.WriteLine("||                      ||");
                Console.WriteLine("==========================");

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

                            switch (inputStructure)
                            {
                                case InputStructure.Normal:
                                    expectedDataStructure = "{ \"name\": \"retrieve_standard_summary\", \"args\": { \"documents\": [ { \"document_id\": \"3ry4ko9zms\" } ] } }";
                                    break;
                                case InputStructure.Model:
                                    expectedDataStructure = "{ \"name\": \"retrieve_standard_summary\", \"args\": { \"model\": { \"documents\": [ { \"document_id\": \"3ry4ko9zms\" } ] } } }";
                                    break;
                                case InputStructure.Input:
                                    expectedDataStructure = "{ \"name\": \"retrieve_standard_summary\", \"args\": { \"input\": { \"documents\": [ { \"document_id\": \"3ry4ko9zms\" } ] } } }";
                                    break;
                            }

                            Console.WriteLine(expectedDataStructure);
                            Console.WriteLine($"Summary Function Call");

                            apiResponse = FunctionDeclerationProvider.SummaryToolResponse;
                        }

                        if (string.Equals(functionCall.Name, queryDocumentsFunction.Name))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Expected data structure:");

                            switch (inputStructure)
                            {
                                case InputStructure.Normal:
                                    expectedDataStructure = "{ \"name\": \"query_documents\", \"args\": { \"query\": \"*\", \"documents\": [ { \"pages\": \"1\", \"document_id\": \"3ry4ko9zms\" } ] } }";
                                    break;
                                case InputStructure.Model:
                                    expectedDataStructure = "{ \"name\": \"query_documents\", \"args\": { \"model\": { \"documents\": [ { \"pages\": \"1\", \"document_id\": \"3ry4ko9zms\" } ], \"query\": \"*\" } } }";
                                    break;
                                case InputStructure.Input:
                                    expectedDataStructure = "{ \"name\": \"query_documents\", \"args\": { \"input\": { \"documents\": [ { \"pages\": \"1\", \"document_id\": \"3ry4ko9zms\" } ], \"query\": \"*\" } } }";
                                    break;
                            }

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

        public enum InputStructure
        {
            Normal,
            Model,
            Input
        }
    }
}
