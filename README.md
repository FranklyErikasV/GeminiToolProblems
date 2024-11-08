# GeminiToolProblems

This is a tiny project showcasing Gemini's (Gemini 1.5 Pro) inability to currectly structure tool call input data for the Danfoss document reading chatbot use case.

## System Message

```
You are a useful assistant in a professional work environment.
Emojis are forbidden.
You have access to user uploaded documents via the tools.
You cannot use Google Search.

# Available documents: 

|document_id|name|description|number of pages|status|
|-|-|-|-|-|
|3ry4ko9zms|The Importance of a Balanced Diet|-|1|ready|


Use tools frequently to make sure you have the necessary knowledge when responding to the user.

```

## The Tools

The responses of the tools are hardcoded as they are used for test case demo purposes.

### Summary Tool

The summary tool is used to summarize a document or multiple documents whose id/s are passed as input.

Example input:
```json
{
  "documents": [
    {
      "document_id": "3ry4ko9zms"
    }
  ]
}
```

### Query Tool

The query tool is used to query (vector search) provided document/s, in chunks, in the defined pages.
A _query_ of **\*** gets all content.

Example input:
```json
{
  "query": "*",
  "documents": [
    {
      "pages": "1",
      "document_id": "3ry4ko9zms"
    }
  ]
}
```

## How To Use

Most of the example is contained in the _Program.cs_ file, in a simple _Main_ method. It is a basic Console Application.

While the variable _useAutoChat_ is set to **true**, the program will use a predefined list of user inputs to interact with Gemini.
If _useAutoChat_ is *false* then the user can chat with the Gemini chatbot through Console inputs. Enter an empty string or "q" to stop the interaction.

_useModelFunctionStructure_ is used to determine whether the tool input's schema is nested into an additional "model" object. As it is currently implemented in the Danfoss document reading chatbot.
Summary Tool Example when _useModelFunctionStructure_ is **true**:
```json
{
  "model": {
    "documents": [
      {
        "document_id": "3ry4ko9zms"
      }
    ]
  }
}
```

Summary Tool Example when _useModelFunctionStructure_ is **false**:
```json
{
  "documents": [
    {
      "document_id": "3ry4ko9zms"
    }
  ]
}
```

### Test Cases

There are currently 4 test cases which are represented by different Lists of strings as predefined user inputs in the code.

1. _Query Tool Input Structure Multishot Case_: A conversation with Gemini where the end result should be Gemini correctly calling the Query Tool and listing the 6 headers/sections of the document.
2. _Summary Tool Input Structure Multishot Case_: A conversation with Gemini where the end result should be Gemini correctly calling the Summary Tool for the document and passing the summary to the user.
3. _Query Tool Input Structure Zeroshot Case_: A single request from the user to Gemini where Gemini should correctly call the Query Tool and provide the user with the list of 6 headers/sections from the document.
4. _Summary Tool Input Structure Zeroshot Case_: A single request from the user to Gemini where Gemini should correctly call the Summary Tool and provide the user with the summary of the document.

A specific test case can be set via the _currentCase_ variable.
```csharp
List<string> currentCase = queryToolInputStructureMultishotCase; // Change this to run a different auto-chat case
```