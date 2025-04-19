using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;

namespace Cli.Helpers
{
    using Spectre.Console;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json;

    public static class CliWidgets
    {
        private static class GalaxyStyle
        {
            public static Color Primary = Color.MediumPurple;
            public static Color Accent = Color.DeepSkyBlue1;
            public static Color Success = Color.Green;
            public static Color Background = Color.Black;
            public static Color Text = Color.Silver;
            public static Color Border = Color.Plum4;
            public static Color Header = Color.Orchid1;
        }

        public static void RenderRule(string title)
        {
            AnsiConsole.Write(new Rule($"[bold {GalaxyStyle.Header}]{title}[/]").RuleStyle(GalaxyStyle.Border));
        }

        public static void RenderError(string message)
        {
            var panel = new Panel(new Markup($"[red]{message}[/]"))
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(foreground: Color.Red))
                .Padding(1, 1)
                .Header("❌ Error", Justify.Center)
                .Expand();

            AnsiConsole.Write(panel);
        }

        public static void RenderError(Exception ex)
        {
            string userMessage = "Something went wrong. Please try again or contact support.";
            string detail = ex?.Message ?? "No additional details are available.";

            // Optional: log full exception elsewhere here, if needed for diagnostics

            var content = new StringBuilder();
            content.AppendLine($"[red]{userMessage}[/]");
            content.AppendLine();
            content.AppendLine($"[grey]Details: {detail}[/]");

            var panel = new Panel(new Markup(content.ToString().Trim()))
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(foreground: Color.Red))
                .Padding(1, 1)
                .Header("❌ Error", Justify.Center)
                .Expand();

