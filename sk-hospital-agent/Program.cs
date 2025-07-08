using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;

// Initialize a Kernel with a chat-completion service
IKernelBuilder builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(/* params */);

Kernel kernel = builder.Build();

ChatCompletionAgent wardAgent = new ChatCompletionAgent
{
    Name = "WardAgent",
    Description = "A hospital ward bed booking agent.",
    Instructions = "You are an agent that provides availability information about hospital wards, and makes bookings for beds.",
    Kernel = kernel,
};

ChatCompletionAgent operatingTheatreAgent = new ChatCompletionAgent
{
    Name = "OperatingTheatreAgent",
    Description = "An operating theatre booking agent.",
    Instructions = "You are an agent that provides availability information about operating theatres, and makes bookings for those operating theatres.",
    Kernel = kernel,
};

ChatHistory history = [];

ValueTask responseCallback(ChatMessageContent response)
{
    history.Add(response);
    return ValueTask.CompletedTask;
}

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
GroupChatOrchestration orchestration = new GroupChatOrchestration(
    new RoundRobinGroupChatManager { MaximumInvocationCount = 5 },
    wardAgent,
    operatingTheatreAgent)
{
    ResponseCallback = responseCallback,
};


InProcessRuntime runtime = new InProcessRuntime();
await runtime.StartAsync();


var result = await orchestration.InvokeAsync(
    "Create a slogan for a new electric SUV that is affordable and fun to drive.",
    runtime);


string output = await result.GetValueAsync(TimeSpan.FromSeconds(60));
Console.WriteLine($"\n# RESULT: "); // {text}");
Console.WriteLine("\n\nORCHESTRATION HISTORY");
foreach (ChatMessageContent message in history)
{
    Console.WriteLine(message);
}

#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

await runtime.RunUntilIdleAsync();
