using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

List<Person> users = new List<Person>
{
    new(){ Id = Guid.NewGuid().ToString(), Name = "Jhony", Age = 25 },
    new(){ Id = Guid.NewGuid().ToString(), Name = "Ann", Age = 22 },
    new(){Id = Guid.NewGuid().ToString(), Name = "Hiro", Age = 77 }

};

app.Run( async (context) =>
{
    var request = context.Request;
    var response = context.Response;
    var path = context.Request.Path;

    string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/api/users" && request.Method == "GET")
    {
        await GetAllUsersAsync(response);
    }
    else if(Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        string? id = path.Value?.Split('/')[3];
        await GetPerson(id,response); 
    }
    else if(path == "/api/users" && request.Method == "PUT")
    {
        await EditUser(response, request);
    }
    else if(Regex.IsMatch(path,expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeleteUser(id, response);
    }
    else if(path == "/api/users" && request.Method == "POST")
    {
        await CreateUser(response,request);
    }
    else
    {
        response.ContentType = "text/html";
        await response.SendFileAsync("html/index.html");
    }
});
app.Run();

async Task GetAllUsersAsync(HttpResponse response)
{
    await response.WriteAsJsonAsync(users);
}

async Task GetPerson(string? id, HttpResponse response)
{
    Person? user = users.FirstOrDefault((users) => users.Id == id);
    if (user != null)
    {
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new {message = "User wasn't found"});
    }
}

async Task EditUser(HttpResponse response, HttpRequest request)
{
   
        Person? user = await request.ReadFromJsonAsync<Person>();
        if (user != null)
        {
           Person? person =  users.FirstOrDefault((u) => u.Id == user.Id);
           if (person != null) 
           { 
                person.Name = user.Name;
                person.Age = user.Age;
                await response.WriteAsJsonAsync<Person>(person);
           }
           else 
           { 
               response.StatusCode = 404;
               await response.WriteAsJsonAsync(new { message = "Uncorrect data" });
           }
        }
    
}

async Task CreateUser(HttpResponse response,HttpRequest request)
{
    Person? user = await request.ReadFromJsonAsync<Person>();
    if (user != null) 
    { 
        user.Id = Guid.NewGuid().ToString();
        users.Add(user);
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Uncorrect data" });
    }
}

async Task DeleteUser(string id,HttpResponse response)
{
    Person? user = users.FirstOrDefault<Person>(u => u.Id == id);
    if (user != null)
    {
        users.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "User wasn't found" });
    }
}

public class Person
{
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";
    public int Age { get; set; }
}