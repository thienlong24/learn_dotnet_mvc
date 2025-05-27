# MvcMovie

This is an ASP.NET MVC project designed to demonstrate the basics of building web applications using the ASP.NET framework. The project is a simple movie database application where users can view, create, edit, and delete movie entries.

## Features

- **Movie Management**: Add, edit, delete, and view movie details.  
- **Search Functionality**: Search movies by title or genre.  
- **Responsive Design**: Optimized for various screen sizes.  
- **Validation**: Built-in form validation for movie data.  

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)  
- A code editor like [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/).  

## Build and Run

1. Clone the repository:  
    ```bash  
    git clone <repository-url>  
    cd MvcMovie  
    ```  

2. Restore dependencies:  
    ```bash  
    dotnet restore  
    ```  

3. Build the project:  
    ```bash  
    dotnet build  
    ```  

4. Run the application:  
    ```bash  
    dotnet run  
    ```  

5. Open your browser and navigate to `http://localhost:5129` (or the port specified in the terminal).  

## Project Structure

```plaintext  
MvcMovie/  
├── Controllers/         # Contains controller classes for handling requests  
├── Models/              # Contains data models for the application  
├── Views/               # Contains Razor views for rendering UI  
├── wwwroot/             # Static files like CSS, JS, and images  
├── appsettings.json     # Configuration settings  
├── Program.cs           # Entry point of the application  
├── Startup.cs           # Configures services and middleware  
└── README.md            # Project documentation  
```  

## License

This project is licensed under the MIT License.  