            AnsiConsole.Write(panel);
        }


        public static string RenderSelection(string title, IEnumerable<string> options)
        {
            var panel = new Panel($"[bold {GalaxyStyle.Accent}]{title}[/]")
                .Header("☄️ Select an Option", Justify.Center)
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(foreground: GalaxyStyle.Border))
                .Padding(1, 1)
                .Expand();

            AnsiConsole.Write(panel);

            var prompt = new SelectionPrompt<string>()
                .AddChoices(options)
                .HighlightStyle(new Style(foreground: GalaxyStyle.Accent))
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .UseConverter(opt => $"[white]{opt}[/]");

            return AnsiConsole.Prompt(prompt);
        }


        public static void RenderPanel(string content, string? header = null)
        {
            var panel = new Panel($"[white]{content}[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(foreground: GalaxyStyle.Border))
                .Padding(1, 1)
                .Header(header ?? "", Justify.Center)
                .Expand();

            AnsiConsole.Write(panel);
        }

        public static void RenderTable(string title, IEnumerable<string> columns, IEnumerable<IEnumerable<string>> rows)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(GalaxyStyle.Border)
                .Title($"[bold {GalaxyStyle.Primary}]{title}[/]");

            foreach (var column in columns)
            {
                table.AddColumn(new TableColumn($"[bold {GalaxyStyle.Accent}]{column}[/]"));
            }

            foreach (var row in rows)
            {
                table.AddRow(row.Select(cell => $"[white]{cell}[/]").ToArray());
            }

            AnsiConsole.Write(table);
        }

        public static void RenderTable(string panelTitle, Table table)
        {
            table.Border(TableBorder.Rounded);
            table.BorderColor(GalaxyStyle.Border);

            var panel = new Panel(table)
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(foreground: GalaxyStyle.Border))
                .Padding(1, 1)
                .Header(panelTitle, Justify.Center).Expand();

            AnsiConsole.Write(panel);
        }

        public static void RenderText(string text)
        {
            AnsiConsole.Markup($"[{GalaxyStyle.Text}]{text}[/]\n");
        }

        public static string PromptText(string question)
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold {GalaxyStyle.Accent}]{question}[/]")
                    .PromptStyle(GalaxyStyle.Primary)
                    .DefaultValue("N/A")
                    .ShowDefaultValue());
        }

        public static bool Confirm(string question, bool defaultAnswer = true)
        {
            return AnsiConsole.Confirm($"[bold {GalaxyStyle.Accent}]{question}[/]", defaultAnswer);
        }

        public static void RenderPaginatedTable(string panelTitle, IEnumerable<string> columns, IEnumerable<IEnumerable<string>> rows, int pageSize = 10)
        {
            var rowList = rows.ToList();
            int totalPages = (int)Math.Ceiling((double)rowList.Count / pageSize);
            int currentPage = 0;

            while (true)
            {
                // Slice current page
                var pageRows = rowList.Skip(currentPage * pageSize).Take(pageSize).ToList();

                // Build table
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(GalaxyStyle.Border);

                foreach (var col in columns)
                    table.AddColumn($"[bold {GalaxyStyle.Accent}]{col}[/]");

                foreach (var row in pageRows)
                    table.AddRow(row.Select(cell => $"[white]{cell}[/]").ToArray());

                // Wrap in panel
                var panel = new Panel(table)
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(foreground: GalaxyStyle.Border))
                    .Padding(1, 1)
                    .Header($"{panelTitle} - Page {currentPage + 1}/{totalPages}", Justify.Center).Expand();

                AnsiConsole.Clear(); // optional: clear console for cleaner navigation
                AnsiConsole.Write(panel);

                // Navigation
                var choices = new List<string> { "Quit" };
                if (currentPage > 0) choices.Add("Previous Page");
                if (currentPage < totalPages - 1) choices.Add("Next Page");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold silver]Navigate:[/]")
                        .AddChoices(choices)
                        .UseConverter(opt => $"[bold {GalaxyStyle.Accent}]{opt}[/]"));

                if (choice == "Next Page") currentPage++;
                else if (choice == "Previous Page") currentPage--;
                else break;
            }
        }

        public static void RenderPaginatedTable(string panelTitle, Table fullTable, int pageSize = 10)
        {
            // Extract rows manually
            var rows = new List<TableRow>();
            foreach (var row in fullTable.Rows)
                rows.Add(row);

            var totalPages = (int)Math.Ceiling((double)rows.Count / pageSize);
            int currentPage = 0;

            while (true)
            {
                // Clone the base table structure
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(GalaxyStyle.Border);

                // Clone columns
                foreach (var col in fullTable.Columns)
                {
                    table.AddColumn(col);
                }

                // Add only the rows for the current page
                var pageRows = rows
                    .Skip(currentPage * pageSize)
                    .Take(pageSize)
                    .ToList();

                foreach (var row in pageRows)
                    table.AddRow(row);

                // Wrap table in panel
                var panel = new Panel(table)
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(foreground: GalaxyStyle.Border))
                    .Padding(1, 1)
                    .Header($"{panelTitle} - Page {currentPage + 1}/{totalPages}", Justify.Center).Expand();

                AnsiConsole.Clear();
                AnsiConsole.Write(panel);

                // Pagination controls
                var choices = new List<string> { "Quit" };
                if (currentPage > 0) choices.Add("Previous Page");
                if (currentPage < totalPages - 1) choices.Add("Next Page");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold silver]Navigate:[/]")
                        .AddChoices(choices)
                        .UseConverter(opt => $"[bold {GalaxyStyle.Accent}]{opt}[/]"));

                if (choice == "Next Page") currentPage++;
                else if (choice == "Previous Page") currentPage--;
                else break;
            }
        }

        public static void RenderWarning(string message)
        {
            var panel = new Panel($"[yellow]{message}[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(foreground: Color.LightGoldenrod1))
                .Padding(1, 1)
                .Header("⚠️ Notice", Justify.Center).Expand();

            AnsiConsole.Write(panel);
        }

        public static void RenderSuccess(string message)
        {
            var panel = new Panel($"[green]{message}[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(foreground: Color.Green))
                .Padding(1, 1)
                .Header("✅ Success", Justify.Center).Expand();

            AnsiConsole.Write(panel);
        }


        public static void RenderHttpResponseAsync(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                var panel = new Panel($"[white]{content}[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(foreground: Color.Green))
                    .Padding(1, 1)
                    .Header("✅ Success", Justify.Center)
                    .Expand();

                AnsiConsole.Write(panel);
            }
            else
            {
                string title = "An error occurred.";
                string detail = $@"
                    We could not figure out what went wrong. 
                    Please log this error with support staff. 
                    You can do this by screenshotting the full error message or copying and pasting the full error message and emailing it to the support email.
                    \n\n{content}";

                try
                {
                    var error = JsonSerializer.Deserialize<Api.DTOs.ErrorResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (error != null)
                    {
                        title = !string.IsNullOrWhiteSpace(error.Title)
                            ? error.Title
                            : "There was an error performing that action. Please try again later.";

                        detail = error.Detail ?? detail;
                    }
                }
                catch
                {
                    // If deserialization fails, fallback values will be used
                }

                var panel = new Panel($"[red]{detail}[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(new Style(foreground: Color.Red))
                    .Padding(1, 1)
                    .Header($"❌ {title}", Justify.Center)
                    .Expand();

                AnsiConsole.Write(panel);
            }
        }


    }

}